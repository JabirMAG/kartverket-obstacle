using FirstWebApplication.Controllers;
using FirstWebApplication.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Moq;
using System.Threading.Tasks;

namespace Kartverket.Tests.Controllers
{
    public class HomeControllerTest
    {
        public HomeControllerTest() { }

        [Fact]
        public async Task DataForm_ShouldReturnAGreeting()
        {
            //lager liksom logger for Homecontroller (påkrevd av homekontroller construtor)
            var logger = new Mock<ILogger<HomeController>>();
            
            //lager mock for UserManager (påkrevd av homekontroller construtor)
            var userManager = new Mock<UserManager<ApplicationUser>>(
                Mock.Of<IUserStore<ApplicationUser>>(),
                null, null, null, null, null, null, null, null);

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

            var controller = new HomeController(config, logger.Object, userManager.Object);
            await controller.Index();
            var greetings = new[] { "God morgen!", "God ettermiddag!", "God kveld!" };
            var controllerGreeting = controller.ViewBag.Greeting;

            Assert.NotNull(controllerGreeting);
            Assert.Contains(controllerGreeting, greetings);
        }
    }
}
