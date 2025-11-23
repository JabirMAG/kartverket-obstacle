using FirstWebApplication.Models;
using Microsoft.AspNetCore.Identity;

namespace Kartverket.Tests.Models
{
    /// <summary>
    /// Tests for ApplicationUser domain model
    /// </summary>
    public class ApplicationUserTest
    {
        /// <summary>
        /// Tests that ApplicationUser can be instantiated with default values
        /// </summary>
        [Fact]
        public void ApplicationUser_ShouldBeInstantiable_WithDefaultValues()
        {
            // Arrange & Act
            var user = new ApplicationUser();

            // Assert
            Assert.NotNull(user);
            Assert.Null(user.DesiredRole);
            Assert.False(user.IaApproved); // Default should be false
            Assert.Null(user.FullName);
            Assert.Null(user.Email);
            Assert.Null(user.Organization);
        }

        /// <summary>
        /// Tests that ApplicationUser has default IaApproved value of false
        /// </summary>
        [Fact]
        public void ApplicationUser_ShouldHaveDefault_IaApprovedAsFalse()
        {
            // Arrange & Act
            var user = new ApplicationUser();

            // Assert
            Assert.False(user.IaApproved);
        }

        /// <summary>
        /// Tests that ApplicationUser can be instantiated with all custom properties set
        /// </summary>
        [Fact]
        public void ApplicationUser_ShouldBeInstantiable_WithAllCustomPropertiesSet()
        {
            // Arrange
            var desiredRole = "Pilot";
            var isApproved = true;
            var fullName = "John Doe";
            var email = "john.doe@example.com";
            var organization = "Kartverket";

            // Act
            var user = new ApplicationUser
            {
                DesiredRole = desiredRole,
                IaApproved = isApproved,
                FullName = fullName,
                Email = email,
                Organization = organization
            };

            // Assert
            Assert.NotNull(user);
            Assert.Equal(desiredRole, user.DesiredRole);
            Assert.Equal(isApproved, user.IaApproved);
            Assert.Equal(fullName, user.FullName);
            Assert.Equal(email, user.Email);
            Assert.Equal(organization, user.Organization);
        }

        /// <summary>
        /// Tests that ApplicationUser properties can be modified after instantiation
        /// </summary>
        [Fact]
        public void ApplicationUser_ShouldAllowPropertyModification_AfterInstantiation()
        {
            // Arrange
            var user = new ApplicationUser
            {
                DesiredRole = "Pilot",
                IaApproved = false,
                FullName = "Initial Name",
                Email = "initial@example.com",
                Organization = "Kartverket"
            };

            // Act
            user.DesiredRole = "Registerfører";
            user.IaApproved = true;
            user.FullName = "Updated Name";
            user.Email = "updated@example.com";
            user.Organization = "Politi";

            // Assert
            Assert.Equal("Registerfører", user.DesiredRole);
            Assert.True(user.IaApproved);
            Assert.Equal("Updated Name", user.FullName);
            Assert.Equal("updated@example.com", user.Email);
            Assert.Equal("Politi", user.Organization);
        }

        /// <summary>
        /// Tests that ApplicationUser can handle null values for nullable properties
        /// </summary>
        [Fact]
        public void ApplicationUser_ShouldHandleNullValues_ForNullableProperties()
        {
            // Arrange & Act
            var user = new ApplicationUser
            {
                DesiredRole = null,
                FullName = null,
                Email = null,
                Organization = null
            };

            // Assert
            Assert.Null(user.DesiredRole);
            Assert.Null(user.FullName);
            Assert.Null(user.Email);
            Assert.Null(user.Organization);
        }

        /// <summary>
        /// Tests that ApplicationUser can have null desired role
        /// </summary>
        [Fact]
        public void ApplicationUser_ShouldAccept_NullDesiredRole()
        {
            // Arrange & Act
            var user = new ApplicationUser
            {
                DesiredRole = null
            };

            // Assert
            Assert.Null(user.DesiredRole);
        }

        /// <summary>
        /// Tests that ApplicationUser can have null organization
        /// </summary>
        [Fact]
        public void ApplicationUser_ShouldAccept_NullOrganization()
        {
            // Arrange & Act
            var user = new ApplicationUser
            {
                Organization = null
            };

            // Assert
            Assert.Null(user.Organization);
        }

        /// <summary>
        /// Tests that ApplicationUser can handle empty string values
        /// </summary>
        [Fact]
        public void ApplicationUser_ShouldHandleEmptyStringValues()
        {
            // Arrange & Act
            var user = new ApplicationUser
            {
                DesiredRole = string.Empty,
                FullName = string.Empty,
                Email = string.Empty,
                Organization = string.Empty
            };

            // Assert
            Assert.Equal(string.Empty, user.DesiredRole);
            Assert.Equal(string.Empty, user.FullName);
            Assert.Equal(string.Empty, user.Email);
            Assert.Equal(string.Empty, user.Organization);
        }

        /// <summary>
        /// Tests that ApplicationUser inherits from IdentityUser
        /// </summary>
        [Fact]
        public void ApplicationUser_ShouldInherit_FromIdentityUser()
        {
            // Arrange & Act
            var user = new ApplicationUser();

            // Assert
            Assert.IsAssignableFrom<IdentityUser>(user);
        }

        /// <summary>
        /// Tests that ApplicationUser can use IdentityUser properties
        /// </summary>
        [Fact]
        public void ApplicationUser_ShouldUse_IdentityUserProperties()
        {
            // Arrange
            var userName = "testuser";
            var userId = "user-id-123";

            // Act
            var user = new ApplicationUser
            {
                UserName = userName,
                Id = userId
            };

            // Assert
            Assert.Equal(userName, user.UserName);
            Assert.Equal(userId, user.Id);
        }

        /// <summary>
        /// Tests that ApplicationUser can have different desired roles
        /// </summary>
        [Theory]
        [InlineData("Pilot")]
        [InlineData("Registerfører")]
        [InlineData("Admin")]
        [InlineData("")]
        public void ApplicationUser_ShouldAccept_DifferentDesiredRoles(string desiredRole)
        {
            // Arrange & Act
            var user = new ApplicationUser
            {
                DesiredRole = desiredRole
            };

            // Assert
            Assert.Equal(desiredRole, user.DesiredRole);
        }

        /// <summary>
        /// Tests that ApplicationUser can have different organizations
        /// </summary>
        [Theory]
        [InlineData("Kartverket")]
        [InlineData("Politi")]
        [InlineData("Ambulanse")]
        [InlineData("")]
        public void ApplicationUser_ShouldAccept_DifferentOrganizations(string organization)
        {
            // Arrange & Act
            var user = new ApplicationUser
            {
                Organization = organization
            };

            // Assert
            Assert.Equal(organization, user.Organization);
        }

        /// <summary>
        /// Tests that ApplicationUser approval status can be toggled
        /// </summary>
        [Fact]
        public void ApplicationUser_ApprovalStatus_ShouldBeToggleable()
        {
            // Arrange
            var user = new ApplicationUser { IaApproved = false };

            // Act - Approve
            user.IaApproved = true;

            // Assert
            Assert.True(user.IaApproved);

            // Act - Reject
            user.IaApproved = false;

            // Assert
            Assert.False(user.IaApproved);
        }
    }
}

