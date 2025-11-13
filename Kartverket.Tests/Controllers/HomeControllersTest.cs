using FirstWebApplication.Controllers;
using FirstWebApplication.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using System.Security.Claims;
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
            var userStore = Mock.Of<IUserStore<ApplicationUser>>();
            var userManager = new Mock<UserManager<ApplicationUser>>(
                userStore,
                Mock.Of<IOptions<IdentityOptions>>(),
                Mock.Of<IPasswordHasher<ApplicationUser>>(),
                new List<IUserValidator<ApplicationUser>>(),
                new List<IPasswordValidator<ApplicationUser>>(),
                Mock.Of<ILookupNormalizer>(),
                Mock.Of<IdentityErrorDescriber>(),
                Mock.Of<IServiceProvider>(),
                Mock.Of<ILogger<UserManager<ApplicationUser>>>());

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
            
            // Set up HttpContext with a non-authenticated user
            var httpContext = new DefaultHttpContext();
            var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity());
            httpContext.User = claimsPrincipal;
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };
            
            var result = await controller.Index();
            var greetings = new[] { "God morgen!", "God ettermiddag!", "God kveld!" };
            var controllerGreeting = controller.ViewBag.Greeting;

            Assert.NotNull(controllerGreeting);
            Assert.Contains(controllerGreeting, greetings);
        }
    }
}
