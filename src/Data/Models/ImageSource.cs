namespace Yorit.Api.Data.Models
{
    public class ImageSource
    {
        public Guid Id { get; set; } = Guid.NewGuid();  // Unique identifier
        public required string Path { get; set; }  // Relative path to the registered root
        public required string DisplayName { get; set; }  // Friendly name for UI
        public DateTime DateAdded { get; set; } = DateTime.UtcNow;  // When it was added
    }
}
