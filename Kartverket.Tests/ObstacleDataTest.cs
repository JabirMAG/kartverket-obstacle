using System.ComponentModel.DataAnnotations;
using System.Runtime.CompilerServices;
using FirstWebApplication.Models;

namespace Kartverket.Tests
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
        public void ObsacleData_ShouldBeValid_WhenAllFieldsAreCorrect()
        {
            var obstacle = new ObstacleData
            {
                ObstacleName = "Tre",
                ObstacleHeight = 15,
                ObstacleDescription = "Et høyt tre nær landingsone",
                GeomertyGeoJason = "{}",
            };
            var isValid = ValidateModel(obstacle, out var results);

            Assert.True(isValid);
            Assert.Empty(results);
        }

        [Fact]
        public void ObsacleData_ShouldFail_WhenRequiredFieldMissing()
        {
            var obstacle = new ObstacleData
            {
                ObstacleName = "", // må fylle inn navn på hindring, tomt felt skal ikke være lov
                ObstacleHeight = 50,
                ObstacleDescription = "", // må fylle inn beskrivelse, altså tomt felt skal feile
            };
            var isValid = ValidateModel(obstacle, out var results);

            Assert.False(isValid);

            //sjekker at feilmelding kommer ved tomme felt, nb "Field is required"
            Assert.Contains(results, r => r.ErrorMessage.Contains("Field is required"));
        }

        [Fact]
        public void ObstacleData_ShouldFail_WhenHeightOutOfRange()
        {
            var obstacle = new ObstacleData
            {
                ObstacleName = "Stolpe",
                ObstacleHeight = 500, //
                ObstacleDescription = "en enorm stolpe",
            };

            var isValid = ValidateModel(obstacle, out var results);

            Assert.False(isValid);

            //sjekker at feilmelding kommer når stolpen er 500 som er over maxverdi (200)
            Assert.Contains(
                results,
                r => r.ErrorMessage.Contains("The field ObstacleHeight must be between 0 and 200.")
            );
        }
    }
}
