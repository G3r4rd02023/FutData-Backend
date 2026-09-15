namespace FutData.Application.DTOs
{
    public class TeamDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string? Stadium { get; set; }
        public string? LogoUrl { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
