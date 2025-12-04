using FirstWebApplication.Models;
using FirstWebApplication.Models.ViewModel;

namespace FirstWebApplication.Repositories
{
    // Repository-grensesnitt for hindringsdataoperasjoner
    public interface IObstacleRepository
    {
        // Legger til en ny hindring
        Task<ObstacleData> AddObstacle(ObstacleData obstacles);
        
        // Henter en hindring etter ID
        Task<ObstacleData?> GetElementById(int Obstacleid);
        
        // Henter alle hindringer
        Task<IEnumerable<ObstacleData>> GetAllObstacles();
        
        // Oppdaterer en eksisterende hindring
        Task<ObstacleData> UpdateObstacles(ObstacleData obstacles);
        
        // Henter alle hindringer eid av en spesifikk bruker
        Task<IEnumerable<ObstacleData>> GetObstaclesByOwner(string ownerUserId);
        
        // Henter en hindring etter ID hvis den eksisterer og eies av den spesifiserte brukeren
        Task<ObstacleData?> GetObstacleByOwnerAndId(int obstacleId, string ownerUserId);
        
        // Henter alle hindrings-IDer eid av en spesifikk bruker
        Task<HashSet<int>> GetObstacleIdsByOwner(string ownerUserId);
        
        // Henter alle hindringer som er under behandling eller godkjent (status 1 eller 2)
        Task<List<ObstacleData>> GetReportedObstacles();
        
        // Henter alle hindringer under behandling (status 1)
        Task<List<ObstacleData>> GetPendingObstacles();
        
        // Oppdaterer statusen til en hindring. Hvis status er Avslått (3), slettes hindringen.
        Task<ObstacleData?> UpdateObstacleStatus(int obstacleId, int status);
        
        // Sjekker om en hindring kan redigeres (må ha status under behandling)
        Task<bool> CanEditObstacle(int obstacleId);
        
        // Oppdaterer egenskapene til en hindring (navn, beskrivelse, høyde)
        Task<ObstacleData?> UpdateObstacleProperties(int obstacleId, string name, string description, double height);
        
        // Setter eieren av en hindring fra nåværende brukerkontekst
        ObstacleData SetObstacleOwner(ObstacleData obstacle, string userId);
        
        // Mapper ObstacleData til JSON-vennlig format med små bokstaver i egenskapsnavn
        List<object> MapToJsonFormat(IEnumerable<ObstacleData> obstacles);
        
        // Normaliserer hindringsdata for hurtiglagring ved å sette standardverdier for valgfrie felt
        ObstacleData NormalizeForQuickSave(ObstacleData obstacle);
        
        // Validerer at GeometryGeoJson ikke er tom (påkrevd for hurtiglagring)
        bool IsGeometryValid(ObstacleData obstacle);
        
        // Mapper ObstacleDataViewModel til ObstacleData-entitet
        ObstacleData MapFromViewModel(ObstacleDataViewModel viewModel);
        
        // Mapper ObstacleData-entitet til ObstacleDataViewModel
        ObstacleDataViewModel MapToViewModel(ObstacleData obstacle);
    }
}