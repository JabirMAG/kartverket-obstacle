using FirstWebApplication.Controllers;
using FirstWebApplication.Models;
using FirstWebApplication.Repositories;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;

namespace Kartverket.Tests.Controllers
{
    public class AccountControllerTests
    {
        private readonly Mock<UserManager<ApplicationUser>> _mockUserManager;
        private readonly Mock<SignInManager<ApplicationUser>> _mockSignInManager;
        private readonly Mock<IWebHostEnvironment> _mockEnvironment;
        private readonly Mock<ILogger<AccountController>> _mockLogger;
        private readonly AccountController _controller;

        public AccountControllerTests()
        {
            // Mock UserManager
            var store = new Mock<IUserStore<ApplicationUser>>();
            _mockUserManager = new Mock<UserManager<ApplicationUser>>(
                store.Object, null!, null!, null!, null!, null!, null!, null!, null!);

            // Mock SignInManager
            _mockSignInManager = new Mock<SignInManager<ApplicationUser>>(
                _mockUserManager.Object,
                Mock.Of<Microsoft.AspNetCore.Http.IHttpContextAccessor>(),
                Mock.Of<IUserClaimsPrincipalFactory<ApplicationUser>>(),
                null!, null!, null!, null!);

            // Mock Environment
            _mockEnvironment = new Mock<IWebHostEnvironment>();
            _mockEnvironment.Setup(e => e.EnvironmentName).Returns("Production");

            // Mock Logger
            _mockLogger = new Mock<ILogger<AccountController>>();

            // Create controller
            var userRepositoryMock = new Mock<IUserRepository>();
            _controller = new AccountController(
                userRepositoryMock.Object,
                _mockSignInManager.Object,
                _mockEnvironment.Object,
                _mockLogger.Object);
        }

        [Fact]
        public void Register_Get_ReturnsView()
        {
            // Act
            var result = _controller.Register();

            // Assert
            Assert.NotNull(result);
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.NotNull(_controller.ViewBag.PasswordRequirements);
        }

        [Fact]
        public void RegisterConfirmation_Get_ReturnsView()
        {
            // Act
            var result = _controller.RegisterConfirmation();

            // Assert
            Assert.NotNull(result);
            Assert.IsType<ViewResult>(result);
        }

        [Fact]
        public async Task Login_Get_ReturnsView()
        {
            // Arrange
            var httpContext = new Microsoft.AspNetCore.Http.DefaultHttpContext();
            var claimsPrincipal = new System.Security.Claims.ClaimsPrincipal(new System.Security.Claims.ClaimsIdentity());
            httpContext.User = claimsPrincipal;
            _controller.ControllerContext = new Microsoft.AspNetCore.Mvc.ControllerContext
            {
                HttpContext = httpContext
            };
            _mockSignInManager.Setup(x => x.IsSignedIn(claimsPrincipal))
                .Returns(false);

            // Act
            var result = await _controller.Login();

            // Assert
            Assert.NotNull(result);
            Assert.IsType<ViewResult>(result);
        }

        [Fact]
        public void ForgotPassword_Get_ReturnsView()
        {
            // Act
            var result = _controller.ForgotPassword();

            // Assert
            Assert.NotNull(result);
            Assert.IsType<ViewResult>(result);
        }

        [Fact]
        public void ForgotPasswordConfirmation_Get_ReturnsView()
        {
            // Act
            var result = _controller.ForgotPasswordConfirmation();

            // Assert
            Assert.NotNull(result);
            Assert.IsType<ViewResult>(result);
        }

        [Fact]
        public void ResetPasswordConfirmation_Get_ReturnsView()
        {
            // Act
            var result = _controller.ResetPasswordConfirmation();

            // Assert
            Assert.NotNull(result);
            Assert.IsType<ViewResult>(result);
        }
    }
}

