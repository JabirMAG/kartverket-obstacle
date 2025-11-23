using FirstWebApplication.DataContext;
using FirstWebApplication.Models;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace FirstWebApplication.Repositories
{
    /// <summary>
    /// Repository for archiving obstacles and their reports
    /// </summary>
    public class ArchiveRepository : IArchiveRepository
    {
        private readonly ApplicationDBContext _context;
        private readonly IObstacleRepository _obstacleRepository;

        public ArchiveRepository(ApplicationDBContext context, IObstacleRepository obstacleRepository)
        {
            _context = context;
            _obstacleRepository = obstacleRepository;
        }

        /// <summary>
        /// Archives an obstacle and all its associated reports
        /// </summary>
        /// <returns>The number of reports that were archived</returns>
        public async Task<int> ArchiveObstacleAsync(ObstacleData obstacle)
        {
            var rapports = await _context.Rapports
                .Where(r => r.ObstacleId == obstacle.ObstacleId)
                .ToListAsync();
            
            var rapportComments = rapports.Select(r => r.RapportComment).ToList();
            var rapportCommentsJson = JsonSerializer.Serialize(rapportComments);
            
            var archivedReport = new ArchivedReport
            {
                OriginalObstacleId = obstacle.ObstacleId,
                ObstacleName = obstacle.ObstacleName,
                ObstacleHeight = obstacle.ObstacleHeight,
                ObstacleDescription = obstacle.ObstacleDescription,
                GeometryGeoJson = obstacle.GeometryGeoJson,
                ObstacleStatus = 3,
                ArchivedDate = DateTime.UtcNow,
                RapportComments = rapportCommentsJson
            };
            
            await _context.ArchivedReports.AddAsync(archivedReport);
            
            if (rapports.Any())
            {
                _context.Rapports.RemoveRange(rapports);
            }
            
            await _context.SaveChangesAsync();

            // Set obstacle status to 3 (Rejected) so UpdateObstacles will delete it
            obstacle.ObstacleStatus = 3;
            await _obstacleRepository.UpdateObstacles(obstacle);
            
            return rapports.Count;
        }
    }
}

