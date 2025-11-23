using FirstWebApplication.DataContext;
using FirstWebApplication.Models;
using FirstWebApplication.Repositories;
using Kartverket.Tests.Helpers;
using Microsoft.EntityFrameworkCore;

namespace Kartverket.Tests.Repository
{
    /// <summary>
    /// Tests for ObstacleRepository
    /// Verifies CRUD operations, null value handling, filtering, and data transformations
    /// </summary>
    public class ObstacleRepositoryTest : IDisposable
    {
        private readonly ApplicationDBContext _context;
        private readonly ObstacleRepository _repository;

        /// <summary>
        /// Initializes a new instance of the ObstacleRepositoryTest class
        /// </summary>
        public ObstacleRepositoryTest()
        {
            _context = DatabaseFixture.CreateContext();
            _repository = new ObstacleRepository(_context);
        }

        /// <summary>
        /// Tests that AddObstacle adds obstacle to database and returns obstacle with generated ID
        /// </summary>
        [Fact]
        public async Task AddObstacle_ShouldAddToDatabase_AndReturnObstacleWithId()
        {
            // Arrange
            var obstacle = TestDataBuilder.CreateValidObstacle();

            // Act
            var result = await _repository.AddObstacle(obstacle);

            // Assert
            Assert.True(result.ObstacleId > 0);
            var savedObstacle = await _context.ObstaclesData.FindAsync(result.ObstacleId);
            Assert.NotNull(savedObstacle);
            Assert.Equal(obstacle.ObstacleName, savedObstacle.ObstacleName);
            Assert.Equal(obstacle.ObstacleHeight, savedObstacle.ObstacleHeight);
        }

        /// <summary>
        /// Tests that GetElementById returns obstacle when it exists
        /// </summary>
        [Fact]
        public async Task GetElementById_ShouldReturnObstacle_WhenExists()
        {
            // Arrange
            var obstacle = TestDataBuilder.CreateValidObstacle();
            await _context.ObstaclesData.AddAsync(obstacle);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetElementById(obstacle.ObstacleId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(obstacle.ObstacleId, result.ObstacleId);
            Assert.Equal(obstacle.ObstacleName, result.ObstacleName);
        }

        /// <summary>
        /// Tests that GetElementById returns null when obstacle does not exist
        /// </summary>
        [Fact]
        public async Task GetElementById_ShouldReturnNull_WhenNotExists()
        {
            // Act
            var result = await _repository.GetElementById(999);

            // Assert
            Assert.Null(result);
        }

        /// <summary>
        /// Tests that GetElementById transforms null values from database to empty strings
        /// </summary>
        [Fact]
        public async Task GetElementById_ShouldHandleNullValues_FromDatabase()
        {
            // Arrange
            var obstacle = TestDataBuilder.CreateMinimalObstacle();
            obstacle.ObstacleName = null;
            obstacle.ObstacleDescription = null;
            await _context.ObstaclesData.AddAsync(obstacle);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetElementById(obstacle.ObstacleId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(string.Empty, result.ObstacleName);
            Assert.Equal(string.Empty, result.ObstacleDescription);
        }

        /// <summary>
        /// Tests that GetAllObstacles returns obstacles ordered by ID in descending order
        /// </summary>
        [Fact]
        public async Task GetAllObstacles_ShouldReturnObstacles_OrderedByDescendingId()
        {
            // Arrange
            var obstacle1 = TestDataBuilder.CreateValidObstacle();
            var obstacle2 = TestDataBuilder.CreateValidObstacle();
            await _context.ObstaclesData.AddRangeAsync(obstacle1, obstacle2);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetAllObstacles();

            // Assert
            Assert.NotNull(result);
            var obstaclesList = result.ToList();
            Assert.True(obstaclesList.Count >= 2);
            // Verify ordering is descending by ID
            for (int i = 0; i < obstaclesList.Count - 1; i++)
            {
                Assert.True(obstaclesList[i].ObstacleId >= obstaclesList[i + 1].ObstacleId);
            }
        }

        /// <summary>
        /// Tests that GetAllObstacles returns maximum 50 obstacles (filters/limits results)
        /// </summary>
        [Fact]
        public async Task GetAllObstacles_ShouldReturnMax50Obstacles()
        {
            // Arrange
            var obstacles = new List<ObstacleData>();
            for (int i = 0; i < 60; i++)
            {
                obstacles.Add(TestDataBuilder.CreateValidObstacle());
            }
            await _context.ObstaclesData.AddRangeAsync(obstacles);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetAllObstacles();

            // Assert
            var obstaclesList = result.ToList();
            Assert.True(obstaclesList.Count <= 50);
        }

        /// <summary>
        /// Tests that GetAllObstacles transforms null values from database to empty strings
        /// </summary>
        [Fact]
        public async Task GetAllObstacles_ShouldHandleNullValues_FromDatabase()
        {
            // Arrange
            var obstacle = TestDataBuilder.CreateMinimalObstacle();
            obstacle.ObstacleName = null;
            obstacle.ObstacleDescription = null;
            await _context.ObstaclesData.AddAsync(obstacle);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetAllObstacles();

            // Assert
            var obstaclesList = result.ToList();
            var foundObstacle = obstaclesList.FirstOrDefault(o => o.ObstacleId == obstacle.ObstacleId);
            Assert.NotNull(foundObstacle);
            Assert.Equal(string.Empty, foundObstacle.ObstacleName);
            Assert.Equal(string.Empty, foundObstacle.ObstacleDescription);
        }

        /// <summary>
        /// Tests that GetObstaclesByOwner filters and returns only obstacles belonging to the specified owner
        /// </summary>
        [Fact]
        public async Task GetObstaclesByOwner_ShouldReturnOnlyOwnersObstacles()
        {
            // Arrange
            var ownerId1 = "owner1";
            var ownerId2 = "owner2";
            var obstacle1 = TestDataBuilder.CreateValidObstacle(ownerId1);
            var obstacle2 = TestDataBuilder.CreateValidObstacle(ownerId1);
            var obstacle3 = TestDataBuilder.CreateValidObstacle(ownerId2);
            await _context.ObstaclesData.AddRangeAsync(obstacle1, obstacle2, obstacle3);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetObstaclesByOwner(ownerId1);

            // Assert
            var obstaclesList = result.ToList();
            Assert.Equal(2, obstaclesList.Count);
            Assert.All(obstaclesList, o => Assert.Equal(ownerId1, o.OwnerUserId));
        }

        /// <summary>
        /// Tests that GetObstaclesByOwner returns empty collection when owner has no obstacles
        /// </summary>
        [Fact]
        public async Task GetObstaclesByOwner_ShouldReturnEmpty_WhenOwnerHasNoObstacles()
        {
            // Act
            var result = await _repository.GetObstaclesByOwner("nonexistent");

            // Assert
            Assert.Empty(result);
        }

        /// <summary>
        /// Tests that UpdateObstacles updates obstacle when status is not 3 (rejected)
        /// </summary>
        [Fact]
        public async Task UpdateObstacles_ShouldUpdateObstacle_WhenStatusIsNot3()
        {
            // Arrange
            var obstacle = TestDataBuilder.CreateValidObstacle();
            await _context.ObstaclesData.AddAsync(obstacle);
            await _context.SaveChangesAsync();
            obstacle.ObstacleName = "Updated Name";
            obstacle.ObstacleStatus = 2; // Approved

            // Act
            var result = await _repository.UpdateObstacles(obstacle);

            // Assert
            Assert.Equal("Updated Name", result.ObstacleName);
            var updatedObstacle = await _context.ObstaclesData.FindAsync(obstacle.ObstacleId);
            Assert.NotNull(updatedObstacle);
            Assert.Equal("Updated Name", updatedObstacle.ObstacleName);
            Assert.Equal(2, updatedObstacle.ObstacleStatus);
        }

        /// <summary>
        /// Tests that UpdateObstacles deletes obstacle when status is 3 (rejected)
        /// </summary>
        [Fact]
        public async Task UpdateObstacles_ShouldDeleteObstacle_WhenStatusIs3()
        {
            // Arrange
            var obstacle = TestDataBuilder.CreateValidObstacle();
            await _context.ObstaclesData.AddAsync(obstacle);
            await _context.SaveChangesAsync();
            obstacle.ObstacleStatus = 3; // Rejected (should delete)

            // Act
            var result = await _repository.UpdateObstacles(obstacle);

            // Assert
            var deletedObstacle = await _context.ObstaclesData.FindAsync(obstacle.ObstacleId);
            Assert.Null(deletedObstacle);
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

