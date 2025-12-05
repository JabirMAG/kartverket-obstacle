using FirstWebApplication.Models;

namespace FirstWebApplication.Repositories
{
    // Repository-grensesnitt for arkivering av hindringer og deres rapporter
    public interface IArchiveRepository
    {
        // Arkiverer en hindring og alle tilknyttede rapporter
        Task<int> ArchiveObstacleAsync(ObstacleData obstacle);
        
        // Henter alle arkiverte rapporter sortert etter arkiveringsdato synkende
        Task<List<ArchivedReport>> GetAllArchivedReportsAsync();
        
        // Henter en arkivert rapport etter ID
        Task<ArchivedReport?> GetArchivedReportByIdAsync(int archivedReportId);
        
        // Gjenoppretter en arkivert rapport tilbake til aktive hindringer med en ny status
        Task<int> RestoreArchivedReportAsync(int archivedReportId, int newStatus);
        
        // Sletter en arkivert rapport
        Task DeleteArchivedReportAsync(ArchivedReport archivedReport);
    }
}
