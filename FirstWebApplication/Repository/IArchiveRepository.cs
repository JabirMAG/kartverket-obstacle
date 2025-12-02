using FirstWebApplication.Models;

namespace FirstWebApplication.Repositories
{
    /// <summary>
    /// Repository interface for archiving obstacles and their reports
    /// </summary>
    public interface IArchiveRepository
    {
        /// <summary>
        /// Archives an obstacle and all its associated reports
        /// </summary>
        /// <returns>The number of reports that were archived</returns>
        Task<int> ArchiveObstacleAsync(ObstacleData obstacle);
        
        /// <summary>
        /// Gets all archived reports ordered by archived date descending
        /// </summary>
        /// <returns>List of archived reports</returns>
        Task<List<ArchivedReport>> GetAllArchivedReportsAsync();
        
        /// <summary>
        /// Gets an archived report by ID
        /// </summary>
        /// <param name="archivedReportId">The ID of the archived report</param>
        /// <returns>The archived report if found, null otherwise</returns>
        Task<ArchivedReport?> GetArchivedReportByIdAsync(int archivedReportId);
        
        /// <summary>
        /// Restores an archived report back to active obstacles with a new status
        /// </summary>
        /// <param name="archivedReportId">The ID of the archived report to restore</param>
        /// <param name="newStatus">The new status for the restored obstacle (1 = Under behandling, 2 = Godkjent)</param>
        /// <returns>The number of reports that were restored</returns>
        Task<int> RestoreArchivedReportAsync(int archivedReportId, int newStatus);
        
        /// <summary>
        /// Deletes an archived report
        /// </summary>
        /// <param name="archivedReport">The archived report to delete</param>
        /// <returns>Task representing the async operation</returns>
        Task DeleteArchivedReportAsync(ArchivedReport archivedReport);
    }
}

