using FutData.Application.DTOs;
using FutData.Domain.Enums;

namespace FutData.Application.Interfaces
{
    public interface IAuthService
    {
        Task<AuthResponseDto> RegisterAsync(RegisterDto dto);
        Task<AuthResponseDto> LoginAsync(LoginDto dto);
        Task<UserProfileDto> GetProfileAsync(Guid userId);
        Task<UserProfileDto> UpdateProfileAsync(Guid userId, UpdateProfileDto dto);
        Task<List<UserProfileDto>> GetAllUsersAsync();
        Task<UserProfileDto> UpdateUserRoleAsync(Guid userId, UserRole role);
    }
}
