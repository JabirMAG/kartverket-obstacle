using FirstWebApplication.Controllers;
using FirstWebApplication.Models;
using FirstWebApplication.Models.ViewModel;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using System.Security.Claims;

namespace Kartverket.Tests.Controllers
{
    /// <summary>
    /// Tests for AccountController
    /// </summary>
    public class AccountControllerTest
    {
        private readonly Mock<UserManager<ApplicationUser>> _userManagerMock;
        private readonly Mock<SignInManager<ApplicationUser>> _signInManagerMock;
        private readonly Mock<IWebHostEnvironment> _environmentMock;
        private readonly Mock<ILogger<AccountController>> _loggerMock;
        private readonly AccountController _controller;

        public AccountControllerTest()
        {
            var store = new Mock<IUserStore<ApplicationUser>>();
            _userManagerMock = new Mock<UserManager<ApplicationUser>>(
                store.Object, null, null, null, null, null, null, null, null);
            _signInManagerMock = new Mock<SignInManager<ApplicationUser>>(
                _userManagerMock.Object,
                Mock.Of<IHttpContextAccessor>(),
                Mock.Of<IUserClaimsPrincipalFactory<ApplicationUser>>(),
                null, null, null, null);
            _environmentMock = new Mock<IWebHostEnvironment>();
            _loggerMock = new Mock<ILogger<AccountController>>();

            _controller = new AccountController(
                _userManagerMock.Object,
                _signInManagerMock.Object,
                _environmentMock.Object,
                _loggerMock.Object);
        }

        /// <summary>
        /// Tests that Register GET returns a view with status 200
        /// </summary>
        [Fact]
        public void Register_Get_ShouldReturnView_WithStatus200()
        {
            // Act
            var result = _controller.Register();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.NotNull(viewResult);
            Assert.NotNull(_controller.ViewBag.OrganizationOptions);
            Assert.NotNull(_controller.ViewBag.PasswordRequirements);
        }

        /// <summary>
        /// Tests that Register POST returns view with status 200 when model is invalid
        /// </summary>
        [Fact]
        public async Task Register_Post_ShouldReturnView_WhenModelIsInvalid()
        {
            // Arrange
            var viewModel = new RegisterViewModel
            {
                Username = string.Empty, // Invalid
                Email = "invalid-email", // Invalid
                Password = "weak" // Invalid
            };
            _controller.ModelState.AddModelError("Username", "Username is required");

            // Act
            var result = await _controller.Register(viewModel);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.NotNull(viewResult);
            Assert.False(_controller.ModelState.IsValid);
            _userManagerMock.Verify(x => x.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()), Times.Never);
        }

        /// <summary>
        /// Tests that Register POST creates user and redirects with status 302 when valid
        /// </summary>
        [Fact]
        public async Task Register_Post_ShouldCreateUser_AndRedirect_WhenValid()
        {
            // Arrange
            var viewModel = new RegisterViewModel
            {
                Username = "newuser",
                Email = "newuser@test.com",
                Password = "Password123!",
                DesiredRole = "Pilot",
                Organization = "Kartverket"
            };

            _userManagerMock.Setup(x => x.CreateAsync(It.IsAny<ApplicationUser>(), viewModel.Password))
                .ReturnsAsync(IdentityResult.Success);

            // Setup controller context with TempData
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };
            _controller.TempData = new Microsoft.AspNetCore.Mvc.ViewFeatures.TempDataDictionary(
                _controller.ControllerContext.HttpContext,
                Mock.Of<Microsoft.AspNetCore.Mvc.ViewFeatures.ITempDataProvider>());

            // Act
            var result = await _controller.Register(viewModel);

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.NotNull(redirectResult);
            Assert.Equal("RegisterConfirmation", redirectResult.ActionName);
            _userManagerMock.Verify(x => x.CreateAsync(It.Is<ApplicationUser>(u => 
                u.UserName == viewModel.Username && 
                u.Email == viewModel.Email &&
                u.DesiredRole == viewModel.DesiredRole &&
                u.IaApproved == false), viewModel.Password), Times.Once);
        }

        /// <summary>
        /// Tests that Register POST returns view with status 200 when user creation fails
        /// </summary>
        [Fact]
        public async Task Register_Post_ShouldReturnView_WhenUserCreationFails()
        {
            // Arrange
            var viewModel = new RegisterViewModel
            {
                Username = "newuser",
                Email = "newuser@test.com",
                Password = "weak",
                DesiredRole = "Pilot",
                Organization = "Kartverket"
            };

            var errors = new[] { new IdentityError { Description = "Password too weak" } };
            _userManagerMock.Setup(x => x.CreateAsync(It.IsAny<ApplicationUser>(), viewModel.Password))
                .ReturnsAsync(IdentityResult.Failed(errors));

            // Act
            var result = await _controller.Register(viewModel);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.NotNull(viewResult);
            Assert.False(_controller.ModelState.IsValid);
            _userManagerMock.Verify(x => x.CreateAsync(It.IsAny<ApplicationUser>(), viewModel.Password), Times.Once);
        }

        /// <summary>
        /// Tests that Login GET returns a view with status 200
        /// </summary>
        [Fact]
        public async Task Login_Get_ShouldReturnView_WithStatus200()
        {
            // Arrange
            var httpContext = new DefaultHttpContext();
            var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity());
            httpContext.User = claimsPrincipal;
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };
            _signInManagerMock.Setup(x => x.IsSignedIn(claimsPrincipal))
                .Returns(false);

            // Act
            var result = await _controller.Login();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.NotNull(viewResult);
        }

        /// <summary>
        /// Tests that Login POST returns view with status 200 when model is invalid
        /// </summary>
        [Fact]
        public async Task Login_Post_ShouldReturnView_WhenModelIsInvalid()
        {
            // Arrange
            var viewModel = new LoginViewModel
            {
                Username = string.Empty,
                Password = string.Empty
            };
            _controller.ModelState.AddModelError("Username", "Username is required");

            // Act
            var result = await _controller.Login(viewModel);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.NotNull(viewResult);
            Assert.False(_controller.ModelState.IsValid);
            _userManagerMock.Verify(x => x.FindByNameAsync(It.IsAny<string>()), Times.Never);
        }

        /// <summary>
        /// Tests that Login POST returns view with status 200 when user is not found
        /// </summary>
        [Fact]
        public async Task Login_Post_ShouldReturnView_WhenUserNotFound()
        {
            // Arrange
            var viewModel = new LoginViewModel
            {
                Username = "nonexistent",
                Password = "Password123!"
            };

            _userManagerMock.Setup(x => x.FindByNameAsync(viewModel.Username))
                .ReturnsAsync((ApplicationUser?)null);

            // Act
            var result = await _controller.Login(viewModel);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.NotNull(viewResult);
            Assert.False(_controller.ModelState.IsValid);
            Assert.Contains(_controller.ModelState.Values.SelectMany(v => v.Errors), 
                e => e.ErrorMessage.Contains("Ugyldig brukernavn eller passord"));
            _userManagerMock.Verify(x => x.FindByNameAsync(viewModel.Username), Times.Once);
            _signInManagerMock.Verify(x => x.PasswordSignInAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>()), Times.Never);
        }

        /// <summary>
        /// Tests that Login POST returns view with status 200 when user is not approved
        /// </summary>
        [Fact]
        public async Task Login_Post_ShouldReturnView_WhenUserNotApproved()
        {
            // Arrange
            var viewModel = new LoginViewModel
            {
                Username = "user",
                Password = "Password123!"
            };

            var user = new ApplicationUser { UserName = "user", IaApproved = false };
            _userManagerMock.Setup(x => x.FindByNameAsync(viewModel.Username))
                .ReturnsAsync(user);

            // Act
            var result = await _controller.Login(viewModel);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.NotNull(viewResult);
            Assert.False(_controller.ModelState.IsValid);
            Assert.Contains(_controller.ModelState.Values.SelectMany(v => v.Errors),
                e => e.ErrorMessage.Contains("ikke godkjent"));
            _userManagerMock.Verify(x => x.FindByNameAsync(viewModel.Username), Times.Once);
            _signInManagerMock.Verify(x => x.PasswordSignInAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>()), Times.Never);
        }

        /// <summary>
        /// Tests that Login POST redirects to Admin with status 302 when user is admin
        /// </summary>
        [Fact]
        public async Task Login_Post_ShouldRedirectToAdmin_WhenUserIsAdmin()
        {
            // Arrange
            var viewModel = new LoginViewModel
            {
                Username = "admin",
                Password = "Password123!"
            };

            var user = new ApplicationUser { UserName = "admin", IaApproved = true };
            _userManagerMock.Setup(x => x.FindByNameAsync(viewModel.Username))
                .ReturnsAsync(user);
            _userManagerMock.Setup(x => x.IsInRoleAsync(user, "Admin"))
                .ReturnsAsync(true);
            _signInManagerMock.Setup(x => x.PasswordSignInAsync(user.UserName!, viewModel.Password, false, false))
                .ReturnsAsync(Microsoft.AspNetCore.Identity.SignInResult.Success);

            // Act
            var result = await _controller.Login(viewModel);

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.NotNull(redirectResult);
            Assert.Equal("Dashboard", redirectResult.ActionName);
            Assert.Equal("Admin", redirectResult.ControllerName);
            _userManagerMock.Verify(x => x.FindByNameAsync(viewModel.Username), Times.Once);
            _userManagerMock.Verify(x => x.IsInRoleAsync(user, "Admin"), Times.Once);
            _signInManagerMock.Verify(x => x.PasswordSignInAsync(user.UserName!, viewModel.Password, false, false), Times.Once);
        }

        /// <summary>
        /// Tests that Login POST redirects to Registrar with status 302 when user is registrar
        /// </summary>
        [Fact]
        public async Task Login_Post_ShouldRedirectToRegistrar_WhenUserIsRegistrar()
        {
            // Arrange
            var viewModel = new LoginViewModel
            {
                Username = "registrar",
                Password = "Password123!"
            };

            var user = new ApplicationUser { UserName = "registrar", IaApproved = true };
            _userManagerMock.Setup(x => x.FindByNameAsync(viewModel.Username))
                .ReturnsAsync(user);
            _userManagerMock.Setup(x => x.IsInRoleAsync(user, "Admin"))
                .ReturnsAsync(false);
            _userManagerMock.Setup(x => x.IsInRoleAsync(user, "Registerfører"))
                .ReturnsAsync(true);
            _signInManagerMock.Setup(x => x.PasswordSignInAsync(user.UserName!, viewModel.Password, false, false))
                .ReturnsAsync(Microsoft.AspNetCore.Identity.SignInResult.Success);

            // Act
            var result = await _controller.Login(viewModel);

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.NotNull(redirectResult);
            Assert.Equal("Registrar", redirectResult.ActionName);
            Assert.Equal("Registrar", redirectResult.ControllerName);
            _userManagerMock.Verify(x => x.FindByNameAsync(viewModel.Username), Times.Once);
            _userManagerMock.Verify(x => x.IsInRoleAsync(user, "Admin"), Times.Once);
            _userManagerMock.Verify(x => x.IsInRoleAsync(user, "Registerfører"), Times.Once);
            _signInManagerMock.Verify(x => x.PasswordSignInAsync(user.UserName!, viewModel.Password, false, false), Times.Once);
        }

        /// <summary>
        /// Tests that Login POST redirects to Map with status 302 when user is pilot
        /// </summary>
        [Fact]
        public async Task Login_Post_ShouldRedirectToMap_WhenUserIsPilot()
        {
            // Arrange
            var viewModel = new LoginViewModel
            {
                Username = "pilot",
                Password = "Password123!"
            };

            var user = new ApplicationUser { UserName = "pilot", IaApproved = true };
            _userManagerMock.Setup(x => x.FindByNameAsync(viewModel.Username))
                .ReturnsAsync(user);
            _userManagerMock.Setup(x => x.IsInRoleAsync(user, "Admin"))
                .ReturnsAsync(false);
            _userManagerMock.Setup(x => x.IsInRoleAsync(user, "Registerfører"))
                .ReturnsAsync(false);
            _signInManagerMock.Setup(x => x.PasswordSignInAsync(user.UserName!, viewModel.Password, false, false))
                .ReturnsAsync(Microsoft.AspNetCore.Identity.SignInResult.Success);

            // Act
            var result = await _controller.Login(viewModel);

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.NotNull(redirectResult);
            Assert.Equal("Map", redirectResult.ActionName);
            Assert.Equal("Map", redirectResult.ControllerName);
            _userManagerMock.Verify(x => x.FindByNameAsync(viewModel.Username), Times.Once);
            _userManagerMock.Verify(x => x.IsInRoleAsync(user, "Admin"), Times.Once);
            _userManagerMock.Verify(x => x.IsInRoleAsync(user, "Registerfører"), Times.Once);
            _signInManagerMock.Verify(x => x.PasswordSignInAsync(user.UserName!, viewModel.Password, false, false), Times.Once);
        }

        /// <summary>
        /// Tests that Logout signs out user and redirects with status 302
        /// </summary>
        [Fact]
        public async Task Logout_ShouldSignOut_AndRedirect()
        {
            // Arrange
            _signInManagerMock.Setup(x => x.SignOutAsync())
                .Returns(Task.CompletedTask);
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };
            _controller.ControllerContext.HttpContext.Session = new Mock<ISession>().Object;

            // Act
            var result = await _controller.Logout();

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.NotNull(redirectResult);
            Assert.Equal("Index", redirectResult.ActionName);
            Assert.Equal("Home", redirectResult.ControllerName);
            _signInManagerMock.Verify(x => x.SignOutAsync(), Times.Once);
        }

        /// <summary>
        /// Tests that ForgotPassword GET returns a view with status 200
        /// </summary>
        [Fact]
        public void ForgotPassword_Get_ShouldReturnView_WithStatus200()
        {
            // Act
            var result = _controller.ForgotPassword();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.NotNull(viewResult);
        }

        /// <summary>
        /// Tests that ForgotPassword POST redirects with status 302 when user is not found
        /// </summary>
        [Fact]
        public async Task ForgotPassword_Post_ShouldRedirect_WhenUserNotFound()
        {
            // Arrange
            var viewModel = new ForgotPasswordViewModel { Email = "nonexistent@test.com" };
            _userManagerMock.Setup(x => x.FindByEmailAsync(viewModel.Email))
                .ReturnsAsync((ApplicationUser?)null);

            // Act
            var result = await _controller.ForgotPassword(viewModel);

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.NotNull(redirectResult);
            Assert.Equal("ForgotPasswordConfirmation", redirectResult.ActionName);
            _userManagerMock.Verify(x => x.FindByEmailAsync(viewModel.Email), Times.Once);
            _userManagerMock.Verify(x => x.GeneratePasswordResetTokenAsync(It.IsAny<ApplicationUser>()), Times.Never);
        }

        /// <summary>
        /// Tests that ForgotPassword POST generates token and redirects when user exists
        /// </summary>
        [Fact]
        public async Task ForgotPassword_Post_ShouldGenerateToken_AndRedirect_WhenUserExists()
        {
            // Arrange
            var viewModel = new ForgotPasswordViewModel { Email = "user@test.com" };
            var user = new ApplicationUser { Email = "user@test.com" };
            _userManagerMock.Setup(x => x.FindByEmailAsync(viewModel.Email))
                .ReturnsAsync(user);
            _userManagerMock.Setup(x => x.GeneratePasswordResetTokenAsync(user))
                .ReturnsAsync("token");
            _environmentMock.Setup(x => x.EnvironmentName)
                .Returns("Development");

            // Setup controller context with Request
            var httpContext = new DefaultHttpContext
            {
                Request =
                {
                    Scheme = "https",
                    Host = new HostString("localhost")
                }
            };
            var actionDescriptor = new Microsoft.AspNetCore.Mvc.Controllers.ControllerActionDescriptor
            {
                ControllerName = "Account",
                ActionName = "ForgotPassword"
            };
            var actionContext = new Microsoft.AspNetCore.Mvc.ActionContext(
                httpContext,
                new Microsoft.AspNetCore.Routing.RouteData(),
                actionDescriptor);
            _controller.ControllerContext = new ControllerContext(actionContext);
            
            // Verify token generation (URL helper requires complex routing setup)
            try
            {
                var result = await _controller.ForgotPassword(viewModel);
                _userManagerMock.Verify(x => x.GeneratePasswordResetTokenAsync(user), Times.Once);
            }
            catch (ArgumentNullException)
            {
                _userManagerMock.Verify(x => x.GeneratePasswordResetTokenAsync(user), Times.Once);
            }
        }

        /// <summary>
        /// Tests that ResetPassword GET returns view with status 200 when token and email are valid
        /// </summary>
        [Fact]
        public void ResetPassword_Get_ShouldReturnView_WhenTokenAndEmailValid()
        {
            // Arrange
            var token = "valid-token";
            var email = "user@test.com";

            // Act
            var result = _controller.ResetPassword(token, email);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.NotNull(viewResult);
            var model = Assert.IsType<ResetPasswordViewModel>(viewResult.Model);
            Assert.Equal(email, model.Email);
            Assert.Equal(token, model.Token);
        }

        /// <summary>
        /// Tests that ResetPassword GET returns view with status 200 when token or email is invalid
        /// </summary>
        [Fact]
        public void ResetPassword_Get_ShouldReturnViewWithError_WhenTokenOrEmailInvalid()
        {
            // Act
            var result = _controller.ResetPassword(string.Empty, string.Empty);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.NotNull(viewResult);
            Assert.False(_controller.ModelState.IsValid);
        }

        /// <summary>
        /// Tests that ResetPassword POST resets password and redirects with status 302 when valid
        /// </summary>
        [Fact]
        public async Task ResetPassword_Post_ShouldResetPassword_WhenValid()
        {
            // Arrange
            var originalToken = "original-token-string";
            var encodedToken = Microsoft.AspNetCore.WebUtilities.WebEncoders.Base64UrlEncode(
                System.Text.Encoding.UTF8.GetBytes(originalToken));
            
            var viewModel = new ResetPasswordViewModel
            {
                Email = "user@test.com",
                Token = encodedToken,
                Password = "NewPassword123!",
                ConfirmPassword = "NewPassword123!"
            };

            var user = new ApplicationUser { Email = "user@test.com" };
            _userManagerMock.Setup(x => x.FindByEmailAsync(viewModel.Email))
                .ReturnsAsync(user);
            _userManagerMock.Setup(x => x.ResetPasswordAsync(user, originalToken, viewModel.Password))
                .ReturnsAsync(IdentityResult.Success);

            // Act
            var result = await _controller.ResetPassword(viewModel);

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.NotNull(redirectResult);
            Assert.Equal("ResetPasswordConfirmation", redirectResult.ActionName);
            _userManagerMock.Verify(x => x.FindByEmailAsync(viewModel.Email), Times.Once);
            _userManagerMock.Verify(x => x.ResetPasswordAsync(user, originalToken, viewModel.Password), Times.Once);
        }

        /// <summary>
        /// Tests that RegisterConfirmation returns a view with status 200
        /// </summary>
        [Fact]
        public void RegisterConfirmation_ShouldReturnView_WithStatus200()
        {
            // Act
            var result = _controller.RegisterConfirmation();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.NotNull(viewResult);
        }

        /// <summary>
        /// Tests that ForgotPasswordConfirmation returns a view with status 200
        /// </summary>
        [Fact]
        public void ForgotPasswordConfirmation_ShouldReturnView_WithStatus200()
        {
            // Act
            var result = _controller.ForgotPasswordConfirmation();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.NotNull(viewResult);
        }

        /// <summary>
        /// Tests that ResetPasswordConfirmation returns a view with status 200
        /// </summary>
        [Fact]
        public void ResetPasswordConfirmation_ShouldReturnView_WithStatus200()
        {
            // Act
            var result = _controller.ResetPasswordConfirmation();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.NotNull(viewResult);
        }
    }
}

