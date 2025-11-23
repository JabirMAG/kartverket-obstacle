using FirstWebApplication.Models.ViewModel;

namespace Kartverket.Tests.Models.ViewModel
{
    /// <summary>
    /// Tests for User ViewModel
    /// </summary>
    public class UserTest
    {
        /// <summary>
        /// Tests that User can be instantiated with default values
        /// </summary>
        [Fact]
        public void User_ShouldBeInstantiable_WithDefaultValues()
        {
            // Arrange & Act
            var user = new User();

            // Assert
            Assert.NotNull(user);
            Assert.Equal(Guid.Empty, user.Id);
            Assert.Null(user.UserName);
            Assert.Null(user.EmailAdress);
        }

        /// <summary>
        /// Tests that User can be instantiated with all properties set
        /// </summary>
        [Fact]
        public void User_ShouldBeInstantiable_WithAllPropertiesSet()
        {
            // Arrange
            var id = Guid.NewGuid();
            var userName = "testuser";
            var email = "test@example.com";

            // Act
            var user = new User
            {
                Id = id,
                UserName = userName,
                EmailAdress = email
            };

            // Assert
            Assert.NotNull(user);
            Assert.Equal(id, user.Id);
            Assert.Equal(userName, user.UserName);
            Assert.Equal(email, user.EmailAdress);
        }

        /// <summary>
        /// Tests that User properties can be modified after instantiation
        /// </summary>
        [Fact]
        public void User_ShouldAllowPropertyModification_AfterInstantiation()
        {
            // Arrange
            var user = new User
            {
                Id = Guid.NewGuid(),
                UserName = "initial",
                EmailAdress = "initial@example.com"
            };
            var newId = Guid.NewGuid();

            // Act
            user.Id = newId;
            user.UserName = "updated";
            user.EmailAdress = "updated@example.com";

            // Assert
            Assert.Equal(newId, user.Id);
            Assert.Equal("updated", user.UserName);
            Assert.Equal("updated@example.com", user.EmailAdress);
        }

        /// <summary>
        /// Tests that User can handle null values for string properties
        /// </summary>
        [Fact]
        public void User_ShouldHandle_NullValuesForStringProperties()
        {
            // Arrange & Act
            var user = new User
            {
                Id = Guid.NewGuid(),
                UserName = null,
                EmailAdress = null
            };

            // Assert
            Assert.Null(user.UserName);
            Assert.Null(user.EmailAdress);
        }

        /// <summary>
        /// Tests that User can handle empty string values
        /// </summary>
        [Fact]
        public void User_ShouldHandle_EmptyStringValues()
        {
            // Arrange & Act
            var user = new User
            {
                Id = Guid.NewGuid(),
                UserName = string.Empty,
                EmailAdress = string.Empty
            };

            // Assert
            Assert.Equal(string.Empty, user.UserName);
            Assert.Equal(string.Empty, user.EmailAdress);
        }
    }
}

