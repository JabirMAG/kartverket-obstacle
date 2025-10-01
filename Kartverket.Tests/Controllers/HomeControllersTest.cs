using FirstWebApplication.Controllers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Moq;

namespace Kartverket.Tests.Controllers
{
    public class HomeControllerTest
    {
        public HomeControllerTest() { }

        [Fact]
        public void DataForm_ShouldReturnAGreeting()
        {
            //lager liksom logger for Homecontroller (påkrevd av homekontroller construtor)
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
            controller.DataForm();
            var greetings = new[] { "Good Morning!", "Good Afternoon!", "Good Evening!" };
            var controllerGreeting = controller.ViewBag.Greeting;

            Assert.NotNull(controllerGreeting);
            Assert.Contains(controllerGreeting, greetings);
        }
    }
}
