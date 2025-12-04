using System.Security.Cryptography;
using Serilog;

namespace UpdateBuilder.Services
{
    public class FileHasher
    {
        public static async Task<string> CalculateFileHashAsync(string filePath, CancellationToken cancellationToken = default)
        {
            try
            {
                Log.Debug("Calculating SHA256 hash for {FilePath}", filePath);
                using var sha256 = SHA256.Create();
                using var stream = File.OpenRead(filePath);
                var hashBytes = await sha256.ComputeHashAsync(stream, cancellationToken);
                var hash = BitConverter.ToString(hashBytes).Replace("-", "");
                Log.Debug("Hash calculated: {Hash}", hash);
                return hash;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to calculate hash for {FilePath}", filePath);
                throw;
            }
        }
    }
}
