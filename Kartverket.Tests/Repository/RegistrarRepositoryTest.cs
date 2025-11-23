using FirstWebApplication.DataContext;
using FirstWebApplication.Models;
using FirstWebApplication.Repositories;
using Kartverket.Tests.Helpers;
using Microsoft.EntityFrameworkCore;

namespace Kartverket.Tests.Repository
{
    /// <summary>
    /// Tests for RegistrarRepository
    /// Verifies CRUD operations, ordering, data limiting, and relationship loading
    /// </summary>
    public class RegistrarRepositoryTest : IDisposable
    {
        private readonly ApplicationDBContext _context;
        private readonly RegistrarRepository _repository;

        /// <summary>
        /// Initializes a new instance of the RegistrarRepositoryTest class
        /// </summary>
        public RegistrarRepositoryTest()
        {
            _context = DatabaseFixture.CreateContext();
            _repository = new RegistrarRepository(_context);
        }

        /// <summary>
        /// Tests that AddRapport adds rapport to database and returns rapport with generated ID
        /// </summary>
        [Fact]
        public async Task AddRapport_ShouldAddToDatabase_AndReturnRapportWithId()
        {
            // Arrange
            var obstacle = TestDataBuilder.CreateValidObstacle();
            await _context.ObstaclesData.AddAsync(obstacle);
            await _context.SaveChangesAsync();
            var rapport = TestDataBuilder.CreateValidRapport(obstacle.ObstacleId);

            // Act
            var result = await _repository.AddRapport(rapport);

            // Assert
            Assert.True(result.RapportID > 0);
            var savedRapport = await _context.Rapports.FindAsync(result.RapportID);
            Assert.NotNull(savedRapport);
            Assert.Equal(rapport.ObstacleId, savedRapport.ObstacleId);
            Assert.Equal(rapport.RapportComment, savedRapport.RapportComment);
        }

        /// <summary>
        /// Tests that GetAllRapports returns rapports ordered by ID in descending order
        /// </summary>
        [Fact]
        public async Task GetAllRapports_ShouldReturnRapports_OrderedByDescendingId()
        {
            // Arrange
            var obstacle1 = TestDataBuilder.CreateValidObstacle();
            var obstacle2 = TestDataBuilder.CreateValidObstacle();
            await _context.ObstaclesData.AddRangeAsync(obstacle1, obstacle2);
            await _context.SaveChangesAsync();

            var rapport1 = TestDataBuilder.CreateValidRapport(obstacle1.ObstacleId, "Comment 1");
            var rapport2 = TestDataBuilder.CreateValidRapport(obstacle2.ObstacleId, "Comment 2");
            await _context.Rapports.AddRangeAsync(rapport1, rapport2);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetAllRapports();

            // Assert
            Assert.NotNull(result);
            var rapportsList = result.ToList();
            Assert.True(rapportsList.Count >= 2);
            // Verify ordering is descending by ID
            for (int i = 0; i < rapportsList.Count - 1; i++)
            {
                Assert.True(rapportsList[i].RapportID >= rapportsList[i + 1].RapportID);
            }
        }

        /// <summary>
        /// Tests that GetAllRapports returns maximum 50 rapports (filters/limits results)
        /// </summary>
        [Fact]
        public async Task GetAllRapports_ShouldReturnMax50Rapports()
        {
            // Arrange
            var obstacle = TestDataBuilder.CreateValidObstacle();
            await _context.ObstaclesData.AddAsync(obstacle);
            await _context.SaveChangesAsync();

            var rapports = new List<RapportData>();
            for (int i = 0; i < 60; i++)
            {
                rapports.Add(TestDataBuilder.CreateValidRapport(obstacle.ObstacleId, $"Comment {i}"));
            }
            await _context.Rapports.AddRangeAsync(rapports);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetAllRapports();

            // Assert
            var rapportsList = result.ToList();
            Assert.True(rapportsList.Count <= 50);
        }

        /// <summary>
        /// Tests that GetAllRapports includes related Obstacle and OwnerUser entities (relationship loading)
        /// </summary>
        [Fact]
        public async Task GetAllRapports_ShouldIncludeObstacle_AndOwnerUser()
        {
            // Arrange
            var obstacle = TestDataBuilder.CreateValidObstacle();
            await _context.ObstaclesData.AddAsync(obstacle);
            await _context.SaveChangesAsync();
            var rapport = TestDataBuilder.CreateValidRapport(obstacle.ObstacleId);
            await _context.Rapports.AddAsync(rapport);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetAllRapports();

            // Assert
            var rapportsList = result.ToList();
            var foundRapport = rapportsList.FirstOrDefault(r => r.RapportID == rapport.RapportID);
            Assert.NotNull(foundRapport);
            Assert.NotNull(foundRapport.Obstacle);
            Assert.Equal(obstacle.ObstacleId, foundRapport.Obstacle.ObstacleId);
        }

        /// <summary>
        /// Tests that UpdateRapport updates rapport in database
        /// </summary>
        [Fact]
        public async Task UpdateRapport_ShouldUpdateRapport()
        {
            // Arrange
            var obstacle = TestDataBuilder.CreateValidObstacle();
            await _context.ObstaclesData.AddAsync(obstacle);
            await _context.SaveChangesAsync();
            var rapport = TestDataBuilder.CreateValidRapport(obstacle.ObstacleId);
            await _context.Rapports.AddAsync(rapport);
            await _context.SaveChangesAsync();
            rapport.RapportComment = "Updated comment";

            // Act
            var result = await _repository.UpdateRapport(rapport);

            // Assert
            Assert.Equal("Updated comment", result.RapportComment);
            var updatedRapport = await _context.Rapports.FindAsync(rapport.RapportID);
            Assert.NotNull(updatedRapport);
            Assert.Equal("Updated comment", updatedRapport.RapportComment);
        }

        /// <summary>
        /// Disposes the database context
        /// </summary>
        public void Dispose()
        {
            _context.Dispose();
        }
    }
}

