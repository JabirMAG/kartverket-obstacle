using FirstWebApplication.Models;

namespace FirstWebApplication.Repositories
{
    /// <summary>
    /// Repository interface for report/rapport data operations
    /// </summary>
    public interface IRegistrarRepository
    {
        /// <summary>
        /// Adds a new report
        /// </summary>
        Task<RapportData> AddRapport(RapportData rapport);
        
        /// <summary>
        /// Gets all reports
        /// </summary>
        Task<IEnumerable<RapportData>> GetAllRapports();
        
        /// <summary>
        /// Updates an existing report
        /// </summary>
        Task<RapportData> UpdateRapport(RapportData rapports);
    }
}
