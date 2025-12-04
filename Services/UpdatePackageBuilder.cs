using System.Collections.Concurrent;
using System.IO.Compression;
using UpdateBuilder.Models;
using UpdateBuilder.Utilities;
using Serilog;

namespace UpdateBuilder.Services
{
    public class UpdatePackageBuilder
    {
        private readonly FileHasher _hasher;
        private readonly FileArchiver _archiver;
        private readonly ValidationService _validator;
        private readonly FileListManager _fileListManager;

        public UpdatePackageBuilder()
        {
            _hasher = new FileHasher();
            _archiver = new FileArchiver();
            _validator = new ValidationService();
            _fileListManager = new FileListManager();
        }

        public static async Task<UpdatePackage> BuildPackageAsync(
            string inputDirectory,
            string outputDirectory,
            string productName,
            string version,
            CompressionLevel compressionLevel,
            BuildConfiguration config,
            IProgress<BuildProgress>? progress,
            CancellationToken cancellationToken = default)
        {
            Log.Information("Starting package build. Input: {InputDir}, Output: {OutputDir}", inputDirectory, outputDirectory);
            Log.Information("Product: {Product}, Version: {Version}, Compression: {Compression}",
                productName, version, compressionLevel);

            // Validate product name length
            if (productName.Length > config.MaxProductNameLength)
            {
                throw new ArgumentException(
                    $"Product name exceeds maximum length of {config.MaxProductNameLength} characters. " +
                    $"Current length: {productName.Length}");
            }

            // Clear output directory
            ClearOutputDirectory(outputDirectory);

            // Get all files
            var exclusionMatcher = new ExclusionMatcher(config.ExcludedExtensions, config.ExcludedFolders);
            string[] allFiles = Directory.GetFiles(inputDirectory, "*", SearchOption.AllDirectories);

            // Filter excluded files
            var filesToProcess = allFiles
                .Select(file => new
                {
                    FullPath = file,
                    RelativePath = file[(inputDirectory.Length + 1)..].Replace("\\", "/")
                })
                .Where(f => !exclusionMatcher.ShouldExclude(f.RelativePath))
                .ToList();

            Log.Information("Found {TotalFiles} files, {ProcessedFiles} after exclusions",
                allFiles.Length, filesToProcess.Count);

            int totalFiles = filesToProcess.Count;
            int filesProcessed = 0;
            long totalUnpackedSize = 0;
            long totalPackedSize = 0;

            var fileEntries = new ConcurrentBag<FileListEntry>();

            // Process files in parallel
            await Parallel.ForEachAsync(filesToProcess,
                new ParallelOptions
                {
                    MaxDegreeOfParallelism = Environment.ProcessorCount,
                    CancellationToken = cancellationToken
                },
                async (fileInfo, token) =>
                {
                    try
                    {
                        progress?.Report(new BuildProgress
                        {
                            CurrentFile = fileInfo.RelativePath,
                            ProcessedFiles = filesProcessed,
                            TotalFiles = totalFiles
                        });

                        // Calculate hash
                        string hash = await FileHasher.CalculateFileHashAsync(fileInfo.FullPath, token);
                        long fileSize = new FileInfo(fileInfo.FullPath).Length;

                        // Create archive
                        string archivePath = Path.Combine(outputDirectory, fileInfo.RelativePath + ".zip");
                        long archiveSize = await FileArchiver.ArchiveFileAsync(
                            fileInfo.FullPath, archivePath, compressionLevel, token);

                        // Validate archive
                        bool isValid = await ValidationService.ValidateArchiveAsync(
                            archivePath, hash, fileSize, token);

                        if (!isValid)
                        {
                            throw new InvalidOperationException(
                                $"Archive validation failed for {fileInfo.RelativePath}");
                        }

                        // Add to collection
                        fileEntries.Add(new FileListEntry
                        {
                            RelativePath = fileInfo.RelativePath,
                            Sha256Hash = hash,
                            FileSize = fileSize
                        });

                        Interlocked.Add(ref totalUnpackedSize, fileSize);
                        Interlocked.Add(ref totalPackedSize, archiveSize);

                        int processed = Interlocked.Increment(ref filesProcessed);
                        progress?.Report(new BuildProgress
                        {
                            CurrentFile = fileInfo.RelativePath,
                            ProcessedFiles = processed,
                            TotalFiles = totalFiles
                        });
                    }
                    catch (Exception ex)
                    {
                        Log.Error(ex, "Failed to process file {FilePath}", fileInfo.RelativePath);
                        throw;
                    }
                });

            // Create package
            var package = new UpdatePackage
            {
                Timestamp = DateTime.Now,
                ProductName = productName,
                Version = version,
                TotalPackedSize = totalPackedSize,
                TotalUnpackedSize = totalUnpackedSize,
                Files = [.. fileEntries.OrderBy(f => f.RelativePath)]
            };

            // Write filelist
            string fileListPath = Path.Combine(outputDirectory, "filelist.bin");
            await FileListManager.WriteFileListAsync(fileListPath, package, cancellationToken);

            Log.Information("Package build completed successfully. " +
                "Files: {FileCount}, Packed: {PackedSize} bytes, Unpacked: {UnpackedSize} bytes",
                package.Files.Count, totalPackedSize, totalUnpackedSize);

            return package;
        }

        public static async Task<string> GetNextVersionAsync(string outputDirectory, string? manualVersion = null)
        {
            if (!string.IsNullOrWhiteSpace(manualVersion))
            {
                return manualVersion;
            }

            string fileListPath = Path.Combine(outputDirectory, "filelist.bin");
            var existingPackage = await FileListManager.ReadFileListAsync(fileListPath);

            if (existingPackage != null && Version.TryParse(existingPackage.Version, out Version? existingVersion))
            {
                if (existingVersion.Build == -1)
                {
                    return new Version(existingVersion.Major, existingVersion.Minor + 1).ToString();
                }
                else
                {
                    return new Version(existingVersion.Major, existingVersion.Minor, existingVersion.Build + 1).ToString();
                }
            }

            return "0.0.1";
        }

        private static void ClearOutputDirectory(string directory)
        {
            Log.Debug("Clearing output directory: {Directory}", directory);

            foreach (string file in Directory.GetFiles(directory))
            {
                if (!string.Equals(Path.GetFileName(file), "filelist.bin", StringComparison.OrdinalIgnoreCase))
                {
                    File.Delete(file);
                }
            }

            foreach (string dir in Directory.GetDirectories(directory))
            {
                Directory.Delete(dir, true);
            }

            Log.Debug("Output directory cleared");
        }
    }

    public class BuildProgress
    {
        public string CurrentFile { get; set; } = string.Empty;
        public int ProcessedFiles { get; set; }
        public int TotalFiles { get; set; }
        public int PercentComplete => TotalFiles > 0 ? (ProcessedFiles * 100 / TotalFiles) : 0;
    }
}
