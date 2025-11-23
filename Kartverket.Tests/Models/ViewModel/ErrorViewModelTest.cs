using FirstWebApplication.Models;

namespace Kartverket.Tests.Models.ViewModel
{
    /// <summary>
    /// Tests for ErrorViewModel
    /// </summary>
    public class ErrorViewModelTest
    {
        /// <summary>
        /// Tests that ErrorViewModel can be instantiated with default values
        /// </summary>
        [Fact]
        public void ErrorViewModel_ShouldBeInstantiable_WithDefaultValues()
        {
            // Arrange & Act
            var viewModel = new ErrorViewModel();

            // Assert
            Assert.NotNull(viewModel);
            Assert.Null(viewModel.RequestId);
            Assert.False(viewModel.ShowRequestId);
        }

        /// <summary>
        /// Tests that ErrorViewModel ShowRequestId returns false when RequestId is null
        /// </summary>
        [Fact]
        public void ErrorViewModel_ShowRequestId_ShouldReturnFalse_WhenRequestIdIsNull()
        {
            // Arrange
            var viewModel = new ErrorViewModel
            {
                RequestId = null
            };

            // Act & Assert
            Assert.False(viewModel.ShowRequestId);
        }

        /// <summary>
        /// Tests that ErrorViewModel ShowRequestId returns false when RequestId is empty
        /// </summary>
        [Fact]
        public void ErrorViewModel_ShowRequestId_ShouldReturnFalse_WhenRequestIdIsEmpty()
        {
            // Arrange
            var viewModel = new ErrorViewModel
            {
                RequestId = string.Empty
            };

            // Act & Assert
            Assert.False(viewModel.ShowRequestId);
        }

        /// <summary>
        /// Tests that ErrorViewModel ShowRequestId returns true when RequestId has value
        /// </summary>
        [Fact]
        public void ErrorViewModel_ShowRequestId_ShouldReturnTrue_WhenRequestIdHasValue()
        {
            // Arrange
            var viewModel = new ErrorViewModel
            {
                RequestId = "request-id-123"
            };

            // Act & Assert
            Assert.True(viewModel.ShowRequestId);
        }

        /// <summary>
        /// Tests that ErrorViewModel properties can be modified after instantiation
        /// </summary>
        [Fact]
        public void ErrorViewModel_ShouldAllowPropertyModification_AfterInstantiation()
        {
            // Arrange
            var viewModel = new ErrorViewModel
            {
                RequestId = "initial-id"
            };

            // Act
            viewModel.RequestId = "updated-id";

            // Assert
            Assert.Equal("updated-id", viewModel.RequestId);
            Assert.True(viewModel.ShowRequestId);
        }
    }
}

