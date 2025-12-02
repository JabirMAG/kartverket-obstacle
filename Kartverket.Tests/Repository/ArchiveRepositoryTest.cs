using FirstWebApplication.DataContext;
using FirstWebApplication.Models;
using FirstWebApplication.Repositories;
using Kartverket.Tests.Helpers;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace Kartverket.Tests.Repository
{
    /// <summary>
    /// Tests for ArchiveRepository
    /// Verifies archiving operations, data transformation (JSON serialization), and report counting
    /// </summary>
    public class ArchiveRepositoryTest : IDisposable
    {
        private readonly ApplicationDBContext _context;
        private readonly ObstacleRepository _obstacleRepository;
        private readonly RegistrarRepository _registrarRepository;
        private readonly ArchiveRepository _archiveRepository;

        /// <summary>
        /// Initializes a new instance of the ArchiveRepositoryTest class
        /// </summary>
        public ArchiveRepositoryTest()
        {
            _context = DatabaseFixture.CreateContext();
            _obstacleRepository = new ObstacleRepository(_context);
            _registrarRepository = new RegistrarRepository(_context);
            _archiveRepository = new ArchiveRepository(_context, _obstacleRepository, _registrarRepository);
        }

        /// <summary>
        /// Tests that ArchiveObstacleAsync archives obstacle and all associated reports, and returns correct count
        /// </summary>
        [Fact]
        public async Task ArchiveObstacleAsync_ShouldArchiveObstacle_AndAllReports_AndReturnCount()
        {
            // Arrange
            var obstacle = TestDataBuilder.CreateValidObstacle();
            await _context.ObstaclesData.AddAsync(obstacle);
            await _context.SaveChangesAsync();

            var rapport1 = TestDataBuilder.CreateValidRapport(obstacle.ObstacleId, "Comment 1");
            var rapport2 = TestDataBuilder.CreateValidRapport(obstacle.ObstacleId, "Comment 2");
            await _context.Rapports.AddRangeAsync(rapport1, rapport2);
            await _context.SaveChangesAsync();

            // Act
            var result = await _archiveRepository.ArchiveObstacleAsync(obstacle);

            // Assert
            Assert.Equal(2, result); // Should return count of archived reports
            var archivedReport = await _context.ArchivedReports
                .FirstOrDefaultAsync(ar => ar.OriginalObstacleId == obstacle.ObstacleId);
            Assert.NotNull(archivedReport);
            Assert.Equal(obstacle.ObstacleName, archivedReport.ObstacleName);
            Assert.Equal(3, archivedReport.ObstacleStatus); // Should be set to rejected status
        }

        /// <summary>
        /// Tests that ArchiveObstacleAsync transforms report comments to JSON array
        /// </summary>
        [Fact]
        public async Task ArchiveObstacleAsync_ShouldTransformReportComments_ToJsonArray()
        {
            // Arrange
            var obstacle = TestDataBuilder.CreateValidObstacle();
            await _context.ObstaclesData.AddAsync(obstacle);
            await _context.SaveChangesAsync();

            var rapport1 = TestDataBuilder.CreateValidRapport(obstacle.ObstacleId, "Comment 1");
            var rapport2 = TestDataBuilder.CreateValidRapport(obstacle.ObstacleId, "Comment 2");
            await _context.Rapports.AddRangeAsync(rapport1, rapport2);
            await _context.SaveChangesAsync();

            // Act
            await _archiveRepository.ArchiveObstacleAsync(obstacle);

            // Assert
            var archivedReport = await _context.ArchivedReports
                .FirstOrDefaultAsync(ar => ar.OriginalObstacleId == obstacle.ObstacleId);
            Assert.NotNull(archivedReport);
            Assert.NotNull(archivedReport.RapportComments);
            
            // Verify JSON can be deserialized
            var comments = JsonSerializer.Deserialize<List<string>>(archivedReport.RapportComments);
            Assert.NotNull(comments);
            Assert.Equal(2, comments.Count);
            Assert.Contains("Comment 1", comments);
            Assert.Contains("Comment 2", comments);
        }

        /// <summary>
        /// Tests that ArchiveObstacleAsync deletes reports from Rapports table after archiving
        /// </summary>
        [Fact]
        public async Task ArchiveObstacleAsync_ShouldDeleteReports_FromRapportsTable()
        {
            // Arrange
            var obstacle = TestDataBuilder.CreateValidObstacle();
            await _context.ObstaclesData.AddAsync(obstacle);
            await _context.SaveChangesAsync();

            var rapport1 = TestDataBuilder.CreateValidRapport(obstacle.ObstacleId, "Comment 1");
            var rapport2 = TestDataBuilder.CreateValidRapport(obstacle.ObstacleId, "Comment 2");
            await _context.Rapports.AddRangeAsync(rapport1, rapport2);
            await _context.SaveChangesAsync();

            // Act
            await _archiveRepository.ArchiveObstacleAsync(obstacle);

            // Assert
            var remainingRapports = await _context.Rapports
                .Where(r => r.ObstacleId == obstacle.ObstacleId)
                .ToListAsync();
            Assert.Empty(remainingRapports); // Reports should be deleted
        }

        /// <summary>
        /// Tests that ArchiveObstacleAsync deletes obstacle from ObstaclesData table
        /// </summary>
        [Fact]
        public async Task ArchiveObstacleAsync_ShouldDeleteObstacle_FromObstaclesDataTable()
        {
            // Arrange
            var obstacle = TestDataBuilder.CreateValidObstacle();
            await _context.ObstaclesData.AddAsync(obstacle);
            await _context.SaveChangesAsync();

            var rapport = TestDataBuilder.CreateValidRapport(obstacle.ObstacleId);
            await _context.Rapports.AddAsync(rapport);
            await _context.SaveChangesAsync();

            // Act
            await _archiveRepository.ArchiveObstacleAsync(obstacle);

            // Assert
            var deletedObstacle = await _context.ObstaclesData.FindAsync(obstacle.ObstacleId);
            Assert.Null(deletedObstacle); // Obstacle should be deleted
        }

        /// <summary>
        /// Tests that ArchiveObstacleAsync returns zero when obstacle has no reports
        /// </summary>
        [Fact]
        public async Task ArchiveObstacleAsync_ShouldReturnZero_WhenObstacleHasNoReports()
        {
            // Arrange
            var obstacle = TestDataBuilder.CreateValidObstacle();
            await _context.ObstaclesData.AddAsync(obstacle);
            await _context.SaveChangesAsync();

            // Act
            var result = await _archiveRepository.ArchiveObstacleAsync(obstacle);

            // Assert
            Assert.Equal(0, result); // Should return 0 when no reports exist
            var archivedReport = await _context.ArchivedReports
                .FirstOrDefaultAsync(ar => ar.OriginalObstacleId == obstacle.ObstacleId);
            Assert.NotNull(archivedReport);
        }

        /// <summary>
        /// Tests that ArchiveObstacleAsync sets ArchivedDate to current UTC time
        /// </summary>
        [Fact]
        public async Task ArchiveObstacleAsync_ShouldSetArchivedDate_ToCurrentUtcTime()
        {
            // Arrange
            var obstacle = TestDataBuilder.CreateValidObstacle();
            await _context.ObstaclesData.AddAsync(obstacle);
            await _context.SaveChangesAsync();

            var beforeArchive = DateTime.UtcNow;

            // Act
            await _archiveRepository.ArchiveObstacleAsync(obstacle);

            // Assert
            var archivedReport = await _context.ArchivedReports
                .FirstOrDefaultAsync(ar => ar.OriginalObstacleId == obstacle.ObstacleId);
            Assert.NotNull(archivedReport);
            Assert.True(archivedReport.ArchivedDate >= beforeArchive);
            Assert.True(archivedReport.ArchivedDate <= DateTime.UtcNow.AddSeconds(5)); // Allow small time difference
        }

        /// <summary>
        /// Tests that ArchiveObstacleAsync preserves all obstacle data in archived report
        /// </summary>
        [Fact]
        public async Task ArchiveObstacleAsync_ShouldPreserveAllObstacleData()
        {
            // Arrange
            var obstacle = TestDataBuilder.CreateValidObstacle();
            obstacle.ObstacleName = "Test Obstacle";
            obstacle.ObstacleHeight = 50.5;
            obstacle.ObstacleDescription = "Test Description";
            obstacle.GeometryGeoJson = "{\"type\":\"Point\",\"coordinates\":[10.0,59.0]}";
            await _context.ObstaclesData.AddAsync(obstacle);
            await _context.SaveChangesAsync();

            // Act
            await _archiveRepository.ArchiveObstacleAsync(obstacle);

            // Assert
            var archivedReport = await _context.ArchivedReports
                .FirstOrDefaultAsync(ar => ar.OriginalObstacleId == obstacle.ObstacleId);
            Assert.NotNull(archivedReport);
            Assert.Equal(obstacle.ObstacleName, archivedReport.ObstacleName);
            Assert.Equal(obstacle.ObstacleHeight, archivedReport.ObstacleHeight);
            Assert.Equal(obstacle.ObstacleDescription, archivedReport.ObstacleDescription);
            Assert.Equal(obstacle.GeometryGeoJson, archivedReport.GeometryGeoJson);
            Assert.Equal(obstacle.ObstacleId, archivedReport.OriginalObstacleId);
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

