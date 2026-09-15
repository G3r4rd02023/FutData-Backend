using FutData.Application.DTOs;
using FutData.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace FutData.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController(IAuthService authService) : ControllerBase
    {
        private readonly IAuthService _authService = authService;

        [HttpPost("register")]
        public async Task<ActionResult<AuthResponseDto>> Register([FromBody] RegisterDto dto)
        {
            try
            {
                var result = await _authService.RegisterAsync(dto);
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("login")]
        public async Task<ActionResult<AuthResponseDto>> Login([FromBody] LoginDto dto)
        {
            try
            {
                var result = await _authService.LoginAsync(dto);
                return Ok(result);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
        }

        [Authorize]
        [HttpGet("profile")]
        public async Task<ActionResult<UserProfileDto>> GetProfile()
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized();

            var result = await _authService.GetProfileAsync(userId.Value);
            return Ok(result);
        }

        [Authorize]
        [HttpPut("profile")]
        public async Task<ActionResult<UserProfileDto>> UpdateProfile([FromBody] UpdateProfileDto dto)
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized();

            try
            {
                var result = await _authService.UpdateProfileAsync(userId.Value, dto);
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("users")]
        public async Task<ActionResult<List<UserProfileDto>>> GetAllUsers()
        {
            var result = await _authService.GetAllUsersAsync();
            return Ok(result);
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("users/{id}/role")]
        public async Task<ActionResult<UserProfileDto>> UpdateUserRole(Guid id, [FromBody] UpdateRoleDto dto)
        {
            try
            {
                var result = await _authService.UpdateUserRoleAsync(id, dto.Role);
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        private Guid? GetUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier) ?? User.FindFirst("sub");
            if (userIdClaim == null) return null;

            return Guid.TryParse(userIdClaim.Value, out var userId) ? userId : null;
        }
    }
}
