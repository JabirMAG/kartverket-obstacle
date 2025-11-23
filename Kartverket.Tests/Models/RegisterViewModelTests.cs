using FirstWebApplication.Models.ViewModel;
using System.ComponentModel.DataAnnotations;

namespace Kartverket.Tests.Models
{
    public class RegisterViewModelTests
    {
        private bool ValidateModel(object model, out List<ValidationResult> results)
        {
            var context = new ValidationContext(model, null, null);
            results = new List<ValidationResult>();
            return Validator.TryValidateObject(model, context, results, true);
        }

        [Fact]
        public void RegisterViewModel_ShouldBeValid_WhenAllFieldsAreCorrect()
        {
            // Arrange
            var model = new RegisterViewModel
            {
                Username = "testuser",
                Email = "test@test.com",
                DesiredRole = "Pilot",
                Password = "Test123!",
                Organization = "Kartverket"
            };

            // Act
            var isValid = ValidateModel(model, out var results);

            // Assert
            Assert.True(isValid);
            Assert.Empty(results);
        }

        [Fact]
        public void RegisterViewModel_ShouldFail_WhenUsernameIsMissing()
        {
            // Arrange
            var model = new RegisterViewModel
            {
                Username = "", // Tomt
                Email = "test@test.com",
                DesiredRole = "Pilot",
                Password = "Test123!",
                Organization = "Kartverket"
            };

            // Act
            var isValid = ValidateModel(model, out var results);

            // Assert
            Assert.False(isValid);
            Assert.Contains(results, r => r.MemberNames.Contains("Username"));
        }

        [Fact]
        public void RegisterViewModel_ShouldFail_WhenEmailIsInvalid()
        {
            // Arrange
            var model = new RegisterViewModel
            {
                Username = "testuser",
                Email = "invalid-email", // Ugyldig e-post
                DesiredRole = "Pilot",
                Password = "Test123!",
                Organization = "Kartverket"
            };

            // Act
            var isValid = ValidateModel(model, out var results);

            // Assert
            Assert.False(isValid);
            Assert.Contains(results, r => r.MemberNames.Contains("Email"));
        }

        [Fact]
        public void RegisterViewModel_ShouldFail_WhenPasswordIsTooShort()
        {
            // Arrange
            var model = new RegisterViewModel
            {
                Username = "testuser",
                Email = "test@test.com",
                DesiredRole = "Pilot",
                Password = "Test1", // For kort (minimum 6 tegn)
                Organization = "Kartverket"
            };

            // Act
            var isValid = ValidateModel(model, out var results);

            // Assert
            Assert.False(isValid);
            Assert.Contains(results, r => r.MemberNames.Contains("Password"));
        }

        [Fact]
        public void RegisterViewModel_ShouldFail_WhenDesiredRoleIsMissing()
        {
            // Arrange
            var model = new RegisterViewModel
            {
                Username = "testuser",
                Email = "test@test.com",
                DesiredRole = "", // Tomt
                Password = "Test123!",
                Organization = "Kartverket"
            };

            // Act
            var isValid = ValidateModel(model, out var results);

            // Assert
            Assert.False(isValid);
            Assert.Contains(results, r => r.MemberNames.Contains("DesiredRole"));
        }
    }
}

