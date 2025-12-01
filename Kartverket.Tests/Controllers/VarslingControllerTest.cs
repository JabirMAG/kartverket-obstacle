using FirstWebApplication.Controllers;
using FirstWebApplication.Models;
using FirstWebApplication.Repositories;
using FirstWebApplication.Services;
using Kartverket.Tests.Helpers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Session;
using Moq;
using System.Security.Claims;
using FirstWebApplication.Models.ViewModel;

namespace Kartverket.Tests.Controllers
{
    /// <summary>
    /// Tests for VarslingController
    /// </summary>
    public class VarslingControllerTest
    {
        private readonly Mock<IRegistrarRepository> _registrarRepositoryMock;
        private readonly Mock<IObstacleRepository> _obstacleRepositoryMock;
        private readonly Mock<UserManager<ApplicationUser>> _userManagerMock;
        private readonly Mock<IPilotHelperService> _pilotHelperServiceMock;
        private readonly VarslingController _controller;

        public VarslingControllerTest()
        {
            var store = new Mock<IUserStore<ApplicationUser>>();
            _userManagerMock = new Mock<UserManager<ApplicationUser>>(
                store.Object, null, null, null, null, null, null, null, null);
            _registrarRepositoryMock = new Mock<IRegistrarRepository>();
            _obstacleRepositoryMock = new Mock<IObstacleRepository>();
            _pilotHelperServiceMock = new Mock<IPilotHelperService>();

            _controller = new VarslingController(
                _registrarRepositoryMock.Object,
                _obstacleRepositoryMock.Object,
                _userManagerMock.Object,
                _pilotHelperServiceMock.Object);

            // Setup controller context with user
            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.NameIdentifier, "user-id")
            }, "mock"));
            var httpContext = new DefaultHttpContext();
            httpContext.User = user;
            httpContext.Session = new TestSession();
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };
        }

        /// <summary>
        /// Tests that Index returns view with status 200 and notifications for user's obstacles
        /// </summary>
        [Fact]
        public async Task Index_ShouldReturnView_WithUserNotifications()
        {
            // Arrange
            var user = new ApplicationUser { Id = "user-id" };
            var obstacle = TestDataBuilder.CreateValidObstacle("user-id");
            obstacle.ObstacleId = 1;
            var rapports = new List<RapportData>
            {
                TestDataBuilder.CreateValidRapport(1, "Manual comment")
            };

            _userManagerMock.Setup(x => x.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
                .ReturnsAsync(user);
            _obstacleRepositoryMock.Setup(x => x.GetObstaclesByOwner("user-id"))
                .ReturnsAsync(new List<ObstacleData> { obstacle });
            _registrarRepositoryMock.Setup(x => x.GetAllRapports())
                .ReturnsAsync(rapports);
            _obstacleRepositoryMock.Setup(x => x.GetElementById(1))
                .ReturnsAsync(obstacle);

            // Act
            var result = await _controller.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.NotNull(viewResult);
            Assert.NotNull(viewResult.Model);
            _userManagerMock.Verify(x => x.GetUserAsync(It.IsAny<ClaimsPrincipal>()), Times.Once);
            _obstacleRepositoryMock.Verify(x => x.GetObstaclesByOwner("user-id"), Times.Once);
        }

        /// <summary>
        /// Tests that Index returns view with status 200 and empty list when user is null
        /// </summary>
        [Fact]
        public async Task Index_ShouldReturnEmptyList_WhenUserIsNull()
        {
            // Arrange
            _userManagerMock.Setup(x => x.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
                .ReturnsAsync((ApplicationUser?)null);

            // Act
            var result = await _controller.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.NotNull(viewResult);
            var model = Assert.IsAssignableFrom<IEnumerable<VarslingViewModel>>(viewResult.Model);
            Assert.Empty(model);
            _userManagerMock.Verify(x => x.GetUserAsync(It.IsAny<ClaimsPrincipal>()), Times.Once);
            _obstacleRepositoryMock.Verify(x => x.GetObstaclesByOwner(It.IsAny<string>()), Times.Never);
        }

        /// <summary>
        /// Tests that Details returns view with obstacle and reports
        /// </summary>
        [Fact]
        public async Task Details_ShouldReturnView_WithObstacleAndReports()
        {
            // Arrange
            var user = new ApplicationUser { Id = "user-id" };
            var obstacle = TestDataBuilder.CreateValidObstacle("user-id");
            obstacle.ObstacleId = 1;
            var rapports = new List<RapportData>
            {
                TestDataBuilder.CreateValidRapport(1, "Comment")
            };

            // Mock that user is a Pilot (required by [Authorize(Roles = "Pilot")])
            _userManagerMock.Setup(x => x.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
                .ReturnsAsync(user);
            _userManagerMock.Setup(x => x.IsInRoleAsync(user, "Pilot"))
                .ReturnsAsync(true);

            _pilotHelperServiceMock.Setup(x => x.GetUserObstacleAsync(1, "user-id"))
                .ReturnsAsync(obstacle);
            _pilotHelperServiceMock.Setup(x => x.GetObstacleRapportsAsync(1))
                .ReturnsAsync(rapports);

            // Setup TempData
            _controller.TempData = new Microsoft.AspNetCore.Mvc.ViewFeatures.TempDataDictionary(
                _controller.ControllerContext.HttpContext,
                Mock.Of<Microsoft.AspNetCore.Mvc.ViewFeatures.ITempDataProvider>());

            // Act
            var result = await _controller.Details(1);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.NotNull(_controller.ViewBag.Obstacle);
            Assert.NotNull(_controller.ViewBag.Rapports);
        }

        /// <summary>
        /// Tests that GetNotificationCount returns JSON with status 200 and zero when user is not authenticated
        /// </summary>
        [Fact]
        public async Task GetNotificationCount_ShouldReturnZero_WhenUserNotAuthenticated()
        {
            // Arrange
            _controller.ControllerContext.HttpContext.User = new ClaimsPrincipal();

            // Act
            var result = await _controller.GetNotificationCount();

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            Assert.NotNull(jsonResult);
            Assert.NotNull(jsonResult.Value);
        }

        /// <summary>
        /// Tests that GetNotificationCount returns JSON with status 200 and count of unread notifications
        /// </summary>
        [Fact]
        public async Task GetNotificationCount_ShouldReturnCount_OfUnreadNotifications()
        {
            // Arrange
            var user = new ApplicationUser { Id = "user-id" };
            var obstacle = TestDataBuilder.CreateValidObstacle("user-id");
            obstacle.ObstacleId = 1;
            var rapports = new List<RapportData>
            {
                TestDataBuilder.CreateValidRapport(1, "Manual comment")
            };
            rapports[0].RapportID = 10;

            _userManagerMock.Setup(x => x.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
                .ReturnsAsync(user);
            _obstacleRepositoryMock.Setup(x => x.GetObstaclesByOwner("user-id"))
                .ReturnsAsync(new List<ObstacleData> { obstacle });
            _registrarRepositoryMock.Setup(x => x.GetAllRapports())
                .ReturnsAsync(rapports);

            // Act
            var result = await _controller.GetNotificationCount();

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            Assert.NotNull(jsonResult);
            Assert.NotNull(jsonResult.Value);
            _userManagerMock.Verify(x => x.GetUserAsync(It.IsAny<ClaimsPrincipal>()), Times.Once);
        }

        /// <summary>
        /// Tests that GetNotificationCount calculates correct count of unread notifications based on lastViewedRapportId
        /// </summary>
        [Fact]
        public async Task GetNotificationCount_ShouldCalculateCorrectCount_BasedOnLastViewedRapportId()
        {
            // Arrange
            var user = new ApplicationUser { Id = "user-id" };
            var obstacle = TestDataBuilder.CreateValidObstacle("user-id");
            obstacle.ObstacleId = 1;
            
            // Create rapports with different IDs
            var rapports = new List<RapportData>
            {
                TestDataBuilder.CreateValidRapport(1, "Hindring 'Test' ble sendt inn. Høyde: 10"), // Auto-generated, should be filtered
                TestDataBuilder.CreateValidRapport(1, "Manual comment 1"), // Manual
                TestDataBuilder.CreateValidRapport(1, "Manual comment 2"), // Manual
            };
            rapports[0].RapportID = 5;
            rapports[1].RapportID = 10;
            rapports[2].RapportID = 15;
            rapports[0].Obstacle = obstacle;
            rapports[1].Obstacle = obstacle;
            rapports[2].Obstacle = obstacle;

            // Set lastViewedRapportId to 10, so only RapportID 15 should be unread
            var session = new TestSession();
            session.SetInt32($"LastViewedRapportId_{user.Id}", 10); // Using extension method
            _controller.ControllerContext.HttpContext.Session = session;

            // Mock that user is a Pilot (required by GetNotificationCount check)
            _userManagerMock.Setup(x => x.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
                .ReturnsAsync(user);
            _userManagerMock.Setup(x => x.IsInRoleAsync(user, "Pilot"))
                .ReturnsAsync(true);
            _obstacleRepositoryMock.Setup(x => x.GetObstaclesByOwner("user-id"))
                .ReturnsAsync(new List<ObstacleData> { obstacle });
            _registrarRepositoryMock.Setup(x => x.GetAllRapports())
                .ReturnsAsync(rapports);

            // Act
            var result = await _controller.GetNotificationCount();

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            Assert.NotNull(jsonResult.Value);
            
            // Verify count is correct (only RapportID 15 is unread, RapportID 5 is auto-generated and filtered)
            var jsonValue = jsonResult.Value;
            var countProperty = jsonValue.GetType().GetProperty("count");
            Assert.NotNull(countProperty);
            var count = (int)countProperty.GetValue(jsonValue)!;
            Assert.Equal(1, count); // Only one unread notification (RapportID 15 > 10, and RapportID 5 is filtered)
        }

        /// <summary>
        /// Tests that Index filters out auto-generated comments using FilterOutAutoGeneratedComments
        /// </summary>
        [Fact]
        public async Task Index_ShouldFilterOutAutoGeneratedComments()
        {
            // Arrange
            var user = new ApplicationUser { Id = "user-id" };
            var obstacle = TestDataBuilder.CreateValidObstacle("user-id");
            obstacle.ObstacleId = 1;
            
            var rapports = new List<RapportData>
            {
                TestDataBuilder.CreateValidRapport(1, "Hindring 'Test' ble sendt inn. Høyde: 10"), // Auto-generated
                TestDataBuilder.CreateValidRapport(1, "Manual comment from admin"), // Manual
                TestDataBuilder.CreateValidRapport(1, "Another manual comment") // Manual
            };
            rapports[0].Obstacle = obstacle;
            rapports[1].Obstacle = obstacle;
            rapports[2].Obstacle = obstacle;

            _userManagerMock.Setup(x => x.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
                .ReturnsAsync(user);
            _obstacleRepositoryMock.Setup(x => x.GetObstaclesByOwner("user-id"))
                .ReturnsAsync(new List<ObstacleData> { obstacle });
            _registrarRepositoryMock.Setup(x => x.GetAllRapports())
                .ReturnsAsync(rapports);
            _obstacleRepositoryMock.Setup(x => x.GetElementById(1))
                .ReturnsAsync(obstacle);

            // Setup session
            _controller.ControllerContext.HttpContext.Session = new TestSession();

            // Act
            var result = await _controller.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.NotNull(viewResult.Model);
            
            var model = Assert.IsAssignableFrom<IEnumerable<VarslingViewModel>>(viewResult.Model);
            var varslinger = model.ToList();
            Assert.Single(varslinger); // Should have one obstacle with notifications
            
            var varsling = varslinger.First();
            Assert.Equal(2, varsling.CommentCount); // Should only have 2 manual comments (auto-generated filtered out)
            Assert.Equal(2, varsling.Comments.Count());
            Assert.All(varsling.Comments, c => Assert.DoesNotContain("Hindring '", c.RapportComment));
        }

        /// <summary>
        /// Tests that Index groups comments by ObstacleId and calculates CommentCount correctly
        /// </summary>
        [Fact]
        public async Task Index_ShouldGroupCommentsByObstacleId_AndCalculateCommentCount()
        {
            // Arrange
            var user = new ApplicationUser { Id = "user-id" };
            var obstacle1 = TestDataBuilder.CreateValidObstacle("user-id");
            obstacle1.ObstacleId = 1;
            var obstacle2 = TestDataBuilder.CreateValidObstacle("user-id");
            obstacle2.ObstacleId = 2;
            
            var rapports = new List<RapportData>
            {
                TestDataBuilder.CreateValidRapport(1, "Comment 1 for obstacle 1"),
                TestDataBuilder.CreateValidRapport(1, "Comment 2 for obstacle 1"),
                TestDataBuilder.CreateValidRapport(2, "Comment 1 for obstacle 2")
            };
            rapports[0].Obstacle = obstacle1;
            rapports[1].Obstacle = obstacle1;
            rapports[2].Obstacle = obstacle2;

            _userManagerMock.Setup(x => x.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
                .ReturnsAsync(user);
            _obstacleRepositoryMock.Setup(x => x.GetObstaclesByOwner("user-id"))
                .ReturnsAsync(new List<ObstacleData> { obstacle1, obstacle2 });
            _registrarRepositoryMock.Setup(x => x.GetAllRapports())
                .ReturnsAsync(rapports);
            _obstacleRepositoryMock.Setup(x => x.GetElementById(1))
                .ReturnsAsync(obstacle1);
            _obstacleRepositoryMock.Setup(x => x.GetElementById(2))
                .ReturnsAsync(obstacle2);

            // Setup session
            _controller.ControllerContext.HttpContext.Session = new TestSession();

            // Act
            var result = await _controller.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<IEnumerable<VarslingViewModel>>(viewResult.Model);
            var varslinger = model.ToList();
            Assert.Equal(2, varslinger.Count); // Should have 2 obstacles
            
            var varsling1 = varslinger.FirstOrDefault(v => v.Obstacle.ObstacleId == 1);
            var varsling2 = varslinger.FirstOrDefault(v => v.Obstacle.ObstacleId == 2);
            
            Assert.NotNull(varsling1);
            Assert.Equal(2, varsling1.CommentCount); // Obstacle 1 has 2 comments
            Assert.Equal(2, varsling1.Comments.Count());
            
            Assert.NotNull(varsling2);
            Assert.Equal(1, varsling2.CommentCount); // Obstacle 2 has 1 comment
            Assert.Single(varsling2.Comments);
        }

        /// <summary>
        /// Tests that Index calculates and stores Max RapportID in session
        /// </summary>
        [Fact]
        public async Task Index_ShouldCalculateAndStoreMaxRapportId_InSession()
        {
            // Arrange
            var user = new ApplicationUser { Id = "user-id" };
            var obstacle = TestDataBuilder.CreateValidObstacle("user-id");
            obstacle.ObstacleId = 1;
            
            var rapports = new List<RapportData>
            {
                TestDataBuilder.CreateValidRapport(1, "Comment 1"),
                TestDataBuilder.CreateValidRapport(1, "Comment 2"),
                TestDataBuilder.CreateValidRapport(1, "Comment 3")
            };
            rapports[0].RapportID = 5;
            rapports[1].RapportID = 10;
            rapports[2].RapportID = 15; // Max ID
            rapports[0].Obstacle = obstacle;
            rapports[1].Obstacle = obstacle;
            rapports[2].Obstacle = obstacle;

            var session = new TestSession();
            _controller.ControllerContext.HttpContext.Session = session;

            _userManagerMock.Setup(x => x.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
                .ReturnsAsync(user);
            _obstacleRepositoryMock.Setup(x => x.GetObstaclesByOwner("user-id"))
                .ReturnsAsync(new List<ObstacleData> { obstacle });
            _registrarRepositoryMock.Setup(x => x.GetAllRapports())
                .ReturnsAsync(rapports);
            _obstacleRepositoryMock.Setup(x => x.GetElementById(1))
                .ReturnsAsync(obstacle);

            // Act
            var result = await _controller.Index();

            // Assert
            var maxRapportId = session.GetInt32($"LastViewedRapportId_{user.Id}");
            Assert.Equal(15, maxRapportId); // Should be set to highest RapportID
        }
    }
}

