using FirstWebApplication.Models;

namespace Kartverket.Tests.Models
{
    public class ApplicationUserTests
    {
        [Fact]
        public void ApplicationUser_ShouldHaveDefaultApprovalAsFalse()
        {
            // Arrange & Act
            var user = new ApplicationUser();

            // Assert
            Assert.False(user.IaApproved);
        }

        [Fact]
        public void ApplicationUser_ShouldSetAllProperties()
        {
            // Arrange & Act
            var user = new ApplicationUser
            {
                UserName = "testuser",
                DesiredRole = "Pilot",
                IaApproved = true,
                FullName = "Test User",
                Email = "test@test.com",
                Organization = "Kartverket"
            };

            // Assert
            Assert.Equal("testuser", user.UserName);
            Assert.Equal("Pilot", user.DesiredRole);
            Assert.True(user.IaApproved);
            Assert.Equal("Test User", user.FullName);
            Assert.Equal("test@test.com", user.Email);
            Assert.Equal("Kartverket", user.Organization);
        }

        [Fact]
        public void ApplicationUser_ShouldAllowNullOptionalProperties()
        {
            // Arrange & Act
            var user = new ApplicationUser
            {
                UserName = "testuser"
            };

            // Assert
            Assert.Null(user.DesiredRole);
            Assert.Null(user.FullName);
            Assert.Null(user.Email);
            Assert.Null(user.Organization);
        }
    }
}

