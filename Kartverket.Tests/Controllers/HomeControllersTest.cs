using FirstWebApplication.Controllers;
using Microsoft.AspNetCore.Mvc;

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
            // Arrange
            var controller = new HomeController();
            var result = controller.Index();
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.NotNull(viewResult);
            
            var greetings = new[] { "God morgen!", "God ettermiddag!", "God kveld!" };
            var controllerGreeting = controller.ViewBag.Greeting;

            Assert.NotNull(controllerGreeting);
            Assert.Contains(controllerGreeting, greetings);
        }

        /// <summary>
        /// Tests that Privacy returns a view with status 200
        /// </summary>
        [Fact]
        public void Privacy_ShouldReturnView_WithStatus200()
        {
            // Arrange
            var controller = new HomeController();

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
            var controller = new HomeController();

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
            var controller = new HomeController();
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
