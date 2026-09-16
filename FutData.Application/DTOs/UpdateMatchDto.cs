using System.ComponentModel.DataAnnotations;

namespace FutData.Application.DTOs
{
    public class UpdateMatchDto
    {
        [Required(ErrorMessage = "La liga es requerida")]
        public Guid LeagueId { get; set; }

        [Required(ErrorMessage = "El equipo local es requerido")]
        public Guid HomeTeamId { get; set; }

        [Required(ErrorMessage = "El equipo visitante es requerido")]
        public Guid AwayTeamId { get; set; }

        [Required(ErrorMessage = "La fecha del partido es requerida")]
        public DateTime MatchDate { get; set; }

        public int? Round { get; set; }

        [MaxLength(500, ErrorMessage = "Las notas no pueden exceder 500 caracteres")]
        public string? Notes { get; set; }
    }
}
