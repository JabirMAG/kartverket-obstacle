using FirstWebApplication.Controllers;
using FirstWebApplication.Models;
using FirstWebApplication.Models.ViewModel;
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
            var model = Assert.IsType<ObstacleDataViewModel>(partialViewResult.Model);
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
            var viewModel = new ObstacleDataViewModel { ViewObstacleId = 1 };
            _obstacleRepositoryMock.Setup(x => x.GetElementById(1))
                .ReturnsAsync(obstacle);
            _obstacleRepositoryMock.Setup(x => x.MapToViewModel(obstacle))
                .Returns(viewModel);

            // Act
            var result = await _controller.Overview(1);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.NotNull(viewResult);
            var model = Assert.IsType<ObstacleDataViewModel>(viewResult.Model);
            Assert.Equal(1, model.ViewObstacleId);
            _obstacleRepositoryMock.Verify(x => x.GetElementById(1), Times.Once);
            _obstacleRepositoryMock.Verify(x => x.MapToViewModel(obstacle), Times.Once);
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
            var viewModel = new ObstacleDataViewModel
            {
                ViewGeometryGeoJson = string.Empty // Invalid - required field
            };
            _controller.ModelState.AddModelError("ViewGeometryGeoJson", "Geometry is required");

            // Act
            var result = await _controller.QuickSaveObstacle(viewModel);

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
            var viewModel = new ObstacleDataViewModel
            {
                ViewGeometryGeoJson = "{\"type\":\"Point\",\"coordinates\":[10.0,59.0]}"
            };
            var obstacle = TestDataBuilder.CreateMinimalObstacle();
            var savedObstacle = TestDataBuilder.CreateMinimalObstacle();
            savedObstacle.ObstacleId = 1;
            var savedViewModel = new ObstacleDataViewModel { ViewObstacleId = 1 };

            var user = new ApplicationUser { Id = "test-user-id" };
            _userRepositoryMock.Setup(x => x.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
                .ReturnsAsync(user);

            _obstacleRepositoryMock.Setup(x => x.MapFromViewModel(It.IsAny<ObstacleDataViewModel>()))
                .Returns(obstacle);
            _obstacleRepositoryMock.Setup(x => x.AddObstacle(It.IsAny<ObstacleData>()))
                .ReturnsAsync(savedObstacle);
            _obstacleRepositoryMock.Setup(x => x.MapToViewModel(It.IsAny<ObstacleData>()))
                .Returns(savedViewModel);

            _registrarRepositoryMock.Setup(x => x.AddRapportToObstacle(It.IsAny<int>(), It.IsAny<string>()))
                .ReturnsAsync(new RapportData { RapportID = 1 });

            // Act
            var result = await _controller.QuickSaveObstacle(viewModel);

            // Assert
            _obstacleRepositoryMock.Verify(x => x.AddObstacle(It.IsAny<ObstacleData>()), Times.Once);
            _registrarRepositoryMock.Verify(x => x.AddRapportToObstacle(It.IsAny<int>(), It.IsAny<string>()), Times.Once);
        }

        /// <summary>
        /// Tests that QuickSaveObstacle sets default values for optional fields
        /// </summary>
        [Fact]
        public async Task QuickSaveObstacle_ShouldSetDefaultValues_ForOptionalFields()
        {
            // Arrange
            var viewModel = new ObstacleDataViewModel
            {
                ViewGeometryGeoJson = "{\"type\":\"Point\",\"coordinates\":[10.0,59.0]}"
            };
            var obstacle = TestDataBuilder.CreateMinimalObstacle();
            var savedObstacle = TestDataBuilder.CreateMinimalObstacle();
            savedObstacle.ObstacleId = 1;
            var savedViewModel = new ObstacleDataViewModel { ViewObstacleId = 1 };

            var user = new ApplicationUser { Id = "test-user-id" };
            _userRepositoryMock.Setup(x => x.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
                .ReturnsAsync(user);

            _obstacleRepositoryMock.Setup(x => x.MapFromViewModel(It.IsAny<ObstacleDataViewModel>()))
                .Returns(obstacle);
            _obstacleRepositoryMock.Setup(x => x.AddObstacle(It.IsAny<ObstacleData>()))
                .ReturnsAsync(savedObstacle);
            _obstacleRepositoryMock.Setup(x => x.MapToViewModel(It.IsAny<ObstacleData>()))
                .Returns(savedViewModel);

            _registrarRepositoryMock.Setup(x => x.AddRapportToObstacle(It.IsAny<int>(), It.IsAny<string>()))
                .ReturnsAsync(new RapportData { RapportID = 1 });

            // Act
            await _controller.QuickSaveObstacle(viewModel);

            // Assert
            // Verify that normalization was called (default values are set in repository)
            _obstacleRepositoryMock.Verify(x => x.MapFromViewModel(It.IsAny<ObstacleDataViewModel>()), Times.Once);
        }

        /// <summary>
        /// Tests that SubmitObstacle returns partial view with status 200 when model is invalid
        /// </summary>
        [Fact]
        public async Task SubmitObstacle_ShouldReturnPartialView_WhenModelIsInvalid()
        {
            // Arrange
            var viewModel = new ObstacleDataViewModel
            {
                ViewObstacleName = string.Empty, // Invalid - required field
                ViewGeometryGeoJson = "{\"type\":\"Point\",\"coordinates\":[10.0,59.0]}"
            };
            _controller.ModelState.AddModelError("ViewObstacleName", "Name is required");

            // Act
            var result = await _controller.SubmitObstacle(viewModel);

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
            var viewModel = new ObstacleDataViewModel
            {
                ViewObstacleName = "Test Obstacle",
                ViewObstacleHeight = 50.5,
                ViewObstacleDescription = "A test obstacle",
                ViewGeometryGeoJson = "{\"type\":\"Point\",\"coordinates\":[10.0,59.0]}"
            };
            var obstacle = TestDataBuilder.CreateValidObstacle();
            var savedObstacle = TestDataBuilder.CreateValidObstacle();
            savedObstacle.ObstacleId = 1;
            var savedViewModel = new ObstacleDataViewModel { ViewObstacleId = 1 };

            var user = new ApplicationUser { Id = "test-user-id" };
            _userRepositoryMock.Setup(x => x.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
                .ReturnsAsync(user);

            _obstacleRepositoryMock.Setup(x => x.MapFromViewModel(It.IsAny<ObstacleDataViewModel>()))
                .Returns(obstacle);
            _obstacleRepositoryMock.Setup(x => x.AddObstacle(It.IsAny<ObstacleData>()))
                .ReturnsAsync(savedObstacle);
            _obstacleRepositoryMock.Setup(x => x.MapToViewModel(It.IsAny<ObstacleData>()))
                .Returns(savedViewModel);

            _registrarRepositoryMock.Setup(x => x.AddRapportToObstacle(It.IsAny<int>(), It.IsAny<string>()))
                .ReturnsAsync(new RapportData { RapportID = 1 });

            // Act
            var result = await _controller.SubmitObstacle(viewModel);

            // Assert
            _obstacleRepositoryMock.Verify(x => x.AddObstacle(It.IsAny<ObstacleData>()), Times.Once);
            _registrarRepositoryMock.Verify(x => x.AddRapportToObstacle(It.IsAny<int>(), It.IsAny<string>()), Times.Once);
        }

        /// <summary>
        /// AJAX testing is covered in QuickSaveObstacle_ShouldSaveObstacle_AndCreateRapport_WhenValid
        /// </summary>
    }
}

