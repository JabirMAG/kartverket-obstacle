using FirstWebApplication.Models.ViewModel;
using System.ComponentModel.DataAnnotations;

namespace Kartverket.Tests.Models.ViewModel
{
    /// <summary>
    /// Tests for ForgotPasswordViewModel
    /// </summary>
    public class ForgotPasswordViewModelTest
    {
        private bool ValidateModel(object model, out List<ValidationResult> results)
        {
            var context = new ValidationContext(model, null, null);
            results = new List<ValidationResult>();
            return Validator.TryValidateObject(model, context, results, true);
        }

        /// <summary>
        /// Tests that ForgotPasswordViewModel is valid when Email is correct
        /// </summary>
        [Fact]
        public void ForgotPasswordViewModel_ShouldBeValid_WhenEmailIsCorrect()
        {
            // Arrange
            var viewModel = new ForgotPasswordViewModel
            {
                Email = "test@example.com"
            };

            // Act
            var isValid = ValidateModel(viewModel, out var results);

            // Assert
            Assert.True(isValid);
            Assert.Empty(results);
        }

        /// <summary>
        /// Tests that ForgotPasswordViewModel fails validation when Email is missing
        /// </summary>
        [Fact]
        public void ForgotPasswordViewModel_ShouldFail_WhenEmailIsMissing()
        {
            // Arrange
            var viewModel = new ForgotPasswordViewModel
            {
                Email = null!
            };

            // Act
            var isValid = ValidateModel(viewModel, out var results);

            // Assert
            Assert.False(isValid);
            Assert.Contains(results, r => r.MemberNames.Contains(nameof(ForgotPasswordViewModel.Email)));
        }

        /// <summary>
        /// Tests that ForgotPasswordViewModel fails validation when Email is invalid format
        /// </summary>
        [Fact]
        public void ForgotPasswordViewModel_ShouldFail_WhenEmailIsInvalidFormat()
        {
            // Arrange
            var viewModel = new ForgotPasswordViewModel
            {
                Email = "invalid-email"
            };

            // Act
            var isValid = ValidateModel(viewModel, out var results);

            // Assert
            Assert.False(isValid);
            Assert.Contains(results, r => r.MemberNames.Contains(nameof(ForgotPasswordViewModel.Email)));
        }

        /// <summary>
        /// Tests that ForgotPasswordViewModel fails validation when Email is empty string
        /// </summary>
        [Fact]
        public void ForgotPasswordViewModel_ShouldFail_WhenEmailIsEmpty()
        {
            // Arrange
            var viewModel = new ForgotPasswordViewModel
            {
                Email = string.Empty
            };

            // Act
            var isValid = ValidateModel(viewModel, out var results);

            // Assert
            Assert.False(isValid);
            Assert.Contains(results, r => r.MemberNames.Contains(nameof(ForgotPasswordViewModel.Email)));
        }

        /// <summary>
        /// Tests that ForgotPasswordViewModel properties can be modified after instantiation
        /// </summary>
        [Fact]
        public void ForgotPasswordViewModel_ShouldAllowPropertyModification_AfterInstantiation()
        {
            // Arrange
            var viewModel = new ForgotPasswordViewModel
            {
                Email = "initial@example.com"
            };

            // Act
            viewModel.Email = "updated@example.com";

            // Assert
            Assert.Equal("updated@example.com", viewModel.Email);
        }
    }
}

