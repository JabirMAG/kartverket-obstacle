using FirstWebApplication.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Moq;

namespace Kartverket.Tests.Controllers
{
    /// <summary>
    /// Tests for HomeController
    /// </summary>
    public class HomeControllerTest
    {
        public HomeControllerTest() { }

        /// <summary>
        /// Tests that Index action returns a view with status 200 and greeting based on time of day
        /// </summary>
        [Fact]
        public void Index_ShouldReturnGreeting_BasedOnTimeOfDay()
        {
            // Create mock logger required by HomeController constructor
            var logger = new Mock<ILogger<HomeController>>();

            var inMemorySettings = new Dictionary<string, string?>
            {
                {
                    "ConnectionStrings:DefaultConnection",
                    "Server=localhost;Database=test;Uid=u;Pwd=p;"
                },
            };
            IConfiguration config = new ConfigurationBuilder()
                .AddInMemoryCollection(inMemorySettings)
                .Build();

            var controller = new HomeController(config, logger.Object);
            var result = controller.Index();
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.NotNull(viewResult);
            
            var greetings = new[] { "God morgen!", "God ettermiddag!", "God kveld!" };
            var controllerGreeting = controller.ViewBag.Greeting;

            Assert.NotNull(controllerGreeting);
            Assert.Contains(controllerGreeting, greetings);
        }

        /// <summary>
        /// Tests that DataForm GET returns a view with status 200
        /// </summary>
        [Fact]
        public void DataForm_Get_ShouldReturnView_WithStatus200()
        {
            // Arrange
            var logger = new Mock<ILogger<HomeController>>();
            var inMemorySettings = new Dictionary<string, string?>
            {
                { "ConnectionStrings:DefaultConnection", "Server=localhost;Database=test;Uid=u;Pwd=p;" }
            };
            IConfiguration config = new ConfigurationBuilder()
                .AddInMemoryCollection(inMemorySettings)
                .Build();

            var controller = new HomeController(config, logger.Object);

            // Act
            var result = controller.DataForm();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.NotNull(viewResult);
        }

        /// <summary>
        /// Tests that DataForm POST returns Overview view with status 200 and obstacle data
        /// </summary>
        [Fact]
        public void DataForm_Post_ShouldReturnOverviewView_WithObstacleData()
        {
            // Arrange
            var logger = new Mock<ILogger<HomeController>>();
            var inMemorySettings = new Dictionary<string, string?>
            {
                { "ConnectionStrings:DefaultConnection", "Server=localhost;Database=test;Uid=u;Pwd=p;" }
            };
            IConfiguration config = new ConfigurationBuilder()
                .AddInMemoryCollection(inMemorySettings)
                .Build();

            var controller = new HomeController(config, logger.Object);
            var obstacle = Kartverket.Tests.Helpers.TestDataBuilder.CreateValidObstacle();

            // Act
            var result = controller.DataForm(obstacle);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.NotNull(viewResult);
            Assert.Equal("Overview", viewResult.ViewName);
            Assert.Equal(obstacle, viewResult.Model);
        }

        /// <summary>
        /// Tests that Privacy returns a view with status 200
        /// </summary>
        [Fact]
        public void Privacy_ShouldReturnView_WithStatus200()
        {
            // Arrange
            var logger = new Mock<ILogger<HomeController>>();
            var inMemorySettings = new Dictionary<string, string?>
            {
                { "ConnectionStrings:DefaultConnection", "Server=localhost;Database=test;Uid=u;Pwd=p;" }
            };
            IConfiguration config = new ConfigurationBuilder()
                .AddInMemoryCollection(inMemorySettings)
                .Build();

            var controller = new HomeController(config, logger.Object);

            // Act
            var result = controller.Privacy();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.NotNull(viewResult);
        }

        /// <summary>
        /// Tests that OmOss returns a view with status 200
        /// </summary>
        [Fact]
        public void OmOss_ShouldReturnView_WithStatus200()
        {
            // Arrange
            var logger = new Mock<ILogger<HomeController>>();
            var inMemorySettings = new Dictionary<string, string?>
            {
                { "ConnectionStrings:DefaultConnection", "Server=localhost;Database=test;Uid=u;Pwd=p;" }
            };
            IConfiguration config = new ConfigurationBuilder()
                .AddInMemoryCollection(inMemorySettings)
                .Build();

            var controller = new HomeController(config, logger.Object);

            // Act
            var result = controller.OmOss();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.NotNull(viewResult);
        }

        /// <summary>
        /// Tests that Error returns view with status 200 and ErrorViewModel
        /// </summary>
        [Fact]
        public void Error_ShouldReturnView_WithErrorViewModel()
        {
            // Arrange
            var logger = new Mock<ILogger<HomeController>>();
            var inMemorySettings = new Dictionary<string, string?>
            {
                { "ConnectionStrings:DefaultConnection", "Server=localhost;Database=test;Uid=u;Pwd=p;" }
            };
            IConfiguration config = new ConfigurationBuilder()
                .AddInMemoryCollection(inMemorySettings)
                .Build();

            var controller = new HomeController(config, logger.Object);
            controller.ControllerContext = new Microsoft.AspNetCore.Mvc.ControllerContext
            {
                HttpContext = new Microsoft.AspNetCore.Http.DefaultHttpContext()
            };

            // Act
            var result = controller.Error();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.NotNull(viewResult);
            Assert.NotNull(viewResult.Model);
        }
    }
}
