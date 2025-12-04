namespace UpdateBuilder.Models
{
    public class FileListEntry
    {
        public required string RelativePath { get; set; }
        public required string Sha256Hash { get; set; }
        public long FileSize { get; set; }

        public override string ToString()
        {
            return $"{Sha256Hash} {FileSize,20} {RelativePath}";
        }
    }
}
