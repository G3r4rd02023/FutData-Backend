using FutData.Application.DTOs;
using FutData.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FutData.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class TeamsController : ControllerBase
    {
        private readonly ITeamService _teamService;
        private readonly IImageService _imageService;

        public TeamsController(ITeamService teamService, IImageService imageService)
        {
            _teamService = teamService;
            _imageService = imageService;
        }

        [HttpGet]
        public async Task<ActionResult<List<TeamDto>>> GetAll([FromQuery] string? search)
        {
            var teams = string.IsNullOrWhiteSpace(search)
                ? await _teamService.GetAllAsync()
                : await _teamService.SearchAsync(search);
            return Ok(teams);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<TeamDto>> GetById(Guid id)
        {
            var team = await _teamService.GetByIdAsync(id);
            if (team == null) return NotFound(new { message = "Equipo no encontrado" });
            return Ok(team);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<ActionResult<TeamDto>> Create([FromBody] CreateTeamDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { message = "Datos inválidos", errors = ModelState });

            try
            {
                var team = await _teamService.CreateAsync(dto);
                return CreatedAtAction(nameof(GetById), new { id = team.Id }, team);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Error interno: {ex.Message}", detail = ex.InnerException?.Message });
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("{id}")]
        public async Task<ActionResult<TeamDto>> Update(Guid id, [FromBody] UpdateTeamDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { message = "Datos inválidos", errors = ModelState });

            try
            {
                var team = await _teamService.UpdateAsync(id, dto);
                return Ok(team);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Error interno: {ex.Message}" });
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(Guid id)
        {
            try
            {
                await _teamService.DeleteAsync(id);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Error interno: {ex.Message}" });
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("{id}/logo")]
        public async Task<ActionResult<TeamDto>> UploadLogo(Guid id, IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest(new { message = "No se ha proporcionado un archivo" });

            if (file.Length > 2 * 1024 * 1024)
                return BadRequest(new { message = "El archivo excede el tamaño máximo de 2MB" });

            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png" };
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!allowedExtensions.Contains(extension))
                return BadRequest(new { message = "Formato no válido. Use JPG o PNG" });

            try
            {
                using var stream = file.OpenReadStream();
                var imageUrl = await _imageService.UploadImageAsync(stream, file.FileName);

                var team = await _teamService.UpdateLogoAsync(id, imageUrl);
                return Ok(team);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Error al subir imagen: {ex.Message}" });
            }
        }
    }
}
