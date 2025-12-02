using FirstWebApplication.Controllers;
using FirstWebApplication.Models;
using FirstWebApplication.Repositories;
using Kartverket.Tests.Helpers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Security.Claims;

namespace Kartverket.Tests.Controllers
{
    /// <summary>
    /// Tests for ObstacleController
    /// </summary>
    public class ObstacleControllerTest
    {
        private readonly Mock<IObstacleRepository> _obstacleRepositoryMock;
        private readonly Mock<IRegistrarRepository> _registrarRepositoryMock;
        private readonly Mock<IUserRepository> _userRepositoryMock;
        private readonly ObstacleController _controller;

        public ObstacleControllerTest()
        {
            _obstacleRepositoryMock = new Mock<IObstacleRepository>();
            _registrarRepositoryMock = new Mock<IRegistrarRepository>();
            _userRepositoryMock = new Mock<IUserRepository>();

            _controller = new ObstacleController(
                _obstacleRepositoryMock.Object,
                _registrarRepositoryMock.Object,
                _userRepositoryMock.Object);

            // Setup controller context with user
            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.NameIdentifier, "test-user-id")
            }, "mock"));
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = user }
            };
        }

        /// <summary>
        /// Tests that DataFormPartial returns a partial view with new ObstacleData
        /// </summary>
        [Fact]
        public void DataFormPartial_ShouldReturnPartialView_WithNewObstacleData()
        {
            // Act
            var result = _controller.DataFormPartial();

            // Assert
            var partialViewResult = Assert.IsType<PartialViewResult>(result);
            Assert.Equal("_ObstacleFormPartial", partialViewResult.ViewName);
            var model = Assert.IsType<ObstacleData>(partialViewResult.Model);
            Assert.NotNull(model);
        }

        /// <summary>
        /// Tests that Overview returns a view with status 200 when obstacle exists
        /// </summary>
        [Fact]
        public async Task Overview_ShouldReturnView_WhenObstacleExists()
        {
            // Arrange
            var obstacle = TestDataBuilder.CreateValidObstacle();
            obstacle.ObstacleId = 1;
            _obstacleRepositoryMock.Setup(x => x.GetElementById(1))
                .ReturnsAsync(obstacle);

            // Act
            var result = await _controller.Overview(1);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.NotNull(viewResult);
            var model = Assert.IsType<ObstacleData>(viewResult.Model);
            Assert.Equal(1, model.ObstacleId);
            _obstacleRepositoryMock.Verify(x => x.GetElementById(1), Times.Once);
        }

        /// <summary>
        /// Tests that Overview returns NotFound with status 404 when obstacle does not exist
        /// </summary>
        [Fact]
        public async Task Overview_ShouldReturnNotFound_WhenObstacleDoesNotExist()
        {
            // Arrange
            _obstacleRepositoryMock.Setup(x => x.GetElementById(999))
                .ReturnsAsync((ObstacleData?)null);

            // Act
            var result = await _controller.Overview(999);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundResult>(result);
            Assert.NotNull(notFoundResult);
            _obstacleRepositoryMock.Verify(x => x.GetElementById(999), Times.Once);
        }

        /// <summary>
        /// Tests that QuickSaveObstacle returns partial view with status 200 when model is invalid
        /// </summary>
        [Fact]
        public async Task QuickSaveObstacle_ShouldReturnPartialView_WhenModelIsInvalid()
        {
            // Arrange
            var obstacle = new ObstacleData
            {
                GeometryGeoJson = string.Empty // Invalid - required field
            };
            _controller.ModelState.AddModelError("GeometryGeoJson", "Geometry is required");

            // Act
            var result = await _controller.QuickSaveObstacle(obstacle);

            // Assert
            var partialViewResult = Assert.IsType<PartialViewResult>(result);
            Assert.NotNull(partialViewResult);
            Assert.Equal("_ObstacleFormPartial", partialViewResult.ViewName);
            _obstacleRepositoryMock.Verify(x => x.AddObstacle(It.IsAny<ObstacleData>()), Times.Never);
        }

        /// <summary>
        /// Tests that QuickSaveObstacle saves obstacle and creates rapport when valid
        /// </summary>
        [Fact]
        public async Task QuickSaveObstacle_ShouldSaveObstacle_AndCreateRapport_WhenValid()
        {
            // Arrange
            var obstacle = TestDataBuilder.CreateMinimalObstacle();
            obstacle.GeometryGeoJson = "{\"type\":\"Point\",\"coordinates\":[10.0,59.0]}";
            var savedObstacle = TestDataBuilder.CreateMinimalObstacle();
            savedObstacle.ObstacleId = 1;

            var user = new ApplicationUser { Id = "test-user-id" };
            _userRepositoryMock.Setup(x => x.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
                .ReturnsAsync(user);

            _obstacleRepositoryMock.Setup(x => x.AddObstacle(It.IsAny<ObstacleData>()))
                .ReturnsAsync(savedObstacle);

            _registrarRepositoryMock.Setup(x => x.AddRapport(It.IsAny<RapportData>()))
                .ReturnsAsync(new RapportData { RapportID = 1 });

            // Act
            var result = await _controller.QuickSaveObstacle(obstacle);

            // Assert
            _obstacleRepositoryMock.Verify(x => x.AddObstacle(It.IsAny<ObstacleData>()), Times.Once);
            _registrarRepositoryMock.Verify(x => x.AddRapport(It.IsAny<RapportData>()), Times.Once);
            Assert.Equal("test-user-id", obstacle.OwnerUserId);
        }

        /// <summary>
        /// Tests that QuickSaveObstacle sets default values for optional fields
        /// </summary>
        [Fact]
        public async Task QuickSaveObstacle_ShouldSetDefaultValues_ForOptionalFields()
        {
            // Arrange
            var obstacle = new ObstacleData
            {
                ObstacleName = null,
                ObstacleDescription = null,
                ObstacleHeight = 0,
                GeometryGeoJson = "{\"type\":\"Point\",\"coordinates\":[10.0,59.0]}"
            };
            var savedObstacle = TestDataBuilder.CreateMinimalObstacle();
            savedObstacle.ObstacleId = 1;

            var user = new ApplicationUser { Id = "test-user-id" };
            _userRepositoryMock.Setup(x => x.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
                .ReturnsAsync(user);

            _obstacleRepositoryMock.Setup(x => x.AddObstacle(It.IsAny<ObstacleData>()))
                .ReturnsAsync(savedObstacle);

            _registrarRepositoryMock.Setup(x => x.AddRapport(It.IsAny<RapportData>()))
                .ReturnsAsync(new RapportData { RapportID = 1 });

            // Act
            await _controller.QuickSaveObstacle(obstacle);

            // Assert
            Assert.Equal(string.Empty, obstacle.ObstacleName);
            Assert.Equal(string.Empty, obstacle.ObstacleDescription);
        }

        /// <summary>
        /// Tests that SubmitObstacle returns partial view with status 200 when model is invalid
        /// </summary>
        [Fact]
        public async Task SubmitObstacle_ShouldReturnPartialView_WhenModelIsInvalid()
        {
            // Arrange
            var obstacle = new ObstacleData
            {
                ObstacleName = string.Empty, // Invalid - required field
                GeometryGeoJson = "{\"type\":\"Point\",\"coordinates\":[10.0,59.0]}"
            };
            _controller.ModelState.AddModelError("ObstacleName", "Name is required");

            // Act
            var result = await _controller.SubmitObstacle(obstacle);

            // Assert
            var partialViewResult = Assert.IsType<PartialViewResult>(result);
            Assert.NotNull(partialViewResult);
            Assert.Equal("_ObstacleFormPartial", partialViewResult.ViewName);
            _obstacleRepositoryMock.Verify(x => x.AddObstacle(It.IsAny<ObstacleData>()), Times.Never);
        }

        /// <summary>
        /// Tests that SubmitObstacle saves obstacle and creates rapport when valid
        /// </summary>
        [Fact]
        public async Task SubmitObstacle_ShouldSaveObstacle_AndCreateRapport_WhenValid()
        {
            // Arrange
            var obstacle = TestDataBuilder.CreateValidObstacle();
            var savedObstacle = TestDataBuilder.CreateValidObstacle();
            savedObstacle.ObstacleId = 1;

            var user = new ApplicationUser { Id = "test-user-id" };
            _userRepositoryMock.Setup(x => x.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
                .ReturnsAsync(user);

            _obstacleRepositoryMock.Setup(x => x.AddObstacle(It.IsAny<ObstacleData>()))
                .ReturnsAsync(savedObstacle);

            _registrarRepositoryMock.Setup(x => x.AddRapport(It.IsAny<RapportData>()))
                .ReturnsAsync(new RapportData { RapportID = 1 });

            // Act
            var result = await _controller.SubmitObstacle(obstacle);

            // Assert
            _obstacleRepositoryMock.Verify(x => x.AddObstacle(It.IsAny<ObstacleData>()), Times.Once);
            _registrarRepositoryMock.Verify(x => x.AddRapport(It.IsAny<RapportData>()), Times.Once);
            Assert.Equal("test-user-id", obstacle.OwnerUserId);
        }

        /// <summary>
        /// AJAX testing is covered in QuickSaveObstacle_ShouldSaveObstacle_AndCreateRapport_WhenValid
        /// </summary>
    }
}

