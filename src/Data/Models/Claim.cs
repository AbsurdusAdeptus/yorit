namespace Yorit.Api.Data.Models
{
    public class Claim
    {
        public Guid Id { get; set; }
        public required string Type { get; set; }
        public required string Value { get; set; }
    }
}
