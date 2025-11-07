using FirstWebApplication.Models;
using System.Collections.Generic;

namespace FirstWebApplication.Models.ViewModel
{
    public class RegistrarViewModel
    {
        public IEnumerable<ObstacleData> Obstacles { get; set; } = new List<ObstacleData>();
        public IEnumerable<RapportData> Rapports { get; set; } = new List<RapportData>();
        public RapportData NewRapport { get; set; } = new RapportData();
    }
}