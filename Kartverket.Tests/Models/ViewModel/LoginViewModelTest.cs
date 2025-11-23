using FirstWebApplication.Models.ViewModel;
using System.ComponentModel.DataAnnotations;

namespace Kartverket.Tests.Models.ViewModel
{
    /// <summary>
    /// Tests for LoginViewModel
    /// </summary>
    public class LoginViewModelTest
    {
        private bool ValidateModel(object model, out List<ValidationResult> results)
        {
            var context = new ValidationContext(model, null, null);
            results = new List<ValidationResult>();
            return Validator.TryValidateObject(model, context, results, true);
        }

        /// <summary>
        /// Tests that LoginViewModel is valid when all fields are correct
        /// </summary>
        [Fact]
        public void LoginViewModel_ShouldBeValid_WhenAllFieldsAreCorrect()
        {
            // Arrange
            var viewModel = new LoginViewModel
            {
                Username = "testuser",
                Password = "Password123!"
            };

            // Act
            var isValid = ValidateModel(viewModel, out var results);

            // Assert
            Assert.True(isValid);
            Assert.Empty(results);
        }

        /// <summary>
        /// Tests that LoginViewModel fails validation when Username is missing
        /// </summary>
        [Fact]
        public void LoginViewModel_ShouldFail_WhenUsernameIsMissing()
        {
            // Arrange
            var viewModel = new LoginViewModel
            {
                Username = string.Empty,
                Password = "Password123!"
            };

            // Act
            var isValid = ValidateModel(viewModel, out var results);

            // Assert
            Assert.False(isValid);
            Assert.Contains(results, r => r.MemberNames.Contains(nameof(LoginViewModel.Username)));
        }

        /// <summary>
        /// Tests that LoginViewModel fails validation when Password is missing
        /// </summary>
        [Fact]
        public void LoginViewModel_ShouldFail_WhenPasswordIsMissing()
        {
            // Arrange
            var viewModel = new LoginViewModel
            {
                Username = "testuser",
                Password = string.Empty
            };

            // Act
            var isValid = ValidateModel(viewModel, out var results);

            // Assert
            Assert.False(isValid);
            Assert.Contains(results, r => r.MemberNames.Contains(nameof(LoginViewModel.Password)));
        }

        /// <summary>
        /// Tests that LoginViewModel has default empty string values
        /// </summary>
        [Fact]
        public void LoginViewModel_ShouldHaveDefault_EmptyStringValues()
        {
            // Arrange & Act
            var viewModel = new LoginViewModel();

            // Assert
            Assert.Equal(string.Empty, viewModel.Username);
            Assert.Equal(string.Empty, viewModel.Password);
        }

        /// <summary>
        /// Tests that LoginViewModel properties can be modified after instantiation
        /// </summary>
        [Fact]
        public void LoginViewModel_ShouldAllowPropertyModification_AfterInstantiation()
        {
            // Arrange
            var viewModel = new LoginViewModel
            {
                Username = "initial",
                Password = "initial"
            };

            // Act
            viewModel.Username = "updated";
            viewModel.Password = "updated";

            // Assert
            Assert.Equal("updated", viewModel.Username);
            Assert.Equal("updated", viewModel.Password);
        }
    }
}

