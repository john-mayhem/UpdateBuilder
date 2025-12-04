using System.Text;
using UpdateBuilder.Models;
using Serilog;

namespace UpdateBuilder.Services
{
    public class FileListManager
    {
        private const byte XOR_KEY = 0xAA;

        public static async Task WriteFileListAsync(string filePath, UpdatePackage package, CancellationToken cancellationToken = default)
        {
            try
            {
                Log.Information("Writing filelist to {FilePath}", filePath);

                var content = new StringBuilder();

                // Add system information (10 lines reserved)
                content.AppendLine(package.Timestamp.ToString("dd.MM.yyyy - HH:mm:ss"));
                content.AppendLine(package.ProductName);
                content.AppendLine(package.Version);
                content.AppendLine($"{package.TotalPackedSize}/{package.TotalUnpackedSize}");

                // Lines 5-10 reserved (empty)
                for (int i = 4; i < 10; i++)
                {
                    content.AppendLine("");
                }

                // Add file information
                foreach (var file in package.Files.OrderBy(f => f.RelativePath))
                {
                    content.AppendLine(file.ToString());
                }

                // Encode and write
                byte[] encodedContent = EncodeFileList(content.ToString());
                await File.WriteAllBytesAsync(filePath, encodedContent, cancellationToken);

                Log.Information("Filelist written successfully. Total files: {FileCount}", package.Files.Count);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to write filelist to {FilePath}", filePath);
                throw;
            }
        }

        public static async Task<UpdatePackage?> ReadFileListAsync(string filePath, CancellationToken cancellationToken = default)
        {
            try
            {
                if (!File.Exists(filePath))
                {
                    Log.Warning("Filelist does not exist: {FilePath}", filePath);
                    return null;
                }

                Log.Debug("Reading filelist from {FilePath}", filePath);

                byte[] encodedContent = await File.ReadAllBytesAsync(filePath, cancellationToken);
                string decodedContent = DecodeFileList(encodedContent);

                string[] lines = decodedContent.Split(['\r', '\n'], StringSplitOptions.None);

                if (lines.Length < 10)
                {
                    Log.Warning("Invalid filelist format. Expected at least 10 header lines.");
                    return null;
                }

                var package = new UpdatePackage
                {
                    ProductName = lines[1].Trim(),
                    Version = lines[2].Trim()
                };

                // Parse timestamp
                if (DateTime.TryParseExact(lines[0].Trim(), "dd.MM.yyyy - HH:mm:ss", null,
                    System.Globalization.DateTimeStyles.None, out DateTime timestamp))
                {
                    package.Timestamp = timestamp;
                }

                // Parse sizes
                var sizeParts = lines[3].Split('/');
                if (sizeParts.Length == 2)
                {
                    if (long.TryParse(sizeParts[0], out long packedSize))
                        package.TotalPackedSize = packedSize;
                    if (long.TryParse(sizeParts[1], out long unpackedSize))
                        package.TotalUnpackedSize = unpackedSize;
                }

                // Parse file entries (starting from line 10)
                for (int i = 10; i < lines.Length; i++)
                {
                    string line = lines[i].Trim();
                    if (string.IsNullOrWhiteSpace(line))
                        continue;

                    var parts = line.Split([' '], StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length >= 3)
                    {
                        package.Files.Add(new FileListEntry
                        {
                            Sha256Hash = parts[0],
                            FileSize = long.TryParse(parts[1], out long size) ? size : 0,
                            RelativePath = string.Join(" ", parts.Skip(2))
                        });
                    }
                }

                Log.Debug("Filelist read successfully. Files: {FileCount}", package.Files.Count);
                return package;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to read filelist from {FilePath}", filePath);
                return null;
            }
        }

        private static byte[] EncodeFileList(string content)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(content);
            for (int i = 0; i < bytes.Length; i++)
            {
                bytes[i] = (byte)(bytes[i] ^ XOR_KEY);
            }
            return bytes;
        }

        private static string DecodeFileList(byte[] encodedContent)
        {
            for (int i = 0; i < encodedContent.Length; i++)
            {
                encodedContent[i] = (byte)(encodedContent[i] ^ XOR_KEY);
            }
            return Encoding.UTF8.GetString(encodedContent);
        }
    }
}
