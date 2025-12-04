using System.IO.Compression;
using System.Security.Cryptography;
using Serilog;

namespace UpdateBuilder.Services
{
    public class ValidationService
    {
        public static async Task<bool> ValidateArchiveAsync(string archivePath, string expectedHash, long expectedSize, CancellationToken cancellationToken = default)
        {
            try
            {
                Log.Debug("Validating archive {ArchivePath}", archivePath);

                if (!File.Exists(archivePath))
                {
                    Log.Warning("Archive does not exist: {ArchivePath}", archivePath);
                    return false;
                }

                // Try to open and extract
                string tempExtractPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
                try
                {
                    Directory.CreateDirectory(tempExtractPath);

                    await Task.Run(() =>
                    {
                        cancellationToken.ThrowIfCancellationRequested();
                        ZipFile.ExtractToDirectory(archivePath, tempExtractPath);
                    }, cancellationToken);

                    // Get the extracted file (should be only one)
                    var extractedFiles = Directory.GetFiles(tempExtractPath);
                    if (extractedFiles.Length != 1)
                    {
                        Log.Warning("Archive contains {Count} files, expected 1", extractedFiles.Length);
                        return false;
                    }

                    string extractedFile = extractedFiles[0];
                    var fileInfo = new FileInfo(extractedFile);

                    // Validate size
                    if (fileInfo.Length != expectedSize)
                    {
                        Log.Warning("Size mismatch. Expected: {ExpectedSize}, Actual: {ActualSize}",
                            expectedSize, fileInfo.Length);
                        return false;
                    }

                    // Validate hash
                    using var sha256 = SHA256.Create();
                    using var stream = File.OpenRead(extractedFile);
                    var hashBytes = await sha256.ComputeHashAsync(stream, cancellationToken);
                    var actualHash = BitConverter.ToString(hashBytes).Replace("-", "");

                    if (!actualHash.Equals(expectedHash, StringComparison.OrdinalIgnoreCase))
                    {
                        Log.Warning("Hash mismatch. Expected: {ExpectedHash}, Actual: {ActualHash}",
                            expectedHash, actualHash);
                        return false;
                    }

                    Log.Debug("Archive validation successful");
                    return true;
                }
                finally
                {
                    // Cleanup temp directory
                    if (Directory.Exists(tempExtractPath))
                    {
                        Directory.Delete(tempExtractPath, true);
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Archive validation failed for {ArchivePath}", archivePath);
                return false;
            }
        }
    }
}
