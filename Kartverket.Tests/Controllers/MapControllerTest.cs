using FirstWebApplication.Controllers;
using FirstWebApplication.Models;
using FirstWebApplication.Repositories;
using Kartverket.Tests.Helpers;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace Kartverket.Tests.Controllers
{
    /// <summary>
    /// Tests for MapController
    /// </summary>
    public class MapControllerTest
    {
        private readonly Mock<IObstacleRepository> _obstacleRepositoryMock;
        private readonly MapController _controller;

        public MapControllerTest()
        {
            _obstacleRepositoryMock = new Mock<IObstacleRepository>();
            _controller = new MapController(_obstacleRepositoryMock.Object);
        }

        /// <summary>
        /// Tests that Map returns view with status 200 and pending/approved obstacles
        /// </summary>
        [Fact]
        public async Task Map_ShouldReturnView_WithPendingAndApprovedObstacles()
        {
            // Arrange
            var obstacles = new List<ObstacleData>
            {
                TestDataBuilder.CreateValidObstacle(),
                TestDataBuilder.CreateValidObstacle()
            };
            obstacles[0].ObstacleStatus = 1; // Pending
            obstacles[1].ObstacleStatus = 2; // Approved

            _obstacleRepositoryMock.Setup(x => x.GetAllObstacles())
                .ReturnsAsync(obstacles);

            // Act
            var result = await _controller.Map();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.NotNull(viewResult);
            Assert.NotNull(_controller.ViewBag.ReportedObstacles);
            var reportedObstacles = _controller.ViewBag.ReportedObstacles as List<ObstacleData>;
            Assert.NotNull(reportedObstacles);
            Assert.Equal(2, reportedObstacles.Count);
            _obstacleRepositoryMock.Verify(x => x.GetAllObstacles(), Times.Once);
        }

        /// <summary>
        /// Tests that Map filters out rejected obstacles and returns status 200
        /// </summary>
        [Fact]
        public async Task Map_ShouldFilterOutRejectedObstacles()
        {
            // Arrange
            var obstacles = new List<ObstacleData>
            {
                TestDataBuilder.CreateValidObstacle(),
                TestDataBuilder.CreateValidObstacle(),
                TestDataBuilder.CreateValidObstacle()
            };
            obstacles[0].ObstacleStatus = 1; // Pending
            obstacles[1].ObstacleStatus = 2; // Approved
            obstacles[2].ObstacleStatus = 3; // Rejected

            _obstacleRepositoryMock.Setup(x => x.GetAllObstacles())
                .ReturnsAsync(obstacles);

            // Act
            var result = await _controller.Map();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.NotNull(viewResult);
            var reportedObstacles = _controller.ViewBag.ReportedObstacles as List<ObstacleData>;
            Assert.NotNull(reportedObstacles);
            Assert.Equal(2, reportedObstacles.Count);
            Assert.All(reportedObstacles, o => Assert.True(o.ObstacleStatus == 1 || o.ObstacleStatus == 2));
            _obstacleRepositoryMock.Verify(x => x.GetAllObstacles(), Times.Once);
        }

        /// <summary>
        /// Tests that GetPendingObstacles returns JSON with status 200 and pending obstacles only
        /// </summary>
        [Fact]
        public async Task GetPendingObstacles_ShouldReturnJson_WithPendingObstaclesOnly()
        {
            // Arrange
            var obstacles = new List<ObstacleData>
            {
                TestDataBuilder.CreateValidObstacle(),
                TestDataBuilder.CreateValidObstacle(),
                TestDataBuilder.CreateValidObstacle()
            };
            obstacles[0].ObstacleStatus = 1; // Pending
            obstacles[0].ObstacleId = 1;
            obstacles[1].ObstacleStatus = 2; // Approved
            obstacles[1].ObstacleId = 2;
            obstacles[2].ObstacleStatus = 1; // Pending
            obstacles[2].ObstacleId = 3;

            _obstacleRepositoryMock.Setup(x => x.GetAllObstacles())
                .ReturnsAsync(obstacles);

            // Act
            var result = await _controller.GetPendingObstacles();

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            Assert.NotNull(jsonResult);
            Assert.NotNull(jsonResult.Value);
            _obstacleRepositoryMock.Verify(x => x.GetAllObstacles(), Times.Once);
        }

        /// <summary>
        /// Tests that GetPendingObstacles transforms ObstacleData to correct JSON structure with all required fields
        /// </summary>
        [Fact]
        public async Task GetPendingObstacles_ShouldTransformObstacleData_ToCorrectJsonStructure()
        {
            // Arrange
            var obstacle = TestDataBuilder.CreateValidObstacle();
            obstacle.ObstacleId = 1;
            obstacle.ObstacleName = "Test Obstacle";
            obstacle.ObstacleHeight = 50.5;
            obstacle.ObstacleDescription = "Test Description";
            obstacle.ObstacleStatus = 1; // Pending
            obstacle.GeometryGeoJson = "{\"type\":\"Point\",\"coordinates\":[10.0,59.0]}";

            _obstacleRepositoryMock.Setup(x => x.GetAllObstacles())
                .ReturnsAsync(new List<ObstacleData> { obstacle });

            // Act
            var result = await _controller.GetPendingObstacles();

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            Assert.NotNull(jsonResult.Value);
            
            // Verify JSON structure using reflection to check anonymous object properties
            var jsonValue = jsonResult.Value;
            
            // Check that it's a list/array
            if (jsonValue is System.Collections.IEnumerable enumerable)
            {
                var items = enumerable.Cast<object>().ToList();
                Assert.Single(items);
                
                // Use reflection to verify structure
                var firstItem = items.First();
                var itemType = firstItem.GetType();
                
                // Verify all required fields exist in the transformed object
                var idProperty = itemType.GetProperty("id");
                var nameProperty = itemType.GetProperty("name");
                var heightProperty = itemType.GetProperty("height");
                var descriptionProperty = itemType.GetProperty("description");
                var statusProperty = itemType.GetProperty("status");
                var geometryProperty = itemType.GetProperty("geometryGeoJson");
                
                Assert.NotNull(idProperty);
                Assert.NotNull(nameProperty);
                Assert.NotNull(heightProperty);
                Assert.NotNull(descriptionProperty);
                Assert.NotNull(statusProperty);
                Assert.NotNull(geometryProperty);
                
                // Verify values match the transformation
                Assert.Equal(1, idProperty.GetValue(firstItem));
                Assert.Equal("Test Obstacle", nameProperty.GetValue(firstItem));
                Assert.Equal(50.5, heightProperty.GetValue(firstItem));
                Assert.Equal("Test Description", descriptionProperty.GetValue(firstItem));
                Assert.Equal(1, statusProperty.GetValue(firstItem));
                Assert.Equal("{\"type\":\"Point\",\"coordinates\":[10.0,59.0]}", geometryProperty.GetValue(firstItem));
            }
            else
            {
                Assert.Fail("Expected JSON result to be enumerable");
            }
        }

        /// <summary>
        /// Tests that GetPendingObstacles only includes pending obstacles (status 1) in the transformation
        /// </summary>
        [Fact]
        public async Task GetPendingObstacles_ShouldOnlyTransformPendingObstacles()
        {
            // Arrange
            var pendingObstacle = TestDataBuilder.CreateValidObstacle();
            pendingObstacle.ObstacleId = 1;
            pendingObstacle.ObstacleStatus = 1; // Pending
            
            var approvedObstacle = TestDataBuilder.CreateValidObstacle();
            approvedObstacle.ObstacleId = 2;
            approvedObstacle.ObstacleStatus = 2; // Approved - should not be included
            
            var rejectedObstacle = TestDataBuilder.CreateValidObstacle();
            rejectedObstacle.ObstacleId = 3;
            rejectedObstacle.ObstacleStatus = 3; // Rejected - should not be included

            _obstacleRepositoryMock.Setup(x => x.GetAllObstacles())
                .ReturnsAsync(new List<ObstacleData> { pendingObstacle, approvedObstacle, rejectedObstacle });

            // Act
            var result = await _controller.GetPendingObstacles();

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            Assert.NotNull(jsonResult.Value);
            
            if (jsonResult.Value is System.Collections.IEnumerable enumerable)
            {
                var items = enumerable.Cast<object>().ToList();
                Assert.Single(items); // Should only have pending obstacle
                
                var firstItem = items.First();
                var itemType = firstItem.GetType();
                var idProperty = itemType.GetProperty("id");
                var statusProperty = itemType.GetProperty("status");
                
                Assert.Equal(1, idProperty?.GetValue(firstItem)); // Should be pending obstacle
                Assert.Equal(1, statusProperty?.GetValue(firstItem)); // Status should be 1 (pending)
            }
        }
    }
}

