namespace UpdateBuilder.Models
{
    public class UpdatePackage
    {
        public DateTime Timestamp { get; set; }
        public required string ProductName { get; set; }
        public required string Version { get; set; }
        public long TotalPackedSize { get; set; }
        public long TotalUnpackedSize { get; set; }
        public List<FileListEntry> Files { get; set; } = [];
    }
}
