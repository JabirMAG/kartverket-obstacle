using FirstWebApplication.Models.ViewModel;
using System.ComponentModel.DataAnnotations;

namespace Kartverket.Tests.Models.ViewModel
{
    /// <summary>
    /// Tests for ResetPasswordViewModel
    /// </summary>
    public class ResetPasswordViewModelTest
    {
        private bool ValidateModel(object model, out List<ValidationResult> results)
        {
            var context = new ValidationContext(model, null, null);
            results = new List<ValidationResult>();
            return Validator.TryValidateObject(model, context, results, true);
        }

        /// <summary>
        /// Tests that ResetPasswordViewModel is valid when all fields are correct
        /// </summary>
        [Fact]
        public void ResetPasswordViewModel_ShouldBeValid_WhenAllFieldsAreCorrect()
        {
            // Arrange
            var viewModel = new ResetPasswordViewModel
            {
                Email = "test@example.com",
                Token = "reset-token",
                Password = "NewPassword123!",
                ConfirmPassword = "NewPassword123!"
            };

            // Act
            var isValid = ValidateModel(viewModel, out var results);

            // Assert
            Assert.True(isValid);
            Assert.Empty(results);
        }

        /// <summary>
        /// Tests that ResetPasswordViewModel fails validation when Email is missing
        /// </summary>
        [Fact]
        public void ResetPasswordViewModel_ShouldFail_WhenEmailIsMissing()
        {
            // Arrange
            var viewModel = new ResetPasswordViewModel
            {
                Email = null!,
                Token = "reset-token",
                Password = "NewPassword123!",
                ConfirmPassword = "NewPassword123!"
            };

            // Act
            var isValid = ValidateModel(viewModel, out var results);

            // Assert
            Assert.False(isValid);
            Assert.Contains(results, r => r.MemberNames.Contains(nameof(ResetPasswordViewModel.Email)));
        }

        /// <summary>
        /// Tests that ResetPasswordViewModel fails validation when Token is missing
        /// </summary>
        [Fact]
        public void ResetPasswordViewModel_ShouldFail_WhenTokenIsMissing()
        {
            // Arrange
            var viewModel = new ResetPasswordViewModel
            {
                Email = "test@example.com",
                Token = null!,
                Password = "NewPassword123!",
                ConfirmPassword = "NewPassword123!"
            };

            // Act
            var isValid = ValidateModel(viewModel, out var results);

            // Assert
            Assert.False(isValid);
            Assert.Contains(results, r => r.MemberNames.Contains(nameof(ResetPasswordViewModel.Token)));
        }

        /// <summary>
        /// Tests that ResetPasswordViewModel fails validation when Password is missing
        /// </summary>
        [Fact]
        public void ResetPasswordViewModel_ShouldFail_WhenPasswordIsMissing()
        {
            // Arrange
            var viewModel = new ResetPasswordViewModel
            {
                Email = "test@example.com",
                Token = "reset-token",
                Password = null!,
                ConfirmPassword = "NewPassword123!"
            };

            // Act
            var isValid = ValidateModel(viewModel, out var results);

            // Assert
            Assert.False(isValid);
            Assert.Contains(results, r => r.MemberNames.Contains(nameof(ResetPasswordViewModel.Password)));
        }

        /// <summary>
        /// Tests that ResetPasswordViewModel fails validation when Password is too short
        /// </summary>
        [Fact]
        public void ResetPasswordViewModel_ShouldFail_WhenPasswordIsTooShort()
        {
            // Arrange
            var viewModel = new ResetPasswordViewModel
            {
                Email = "test@example.com",
                Token = "reset-token",
                Password = "Pass1", // Min length is 6
                ConfirmPassword = "Pass1"
            };

            // Act
            var isValid = ValidateModel(viewModel, out var results);

            // Assert
            Assert.False(isValid);
            Assert.Contains(results, r => r.MemberNames.Contains(nameof(ResetPasswordViewModel.Password)));
        }

        /// <summary>
        /// Tests that ResetPasswordViewModel fails validation when ConfirmPassword is missing
        /// </summary>
        [Fact]
        public void ResetPasswordViewModel_ShouldFail_WhenConfirmPasswordIsMissing()
        {
            // Arrange
            var viewModel = new ResetPasswordViewModel
            {
                Email = "test@example.com",
                Token = "reset-token",
                Password = "NewPassword123!",
                ConfirmPassword = null!
            };

            // Act
            var isValid = ValidateModel(viewModel, out var results);

            // Assert
            Assert.False(isValid);
            Assert.Contains(results, r => r.MemberNames.Contains(nameof(ResetPasswordViewModel.ConfirmPassword)));
        }

        /// <summary>
        /// Tests that ResetPasswordViewModel fails validation when passwords do not match
        /// </summary>
        [Fact]
        public void ResetPasswordViewModel_ShouldFail_WhenPasswordsDoNotMatch()
        {
            // Arrange
            var viewModel = new ResetPasswordViewModel
            {
                Email = "test@example.com",
                Token = "reset-token",
                Password = "NewPassword123!",
                ConfirmPassword = "DifferentPassword123!"
            };

            // Act
            var isValid = ValidateModel(viewModel, out var results);

            // Assert
            Assert.False(isValid);
            Assert.Contains(results, r => r.MemberNames.Contains(nameof(ResetPasswordViewModel.ConfirmPassword)));
        }

        /// <summary>
        /// Tests that ResetPasswordViewModel is valid when Password is at minimum length
        /// </summary>
        [Fact]
        public void ResetPasswordViewModel_ShouldBeValid_WhenPasswordIsMinimumLength()
        {
            // Arrange
            var viewModel = new ResetPasswordViewModel
            {
                Email = "test@example.com",
                Token = "reset-token",
                Password = "Pass12", // Exactly min length (6)
                ConfirmPassword = "Pass12"
            };

            // Act
            var isValid = ValidateModel(viewModel, out var results);

            // Assert
            Assert.True(isValid);
            Assert.Empty(results);
        }

        /// <summary>
        /// Tests that ResetPasswordViewModel properties can be modified after instantiation
        /// </summary>
        [Fact]
        public void ResetPasswordViewModel_ShouldAllowPropertyModification_AfterInstantiation()
        {
            // Arrange
            var viewModel = new ResetPasswordViewModel
            {
                Email = "initial@example.com",
                Token = "initial-token",
                Password = "InitialPass123!",
                ConfirmPassword = "InitialPass123!"
            };

            // Act
            viewModel.Email = "updated@example.com";
            viewModel.Token = "updated-token";
            viewModel.Password = "UpdatedPass123!";
            viewModel.ConfirmPassword = "UpdatedPass123!";

            // Assert
            Assert.Equal("updated@example.com", viewModel.Email);
            Assert.Equal("updated-token", viewModel.Token);
            Assert.Equal("UpdatedPass123!", viewModel.Password);
            Assert.Equal("UpdatedPass123!", viewModel.ConfirmPassword);
        }
    }
}

