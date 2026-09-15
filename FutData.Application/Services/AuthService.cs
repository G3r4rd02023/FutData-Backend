using FutData.Application.DTOs;
using FutData.Application.Interfaces;
using FutData.Domain.Entities;
using FutData.Domain.Enums;

namespace FutData.Application.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepository;
        private readonly IJwtTokenGenerator _jwtTokenGenerator;

        public AuthService(IUserRepository userRepository, IJwtTokenGenerator jwtTokenGenerator)
        {
            _userRepository = userRepository;
            _jwtTokenGenerator = jwtTokenGenerator;
        }

        public async Task<AuthResponseDto> RegisterAsync(RegisterDto dto)
        {
            if (await _userRepository.GetByEmailAsync(dto.Email) != null)
                throw new InvalidOperationException("El email ya está registrado");

            if (await _userRepository.GetByUsernameAsync(dto.Username) != null)
                throw new InvalidOperationException("El nombre de usuario ya está en uso");

            var user = new User
            {
                Id = Guid.NewGuid(),
                Username = dto.Username,
                Email = dto.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password, 12),
                Role = UserRole.User,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _userRepository.AddAsync(user);

            var token = _jwtTokenGenerator.GenerateToken(user);

            return new AuthResponseDto
            {
                Token = token,
                User = MapToProfile(user)
            };
        }

        public async Task<AuthResponseDto> LoginAsync(LoginDto dto)
        {
            var user = await _userRepository.GetByEmailAsync(dto.Email);

            if (user == null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
                throw new UnauthorizedAccessException("Email o contraseña incorrectos");

            var token = _jwtTokenGenerator.GenerateToken(user);

            return new AuthResponseDto
            {
                Token = token,
                User = MapToProfile(user)
            };
        }

        public async Task<UserProfileDto> GetProfileAsync(Guid userId)
        {
            var user = await _userRepository.GetByIdAsync(userId)
                ?? throw new KeyNotFoundException("Usuario no encontrado");

            return MapToProfile(user);
        }

        public async Task<UserProfileDto> UpdateProfileAsync(Guid userId, UpdateProfileDto dto)
        {
            var user = await _userRepository.GetByIdAsync(userId)
                ?? throw new KeyNotFoundException("Usuario no encontrado");

            var existingEmail = await _userRepository.GetByEmailAsync(dto.Email);
            if (existingEmail != null && existingEmail.Id != userId)
                throw new InvalidOperationException("El email ya está en uso");

            var existingUsername = await _userRepository.GetByUsernameAsync(dto.Username);
            if (existingUsername != null && existingUsername.Id != userId)
                throw new InvalidOperationException("El nombre de usuario ya está en uso");

            user.Username = dto.Username;
            user.Email = dto.Email;
            user.UpdatedAt = DateTime.UtcNow;

            await _userRepository.UpdateAsync(user);

            return MapToProfile(user);
        }

        public async Task<List<UserProfileDto>> GetAllUsersAsync()
        {
            var users = await _userRepository.GetAllAsync();
            return users.Select(MapToProfile).ToList();
        }

        public async Task<UserProfileDto> UpdateUserRoleAsync(Guid userId, UserRole role)
        {
            var user = await _userRepository.GetByIdAsync(userId)
                ?? throw new KeyNotFoundException("Usuario no encontrado");

            user.Role = role;
            user.UpdatedAt = DateTime.UtcNow;

            await _userRepository.UpdateAsync(user);

            return MapToProfile(user);
        }

        private static UserProfileDto MapToProfile(User user)
        {
            return new UserProfileDto
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                Role = user.Role,
                CreatedAt = user.CreatedAt
            };
        }
    }
}
