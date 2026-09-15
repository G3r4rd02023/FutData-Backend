using System.Text.Json.Serialization;
using FutData.Domain.Enums;

namespace FutData.Application.DTOs
{
    public class UserProfileDto
    {
        public Guid Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public UserRole Role { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
