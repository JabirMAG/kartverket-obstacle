using FirstWebApplication.Models.ViewModel;
using System.ComponentModel.DataAnnotations;

namespace Kartverket.Tests.Models.ViewModel
{
    /// <summary>
    /// Tests for RegisterViewModel
    /// </summary>
    public class RegisterViewModelTest
    {
        private bool ValidateModel(object model, out List<ValidationResult> results)
        {
            var context = new ValidationContext(model, null, null);
            results = new List<ValidationResult>();
            return Validator.TryValidateObject(model, context, results, true);
        }

        /// <summary>
        /// Tests that RegisterViewModel is valid when all fields are correct
        /// </summary>
        [Fact]
        public void RegisterViewModel_ShouldBeValid_WhenAllFieldsAreCorrect()
        {
            // Arrange
            var viewModel = new RegisterViewModel
            {
                Username = "testuser",
                Email = "test@example.com",
                DesiredRole = "Pilot",
                Password = "Password123!",
                Organization = "Kartverket"
            };

            // Act
            var isValid = ValidateModel(viewModel, out var results);

            // Assert
            Assert.True(isValid);
            Assert.Empty(results);
        }

        /// <summary>
        /// Tests that RegisterViewModel fails validation when Username is missing
        /// </summary>
        [Fact]
        public void RegisterViewModel_ShouldFail_WhenUsernameIsMissing()
        {
            // Arrange
            var viewModel = new RegisterViewModel
            {
                Username = null!,
                Email = "test@example.com",
                DesiredRole = "Pilot",
                Password = "Password123!",
                Organization = "Kartverket"
            };

            // Act
            var isValid = ValidateModel(viewModel, out var results);

            // Assert
            Assert.False(isValid);
            Assert.Contains(results, r => r.MemberNames.Contains(nameof(RegisterViewModel.Username)));
        }

        /// <summary>
        /// Tests that RegisterViewModel fails validation when Username exceeds max length
        /// </summary>
        [Fact]
        public void RegisterViewModel_ShouldFail_WhenUsernameExceedsMaxLength()
        {
            // Arrange
            var viewModel = new RegisterViewModel
            {
                Username = new string('A', 51), // Max length is 50
                Email = "test@example.com",
                DesiredRole = "Pilot",
                Password = "Password123!",
                Organization = "Kartverket"
            };

            // Act
            var isValid = ValidateModel(viewModel, out var results);

            // Assert
            Assert.False(isValid);
            Assert.Contains(results, r => r.MemberNames.Contains(nameof(RegisterViewModel.Username)));
        }

        /// <summary>
        /// Tests that RegisterViewModel fails validation when Email is missing
        /// </summary>
        [Fact]
        public void RegisterViewModel_ShouldFail_WhenEmailIsMissing()
        {
            // Arrange
            var viewModel = new RegisterViewModel
            {
                Username = "testuser",
                Email = null!,
                DesiredRole = "Pilot",
                Password = "Password123!",
                Organization = "Kartverket"
            };

            // Act
            var isValid = ValidateModel(viewModel, out var results);

            // Assert
            Assert.False(isValid);
            Assert.Contains(results, r => r.MemberNames.Contains(nameof(RegisterViewModel.Email)));
        }

        /// <summary>
        /// Tests that RegisterViewModel fails validation when Email is invalid format
        /// </summary>
        [Fact]
        public void RegisterViewModel_ShouldFail_WhenEmailIsInvalidFormat()
        {
            // Arrange
            var viewModel = new RegisterViewModel
            {
                Username = "testuser",
                Email = "invalid-email",
                DesiredRole = "Pilot",
                Password = "Password123!",
                Organization = "Kartverket"
            };

            // Act
            var isValid = ValidateModel(viewModel, out var results);

            // Assert
            Assert.False(isValid);
            Assert.Contains(results, r => r.MemberNames.Contains(nameof(RegisterViewModel.Email)));
        }

        /// <summary>
        /// Tests that RegisterViewModel fails validation when DesiredRole is missing
        /// </summary>
        [Fact]
        public void RegisterViewModel_ShouldFail_WhenDesiredRoleIsMissing()
        {
            // Arrange
            var viewModel = new RegisterViewModel
            {
                Username = "testuser",
                Email = "test@example.com",
                DesiredRole = null!,
                Password = "Password123!",
                Organization = "Kartverket"
            };

            // Act
            var isValid = ValidateModel(viewModel, out var results);

            // Assert
            Assert.False(isValid);
            Assert.Contains(results, r => r.MemberNames.Contains(nameof(RegisterViewModel.DesiredRole)));
        }

        /// <summary>
        /// Tests that RegisterViewModel fails validation when Password is missing
        /// </summary>
        [Fact]
        public void RegisterViewModel_ShouldFail_WhenPasswordIsMissing()
        {
            // Arrange
            var viewModel = new RegisterViewModel
            {
                Username = "testuser",
                Email = "test@example.com",
                DesiredRole = "Pilot",
                Password = null!,
                Organization = "Kartverket"
            };

            // Act
            var isValid = ValidateModel(viewModel, out var results);

            // Assert
            Assert.False(isValid);
            Assert.Contains(results, r => r.MemberNames.Contains(nameof(RegisterViewModel.Password)));
        }

        /// <summary>
        /// Tests that RegisterViewModel fails validation when Password is too short
        /// </summary>
        [Fact]
        public void RegisterViewModel_ShouldFail_WhenPasswordIsTooShort()
        {
            // Arrange
            var viewModel = new RegisterViewModel
            {
                Username = "testuser",
                Email = "test@example.com",
                DesiredRole = "Pilot",
                Password = "Pass1", // Min length is 6
                Organization = "Kartverket"
            };

            // Act
            var isValid = ValidateModel(viewModel, out var results);

            // Assert
            Assert.False(isValid);
            Assert.Contains(results, r => r.MemberNames.Contains(nameof(RegisterViewModel.Password)));
        }

        /// <summary>
        /// Tests that RegisterViewModel fails validation when Organization is missing
        /// </summary>
        [Fact]
        public void RegisterViewModel_ShouldFail_WhenOrganizationIsMissing()
        {
            // Arrange
            var viewModel = new RegisterViewModel
            {
                Username = "testuser",
                Email = "test@example.com",
                DesiredRole = "Pilot",
                Password = "Password123!",
                Organization = null!
            };

            // Act
            var isValid = ValidateModel(viewModel, out var results);

            // Assert
            Assert.False(isValid);
            Assert.Contains(results, r => r.MemberNames.Contains(nameof(RegisterViewModel.Organization)));
        }

        /// <summary>
        /// Tests that RegisterViewModel has default Organization value
        /// </summary>
        [Fact]
        public void RegisterViewModel_ShouldHaveDefault_OrganizationValue()
        {
            // Arrange & Act
            var viewModel = new RegisterViewModel();

            // Assert
            Assert.Equal(FirstWebApplication.Models.OrganizationOptions.Kartverket, viewModel.Organization);
        }

        /// <summary>
        /// Tests that RegisterViewModel is valid when Username is at max length
        /// </summary>
        [Fact]
        public void RegisterViewModel_ShouldBeValid_WhenUsernameIsMaxLength()
        {
            // Arrange
            var viewModel = new RegisterViewModel
            {
                Username = new string('A', 50), // Exactly max length
                Email = "test@example.com",
                DesiredRole = "Pilot",
                Password = "Password123!",
                Organization = "Kartverket"
            };

            // Act
            var isValid = ValidateModel(viewModel, out var results);

            // Assert
            Assert.True(isValid);
            Assert.Empty(results);
        }

        /// <summary>
        /// Tests that RegisterViewModel is valid when Password is at minimum length
        /// </summary>
        [Fact]
        public void RegisterViewModel_ShouldBeValid_WhenPasswordIsMinimumLength()
        {
            // Arrange
            var viewModel = new RegisterViewModel
            {
                Username = "testuser",
                Email = "test@example.com",
                DesiredRole = "Pilot",
                Password = "Pass12", // Exactly min length (6)
                Organization = "Kartverket"
            };

            // Act
            var isValid = ValidateModel(viewModel, out var results);

            // Assert
            Assert.True(isValid);
            Assert.Empty(results);
        }
    }
}

