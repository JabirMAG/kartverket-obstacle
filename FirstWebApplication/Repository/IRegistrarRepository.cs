using FirstWebApplication.Models;

namespace FirstWebApplication.Repositories
{
    public interface IRegistrarRepository
    {
        Task<RapportData> AddRapport(RapportData rapport);
        // Task<ObstacleData> GetElementById(int Obstacleid);
        Task<IEnumerable<RapportData>> GetAllRapports();
        //Task<ObstacleData> DeleteById(int Obstacleid
        Task<RapportData> UpdateRapport(RapportData rapports);
    }
}
