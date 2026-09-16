using System.ComponentModel.DataAnnotations;

namespace FutData.Application.DTOs
{
    public class ResultDto
    {
        [Required(ErrorMessage = "Los goles del local son requeridos")]
        [Range(0, 50, ErrorMessage = "Los goles deben estar entre 0 y 50")]
        public int HomeGoals { get; set; }

        [Required(ErrorMessage = "Los goles del visitante son requeridos")]
        [Range(0, 50, ErrorMessage = "Los goles deben estar entre 0 y 50")]
        public int AwayGoals { get; set; }
    }
}
