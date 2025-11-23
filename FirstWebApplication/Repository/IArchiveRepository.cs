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
    }
}

