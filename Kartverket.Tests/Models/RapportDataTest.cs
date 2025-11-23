using FirstWebApplication.Models;
using System.ComponentModel.DataAnnotations;

namespace Kartverket.Tests.Models
{
    /// <summary>
    /// Tests for RapportData domain model
    /// </summary>
    public class RapportDataTest
    {
        private bool ValidateModel(object model, out List<ValidationResult> results)
        {
            var context = new ValidationContext(model, null, null);
            results = new List<ValidationResult>();
            return Validator.TryValidateObject(model, context, results, true);
        }

        /// <summary>
        /// Tests that RapportData can be instantiated with default values
        /// </summary>
        [Fact]
        public void RapportData_ShouldBeInstantiable_WithDefaultValues()
        {
            // Arrange & Act
            var rapport = new RapportData();

            // Assert
            Assert.NotNull(rapport);
            Assert.Equal(0, rapport.RapportID);
            Assert.Equal(0, rapport.ObstacleId);
            Assert.Equal(string.Empty, rapport.RapportComment);
            Assert.Null(rapport.Obstacle);
        }

        /// <summary>
        /// Tests that RapportData can be instantiated with all properties set
        /// </summary>
        [Fact]
        public void RapportData_ShouldBeInstantiable_WithAllPropertiesSet()
        {
            // Arrange
            var rapportId = 1;
            var obstacleId = 10;
            var comment = "This is a test comment";

            // Act
            var rapport = new RapportData
            {
                RapportID = rapportId,
                ObstacleId = obstacleId,
                RapportComment = comment
            };

            // Assert
            Assert.NotNull(rapport);
            Assert.Equal(rapportId, rapport.RapportID);
            Assert.Equal(obstacleId, rapport.ObstacleId);
            Assert.Equal(comment, rapport.RapportComment);
        }

        /// <summary>
        /// Tests that RapportData is valid when RapportComment is within max length limit
        /// </summary>
        [Fact]
        public void RapportData_ShouldBeValid_WhenRapportCommentIsWithinMaxLength()
        {
            // Arrange
            var rapport = new RapportData
            {
                RapportID = 1,
                ObstacleId = 10,
                RapportComment = new string('A', 1000) // Max length is 1000
            };

            // Act
            var isValid = ValidateModel(rapport, out var results);

            // Assert
            Assert.True(isValid);
            Assert.Empty(results);
        }

        /// <summary>
        /// Tests that RapportData is valid when RapportComment is exactly at max length
        /// </summary>
        [Fact]
        public void RapportData_ShouldBeValid_WhenRapportCommentIsExactlyMaxLength()
        {
            // Arrange
            var rapport = new RapportData
            {
                RapportID = 1,
                ObstacleId = 10,
                RapportComment = new string('A', 1000) // Exactly 1000 characters
            };

            // Act
            var isValid = ValidateModel(rapport, out var results);

            // Assert
            Assert.True(isValid);
            Assert.Empty(results);
            Assert.Equal(1000, rapport.RapportComment.Length);
        }

        /// <summary>
        /// Tests that RapportData is valid when RapportComment is empty string
        /// </summary>
        [Fact]
        public void RapportData_ShouldBeValid_WhenRapportCommentIsEmpty()
        {
            // Arrange
            var rapport = new RapportData
            {
                RapportID = 1,
                ObstacleId = 10,
                RapportComment = string.Empty
            };

            // Act
            var isValid = ValidateModel(rapport, out var results);

            // Assert
            Assert.True(isValid);
            Assert.Empty(results);
        }

        /// <summary>
        /// Tests that RapportData properties can be modified after instantiation
        /// </summary>
        [Fact]
        public void RapportData_ShouldAllowPropertyModification_AfterInstantiation()
        {
            // Arrange
            var rapport = new RapportData
            {
                RapportID = 1,
                ObstacleId = 10,
                RapportComment = "Initial comment"
            };

            // Act
            rapport.RapportID = 2;
            rapport.ObstacleId = 20;
            rapport.RapportComment = "Updated comment";

            // Assert
            Assert.Equal(2, rapport.RapportID);
            Assert.Equal(20, rapport.ObstacleId);
            Assert.Equal("Updated comment", rapport.RapportComment);
        }

        /// <summary>
        /// Tests that RapportData can be associated with an ObstacleData object
        /// </summary>
        [Fact]
        public void RapportData_ShouldBeAssociable_WithObstacleData()
        {
            // Arrange
            var obstacle = new ObstacleData
            {
                ObstacleId = 10,
                ObstacleName = "Test Obstacle"
            };
            var rapport = new RapportData
            {
                RapportID = 1,
                ObstacleId = 10,
                RapportComment = "Test comment"
            };

            // Act
            rapport.Obstacle = obstacle;

            // Assert
            Assert.NotNull(rapport.Obstacle);
            Assert.Equal(10, rapport.Obstacle.ObstacleId);
            Assert.Equal("Test Obstacle", rapport.Obstacle.ObstacleName);
        }
    }
}

