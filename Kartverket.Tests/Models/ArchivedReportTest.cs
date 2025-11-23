using FirstWebApplication.Models;
using System.ComponentModel.DataAnnotations;

namespace Kartverket.Tests.Models
{
    /// <summary>
    /// Tests for ArchivedReport domain model
    /// </summary>
    public class ArchivedReportTest
    {
        private bool ValidateModel(object model, out List<ValidationResult> results)
        {
            var context = new ValidationContext(model, null, null);
            results = new List<ValidationResult>();
            return Validator.TryValidateObject(model, context, results, true);
        }

        /// <summary>
        /// Tests that ArchivedReport can be instantiated with default values
        /// </summary>
        [Fact]
        public void ArchivedReport_ShouldBeInstantiable_WithDefaultValues()
        {
            // Arrange & Act
            var archivedReport = new ArchivedReport();

            // Assert
            Assert.NotNull(archivedReport);
            Assert.Equal(0, archivedReport.ArchivedReportId);
            Assert.Equal(0, archivedReport.OriginalObstacleId);
            Assert.Equal(string.Empty, archivedReport.ObstacleName);
            Assert.Equal(0, archivedReport.ObstacleHeight);
            Assert.Equal(string.Empty, archivedReport.ObstacleDescription);
            Assert.Equal(string.Empty, archivedReport.GeometryGeoJson);
            Assert.Equal(3, archivedReport.ObstacleStatus); // Default status is 3 (Rejected)
            Assert.Equal("[]", archivedReport.RapportComments);
        }

        /// <summary>
        /// Tests that ArchivedReport is valid when all fields are correct
        /// </summary>
        [Fact]
        public void ArchivedReport_ShouldBeValid_WhenAllFieldsAreCorrect()
        {
            // Arrange
            var archivedReport = new ArchivedReport
            {
                ArchivedReportId = 1,
                OriginalObstacleId = 10,
                ObstacleName = "Test Obstacle",
                ObstacleHeight = 50.5,
                ObstacleDescription = "Test description",
                GeometryGeoJson = "{\"type\":\"Point\",\"coordinates\":[10.0,59.0]}",
                ObstacleStatus = 3,
                ArchivedDate = DateTime.UtcNow,
                RapportComments = "[\"Comment 1\", \"Comment 2\"]"
            };

            // Act
            var isValid = ValidateModel(archivedReport, out var results);

            // Assert
            Assert.True(isValid);
            Assert.Empty(results);
        }

        /// <summary>
        /// Tests that ArchivedReport fails validation when GeometryGeoJson is missing
        /// </summary>
        [Fact]
        public void ArchivedReport_ShouldFail_WhenGeometryGeoJsonIsMissing()
        {
            // Arrange
            var archivedReport = new ArchivedReport
            {
                ArchivedReportId = 1,
                OriginalObstacleId = 10,
                ObstacleName = "Test Obstacle",
                ObstacleHeight = 50.5,
                ObstacleDescription = "Test description",
                GeometryGeoJson = string.Empty, // Required field
                ObstacleStatus = 3
            };

            // Act
            var isValid = ValidateModel(archivedReport, out var results);

            // Assert
            Assert.False(isValid);
            Assert.Contains(results, r => r.MemberNames.Contains(nameof(ArchivedReport.GeometryGeoJson)));
        }

        /// <summary>
        /// Tests that ArchivedReport fails validation when ObstacleName exceeds max length
        /// </summary>
        [Fact]
        public void ArchivedReport_ShouldFail_WhenObstacleNameExceedsMaxLength()
        {
            // Arrange
            var archivedReport = new ArchivedReport
            {
                ArchivedReportId = 1,
                OriginalObstacleId = 10,
                ObstacleName = new string('A', 101), // Max length is 100
                ObstacleHeight = 50.5,
                ObstacleDescription = "Test description",
                GeometryGeoJson = "{\"type\":\"Point\",\"coordinates\":[10.0,59.0]}",
                ObstacleStatus = 3
            };

            // Act
            var isValid = ValidateModel(archivedReport, out var results);

            // Assert
            Assert.False(isValid);
            Assert.Contains(results, r => r.MemberNames.Contains(nameof(ArchivedReport.ObstacleName)));
        }

        /// <summary>
        /// Tests that ArchivedReport fails validation when ObstacleDescription exceeds max length
        /// </summary>
        [Fact]
        public void ArchivedReport_ShouldFail_WhenObstacleDescriptionExceedsMaxLength()
        {
            // Arrange
            var archivedReport = new ArchivedReport
            {
                ArchivedReportId = 1,
                OriginalObstacleId = 10,
                ObstacleName = "Test Obstacle",
                ObstacleHeight = 50.5,
                ObstacleDescription = new string('A', 1001), // Max length is 1000
                GeometryGeoJson = "{\"type\":\"Point\",\"coordinates\":[10.0,59.0]}",
                ObstacleStatus = 3
            };

            // Act
            var isValid = ValidateModel(archivedReport, out var results);

            // Assert
            Assert.False(isValid);
            Assert.Contains(results, r => r.MemberNames.Contains(nameof(ArchivedReport.ObstacleDescription)));
        }

        /// <summary>
        /// Tests that ArchivedReport fails validation when ObstacleHeight is negative
        /// </summary>
        [Fact]
        public void ArchivedReport_ShouldFail_WhenObstacleHeightIsNegative()
        {
            // Arrange
            var archivedReport = new ArchivedReport
            {
                ArchivedReportId = 1,
                OriginalObstacleId = 10,
                ObstacleName = "Test Obstacle",
                ObstacleHeight = -10, // Below min value (0)
                ObstacleDescription = "Test description",
                GeometryGeoJson = "{\"type\":\"Point\",\"coordinates\":[10.0,59.0]}",
                ObstacleStatus = 3
            };

            // Act
            var isValid = ValidateModel(archivedReport, out var results);

            // Assert
            Assert.False(isValid);
            Assert.Contains(results, r => r.MemberNames.Contains(nameof(ArchivedReport.ObstacleHeight)));
        }

        /// <summary>
        /// Tests that ArchivedReport fails validation when ObstacleHeight exceeds max value
        /// </summary>
        [Fact]
        public void ArchivedReport_ShouldFail_WhenObstacleHeightExceedsMaxValue()
        {
            // Arrange
            var archivedReport = new ArchivedReport
            {
                ArchivedReportId = 1,
                OriginalObstacleId = 10,
                ObstacleName = "Test Obstacle",
                ObstacleHeight = 250, // Over max value (200)
                ObstacleDescription = "Test description",
                GeometryGeoJson = "{\"type\":\"Point\",\"coordinates\":[10.0,59.0]}",
                ObstacleStatus = 3
            };

            // Act
            var isValid = ValidateModel(archivedReport, out var results);

            // Assert
            Assert.False(isValid);
            Assert.Contains(results, r => r.MemberNames.Contains(nameof(ArchivedReport.ObstacleHeight)));
        }

        /// <summary>
        /// Tests that ArchivedReport is valid when ObstacleHeight is at minimum value (0)
        /// </summary>
        [Fact]
        public void ArchivedReport_ShouldBeValid_WhenObstacleHeightIsMinimum()
        {
            // Arrange
            var archivedReport = new ArchivedReport
            {
                ArchivedReportId = 1,
                OriginalObstacleId = 10,
                ObstacleName = "Test Obstacle",
                ObstacleHeight = 0, // Valid minimum
                ObstacleDescription = "Test description",
                GeometryGeoJson = "{\"type\":\"Point\",\"coordinates\":[10.0,59.0]}",
                ObstacleStatus = 3
            };

            // Act
            var isValid = ValidateModel(archivedReport, out var results);

            // Assert
            Assert.True(isValid);
            Assert.Empty(results);
        }

        /// <summary>
        /// Tests that ArchivedReport is valid when ObstacleHeight is at maximum value (200)
        /// </summary>
        [Fact]
        public void ArchivedReport_ShouldBeValid_WhenObstacleHeightIsMaximum()
        {
            // Arrange
            var archivedReport = new ArchivedReport
            {
                ArchivedReportId = 1,
                OriginalObstacleId = 10,
                ObstacleName = "Test Obstacle",
                ObstacleHeight = 200, // Valid maximum
                ObstacleDescription = "Test description",
                GeometryGeoJson = "{\"type\":\"Point\",\"coordinates\":[10.0,59.0]}",
                ObstacleStatus = 3
            };

            // Act
            var isValid = ValidateModel(archivedReport, out var results);

            // Assert
            Assert.True(isValid);
            Assert.Empty(results);
        }

        /// <summary>
        /// Tests that ArchivedReport is valid when ObstacleName is at maximum length
        /// </summary>
        [Fact]
        public void ArchivedReport_ShouldBeValid_WhenObstacleNameIsMaxLength()
        {
            // Arrange
            var archivedReport = new ArchivedReport
            {
                ArchivedReportId = 1,
                OriginalObstacleId = 10,
                ObstacleName = new string('A', 100), // Exactly max length
                ObstacleHeight = 50.5,
                ObstacleDescription = "Test description",
                GeometryGeoJson = "{\"type\":\"Point\",\"coordinates\":[10.0,59.0]}",
                ObstacleStatus = 3
            };

            // Act
            var isValid = ValidateModel(archivedReport, out var results);

            // Assert
            Assert.True(isValid);
            Assert.Empty(results);
        }

        /// <summary>
        /// Tests that ArchivedReport is valid when ObstacleDescription is at maximum length
        /// </summary>
        [Fact]
        public void ArchivedReport_ShouldBeValid_WhenObstacleDescriptionIsMaxLength()
        {
            // Arrange
            var archivedReport = new ArchivedReport
            {
                ArchivedReportId = 1,
                OriginalObstacleId = 10,
                ObstacleName = "Test Obstacle",
                ObstacleHeight = 50.5,
                ObstacleDescription = new string('A', 1000), // Exactly max length
                GeometryGeoJson = "{\"type\":\"Point\",\"coordinates\":[10.0,59.0]}",
                ObstacleStatus = 3
            };

            // Act
            var isValid = ValidateModel(archivedReport, out var results);

            // Assert
            Assert.True(isValid);
            Assert.Empty(results);
        }

        /// <summary>
        /// Tests that ArchivedReport has default ObstacleStatus of 3 (Rejected)
        /// </summary>
        [Fact]
        public void ArchivedReport_ShouldHaveDefaultStatus_OfRejected()
        {
            // Arrange & Act
            var archivedReport = new ArchivedReport();

            // Assert
            Assert.Equal(3, archivedReport.ObstacleStatus);
        }

        /// <summary>
        /// Tests that ArchivedReport can store JSON array in RapportComments
        /// </summary>
        [Fact]
        public void ArchivedReport_ShouldStoreJsonArray_InRapportComments()
        {
            // Arrange
            var jsonArray = "[\"Comment 1\", \"Comment 2\", \"Comment 3\"]";

            // Act
            var archivedReport = new ArchivedReport
            {
                ArchivedReportId = 1,
                OriginalObstacleId = 10,
                ObstacleName = "Test Obstacle",
                ObstacleHeight = 50.5,
                ObstacleDescription = "Test description",
                GeometryGeoJson = "{\"type\":\"Point\",\"coordinates\":[10.0,59.0]}",
                ObstacleStatus = 3,
                RapportComments = jsonArray
            };

            // Assert
            Assert.Equal(jsonArray, archivedReport.RapportComments);
        }

        /// <summary>
        /// Tests that ArchivedReport ArchivedDate is set to current UTC time by default
        /// </summary>
        [Fact]
        public void ArchivedReport_ShouldSetArchivedDate_ToCurrentUtcTime()
        {
            // Arrange
            var beforeCreation = DateTime.UtcNow;

            // Act
            var archivedReport = new ArchivedReport();
            var afterCreation = DateTime.UtcNow;

            // Assert
            Assert.True(archivedReport.ArchivedDate >= beforeCreation);
            Assert.True(archivedReport.ArchivedDate <= afterCreation);
        }
    }
}

