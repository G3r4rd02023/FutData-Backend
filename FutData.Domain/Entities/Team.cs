namespace FutData.Domain.Entities
{
    public class Team
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string? Stadium { get; set; }
        public string? LogoUrl { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
