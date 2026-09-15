using FutData.Domain.Enums;

namespace FutData.Application.DTOs
{
    public class AuthResponseDto
    {
        public string Token { get; set; } = string.Empty;
        public UserProfileDto User { get; set; } = null!;
    }
}
