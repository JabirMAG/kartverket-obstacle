using FirstWebApplication.Models;
using System.Collections.Generic;

namespace FirstWebApplication.Models.ViewModel
{
    public class VarslingViewModel
    {
        public ObstacleData Obstacle { get; set; }
        public int CommentCount { get; set; }
        public List<RapportData> Comments { get; set; } = new List<RapportData>();
    }
}

