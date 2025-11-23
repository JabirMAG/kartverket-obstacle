using FirstWebApplication.DataContext;
using FirstWebApplication.Models;
using FirstWebApplication.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace Kartverket.Tests.Repository
{
    /// <summary>
    /// Tests for UserRepository
    /// Verifies user query operations and ASP.NET Identity integration
    /// </summary>
    public class UserRepositoryTest
    {
        private readonly Mock<UserManager<ApplicationUser>> _userManagerMock;
        private readonly UserRepository _repository;

        /// <summary>
        /// Initializes a new instance of the UserRepositoryTest class
        /// </summary>
        public UserRepositoryTest()
        {
            var store = new Mock<IUserStore<ApplicationUser>>();
            _userManagerMock = new Mock<UserManager<ApplicationUser>>(
                store.Object, null, null, null, null, null, null, null, null);
            _repository = new UserRepository(_userManagerMock.Object);
        }

        /// <summary>
        /// Tests that Query returns queryable users collection
        /// </summary>
        [Fact]
        public void Query_ShouldReturnQueryableUsers()
        {
            // Arrange
            var users = new List<ApplicationUser>
            {
                new ApplicationUser { Id = "1", UserName = "user1", Email = "user1@test.com" },
                new ApplicationUser { Id = "2", UserName = "user2", Email = "user2@test.com" }
            }.AsQueryable();

            _userManagerMock.Setup(x => x.Users).Returns(users);

            // Act
            var result = _repository.Query();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count());
        }

        /// <summary>
        /// Tests that GetByIdAsync returns user when it exists
        /// </summary>
        [Fact]
        public async Task GetByIdAsync_ShouldReturnUser_WhenExists()
        {
            // Arrange
            var user = new ApplicationUser { Id = "1", UserName = "user1", Email = "user1@test.com" };
            _userManagerMock.Setup(x => x.FindByIdAsync("1"))
                .ReturnsAsync(user);

            // Act
            var result = await _repository.GetByIdAsync("1");

            // Assert
            Assert.NotNull(result);
            Assert.Equal("1", result.Id);
            Assert.Equal("user1", result.UserName);
        }

        /// <summary>
        /// Tests that GetByIdAsync returns null when user does not exist
        /// </summary>
        [Fact]
        public async Task GetByIdAsync_ShouldReturnNull_WhenNotExists()
        {
            // Arrange
            _userManagerMock.Setup(x => x.FindByIdAsync("999"))
                .ReturnsAsync((ApplicationUser?)null);

            // Act
            var result = await _repository.GetByIdAsync("999");

            // Assert
            Assert.Null(result);
        }

        /// <summary>
        /// Tests that GetByEmailAsync returns user when it exists
        /// </summary>
        [Fact]
        public async Task GetByEmailAsync_ShouldReturnUser_WhenExists()
        {
            // Arrange
            var user = new ApplicationUser { Id = "1", UserName = "user1", Email = "user1@test.com" };
            _userManagerMock.Setup(x => x.FindByEmailAsync("user1@test.com"))
                .ReturnsAsync(user);

            // Act
            var result = await _repository.GetByEmailAsync("user1@test.com");

            // Assert
            Assert.NotNull(result);
            Assert.Equal("user1@test.com", result.Email);
        }

        /// <summary>
        /// Tests that GetByEmailAsync returns null when user does not exist
        /// </summary>
        [Fact]
        public async Task GetByEmailAsync_ShouldReturnNull_WhenNotExists()
        {
            // Arrange
            _userManagerMock.Setup(x => x.FindByEmailAsync("nonexistent@test.com"))
                .ReturnsAsync((ApplicationUser?)null);

            // Act
            var result = await _repository.GetByEmailAsync("nonexistent@test.com");

            // Assert
            Assert.Null(result);
        }

        /// <summary>
        /// Tests that CreateAsync returns success when user is valid
        /// </summary>
        [Fact]
        public async Task CreateAsync_ShouldReturnSuccess_WhenUserIsValid()
        {
            // Arrange
            var user = new ApplicationUser { UserName = "newuser", Email = "newuser@test.com" };
            var identityResult = IdentityResult.Success;
            _userManagerMock.Setup(x => x.CreateAsync(user, "Password123!"))
                .ReturnsAsync(identityResult);

            // Act
            var result = await _repository.CreateAsync(user, "Password123!");

            // Assert
            Assert.True(result.Succeeded);
            _userManagerMock.Verify(x => x.CreateAsync(user, "Password123!"), Times.Once);
        }

        /// <summary>
        /// Tests that CreateAsync returns failure when user is invalid
        /// </summary>
        [Fact]
        public async Task CreateAsync_ShouldReturnFailure_WhenUserIsInvalid()
        {
            // Arrange
            var user = new ApplicationUser { UserName = "newuser", Email = "newuser@test.com" };
            var identityResult = IdentityResult.Failed(new IdentityError { Description = "Password too weak" });
            _userManagerMock.Setup(x => x.CreateAsync(user, "weak"))
                .ReturnsAsync(identityResult);

            // Act
            var result = await _repository.CreateAsync(user, "weak");

            // Assert
            Assert.False(result.Succeeded);
            Assert.Contains(result.Errors, e => e.Description == "Password too weak");
        }

        /// <summary>
        /// Tests that UpdateAsync returns success when user is valid
        /// </summary>
        [Fact]
        public async Task UpdateAsync_ShouldReturnSuccess_WhenUserIsValid()
        {
            // Arrange
            var user = new ApplicationUser { Id = "1", UserName = "user1", Email = "updated@test.com" };
            var identityResult = IdentityResult.Success;
            _userManagerMock.Setup(x => x.UpdateAsync(user))
                .ReturnsAsync(identityResult);

            // Act
            var result = await _repository.UpdateAsync(user);

            // Assert
            Assert.True(result.Succeeded);
            _userManagerMock.Verify(x => x.UpdateAsync(user), Times.Once);
        }

        /// <summary>
        /// Tests that DeleteAsync returns success when user exists
        /// </summary>
        [Fact]
        public async Task DeleteAsync_ShouldReturnSuccess_WhenUserExists()
        {
            // Arrange
            var user = new ApplicationUser { Id = "1", UserName = "user1", Email = "user1@test.com" };
            var identityResult = IdentityResult.Success;
            _userManagerMock.Setup(x => x.DeleteAsync(user))
                .ReturnsAsync(identityResult);

            // Act
            var result = await _repository.DeleteAsync(user);

            // Assert
            Assert.True(result.Succeeded);
            _userManagerMock.Verify(x => x.DeleteAsync(user), Times.Once);
        }
    }
}

