using FirstWebApplication.Models;
using System.Collections.Generic;

namespace FirstWebApplication.Models.ViewModel
{
    /// <summary>
    /// ViewModel for displaying notifications/comments on obstacles
    /// </summary>
    public class VarslingViewModel
    {
        public ObstacleData Obstacle { get; set; }
        public int CommentCount { get; set; }
        public List<RapportData> Comments { get; set; } = new List<RapportData>();
    }
}

