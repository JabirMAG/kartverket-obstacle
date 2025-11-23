using FirstWebApplication.Models;
using System.ComponentModel.DataAnnotations;
using Kartverket.Tests.Helpers;

namespace Kartverket.Tests.Models
{
    public class ObstacleDataTest
    {
        private bool ValidateModel(object model, out List<ValidationResult> results)
        {
            var context = new ValidationContext(model, null, null);
            results = new List<ValidationResult>();
            return Validator.TryValidateObject(model, context, results, true);
        }

        [Fact]
        public void ObstacleData_ShouldBeValid_WhenAllFieldsAreCorrect()
        {
            // Arrange
            var obstacle = TestDataBuilder.CreateValidObstacle();

            // Act
            var isValid = ValidateModel(obstacle, out var results);

            // Assert
            Assert.True(isValid);
            Assert.Empty(results);
        }

        [Fact]
        public void ObstacleData_ShouldFail_WhenGeometryGeoJsonIsMissing()
        {
            // Arrange
            var obstacle = new ObstacleData
            {
                ObstacleName = "Tre",
                ObstacleHeight = 15,
                ObstacleDescription = "Et høyt tre",
                GeometryGeoJson = string.Empty // Required field
            };

            // Act
            var isValid = ValidateModel(obstacle, out var results);

            // Assert
            Assert.False(isValid);
            Assert.Contains(results, r => r.ErrorMessage != null && r.ErrorMessage.Contains("Geometry"));
        }

        [Fact]
        public void ObstacleData_ShouldFail_WhenHeightOutOfRange_TooHigh()
        {
            // Arrange
            var obstacle = new ObstacleData
            {
                ObstacleName = "Stolpe",
                ObstacleHeight = 500, // Over max value (200)
                ObstacleDescription = "en enorm stolpe",
                GeometryGeoJson = "{\"type\":\"Point\",\"coordinates\":[10.0,59.0]}"
            };

            // Act
            var isValid = ValidateModel(obstacle, out var results);

            // Assert
            Assert.False(isValid);
            Assert.Contains(results, r => r.ErrorMessage != null && r.ErrorMessage.Contains("200"));
        }

        [Fact]
        public void ObstacleData_ShouldFail_WhenHeightOutOfRange_Negative()
        {
            // Arrange
            var obstacle = new ObstacleData
            {
                ObstacleName = "Stolpe",
                ObstacleHeight = -10, // Below min value (0)
                ObstacleDescription = "en stolpe",
                GeometryGeoJson = "{\"type\":\"Point\",\"coordinates\":[10.0,59.0]}"
            };

            // Act
            var isValid = ValidateModel(obstacle, out var results);

            // Assert
            Assert.False(isValid);
            Assert.Contains(results, r => r.ErrorMessage != null && r.ErrorMessage.Contains("0"));
        }

        [Fact]
        public void ObstacleData_ShouldBeValid_WhenHeightIsZero()
        {
            // Arrange
            var obstacle = new ObstacleData
            {
                ObstacleName = "Stolpe",
                ObstacleHeight = 0, // Valid minimum
                ObstacleDescription = "en stolpe",
                GeometryGeoJson = "{\"type\":\"Point\",\"coordinates\":[10.0,59.0]}"
            };

            // Act
            var isValid = ValidateModel(obstacle, out var results);

            // Assert
            Assert.True(isValid);
        }

        [Fact]
        public void ObstacleData_ShouldBeValid_WhenHeightIsMaxValue()
        {
            // Arrange
            var obstacle = new ObstacleData
            {
                ObstacleName = "Stolpe",
                ObstacleHeight = 200, // Valid maximum
                ObstacleDescription = "en stolpe",
                GeometryGeoJson = "{\"type\":\"Point\",\"coordinates\":[10.0,59.0]}"
            };

            // Act
            var isValid = ValidateModel(obstacle, out var results);

            // Assert
            Assert.True(isValid);
        }

        [Fact]
        public void ObstacleData_ShouldFail_WhenObstacleStatusOutOfRange()
        {
            // Arrange
            var obstacle = new ObstacleData
            {
                ObstacleName = "Stolpe",
                ObstacleHeight = 50,
                ObstacleDescription = "en stolpe",
                GeometryGeoJson = "{\"type\":\"Point\",\"coordinates\":[10.0,59.0]}",
                ObstacleStatus = 5 // Invalid status (should be 1-3)
            };

            // Act
            var isValid = ValidateModel(obstacle, out var results);

            // Assert
            Assert.False(isValid);
            Assert.Contains(results, r => r.ErrorMessage != null && r.ErrorMessage.Contains("3"));
        }

    }
}
