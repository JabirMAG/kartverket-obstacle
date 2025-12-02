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
    /// Tests for PilotController
    /// </summary>
    public class PilotControllerTest
    {
        private readonly Mock<IObstacleRepository> _obstacleRepositoryMock;
        private readonly Mock<IRegistrarRepository> _registrarRepositoryMock;
        private readonly Mock<IUserRepository> _userRepositoryMock;
        private readonly PilotController _controller;

        public PilotControllerTest()
        {
            _obstacleRepositoryMock = new Mock<IObstacleRepository>();
            _registrarRepositoryMock = new Mock<IRegistrarRepository>();
            _userRepositoryMock = new Mock<IUserRepository>();

            _controller = new PilotController(
                _obstacleRepositoryMock.Object,
                _registrarRepositoryMock.Object,
                _userRepositoryMock.Object);

            // Setup controller context with user
            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.NameIdentifier, "pilot-user-id")
            }, "mock"));
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = user }
            };
        }

        /// <summary>
        /// Tests that Index returns view with status 200 and pilot's obstacles
        /// </summary>
        [Fact]
        public async Task Index_ShouldReturnView_WithPilotObstacles()
        {
            // Arrange
            var user = new ApplicationUser { Id = "pilot-user-id" };
            var obstacles = new List<ObstacleData>
            {
                TestDataBuilder.CreateValidObstacle("pilot-user-id"),
                TestDataBuilder.CreateValidObstacle("pilot-user-id")
            };

            _userRepositoryMock.Setup(x => x.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
                .ReturnsAsync(user);
            _obstacleRepositoryMock.Setup(x => x.GetObstaclesByOwner("pilot-user-id"))
                .ReturnsAsync(obstacles);

            // Act
            var result = await _controller.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.NotNull(viewResult);
            var model = Assert.IsAssignableFrom<IEnumerable<ObstacleData>>(viewResult.Model);
            Assert.Equal(2, model.Count());
            _userRepositoryMock.Verify(x => x.GetUserAsync(It.IsAny<ClaimsPrincipal>()), Times.Once);
            _obstacleRepositoryMock.Verify(x => x.GetObstaclesByOwner("pilot-user-id"), Times.Once);
        }

        /// <summary>
        /// Tests that Index returns Unauthorized with status 401 when user is null
        /// </summary>
        [Fact]
        public async Task Index_ShouldReturnUnauthorized_WhenUserIsNull()
        {
            // Arrange
            _userRepositoryMock.Setup(x => x.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
                .ReturnsAsync((ApplicationUser?)null);

            // Act
            var result = await _controller.Index();

            // Assert
            var unauthorizedResult = Assert.IsType<UnauthorizedResult>(result);
            Assert.NotNull(unauthorizedResult);
            _userRepositoryMock.Verify(x => x.GetUserAsync(It.IsAny<ClaimsPrincipal>()), Times.Once);
            _obstacleRepositoryMock.Verify(x => x.GetObstaclesByOwner(It.IsAny<string>()), Times.Never);
        }

        /// <summary>
        /// Tests that DetaljerOmRapport returns view when obstacle belongs to pilot
        /// </summary>
        [Fact]
        public async Task DetaljerOmRapport_ShouldReturnView_WhenObstacleBelongsToPilot()
        {
            // Arrange
            var user = new ApplicationUser { Id = "pilot-user-id" };
            var obstacle = TestDataBuilder.CreateValidObstacle("pilot-user-id");
            obstacle.ObstacleId = 1;
            var rapports = new List<RapportData>
            {
                TestDataBuilder.CreateValidRapport(1, "Comment 1")
            };

            _userRepositoryMock.Setup(x => x.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
                .ReturnsAsync(user);
            _obstacleRepositoryMock.Setup(x => x.GetObstacleByOwnerAndId(1, "pilot-user-id"))
                .ReturnsAsync(obstacle);
            _registrarRepositoryMock.Setup(x => x.GetRapportsByObstacleId(1))
                .ReturnsAsync(rapports);

            // Setup TempData
            _controller.TempData = new Microsoft.AspNetCore.Mvc.ViewFeatures.TempDataDictionary(
                _controller.ControllerContext.HttpContext,
                Mock.Of<Microsoft.AspNetCore.Mvc.ViewFeatures.ITempDataProvider>());

            // Act
            var result = await _controller.DetaljerOmRapport(1);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.NotNull(_controller.ViewBag.Obstacle);
            Assert.NotNull(_controller.ViewBag.Rapports);
        }

        /// <summary>
        /// Tests that DetaljerOmRapport redirects when obstacle does not belong to pilot
        /// </summary>
        [Fact]
        public async Task DetaljerOmRapport_ShouldRedirect_WhenObstacleDoesNotBelongToPilot()
        {
            // Arrange
            var user = new ApplicationUser { Id = "pilot-user-id" };
            var obstacle = TestDataBuilder.CreateValidObstacle("other-user-id");
            obstacle.ObstacleId = 1;

            _userRepositoryMock.Setup(x => x.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
                .ReturnsAsync(user);
            _obstacleRepositoryMock.Setup(x => x.GetObstacleByOwnerAndId(1, "pilot-user-id"))
                .ReturnsAsync((ObstacleData?)null); // Obstacle doesn't belong to user

            // Setup TempData
            _controller.TempData = new Microsoft.AspNetCore.Mvc.ViewFeatures.TempDataDictionary(
                _controller.ControllerContext.HttpContext,
                Mock.Of<Microsoft.AspNetCore.Mvc.ViewFeatures.ITempDataProvider>());

            // Act
            var result = await _controller.DetaljerOmRapport(1);

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectResult.ActionName);
        }

        /// <summary>
        /// Tests that UpdateObstacle updates obstacle when status is pending
        /// </summary>
        [Fact]
        public async Task UpdateObstacle_ShouldUpdateObstacle_WhenStatusIsPending()
        {
            // Arrange
            var user = new ApplicationUser { Id = "pilot-user-id" };
            var obstacle = TestDataBuilder.CreateValidObstacle("pilot-user-id");
            obstacle.ObstacleId = 1;
            obstacle.ObstacleStatus = 1; // Pending

            _userRepositoryMock.Setup(x => x.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
                .ReturnsAsync(user);
            _obstacleRepositoryMock.Setup(x => x.GetObstacleByOwnerAndId(1, "pilot-user-id"))
                .ReturnsAsync(obstacle);
            _obstacleRepositoryMock.Setup(x => x.UpdateObstacles(It.IsAny<ObstacleData>()))
                .ReturnsAsync(obstacle);
            _registrarRepositoryMock.Setup(x => x.AddRapport(It.IsAny<RapportData>()))
                .ReturnsAsync(new RapportData { RapportID = 1 });

            // Setup TempData
            _controller.TempData = new Microsoft.AspNetCore.Mvc.ViewFeatures.TempDataDictionary(
                _controller.ControllerContext.HttpContext,
                Mock.Of<Microsoft.AspNetCore.Mvc.ViewFeatures.ITempDataProvider>());

            // Act
            var result = await _controller.UpdateObstacle(1, "New Name", "New Description", 100.0);

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("DetaljerOmRapport", redirectResult.ActionName);
            _obstacleRepositoryMock.Verify(x => x.UpdateObstacles(It.Is<ObstacleData>(o => 
                o.ObstacleName == "New Name" && 
                o.ObstacleDescription == "New Description" && 
                o.ObstacleHeight == 100.0)), Times.Once);
        }

        /// <summary>
        /// Tests that UpdateObstacle returns error when status is not pending
        /// </summary>
        [Fact]
        public async Task UpdateObstacle_ShouldReturnError_WhenStatusIsNotPending()
        {
            // Arrange
            var user = new ApplicationUser { Id = "pilot-user-id" };
            var obstacle = TestDataBuilder.CreateValidObstacle("pilot-user-id");
            obstacle.ObstacleId = 1;
            obstacle.ObstacleStatus = 2; // Approved

            _userRepositoryMock.Setup(x => x.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
                .ReturnsAsync(user);
            _obstacleRepositoryMock.Setup(x => x.GetObstacleByOwnerAndId(1, "pilot-user-id"))
                .ReturnsAsync(obstacle);

            // Setup TempData
            _controller.TempData = new Microsoft.AspNetCore.Mvc.ViewFeatures.TempDataDictionary(
                _controller.ControllerContext.HttpContext,
                Mock.Of<Microsoft.AspNetCore.Mvc.ViewFeatures.ITempDataProvider>());

            // Act
            var result = await _controller.UpdateObstacle(1, "New Name", "New Description", 100.0);

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("DetaljerOmRapport", redirectResult.ActionName);
            _obstacleRepositoryMock.Verify(x => x.UpdateObstacles(It.IsAny<ObstacleData>()), Times.Never);
        }

        /// <summary>
        /// Tests that DetaljerOmRapport redirects with status 302 when obstacle is not found
        /// </summary>
        [Fact]
        public async Task DetaljerOmRapport_ShouldRedirect_WhenObstacleNotFound()
        {
            // Arrange
            var user = new ApplicationUser { Id = "pilot-user-id" };

            _userRepositoryMock.Setup(x => x.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
                .ReturnsAsync(user);
            _obstacleRepositoryMock.Setup(x => x.GetObstacleByOwnerAndId(999, "pilot-user-id"))
                .ReturnsAsync((ObstacleData?)null);

            // Setup TempData
            _controller.TempData = new Microsoft.AspNetCore.Mvc.ViewFeatures.TempDataDictionary(
                _controller.ControllerContext.HttpContext,
                Mock.Of<Microsoft.AspNetCore.Mvc.ViewFeatures.ITempDataProvider>());

            // Act
            var result = await _controller.DetaljerOmRapport(999);

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.NotNull(redirectResult);
            Assert.Equal("Index", redirectResult.ActionName);
            _obstacleRepositoryMock.Verify(x => x.GetObstacleByOwnerAndId(999, "pilot-user-id"), Times.Once);
        }

        /// <summary>
        /// Tests that UpdateObstacle redirects with status 302 when obstacle is not found
        /// </summary>
        [Fact]
        public async Task UpdateObstacle_ShouldRedirect_WhenObstacleNotFound()
        {
            // Arrange
            var user = new ApplicationUser { Id = "pilot-user-id" };

            _userRepositoryMock.Setup(x => x.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
                .ReturnsAsync(user);
            _obstacleRepositoryMock.Setup(x => x.GetObstacleByOwnerAndId(999, "pilot-user-id"))
                .ReturnsAsync((ObstacleData?)null);

            // Setup TempData
            _controller.TempData = new Microsoft.AspNetCore.Mvc.ViewFeatures.TempDataDictionary(
                _controller.ControllerContext.HttpContext,
                Mock.Of<Microsoft.AspNetCore.Mvc.ViewFeatures.ITempDataProvider>());

            // Act
            var result = await _controller.UpdateObstacle(999, "Name", "Description", 50.0);

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.NotNull(redirectResult);
            Assert.Equal("Index", redirectResult.ActionName);
            _obstacleRepositoryMock.Verify(x => x.GetObstacleByOwnerAndId(999, "pilot-user-id"), Times.Once);
            _obstacleRepositoryMock.Verify(x => x.UpdateObstacles(It.IsAny<ObstacleData>()), Times.Never);
        }
    }
}

