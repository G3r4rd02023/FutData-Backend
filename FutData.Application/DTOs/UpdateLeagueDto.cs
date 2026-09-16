using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using FutData.Domain.Enums;

namespace FutData.Application.DTOs
{
    public class UpdateLeagueDto
    {
        [Required(ErrorMessage = "El nombre es requerido")]
        [MaxLength(150, ErrorMessage = "El nombre no puede exceder 150 caracteres")]
        public string Name { get; set; } = string.Empty;

        [MaxLength(500, ErrorMessage = "La descripción no puede exceder 500 caracteres")]
        public string? Description { get; set; }

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public LeagueFormat Format { get; set; }

        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }
}
