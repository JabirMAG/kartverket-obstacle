using FirstWebApplication.Models;

namespace FirstWebApplication.Repositories
{
    public interface IRegistrarRepository
    {
        Task<RapportData> AddRapport(RapportData rapport);
        Task<IEnumerable<RapportData>> GetAllRapports();
        Task<RapportData> UpdateRapport(RapportData rapports);
    }
}
