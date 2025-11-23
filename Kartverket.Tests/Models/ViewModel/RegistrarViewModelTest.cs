using FirstWebApplication.Models;
using FirstWebApplication.Models.ViewModel;

namespace Kartverket.Tests.Models.ViewModel
{
    /// <summary>
    /// Tests for RegistrarViewModel
    /// </summary>
    public class RegistrarViewModelTest
    {
        /// <summary>
        /// Tests that RegistrarViewModel can be instantiated with default values
        /// </summary>
        [Fact]
        public void RegistrarViewModel_ShouldBeInstantiable_WithDefaultValues()
        {
            // Arrange & Act
            var viewModel = new RegistrarViewModel();

            // Assert
            Assert.NotNull(viewModel);
            Assert.NotNull(viewModel.Obstacles);
            Assert.NotNull(viewModel.Rapports);
            Assert.NotNull(viewModel.NewRapport);
            Assert.Empty(viewModel.Obstacles);
            Assert.Empty(viewModel.Rapports);
        }

        /// <summary>
        /// Tests that RegistrarViewModel can be instantiated with all properties set
        /// </summary>
        [Fact]
        public void RegistrarViewModel_ShouldBeInstantiable_WithAllPropertiesSet()
        {
            // Arrange
            var obstacles = new List<ObstacleData>
            {
                new ObstacleData { ObstacleId = 1, ObstacleName = "Obstacle 1" },
                new ObstacleData { ObstacleId = 2, ObstacleName = "Obstacle 2" }
            };
            var rapports = new List<RapportData>
            {
                new RapportData { RapportID = 1, RapportComment = "Comment 1" }
            };
            var newRapport = new RapportData { RapportID = 2, RapportComment = "New Comment" };

            // Act
            var viewModel = new RegistrarViewModel
            {
                Obstacles = obstacles,
                Rapports = rapports,
                NewRapport = newRapport
            };

            // Assert
            Assert.NotNull(viewModel);
            Assert.Equal(2, viewModel.Obstacles.Count());
            Assert.Single(viewModel.Rapports);
            Assert.Equal(newRapport, viewModel.NewRapport);
        }

        /// <summary>
        /// Tests that RegistrarViewModel properties can be modified after instantiation
        /// </summary>
        [Fact]
        public void RegistrarViewModel_ShouldAllowPropertyModification_AfterInstantiation()
        {
            // Arrange
            var viewModel = new RegistrarViewModel();
            var newObstacles = new List<ObstacleData>
            {
                new ObstacleData { ObstacleId = 1 }
            };
            var newRapports = new List<RapportData>
            {
                new RapportData { RapportID = 1 }
            };

            // Act
            viewModel.Obstacles = newObstacles;
            viewModel.Rapports = newRapports;
            viewModel.NewRapport = new RapportData { RapportID = 2 };

            // Assert
            Assert.Single(viewModel.Obstacles);
            Assert.Single(viewModel.Rapports);
            Assert.Equal(2, viewModel.NewRapport.RapportID);
        }

        /// <summary>
        /// Tests that RegistrarViewModel can handle empty collections
        /// </summary>
        [Fact]
        public void RegistrarViewModel_ShouldHandle_EmptyCollections()
        {
            // Arrange & Act
            var viewModel = new RegistrarViewModel
            {
                Obstacles = new List<ObstacleData>(),
                Rapports = new List<RapportData>()
            };

            // Assert
            Assert.Empty(viewModel.Obstacles);
            Assert.Empty(viewModel.Rapports);
        }
    }
}

