using FirstWebApplication.Models.ViewModel;
using System.ComponentModel.DataAnnotations;

namespace Kartverket.Tests.Models.ViewModel
{
    /// <summary>
    /// Tests for ObstacleDataViewModel
    /// </summary>
    public class ObstacleDataViewModelTest
    {
        private bool ValidateModel(object model, out List<ValidationResult> results)
        {
            var context = new ValidationContext(model, null, null);
            results = new List<ValidationResult>();
            return Validator.TryValidateObject(model, context, results, true);
        }

        /// <summary>
        /// Tests that ObstacleDataViewModel is valid when all fields are correct
        /// </summary>
        [Fact]
        public void ObstacleDataViewModel_ShouldBeValid_WhenAllFieldsAreCorrect()
        {
            // Arrange
            var viewModel = new ObstacleDataViewModel
            {
                ViewObstacleName = "Tre",
                ViewObstacleHeight = 15,
                ViewObstacleDescription = "Et høyt tre nær landingsone",
                ViewGeometryGeoJson = "{\"type\":\"Point\",\"coordinates\":[10.0,59.0]}"
            };

            // Act
            var isValid = ValidateModel(viewModel, out var results);

            // Assert
            Assert.True(isValid);
            Assert.Empty(results);
        }

        /// <summary>
        /// Tests that ObstacleDataViewModel passes validation when ViewObstacleName is empty (it's optional for quick save)
        /// </summary>
        [Fact]
        public void ObstacleDataViewModel_ShouldPass_WhenViewObstacleNameIsEmpty()
        {
            // Arrange
            var viewModel = new ObstacleDataViewModel
            {
                ViewObstacleName = "", // Optional field - empty is allowed
                ViewObstacleHeight = 50,
                ViewObstacleDescription = "Description",
                ViewGeometryGeoJson = "{\"type\":\"Point\",\"coordinates\":[10.0,59.0]}"
            };

            // Act
            var isValid = ValidateModel(viewModel, out var results);

            // Assert
            Assert.True(isValid); // ViewObstacleName is optional, only ViewGeometryGeoJson is required
        }

        /// <summary>
        /// Tests that ObstacleDataViewModel fails validation when ViewObstacleDescription is missing
        /// </summary>
        [Fact]
        public void ObstacleDataViewModel_ShouldFail_WhenViewObstacleDescriptionIsMissing()
        {
            // Arrange
            var viewModel = new ObstacleDataViewModel
            {
                ViewObstacleName = "Test Obstacle",
                ViewObstacleHeight = 50,
                ViewObstacleDescription = "", // Optional field - empty is allowed
                ViewGeometryGeoJson = "{\"type\":\"Point\",\"coordinates\":[10.0,59.0]}"
            };

            // Act
            var isValid = ValidateModel(viewModel, out var results);

            // Assert
            // ViewObstacleDescription is optional, so empty string is valid
            Assert.True(isValid);
        }

        /// <summary>
        /// Tests that ObstacleDataViewModel fails validation when ViewObstacleHeight is out of range (too high)
        /// </summary>
        [Fact]
        public void ObstacleDataViewModel_ShouldFail_WhenHeightOutOfRange_TooHigh()
        {
            // Arrange
            var viewModel = new ObstacleDataViewModel
            {
                ViewObstacleName = "Stolpe",
                ViewObstacleHeight = 500, // Over max value (200)
                ViewObstacleDescription = "en enorm stolpe",
                ViewGeometryGeoJson = "{\"type\":\"Point\",\"coordinates\":[10.0,59.0]}"
            };

            // Act
            var isValid = ValidateModel(viewModel, out var results);

            // Assert
            Assert.False(isValid);
            Assert.Contains(results, r => r.ErrorMessage != null && r.ErrorMessage.Contains("Height must be between 0 and 200"));
        }

        /// <summary>
        /// Tests that ObstacleDataViewModel fails validation when ViewObstacleHeight is negative
        /// </summary>
        [Fact]
        public void ObstacleDataViewModel_ShouldFail_WhenHeightOutOfRange_Negative()
        {
            // Arrange
            var viewModel = new ObstacleDataViewModel
            {
                ViewObstacleName = "Stolpe",
                ViewObstacleHeight = -10, // Below min value (0)
                ViewObstacleDescription = "en stolpe",
                ViewGeometryGeoJson = "{\"type\":\"Point\",\"coordinates\":[10.0,59.0]}"
            };

            // Act
            var isValid = ValidateModel(viewModel, out var results);

            // Assert
            Assert.False(isValid);
            Assert.Contains(results, r => r.MemberNames.Contains(nameof(ObstacleDataViewModel.ViewObstacleHeight)));
        }

        /// <summary>
        /// Tests that ObstacleDataViewModel is valid when ViewObstacleHeight is at minimum value (0)
        /// </summary>
        [Fact]
        public void ObstacleDataViewModel_ShouldBeValid_WhenHeightIsMinimum()
        {
            // Arrange
            var viewModel = new ObstacleDataViewModel
            {
                ViewObstacleName = "Stolpe",
                ViewObstacleHeight = 0, // Valid minimum
                ViewObstacleDescription = "en stolpe",
                ViewGeometryGeoJson = "{\"type\":\"Point\",\"coordinates\":[10.0,59.0]}"
            };

            // Act
            var isValid = ValidateModel(viewModel, out var results);

            // Assert
            Assert.True(isValid);
            Assert.Empty(results);
        }

        /// <summary>
        /// Tests that ObstacleDataViewModel is valid when ViewObstacleHeight is at maximum value (200)
        /// </summary>
        [Fact]
        public void ObstacleDataViewModel_ShouldBeValid_WhenHeightIsMaximum()
        {
            // Arrange
            var viewModel = new ObstacleDataViewModel
            {
                ViewObstacleName = "Stolpe",
                ViewObstacleHeight = 200, // Valid maximum
                ViewObstacleDescription = "en stolpe",
                ViewGeometryGeoJson = "{\"type\":\"Point\",\"coordinates\":[10.0,59.0]}"
            };

            // Act
            var isValid = ValidateModel(viewModel, out var results);

            // Assert
            Assert.True(isValid);
            Assert.Empty(results);
        }

        /// <summary>
        /// Tests that ObstacleDataViewModel fails validation when ViewObstacleName exceeds max length
        /// </summary>
        [Fact]
        public void ObstacleDataViewModel_ShouldFail_WhenViewObstacleNameExceedsMaxLength()
        {
            // Arrange
            var viewModel = new ObstacleDataViewModel
            {
                ViewObstacleName = new string('A', 101), // Max length is 100
                ViewObstacleHeight = 50,
                ViewObstacleDescription = "Description",
                ViewGeometryGeoJson = "{\"type\":\"Point\",\"coordinates\":[10.0,59.0]}"
            };

            // Act
            var isValid = ValidateModel(viewModel, out var results);

            // Assert
            Assert.False(isValid);
            Assert.Contains(results, r => r.MemberNames.Contains(nameof(ObstacleDataViewModel.ViewObstacleName)));
        }

        /// <summary>
        /// Tests that ObstacleDataViewModel fails validation when ViewObstacleDescription exceeds max length
        /// </summary>
        [Fact]
        public void ObstacleDataViewModel_ShouldFail_WhenViewObstacleDescriptionExceedsMaxLength()
        {
            // Arrange
            var viewModel = new ObstacleDataViewModel
            {
                ViewObstacleName = "Test Obstacle",
                ViewObstacleHeight = 50,
                ViewObstacleDescription = new string('A', 1001), // Max length is 1000
                ViewGeometryGeoJson = "{\"type\":\"Point\",\"coordinates\":[10.0,59.0]}"
            };

            // Act
            var isValid = ValidateModel(viewModel, out var results);

            // Assert
            Assert.False(isValid);
            Assert.Contains(results, r => r.MemberNames.Contains(nameof(ObstacleDataViewModel.ViewObstacleDescription)));
        }

        /// <summary>
        /// Tests that ObstacleDataViewModel is valid when ViewObstacleName is at max length
        /// </summary>
        [Fact]
        public void ObstacleDataViewModel_ShouldBeValid_WhenViewObstacleNameIsMaxLength()
        {
            // Arrange
            var viewModel = new ObstacleDataViewModel
            {
                ViewObstacleName = new string('A', 100), // Exactly max length
                ViewObstacleHeight = 50,
                ViewObstacleDescription = "Description",
                ViewGeometryGeoJson = "{\"type\":\"Point\",\"coordinates\":[10.0,59.0]}"
            };

            // Act
            var isValid = ValidateModel(viewModel, out var results);

            // Assert
            Assert.True(isValid);
            Assert.Empty(results);
        }

        /// <summary>
        /// Tests that ObstacleDataViewModel is valid when ViewObstacleDescription is at max length
        /// </summary>
        [Fact]
        public void ObstacleDataViewModel_ShouldBeValid_WhenViewObstacleDescriptionIsMaxLength()
        {
            // Arrange
            var viewModel = new ObstacleDataViewModel
            {
                ViewObstacleName = "Test Obstacle",
                ViewObstacleHeight = 50,
                ViewObstacleDescription = new string('A', 1000), // Exactly max length
                ViewGeometryGeoJson = "{\"type\":\"Point\",\"coordinates\":[10.0,59.0]}"
            };

            // Act
            var isValid = ValidateModel(viewModel, out var results);

            // Assert
            Assert.True(isValid);
            Assert.Empty(results);
        }

        /// <summary>
        /// Tests that ObstacleDataViewModel can be instantiated with default values
        /// </summary>
        [Fact]
        public void ObstacleDataViewModel_ShouldBeInstantiable_WithDefaultValues()
        {
            // Arrange & Act
            var viewModel = new ObstacleDataViewModel();

            // Assert
            Assert.NotNull(viewModel);
            Assert.Equal(0, viewModel.ViewObstacleId);
            Assert.Equal(string.Empty, viewModel.ViewObstacleName);
            Assert.Equal(0, viewModel.ViewObstacleHeight);
            Assert.Equal(string.Empty, viewModel.ViewObstacleDescription);
            Assert.Equal(string.Empty, viewModel.ViewGeometryGeoJson); // Default is empty string, not null
            Assert.Equal(1, viewModel.ViewObstacleStatus); // Default is 1 (Pending)
        }

        /// <summary>
        /// Tests that ObstacleDataViewModel properties can be modified after instantiation
        /// </summary>
        [Fact]
        public void ObstacleDataViewModel_ShouldAllowPropertyModification_AfterInstantiation()
        {
            // Arrange
            var viewModel = new ObstacleDataViewModel
            {
                ViewObstacleId = 1,
                ViewObstacleName = "Initial",
                ViewObstacleHeight = 10,
                ViewObstacleDescription = "Initial description",
                ViewGeometryGeoJson = "{\"type\":\"Point\",\"coordinates\":[10.0,59.0]}",
                ViewObstacleStatus = 1
            };

            // Act
            viewModel.ViewObstacleId = 2;
            viewModel.ViewObstacleName = "Updated";
            viewModel.ViewObstacleHeight = 20;
            viewModel.ViewObstacleDescription = "Updated description";
            viewModel.ViewGeometryGeoJson = "{\"type\":\"Point\",\"coordinates\":[11.0,60.0]}";
            viewModel.ViewObstacleStatus = 2;

            // Assert
            Assert.Equal(2, viewModel.ViewObstacleId);
            Assert.Equal("Updated", viewModel.ViewObstacleName);
            Assert.Equal(20, viewModel.ViewObstacleHeight);
            Assert.Equal("Updated description", viewModel.ViewObstacleDescription);
            Assert.Equal("{\"type\":\"Point\",\"coordinates\":[11.0,60.0]}", viewModel.ViewGeometryGeoJson);
            Assert.Equal(2, viewModel.ViewObstacleStatus);
        }
    }
}

