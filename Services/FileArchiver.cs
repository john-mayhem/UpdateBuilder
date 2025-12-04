using System.IO.Compression;
using Serilog;

namespace UpdateBuilder.Services
{
    public class FileArchiver
    {
        public static async Task<long> ArchiveFileAsync(string sourceFile, string destinationFile, CompressionLevel compressionLevel, CancellationToken cancellationToken = default)
        {
            try
            {
                Log.Debug("Archiving {SourceFile} to {DestinationFile} with {CompressionLevel}",
                    sourceFile, destinationFile, compressionLevel);

                var destinationDirectory = Path.GetDirectoryName(destinationFile);
                if (destinationDirectory != null)
                {
                    Directory.CreateDirectory(destinationDirectory);
                }

                await Task.Run(() =>
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    if (File.Exists(destinationFile))
                    {
                        File.Delete(destinationFile);
                    }

                    using var archive = ZipFile.Open(destinationFile, ZipArchiveMode.Create);
                    archive.CreateEntryFromFile(sourceFile, Path.GetFileName(sourceFile), compressionLevel);
                }, cancellationToken);

                var fileInfo = new FileInfo(destinationFile);
                Log.Debug("Archive created successfully. Size: {Size} bytes", fileInfo.Length);
                return fileInfo.Length;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to archive {SourceFile}", sourceFile);
                throw;
            }
        }
    }
}
