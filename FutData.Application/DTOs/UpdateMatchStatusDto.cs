using System.ComponentModel.DataAnnotations;

namespace FutData.Application.DTOs
{
    public class UpdateMatchStatusDto
    {
        [Required(ErrorMessage = "El estado es requerido")]
        public string Status { get; set; } = string.Empty;
    }
}
