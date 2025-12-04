using System.IO.Compression;

namespace UpdateBuilder.Models
{
    public class BuildConfiguration
    {
        public List<string> ExcludedExtensions { get; set; } = [];
        public List<string> ExcludedFolders { get; set; } = [];
        public int MaxProductNameLength { get; set; } = 32;
        public CompressionLevel DefaultCompressionLevel { get; set; } = CompressionLevel.Optimal;
    }
}
