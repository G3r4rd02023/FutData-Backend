using System.ComponentModel.DataAnnotations;

namespace FutData.Application.DTOs
{
    public class SimulationRequest
    {
        [Required(ErrorMessage = "La liga es requerida")]
        public Guid LeagueId { get; set; }

        [Required(ErrorMessage = "El equipo local es requerido")]
        public Guid HomeTeamId { get; set; }

        [Required(ErrorMessage = "El equipo visitante es requerido")]
        public Guid AwayTeamId { get; set; }

        [Required(ErrorMessage = "La probabilidad de victoria local es requerida")]
        [Range(0, 100)]
        public decimal HomeWinProbability { get; set; }

        [Required(ErrorMessage = "La probabilidad de empate es requerida")]
        [Range(0, 100)]
        public decimal DrawProbability { get; set; }

        [Required(ErrorMessage = "La probabilidad de victoria visitante es requerida")]
        [Range(0, 100)]
        public decimal AwayWinProbability { get; set; }

        [Required(ErrorMessage = "La fecha del partido es requerida")]
        public DateTime MatchDate { get; set; }

        public int? Round { get; set; }
    }
}
