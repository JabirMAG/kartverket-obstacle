using FirstWebApplication.DataContext;
using FirstWebApplication.Models;
using FirstWebApplication.Models.ViewModel;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace FirstWebApplication.Repositories
{
    // Repository for hindringsdataoperasjoner
    public class ObstacleRepository : IObstacleRepository
    {
        private readonly ApplicationDBContext _context;

        public ObstacleRepository(ApplicationDBContext context)
        {
            _context = context;
        }

        // Legger til en ny hindring
        public async Task<ObstacleData> AddObstacle(ObstacleData obstacle)
        {
            await _context.ObstaclesData.AddAsync(obstacle);
            await _context.SaveChangesAsync();
            return obstacle;
        }

        // Henter en hindring etter ID
        public async Task<ObstacleData?> GetElementById(int id)
        {
            var findById = await _context.ObstaclesData
                .Include(o => o.OwnerUser)
                .Where(x => x.ObstacleId == id)
                .FirstOrDefaultAsync();

            if (findById != null)
            {
                findById.ObstacleName = findById.ObstacleName ?? string.Empty;
                findById.ObstacleDescription = findById.ObstacleDescription ?? string.Empty;
                findById.GeometryGeoJson = findById.GeometryGeoJson ?? string.Empty;
                return findById;
            }
            else
            {
                return null;
            }
        }

        // Oppdaterer en eksisterende hindring. Hvis status er 3, slettes hindringen
        public async Task<ObstacleData> UpdateObstacles(ObstacleData obstacle)
        {
            if (obstacle.ObstacleStatus == 3)
            {
                _context.ObstaclesData.Remove(obstacle);
                await _context.SaveChangesAsync();
                return obstacle;
            }
            _context.ObstaclesData.Update(obstacle);
            await _context.SaveChangesAsync();
            return obstacle;
        }

        // Henter de 50 nyeste hindringene
        public async Task<IEnumerable<ObstacleData>> GetAllObstacles()
        {
            var obstacles = await _context.ObstaclesData
                .Include(o => o.OwnerUser)
                .OrderByDescending(x => x.ObstacleId)
                .Take(50)
                .ToListAsync();
            
            foreach (var obstacle in obstacles)
            {
                obstacle.ObstacleName = obstacle.ObstacleName ?? string.Empty;
                obstacle.ObstacleDescription = obstacle.ObstacleDescription ?? string.Empty;
                obstacle.GeometryGeoJson = obstacle.GeometryGeoJson ?? string.Empty;
            }
            
            return obstacles;
        }

        // Henter alle hindringer eid av en spesifikk bruker
        public async Task<IEnumerable<ObstacleData>> GetObstaclesByOwner(string ownerUserId)
        {
            var obstacles = await _context.ObstaclesData
                .Include(o => o.OwnerUser)
                .Where(x => x.OwnerUserId == ownerUserId)
                .OrderByDescending(x => x.ObstacleId)
                .ToListAsync();
            
            foreach (var obstacle in obstacles)
            {
                obstacle.ObstacleName = obstacle.ObstacleName ?? string.Empty;
                obstacle.ObstacleDescription = obstacle.ObstacleDescription ?? string.Empty;
                obstacle.GeometryGeoJson = obstacle.GeometryGeoJson ?? string.Empty;
            }
            
            return obstacles;
        }

        // Henter en hindring etter ID hvis den eksisterer og eies av den spesifiserte brukeren
        public async Task<ObstacleData?> GetObstacleByOwnerAndId(int obstacleId, string ownerUserId)
        {
            var obstacle = await _context.ObstaclesData
                .Include(o => o.OwnerUser)
                .Where(x => x.ObstacleId == obstacleId && x.OwnerUserId == ownerUserId)
                .FirstOrDefaultAsync();

            if (obstacle != null)
            {
                obstacle.ObstacleName = obstacle.ObstacleName ?? string.Empty;
                obstacle.ObstacleDescription = obstacle.ObstacleDescription ?? string.Empty;
                obstacle.GeometryGeoJson = obstacle.GeometryGeoJson ?? string.Empty;
            }

            return obstacle;
        }

        // Henter alle hindrings-IDer eid av en spesifikk bruker
        public async Task<HashSet<int>> GetObstacleIdsByOwner(string ownerUserId)
        {
            return await _context.ObstaclesData
                .Where(x => x.OwnerUserId == ownerUserId)
                .Select(x => x.ObstacleId)
                .ToHashSetAsync();
        }

        // Henter alle hindringer som er ventende eller godkjent (status 1 eller 2)
        public async Task<List<ObstacleData>> GetReportedObstacles()
        {
            var allObstacles = await GetAllObstacles();
            return allObstacles
                .Where(o => o.ObstacleStatus == 1 || o.ObstacleStatus == 2)
                .ToList();
        }

        // Henter alle ventende hindringer (status 1)
        public async Task<List<ObstacleData>> GetPendingObstacles()
        {
            var allObstacles = await GetAllObstacles();
            return allObstacles
                .Where(o => o.ObstacleStatus == 1)
                .ToList();
        }

        // Oppdaterer statusen til en hindring. Hvis status er Avslått (3), slettes hindringen.
        public async Task<ObstacleData?> UpdateObstacleStatus(int obstacleId, int status)
        {
            var obstacle = await GetElementById(obstacleId);
            if (obstacle == null)
            {
                return null;
            }

            obstacle.ObstacleStatus = status;
            return await UpdateObstacles(obstacle);
        }

        // Sjekker om en hindring kan redigeres (må ha ventende status)
        public async Task<bool> CanEditObstacle(int obstacleId)
        {
            var obstacle = await GetElementById(obstacleId);
            if (obstacle == null)
            {
                return false;
            }
            return obstacle.ObstacleStatus == 1; // StatusPending
        }

        // Oppdaterer egenskapene til en hindring (navn, beskrivelse, høyde)
        public async Task<ObstacleData?> UpdateObstacleProperties(int obstacleId, string name, string description, double height)
        {
            var obstacle = await GetElementById(obstacleId);
            if (obstacle == null)
            {
                return null;
            }

            obstacle.ObstacleName = name;
            obstacle.ObstacleDescription = description;
            obstacle.ObstacleHeight = height;

            return await UpdateObstacles(obstacle);
        }

        // Setter eieren av en hindring fra nåværende brukerkontekst
        public ObstacleData SetObstacleOwner(ObstacleData obstacle, string userId)
        {
            obstacle.OwnerUserId = userId;
            return obstacle;
        }

        // Mapper ObstacleData til JSON-vennlig format med små bokstaver i egenskapsnavn
        public List<object> MapToJsonFormat(IEnumerable<ObstacleData> obstacles)
        {
            return obstacles
                .Select(o => new
                {
                    id = o.ObstacleId,
                    name = o.ObstacleName,
                    height = o.ObstacleHeight,
                    description = o.ObstacleDescription,
                    status = o.ObstacleStatus,
                    geometryGeoJson = o.GeometryGeoJson
                })
                .ToList<object>();
        }

        // Normaliserer hindringsdata for hurtiglagring ved å sette standardverdier for valgfrie felt
        public ObstacleData NormalizeForQuickSave(ObstacleData obstacle)
        {
            obstacle.ObstacleName = obstacle.ObstacleName ?? string.Empty;
            obstacle.ObstacleDescription = obstacle.ObstacleDescription ?? string.Empty;
            return obstacle;
        }

        // Validerer at GeometryGeoJson ikke er tom (påkrevd for hurtiglagring)
        public bool IsGeometryValid(ObstacleData obstacle)
        {
            return !string.IsNullOrEmpty(obstacle.GeometryGeoJson);
        }

        // Mapper ObstacleDataViewModel til ObstacleData entity
        public ObstacleData MapFromViewModel(ObstacleDataViewModel viewModel)
        {
            return new ObstacleData
            {
                ObstacleId = viewModel.ViewObstacleId,
                ObstacleName = viewModel.ViewObstacleName ?? string.Empty,
                ObstacleHeight = viewModel.ViewObstacleHeight,
                ObstacleDescription = viewModel.ViewObstacleDescription ?? string.Empty,
                GeometryGeoJson = viewModel.ViewGeometryGeoJson ?? string.Empty,
                ObstacleStatus = viewModel.ViewObstacleStatus
            };
        }

        // Mapper ObstacleData entity til ObstacleDataViewModel
        public ObstacleDataViewModel MapToViewModel(ObstacleData obstacle)
        {
            return new ObstacleDataViewModel
            {
                ViewObstacleId = obstacle.ObstacleId,
                ViewObstacleName = obstacle.ObstacleName ?? string.Empty,
                ViewObstacleHeight = obstacle.ObstacleHeight,
                ViewObstacleDescription = obstacle.ObstacleDescription ?? string.Empty,
                ViewGeometryGeoJson = obstacle.GeometryGeoJson ?? string.Empty,
                ViewObstacleStatus = obstacle.ObstacleStatus
            };
        }
    }
}
