using FirstWebApplication.DataContext;
using FirstWebApplication.Models;
using FirstWebApplication.Repositories;
using Kartverket.Tests.Helpers;
using Microsoft.EntityFrameworkCore;

namespace Kartverket.Tests.Repository
{
    /// <summary>
    /// Tests for AdviceRepository
    /// Verifies CRUD operations, ordering, and data limiting
    /// </summary>
    public class AdviceRepositoryTest : IDisposable
    {
        private readonly ApplicationDBContext _context;
        private readonly AdviceRepository _repository;

        /// <summary>
        /// Initializes a new instance of the AdviceRepositoryTest class
        /// </summary>
        public AdviceRepositoryTest()
        {
            _context = DatabaseFixture.CreateContext();
            _repository = new AdviceRepository(_context);
        }

        /// <summary>
        /// Tests that AddAdvice adds advice to database and returns advice with generated ID
        /// </summary>
        [Fact]
        public async Task AddAdvice_ShouldAddToDatabase_AndReturnAdviceWithId()
        {
            // Arrange
            var advice = TestDataBuilder.CreateValidAdvice();

            // Act
            var result = await _repository.AddAdvice(advice);

            // Assert
            Assert.True(result.adviceID > 0);
            var savedAdvice = await _context.Feedback.FindAsync(result.adviceID);
            Assert.NotNull(savedAdvice);
            Assert.Equal(advice.Email, savedAdvice.Email);
            Assert.Equal(advice.adviceMessage, savedAdvice.adviceMessage);
        }

        /// <summary>
        /// Tests that GetElementById returns advice when it exists
        /// </summary>
        [Fact]
        public async Task GetElementById_ShouldReturnAdvice_WhenExists()
        {
            // Arrange
            var advice = TestDataBuilder.CreateValidAdvice();
            await _context.Feedback.AddAsync(advice);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetElementById(advice.adviceID);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(advice.adviceID, result.adviceID);
            Assert.Equal(advice.Email, result.Email);
        }

        /// <summary>
        /// Tests that GetElementById returns null when advice does not exist
        /// </summary>
        [Fact]
        public async Task GetElementById_ShouldReturnNull_WhenNotExists()
        {
            // Act
            var result = await _repository.GetElementById(999);

            // Assert
            Assert.Null(result);
        }

        /// <summary>
        /// Tests that GetAllAdvice returns advice ordered by ID in descending order
        /// </summary>
        [Fact]
        public async Task GetAllAdvice_ShouldReturnAdvice_OrderedByDescendingId()
        {
            // Arrange
            var advice1 = TestDataBuilder.CreateValidAdvice("test1@example.com", "Message 1");
            var advice2 = TestDataBuilder.CreateValidAdvice("test2@example.com", "Message 2");
            await _context.Feedback.AddRangeAsync(advice1, advice2);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetAllAdvice();

            // Assert
            Assert.NotNull(result);
            var adviceList = result.ToList();
            Assert.True(adviceList.Count >= 2);
            // Verify ordering is descending by ID
            for (int i = 0; i < adviceList.Count - 1; i++)
            {
                Assert.True(adviceList[i].adviceID >= adviceList[i + 1].adviceID);
            }
        }

        /// <summary>
        /// Tests that GetAllAdvice returns maximum 50 advice entries (filters/limits results)
        /// </summary>
        [Fact]
        public async Task GetAllAdvice_ShouldReturnMax50Advice()
        {
            // Arrange
            var adviceList = new List<Advice>();
            for (int i = 0; i < 60; i++)
            {
                adviceList.Add(TestDataBuilder.CreateValidAdvice($"test{i}@example.com", $"Message {i}"));
            }
            await _context.Feedback.AddRangeAsync(adviceList);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetAllAdvice();

            // Assert
            var resultList = result.ToList();
            Assert.True(resultList.Count <= 50);
        }

        /// <summary>
        /// Tests that UpdateAdvice updates advice in database
        /// </summary>
        [Fact]
        public async Task UpdateAdvice_ShouldUpdateAdvice()
        {
            // Arrange
            var advice = TestDataBuilder.CreateValidAdvice();
            await _context.Feedback.AddAsync(advice);
            await _context.SaveChangesAsync();
            advice.adviceMessage = "Updated message";

            // Act
            var result = await _repository.UpdateAdvice(advice);

            // Assert
            Assert.Equal("Updated message", result.adviceMessage);
            var updatedAdvice = await _context.Feedback.FindAsync(advice.adviceID);
            Assert.NotNull(updatedAdvice);
            Assert.Equal("Updated message", updatedAdvice.adviceMessage);
        }

        /// <summary>
        /// Tests that DeleteById deletes advice when it exists
        /// </summary>
        [Fact]
        public async Task DeleteById_ShouldDeleteAdvice_WhenExists()
        {
            // Arrange
            var advice = TestDataBuilder.CreateValidAdvice();
            await _context.Feedback.AddAsync(advice);
            await _context.SaveChangesAsync();
            var adviceId = advice.adviceID;

            // Act
            var result = await _repository.DeleteById(adviceId);

            // Assert
            Assert.NotNull(result);
            var deletedAdvice = await _context.Feedback.FindAsync(adviceId);
            Assert.Null(deletedAdvice);
        }

        /// <summary>
        /// Tests that DeleteById returns null when advice does not exist
        /// </summary>
        [Fact]
        public async Task DeleteById_ShouldReturnNull_WhenNotExists()
        {
            // Act
            var result = await _repository.DeleteById(999);

            // Assert
            Assert.Null(result);
        }

        /// <summary>
        /// Disposes the database context
        /// </summary>
        public void Dispose()
        {
            _context.Dispose();
        }
    }
}

