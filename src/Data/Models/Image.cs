namespace Yorit.Api.Data.Models
{
    public class Image
    {
        public Guid Id { get; set; }
        public required string FilePath { get; set; }

        public string? Hash { get; set; }
        public long FileSize { get; set; }  // In bytes
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public string? ExifData { get; set; }  // JSON format for extracted metadata
    }
}
