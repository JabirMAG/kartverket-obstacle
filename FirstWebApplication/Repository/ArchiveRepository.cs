using FirstWebApplication.DataContext;
using FirstWebApplication.Models;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace FirstWebApplication.Repositories
{
    // Repository for arkivering av hindringer og deres rapporter
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

        // Arkiverer en hindring og alle tilknyttede rapporter
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

            // Setter hindringsstatus til 3 (Avslått) slik at UpdateObstacles vil slette den
            obstacle.ObstacleStatus = 3;
            await _obstacleRepository.UpdateObstacles(obstacle);
            
            return rapports.Count;
        }

        // Henter alle arkiverte rapporter sortert etter arkiveringsdato synkende
        public async Task<List<ArchivedReport>> GetAllArchivedReportsAsync()
        {
            return await _context.ArchivedReports
                .OrderByDescending(ar => ar.ArchivedDate)
                .ToListAsync();
        }

        // Henter en arkivert rapport etter ID
        public async Task<ArchivedReport?> GetArchivedReportByIdAsync(int archivedReportId)
        {
            return await _context.ArchivedReports
                .FirstOrDefaultAsync(ar => ar.ArchivedReportId == archivedReportId);
        }

        // Gjenoppretter en arkivert rapport tilbake til aktive hindringer med en ny status
        public async Task<int> RestoreArchivedReportAsync(int archivedReportId, int newStatus)
        {
            var archivedReport = await GetArchivedReportByIdAsync(archivedReportId);
            if (archivedReport == null)
            {
                throw new InvalidOperationException("Fant ikke arkivert rapport.");
            }

            // Oppretter ny ObstacleData fra ArchivedReport
            var restoredObstacle = new ObstacleData
            {
                ObstacleName = archivedReport.ObstacleName,
                ObstacleHeight = archivedReport.ObstacleHeight,
                ObstacleDescription = archivedReport.ObstacleDescription,
                GeometryGeoJson = archivedReport.GeometryGeoJson,
                ObstacleStatus = newStatus,
                OwnerUserId = null // Vi har ikke OwnerUserId i ArchivedReport, så sett til null
            };

            // Legger hindringen i databasen - AddObstacle returnerer hindring med ObstacleId
            var savedObstacle = await _obstacleRepository.AddObstacle(restoredObstacle);

            // Parser og oppretter RapportData-oppføringer fra RapportComments
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

            // Oppretter RapportData for hver kommentar
            foreach (var comment in rapportComments)
            {
                var rapport = new RapportData
                {
                    ObstacleId = savedObstacle.ObstacleId,
                    RapportComment = comment
                };
                await _registrarRepository.AddRapport(rapport);
            }

            // Sletter fra ArchivedReport
            await DeleteArchivedReportAsync(archivedReport);

            return rapportComments.Count;
        }

        // Sletter en arkivert rapport
        public async Task DeleteArchivedReportAsync(ArchivedReport archivedReport)
        {
            _context.ArchivedReports.Remove(archivedReport);
            await _context.SaveChangesAsync();
        }
    }
}
