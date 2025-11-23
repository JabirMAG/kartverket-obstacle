using FirstWebApplication.Models.AdminViewModels;

namespace Kartverket.Tests.Models.ViewModel
{
    /// <summary>
    /// Tests for UserWithRolesVm
    /// </summary>
    public class UserWithRolesVmTest
    {
        /// <summary>
        /// Tests that UserWithRolesVm can be instantiated with default values
        /// </summary>
        [Fact]
        public void UserWithRolesVm_ShouldBeInstantiable_WithDefaultValues()
        {
            // Arrange & Act
            var viewModel = new UserWithRolesVm();

            // Assert
            Assert.NotNull(viewModel);
            Assert.Null(viewModel.Id);
            Assert.Null(viewModel.Email);
            Assert.Null(viewModel.DisplayName);
            Assert.NotNull(viewModel.Roles);
            Assert.Empty(viewModel.Roles);
            Assert.False(viewModel.IsApproved);
            Assert.Null(viewModel.DesiredRole);
            Assert.Null(viewModel.Organization);
        }

        /// <summary>
        /// Tests that UserWithRolesVm can be instantiated with all properties set
        /// </summary>
        [Fact]
        public void UserWithRolesVm_ShouldBeInstantiable_WithAllPropertiesSet()
        {
            // Arrange
            var roles = new List<string> { "Pilot", "Admin" };

            // Act
            var viewModel = new UserWithRolesVm
            {
                Id = "user-id-123",
                Email = "test@example.com",
                DisplayName = "Test User",
                Roles = roles,
                IsApproved = true,
                DesiredRole = "Pilot",
                Organization = "Kartverket"
            };

            // Assert
            Assert.NotNull(viewModel);
            Assert.Equal("user-id-123", viewModel.Id);
            Assert.Equal("test@example.com", viewModel.Email);
            Assert.Equal("Test User", viewModel.DisplayName);
            Assert.Equal(2, viewModel.Roles.Count);
            Assert.True(viewModel.IsApproved);
            Assert.Equal("Pilot", viewModel.DesiredRole);
            Assert.Equal("Kartverket", viewModel.Organization);
        }

        /// <summary>
        /// Tests that UserWithRolesVm properties can be modified after instantiation
        /// </summary>
        [Fact]
        public void UserWithRolesVm_ShouldAllowPropertyModification_AfterInstantiation()
        {
            // Arrange
            var viewModel = new UserWithRolesVm
            {
                Id = "initial-id",
                Email = "initial@example.com",
                DisplayName = "Initial Name",
                Roles = new List<string> { "Pilot" },
                IsApproved = false,
                DesiredRole = "Pilot",
                Organization = "Kartverket"
            };

            // Act
            viewModel.Id = "updated-id";
            viewModel.Email = "updated@example.com";
            viewModel.DisplayName = "Updated Name";
            viewModel.Roles = new List<string> { "Admin" };
            viewModel.IsApproved = true;
            viewModel.DesiredRole = "Registerfører";
            viewModel.Organization = "Politi";

            // Assert
            Assert.Equal("updated-id", viewModel.Id);
            Assert.Equal("updated@example.com", viewModel.Email);
            Assert.Equal("Updated Name", viewModel.DisplayName);
            Assert.Single(viewModel.Roles);
            Assert.Equal("Admin", viewModel.Roles[0]);
            Assert.True(viewModel.IsApproved);
            Assert.Equal("Registerfører", viewModel.DesiredRole);
            Assert.Equal("Politi", viewModel.Organization);
        }

        /// <summary>
        /// Tests that UserWithRolesVm can handle multiple roles
        /// </summary>
        [Fact]
        public void UserWithRolesVm_ShouldHandle_MultipleRoles()
        {
            // Arrange
            var roles = new List<string> { "Pilot", "Registerfører", "Admin" };

            // Act
            var viewModel = new UserWithRolesVm
            {
                Id = "user-id",
                Email = "test@example.com",
                Roles = roles
            };

            // Assert
            Assert.Equal(3, viewModel.Roles.Count);
            Assert.Contains("Pilot", viewModel.Roles);
            Assert.Contains("Registerfører", viewModel.Roles);
            Assert.Contains("Admin", viewModel.Roles);
        }

        /// <summary>
        /// Tests that UserWithRolesVm can handle null values for nullable properties
        /// </summary>
        [Fact]
        public void UserWithRolesVm_ShouldHandle_NullValuesForNullableProperties()
        {
            // Arrange & Act
            var viewModel = new UserWithRolesVm
            {
                Id = "user-id",
                Email = "test@example.com",
                DisplayName = null,
                DesiredRole = null,
                Organization = null
            };

            // Assert
            Assert.Null(viewModel.DisplayName);
            Assert.Null(viewModel.DesiredRole);
            Assert.Null(viewModel.Organization);
        }
    }
}

