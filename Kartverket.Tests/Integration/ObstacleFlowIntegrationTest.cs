using FirstWebApplication.Controllers;
using FirstWebApplication.DataContext;
using FirstWebApplication.Models;
using FirstWebApplication.Repositories;
using Kartverket.Tests.Helpers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using System.Security.Claims;

namespace Kartverket.Tests.Integration
{
    /// <summary>
    /// Integration tests for the complete obstacle flow from controller to repository to database
    /// </summary>
    public class ObstacleFlowIntegrationTest : IDisposable
    {
        private readonly ApplicationDBContext _context;
        private readonly ObstacleRepository _obstacleRepository;
        private readonly RegistrarRepository _registrarRepository;
        private readonly ObstacleController _controller;
        private readonly Mock<UserManager<ApplicationUser>> _userManagerMock;

        public ObstacleFlowIntegrationTest()
        {
            _context = DatabaseFixture.CreateContext();
            _obstacleRepository = new ObstacleRepository(_context);
            _registrarRepository = new RegistrarRepository(_context);

            var store = new Mock<IUserStore<ApplicationUser>>();
            _userManagerMock = new Mock<UserManager<ApplicationUser>>(
                store.Object, null, null, null, null, null, null, null, null);

            _controller = new ObstacleController(
                _obstacleRepository,
                _registrarRepository,
                _userManagerMock.Object);

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };
        }

        /// <summary>
        /// Tests the complete flow: Create obstacle via controller -> Save to repository -> Verify in database
        /// </summary>
        [Fact]
        public async Task CreateObstacle_EndToEnd_ShouldSaveToDatabase()
        {
            // Arrange
            var obstacleData = new ObstacleData
            {
                ObstacleName = "Integration Test Obstacle",
                ObstacleHeight = 50,
                ObstacleDescription = "Test description",
                GeometryGeoJson = "{\"type\":\"Point\",\"coordinates\":[10.0,59.0]}",
                ObstacleStatus = 1
            };

            var user = new ApplicationUser { Id = "test-user-id", UserName = "testuser" };
            _userManagerMock.Setup(x => x.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
                .ReturnsAsync(user);

            // Act - Use controller method
            var result = await _controller.SubmitObstacle(obstacleData);

            // Assert - Verify in database
            var savedObstacle = await _context.ObstaclesData
                .FirstOrDefaultAsync(o => o.ObstacleName == "Integration Test Obstacle");
            
            Assert.NotNull(savedObstacle);
            Assert.True(savedObstacle.ObstacleId > 0);
            Assert.Equal(50, savedObstacle.ObstacleHeight);
            Assert.Equal("test-user-id", savedObstacle.OwnerUserId);

            // Verify report was created
            var reports = await _context.Rapports
                .Where(r => r.ObstacleId == savedObstacle.ObstacleId)
                .ToListAsync();
            
            Assert.Single(reports);
            Assert.Contains("ble sendt inn", reports[0].RapportComment);
        }

        /// <summary>
        /// Tests the complete flow: Create obstacle -> Update via repository -> Verify changes in database
        /// </summary>
        [Fact]
        public async Task UpdateObstacle_EndToEnd_ShouldUpdateInDatabase()
        {
            // Arrange - Create obstacle via repository
            var obstacle = new ObstacleData
            {
                ObstacleName = "Original Name",
                ObstacleHeight = 30,
                ObstacleDescription = "Original description",
                GeometryGeoJson = "{\"type\":\"Point\",\"coordinates\":[10.0,59.0]}",
                ObstacleStatus = 1
            };
            var savedObstacle = await _obstacleRepository.AddObstacle(obstacle);

            // Act - Update via repository
            savedObstacle.ObstacleName = "Updated Name";
            savedObstacle.ObstacleHeight = 60;
            await _obstacleRepository.UpdateObstacles(savedObstacle);

            // Assert - Verify changes in database
            var updatedObstacle = await _context.ObstaclesData
                .FindAsync(savedObstacle.ObstacleId);
            
            Assert.NotNull(updatedObstacle);
            Assert.Equal("Updated Name", updatedObstacle.ObstacleName);
            Assert.Equal(60, updatedObstacle.ObstacleHeight);
        }

        /// <summary>
        /// Tests the complete flow: Create obstacle -> Add report -> Verify both in database
        /// </summary>
        [Fact]
        public async Task CreateObstacleWithReport_EndToEnd_ShouldSaveBothToDatabase()
        {
            // Arrange
            var obstacle = new ObstacleData
            {
                ObstacleName = "Test Obstacle",
                ObstacleHeight = 40,
                GeometryGeoJson = "{\"type\":\"Point\",\"coordinates\":[10.0,59.0]}",
                ObstacleStatus = 1
            };
            var savedObstacle = await _obstacleRepository.AddObstacle(obstacle);

            // Act - Add report via repository
            var report = new RapportData
            {
                ObstacleId = savedObstacle.ObstacleId,
                RapportComment = "Integration test report"
            };
            await _registrarRepository.AddRapport(report);

            // Assert - Verify both in database
            var dbObstacle = await _context.ObstaclesData
                .Include(o => o.Rapports)
                .FirstOrDefaultAsync(o => o.ObstacleId == savedObstacle.ObstacleId);
            
            Assert.NotNull(dbObstacle);
            Assert.Single(dbObstacle.Rapports);
            Assert.Equal("Integration test report", dbObstacle.Rapports.First().RapportComment);
        }

        public void Dispose()
        {
            _context?.Dispose();
        }
    }
}

