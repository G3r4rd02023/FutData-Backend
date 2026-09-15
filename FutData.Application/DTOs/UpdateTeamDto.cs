using System.ComponentModel.DataAnnotations;

namespace FutData.Application.DTOs
{
    public class UpdateTeamDto
    {
        [Required(ErrorMessage = "El nombre es requerido")]
        [MaxLength(100, ErrorMessage = "El nombre no puede exceder 100 caracteres")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "La ciudad es requerida")]
        [MaxLength(100, ErrorMessage = "La ciudad no puede exceder 100 caracteres")]
        public string City { get; set; } = string.Empty;

        [MaxLength(100, ErrorMessage = "El estadio no puede exceder 100 caracteres")]
        public string? Stadium { get; set; }
    }
}
