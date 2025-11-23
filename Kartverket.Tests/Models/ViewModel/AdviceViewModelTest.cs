using FirstWebApplication.Models.ViewModel;

namespace Kartverket.Tests.Models.ViewModel
{
    /// <summary>
    /// Tests for AdviceViewModel
    /// </summary>
    public class AdviceViewModelTest
    {
        /// <summary>
        /// Tests that AdviceViewModel can be instantiated with default values
        /// </summary>
        [Fact]
        public void AdviceViewModel_ShouldBeInstantiable_WithDefaultValues()
        {
            // Arrange & Act
            var viewModel = new AdviceViewModel();

            // Assert
            Assert.NotNull(viewModel);
            Assert.Equal(0, viewModel.ViewadviceID);
            Assert.Null(viewModel.ViewadviceMessage);
            Assert.Null(viewModel.ViewEmail);
        }

        /// <summary>
        /// Tests that AdviceViewModel can be instantiated with all properties set
        /// </summary>
        [Fact]
        public void AdviceViewModel_ShouldBeInstantiable_WithAllPropertiesSet()
        {
            // Arrange
            var id = 1;
            var message = "Test feedback message";
            var email = "test@example.com";

            // Act
            var viewModel = new AdviceViewModel
            {
                ViewadviceID = id,
                ViewadviceMessage = message,
                ViewEmail = email
            };

            // Assert
            Assert.NotNull(viewModel);
            Assert.Equal(id, viewModel.ViewadviceID);
            Assert.Equal(message, viewModel.ViewadviceMessage);
            Assert.Equal(email, viewModel.ViewEmail);
        }

        /// <summary>
        /// Tests that AdviceViewModel properties can be modified after instantiation
        /// </summary>
        [Fact]
        public void AdviceViewModel_ShouldAllowPropertyModification_AfterInstantiation()
        {
            // Arrange
            var viewModel = new AdviceViewModel
            {
                ViewadviceID = 1,
                ViewadviceMessage = "Initial message",
                ViewEmail = "initial@example.com"
            };

            // Act
            viewModel.ViewadviceID = 2;
            viewModel.ViewadviceMessage = "Updated message";
            viewModel.ViewEmail = "updated@example.com";

            // Assert
            Assert.Equal(2, viewModel.ViewadviceID);
            Assert.Equal("Updated message", viewModel.ViewadviceMessage);
            Assert.Equal("updated@example.com", viewModel.ViewEmail);
        }

        /// <summary>
        /// Tests that AdviceViewModel can handle null values for string properties
        /// </summary>
        [Fact]
        public void AdviceViewModel_ShouldHandleNullValues_ForStringProperties()
        {
            // Arrange & Act
            var viewModel = new AdviceViewModel
            {
                ViewadviceID = 1,
                ViewadviceMessage = null,
                ViewEmail = null
            };

            // Assert
            Assert.Null(viewModel.ViewadviceMessage);
            Assert.Null(viewModel.ViewEmail);
        }

        /// <summary>
        /// Tests that AdviceViewModel can handle empty string values
        /// </summary>
        [Fact]
        public void AdviceViewModel_ShouldHandleEmptyStringValues()
        {
            // Arrange & Act
            var viewModel = new AdviceViewModel
            {
                ViewadviceID = 1,
                ViewadviceMessage = string.Empty,
                ViewEmail = string.Empty
            };

            // Assert
            Assert.Equal(string.Empty, viewModel.ViewadviceMessage);
            Assert.Equal(string.Empty, viewModel.ViewEmail);
        }
    }
}

