using FirstWebApplication.Models;
using FirstWebApplication.Models.ViewModel;

namespace Kartverket.Tests.Models.ViewModel
{
    /// <summary>
    /// Tests for VarslingViewModel
    /// </summary>
    public class VarslingViewModelTest
    {
        /// <summary>
        /// Tests that VarslingViewModel can be instantiated with default values
        /// </summary>
        [Fact]
        public void VarslingViewModel_ShouldBeInstantiable_WithDefaultValues()
        {
            // Arrange & Act
            var viewModel = new VarslingViewModel();

            // Assert
            Assert.NotNull(viewModel);
            Assert.Null(viewModel.Obstacle);
            Assert.Equal(0, viewModel.CommentCount);
            Assert.NotNull(viewModel.Comments);
            Assert.Empty(viewModel.Comments);
        }

        /// <summary>
        /// Tests that VarslingViewModel can be instantiated with all properties set
        /// </summary>
        [Fact]
        public void VarslingViewModel_ShouldBeInstantiable_WithAllPropertiesSet()
        {
            // Arrange
            var obstacle = new ObstacleData
            {
                ObstacleId = 1,
                ObstacleName = "Test Obstacle"
            };
            var comments = new List<RapportData>
            {
                new RapportData { RapportID = 1, RapportComment = "Comment 1" },
                new RapportData { RapportID = 2, RapportComment = "Comment 2" }
            };

            // Act
            var viewModel = new VarslingViewModel
            {
                Obstacle = obstacle,
                CommentCount = 2,
                Comments = comments
            };

            // Assert
            Assert.NotNull(viewModel);
            Assert.Equal(obstacle, viewModel.Obstacle);
            Assert.Equal(2, viewModel.CommentCount);
            Assert.Equal(2, viewModel.Comments.Count);
        }

        /// <summary>
        /// Tests that VarslingViewModel properties can be modified after instantiation
        /// </summary>
        [Fact]
        public void VarslingViewModel_ShouldAllowPropertyModification_AfterInstantiation()
        {
            // Arrange
            var viewModel = new VarslingViewModel
            {
                Obstacle = new ObstacleData { ObstacleId = 1 },
                CommentCount = 1,
                Comments = new List<RapportData> { new RapportData { RapportID = 1 } }
            };

            var newObstacle = new ObstacleData { ObstacleId = 2 };
            var newComments = new List<RapportData>
            {
                new RapportData { RapportID = 2 },
                new RapportData { RapportID = 3 }
            };

            // Act
            viewModel.Obstacle = newObstacle;
            viewModel.CommentCount = 2;
            viewModel.Comments = newComments;

            // Assert
            Assert.Equal(newObstacle, viewModel.Obstacle);
            Assert.Equal(2, viewModel.Obstacle.ObstacleId);
            Assert.Equal(2, viewModel.CommentCount);
            Assert.Equal(2, viewModel.Comments.Count);
        }

        /// <summary>
        /// Tests that VarslingViewModel can handle null Obstacle
        /// </summary>
        [Fact]
        public void VarslingViewModel_ShouldHandle_NullObstacle()
        {
            // Arrange & Act
            var viewModel = new VarslingViewModel
            {
                Obstacle = null,
                CommentCount = 0,
                Comments = new List<RapportData>()
            };

            // Assert
            Assert.Null(viewModel.Obstacle);
            Assert.Equal(0, viewModel.CommentCount);
            Assert.Empty(viewModel.Comments);
        }
    }
}

