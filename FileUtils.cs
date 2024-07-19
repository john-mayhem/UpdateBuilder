using System;
using System.IO;
using System.Security.Cryptography;
using System.IO.Compression;
using System.Threading.Tasks;

namespace UpdateBuilder
{
    public static class FileUtils
    {
        public static async Task<string> CalculateFileHashAsync(string filePath)
        {
            using var sha256 = SHA256.Create();
            using var stream = File.OpenRead(filePath);
            var hashBytes = await sha256.ComputeHashAsync(stream);
            return BitConverter.ToString(hashBytes).Replace("-", "");
        }

        public static async Task ArchiveFileAsync(string sourceFile, string destinationFile)
        {
            var destinationDirectory = Path.GetDirectoryName(destinationFile);
            if (destinationDirectory != null)
            {
                Directory.CreateDirectory(destinationDirectory);
            }

            await Task.Run(() =>
            {
                using var archive = ZipFile.Open(destinationFile, ZipArchiveMode.Create);
                archive.CreateEntryFromFile(sourceFile, Path.GetFileName(sourceFile), CompressionLevel.Optimal);
            });
        }
    }
}