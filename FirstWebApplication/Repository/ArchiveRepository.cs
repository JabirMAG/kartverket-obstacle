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
        private readonly IRegistrarRepository _registrarRepository;

        public ArchiveRepository(ApplicationDBContext context, IObstacleRepository obstacleRepository, IRegistrarRepository registrarRepository)
        {
            _context = context;
            _obstacleRepository = obstacleRepository;
            _registrarRepository = registrarRepository;
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

        /// <summary>
        /// Gets all archived reports ordered by archived date descending
        /// </summary>
        /// <returns>List of archived reports</returns>
        public async Task<List<ArchivedReport>> GetAllArchivedReportsAsync()
        {
            return await _context.ArchivedReports
                .OrderByDescending(ar => ar.ArchivedDate)
                .ToListAsync();
        }

        /// <summary>
        /// Gets an archived report by ID
        /// </summary>
        /// <param name="archivedReportId">The ID of the archived report</param>
        /// <returns>The archived report if found, null otherwise</returns>
        public async Task<ArchivedReport?> GetArchivedReportByIdAsync(int archivedReportId)
        {
            return await _context.ArchivedReports
                .FirstOrDefaultAsync(ar => ar.ArchivedReportId == archivedReportId);
        }

        /// <summary>
        /// Restores an archived report back to active obstacles with a new status
        /// </summary>
        /// <param name="archivedReportId">The ID of the archived report to restore</param>
        /// <param name="newStatus">The new status for the restored obstacle (1 = Under behandling, 2 = Godkjent)</param>
        /// <returns>The number of reports that were restored</returns>
        public async Task<int> RestoreArchivedReportAsync(int archivedReportId, int newStatus)
        {
            var archivedReport = await GetArchivedReportByIdAsync(archivedReportId);
            if (archivedReport == null)
            {
                throw new InvalidOperationException("Fant ikke arkivert rapport.");
            }

            // Create new ObstacleData from ArchivedReport
            var restoredObstacle = new ObstacleData
            {
                ObstacleName = archivedReport.ObstacleName,
                ObstacleHeight = archivedReport.ObstacleHeight,
                ObstacleDescription = archivedReport.ObstacleDescription,
                GeometryGeoJson = archivedReport.GeometryGeoJson,
                ObstacleStatus = newStatus,
                OwnerUserId = null // We don't have OwnerUserId in ArchivedReport, so set to null
            };

            // Put the obstacle in the database - AddObstacle returns obstacle with ObstacleId
            var savedObstacle = await _obstacleRepository.AddObstacle(restoredObstacle);

            // Parse and create RapportData entries from RapportComments
            List<string> rapportComments = new List<string>();
            try
            {
                if (!string.IsNullOrEmpty(archivedReport.RapportComments))
                {
                    rapportComments = JsonSerializer.Deserialize<List<string>>(archivedReport.RapportComments) ?? new List<string>();
                }
            }
            catch
            {
                rapportComments = new List<string>();
            }

            // Create RapportData for each comment
            foreach (var comment in rapportComments)
            {
                var rapport = new RapportData
                {
                    ObstacleId = savedObstacle.ObstacleId,
                    RapportComment = comment
                };
                await _registrarRepository.AddRapport(rapport);
            }

            // Delete from ArchivedReport
            await DeleteArchivedReportAsync(archivedReport);

            return rapportComments.Count;
        }

        /// <summary>
        /// Deletes an archived report
        /// </summary>
        /// <param name="archivedReport">The archived report to delete</param>
        /// <returns>Task representing the async operation</returns>
        public async Task DeleteArchivedReportAsync(ArchivedReport archivedReport)
        {
            _context.ArchivedReports.Remove(archivedReport);
            await _context.SaveChangesAsync();
        }
    }
}

