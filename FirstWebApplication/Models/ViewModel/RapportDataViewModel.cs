using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace FirstWebApplication.Models.ViewModel
{
    // ObstacleData representerer data om en hindring som registreres i systemet.
    // Brukes sammen med skjemaet i ObstacleController
    public class RapportDataViewModel
    {
        public int ViewRapportID { get; set; }

        public int ViewObstacleId { get; set; }

        public ObstacleData ViewObstacle { get; set; }

        [MaxLength(1000)]
        public string ViewRapportComment { get; set; } = string.Empty;

    }
}
