using FirstWebApplication.Models;

namespace Kartverket.Tests.Helpers
{
    // Helper class for creating test data objects
    public static class TestDataBuilder
    {
        // Creates a valid ObstacleData object for testing
        public static ObstacleData CreateValidObstacle(string? ownerUserId = null)
        {
            return new ObstacleData
            {
                ObstacleName = "Test Obstacle",
                ObstacleHeight = 50.5,
                ObstacleDescription = "A test obstacle for testing",
                GeometryGeoJson = "{\"type\":\"Point\",\"coordinates\":[10.0,59.0]}",
                ObstacleStatus = 1,
                OwnerUserId = ownerUserId
            };
        }

        // Creates an ObstacleData object with minimal required data (only geometry)
        public static ObstacleData CreateMinimalObstacle(string? ownerUserId = null)
        {
            return new ObstacleData
            {
                ObstacleName = string.Empty,
                ObstacleHeight = 0,
                ObstacleDescription = string.Empty,
                GeometryGeoJson = "{\"type\":\"Point\",\"coordinates\":[10.0,59.0]}",
                ObstacleStatus = 1,
                OwnerUserId = ownerUserId
            };
        }

        // Creates a RapportData object for testing
        public static RapportData CreateValidRapport(int obstacleId, string comment = "Test comment")
        {
            return new RapportData
            {
                ObstacleId = obstacleId,
                RapportComment = comment
            };
        }

        // Creates an Advice object for testing
        public static Advice CreateValidAdvice(string email = "test@example.com", string message = "Test feedback")
        {
            return new Advice
            {
                Email = email,
                adviceMessage = message
            };
        }
    }
}

