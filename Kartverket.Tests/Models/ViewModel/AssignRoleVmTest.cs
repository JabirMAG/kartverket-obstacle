using FirstWebApplication.Models.AdminViewModels;
using System.ComponentModel.DataAnnotations;

namespace Kartverket.Tests.Models.ViewModel
{
    /// <summary>
    /// Tests for AssignRoleVm
    /// </summary>
    public class AssignRoleVmTest
    {
        private bool ValidateModel(object model, out List<ValidationResult> results)
        {
            var context = new ValidationContext(model, null, null);
            results = new List<ValidationResult>();
            return Validator.TryValidateObject(model, context, results, true);
        }

        /// <summary>
        /// Tests that AssignRoleVm is valid when all fields are correct
        /// </summary>
        [Fact]
        public void AssignRoleVm_ShouldBeValid_WhenAllFieldsAreCorrect()
        {
            // Arrange
            var viewModel = new AssignRoleVm
            {
                UserId = "user-id-123",
                Role = "Pilot"
            };

            // Act
            var isValid = ValidateModel(viewModel, out var results);

            // Assert
            Assert.True(isValid);
            Assert.Empty(results);
        }

        /// <summary>
        /// Tests that AssignRoleVm fails validation when UserId is missing
        /// </summary>
        [Fact]
        public void AssignRoleVm_ShouldFail_WhenUserIdIsMissing()
        {
            // Arrange
            var viewModel = new AssignRoleVm
            {
                UserId = null!,
                Role = "Pilot"
            };

            // Act
            var isValid = ValidateModel(viewModel, out var results);

            // Assert
            Assert.False(isValid);
            Assert.Contains(results, r => r.MemberNames.Contains(nameof(AssignRoleVm.UserId)));
        }

        /// <summary>
        /// Tests that AssignRoleVm fails validation when Role is missing
        /// </summary>
        [Fact]
        public void AssignRoleVm_ShouldFail_WhenRoleIsMissing()
        {
            // Arrange
            var viewModel = new AssignRoleVm
            {
                UserId = "user-id-123",
                Role = null!
            };

            // Act
            var isValid = ValidateModel(viewModel, out var results);

            // Assert
            Assert.False(isValid);
            Assert.Contains(results, r => r.MemberNames.Contains(nameof(AssignRoleVm.Role)));
        }

        /// <summary>
        /// Tests that AssignRoleVm properties can be modified after instantiation
        /// </summary>
        [Fact]
        public void AssignRoleVm_ShouldAllowPropertyModification_AfterInstantiation()
        {
            // Arrange
            var viewModel = new AssignRoleVm
            {
                UserId = "initial-user-id",
                Role = "Pilot"
            };

            // Act
            viewModel.UserId = "updated-user-id";
            viewModel.Role = "Registerfører";

            // Assert
            Assert.Equal("updated-user-id", viewModel.UserId);
            Assert.Equal("Registerfører", viewModel.Role);
        }

        /// <summary>
        /// Tests that AssignRoleVm can accept different role values
        /// </summary>
        [Theory]
        [InlineData("Pilot")]
        [InlineData("Registerfører")]
        [InlineData("Admin")]
        public void AssignRoleVm_ShouldAccept_DifferentRoleValues(string role)
        {
            // Arrange & Act
            var viewModel = new AssignRoleVm
            {
                UserId = "user-id-123",
                Role = role
            };

            // Assert
            Assert.Equal(role, viewModel.Role);
        }
    }
}

