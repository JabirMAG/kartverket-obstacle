using FirstWebApplication.Models.AdminViewModels;
using System.ComponentModel.DataAnnotations;

namespace Kartverket.Tests.Models.ViewModel
{
    /// <summary>
    /// Tests for CreateUserVm
    /// </summary>
    public class CreateUserVmTest
    {
        private bool ValidateModel(object model, out List<ValidationResult> results)
        {
            var context = new ValidationContext(model, null, null);
            results = new List<ValidationResult>();
            return Validator.TryValidateObject(model, context, results, true);
        }

        /// <summary>
        /// Tests that CreateUserVm is valid when all fields are correct
        /// </summary>
        [Fact]
        public void CreateUserVm_ShouldBeValid_WhenAllFieldsAreCorrect()
        {
            // Arrange
            var viewModel = new CreateUserVm
            {
                Username = "testuser",
                Email = "test@example.com",
                Password = "Password123!",
                Role = "Pilot",
                Organization = "Kartverket"
            };

            // Act
            var isValid = ValidateModel(viewModel, out var results);

            // Assert
            Assert.True(isValid);
            Assert.Empty(results);
        }

        /// <summary>
        /// Tests that CreateUserVm fails validation when Username is missing
        /// </summary>
        [Fact]
        public void CreateUserVm_ShouldFail_WhenUsernameIsMissing()
        {
            // Arrange
            var viewModel = new CreateUserVm
            {
                Username = null!,
                Email = "test@example.com",
                Password = "Password123!",
                Role = "Pilot",
                Organization = "Kartverket"
            };

            // Act
            var isValid = ValidateModel(viewModel, out var results);

            // Assert
            Assert.False(isValid);
            Assert.Contains(results, r => r.MemberNames.Contains(nameof(CreateUserVm.Username)));
        }

        /// <summary>
        /// Tests that CreateUserVm fails validation when Username exceeds max length
        /// </summary>
        [Fact]
        public void CreateUserVm_ShouldFail_WhenUsernameExceedsMaxLength()
        {
            // Arrange
            var viewModel = new CreateUserVm
            {
                Username = new string('A', 51), // Max length is 50
                Email = "test@example.com",
                Password = "Password123!",
                Role = "Pilot",
                Organization = "Kartverket"
            };

            // Act
            var isValid = ValidateModel(viewModel, out var results);

            // Assert
            Assert.False(isValid);
            Assert.Contains(results, r => r.MemberNames.Contains(nameof(CreateUserVm.Username)));
        }

        /// <summary>
        /// Tests that CreateUserVm fails validation when Email is missing
        /// </summary>
        [Fact]
        public void CreateUserVm_ShouldFail_WhenEmailIsMissing()
        {
            // Arrange
            var viewModel = new CreateUserVm
            {
                Username = "testuser",
                Email = null!,
                Password = "Password123!",
                Role = "Pilot",
                Organization = "Kartverket"
            };

            // Act
            var isValid = ValidateModel(viewModel, out var results);

            // Assert
            Assert.False(isValid);
            Assert.Contains(results, r => r.MemberNames.Contains(nameof(CreateUserVm.Email)));
        }

        /// <summary>
        /// Tests that CreateUserVm fails validation when Email is invalid format
        /// </summary>
        [Fact]
        public void CreateUserVm_ShouldFail_WhenEmailIsInvalidFormat()
        {
            // Arrange
            var viewModel = new CreateUserVm
            {
                Username = "testuser",
                Email = "invalid-email",
                Password = "Password123!",
                Role = "Pilot",
                Organization = "Kartverket"
            };

            // Act
            var isValid = ValidateModel(viewModel, out var results);

            // Assert
            Assert.False(isValid);
            Assert.Contains(results, r => r.MemberNames.Contains(nameof(CreateUserVm.Email)));
        }

        /// <summary>
        /// Tests that CreateUserVm fails validation when Password is missing
        /// </summary>
        [Fact]
        public void CreateUserVm_ShouldFail_WhenPasswordIsMissing()
        {
            // Arrange
            var viewModel = new CreateUserVm
            {
                Username = "testuser",
                Email = "test@example.com",
                Password = null!,
                Role = "Pilot",
                Organization = "Kartverket"
            };

            // Act
            var isValid = ValidateModel(viewModel, out var results);

            // Assert
            Assert.False(isValid);
            Assert.Contains(results, r => r.MemberNames.Contains(nameof(CreateUserVm.Password)));
        }

        /// <summary>
        /// Tests that CreateUserVm fails validation when Password is too short
        /// </summary>
        [Fact]
        public void CreateUserVm_ShouldFail_WhenPasswordIsTooShort()
        {
            // Arrange
            var viewModel = new CreateUserVm
            {
                Username = "testuser",
                Email = "test@example.com",
                Password = "Pass1", // Min length is 6
                Role = "Pilot",
                Organization = "Kartverket"
            };

            // Act
            var isValid = ValidateModel(viewModel, out var results);

            // Assert
            Assert.False(isValid);
            Assert.Contains(results, r => r.MemberNames.Contains(nameof(CreateUserVm.Password)));
        }

        /// <summary>
        /// Tests that CreateUserVm fails validation when Organization is missing
        /// </summary>
        [Fact]
        public void CreateUserVm_ShouldFail_WhenOrganizationIsMissing()
        {
            // Arrange
            var viewModel = new CreateUserVm
            {
                Username = "testuser",
                Email = "test@example.com",
                Password = "Password123!",
                Role = "Pilot",
                Organization = null!
            };

            // Act
            var isValid = ValidateModel(viewModel, out var results);

            // Assert
            Assert.False(isValid);
            Assert.Contains(results, r => r.MemberNames.Contains(nameof(CreateUserVm.Organization)));
        }

        /// <summary>
        /// Tests that CreateUserVm has default Role value of "Pilot"
        /// </summary>
        [Fact]
        public void CreateUserVm_ShouldHaveDefault_RoleValue()
        {
            // Arrange & Act
            var viewModel = new CreateUserVm();

            // Assert
            Assert.Equal("Pilot", viewModel.Role);
        }

        /// <summary>
        /// Tests that CreateUserVm has default Organization value
        /// </summary>
        [Fact]
        public void CreateUserVm_ShouldHaveDefault_OrganizationValue()
        {
            // Arrange & Act
            var viewModel = new CreateUserVm();

            // Assert
            Assert.Equal(FirstWebApplication.Models.OrganizationOptions.Kartverket, viewModel.Organization);
        }

        /// <summary>
        /// Tests that CreateUserVm is valid when Password is at minimum length
        /// </summary>
        [Fact]
        public void CreateUserVm_ShouldBeValid_WhenPasswordIsMinimumLength()
        {
            // Arrange
            var viewModel = new CreateUserVm
            {
                Username = "testuser",
                Email = "test@example.com",
                Password = "Pass12", // Exactly min length (6)
                Role = "Pilot",
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

