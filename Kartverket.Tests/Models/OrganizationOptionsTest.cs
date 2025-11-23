using FirstWebApplication.Models;

namespace Kartverket.Tests.Models
{
    /// <summary>
    /// Tests for OrganizationOptions static class
    /// </summary>
    public class OrganizationOptionsTest
    {
        /// <summary>
        /// Tests that OrganizationOptions contains the expected constant values
        /// </summary>
        [Fact]
        public void OrganizationOptions_ShouldContain_ExpectedConstants()
        {
            // Assert
            Assert.Equal("Kartverket", OrganizationOptions.Kartverket);
            Assert.Equal("Politi", OrganizationOptions.Politi);
            Assert.Equal("Ambulanse", OrganizationOptions.Ambulanse);
        }

        /// <summary>
        /// Tests that OrganizationOptions.All array contains all expected organizations
        /// </summary>
        [Fact]
        public void OrganizationOptions_All_ShouldContain_AllExpectedOrganizations()
        {
            // Arrange
            var expectedOrganizations = new[] { "Kartverket", "Politi", "Ambulanse" };

            // Act
            var allOrganizations = OrganizationOptions.All;

            // Assert
            Assert.NotNull(allOrganizations);
            Assert.Equal(3, allOrganizations.Length);
            Assert.Contains(OrganizationOptions.Kartverket, allOrganizations);
            Assert.Contains(OrganizationOptions.Politi, allOrganizations);
            Assert.Contains(OrganizationOptions.Ambulanse, allOrganizations);
        }

        /// <summary>
        /// Tests that OrganizationOptions.All array contains organizations in correct order
        /// </summary>
        [Fact]
        public void OrganizationOptions_All_ShouldContainOrganizations_InCorrectOrder()
        {
            // Act
            var allOrganizations = OrganizationOptions.All;

            // Assert
            Assert.Equal(OrganizationOptions.Kartverket, allOrganizations[0]);
            Assert.Equal(OrganizationOptions.Politi, allOrganizations[1]);
            Assert.Equal(OrganizationOptions.Ambulanse, allOrganizations[2]);
        }

        /// <summary>
        /// Tests that OrganizationOptions.All array does not contain duplicate values
        /// </summary>
        [Fact]
        public void OrganizationOptions_All_ShouldNotContain_DuplicateValues()
        {
            // Act
            var allOrganizations = OrganizationOptions.All;

            // Assert
            var distinctOrganizations = allOrganizations.Distinct();
            Assert.Equal(allOrganizations.Length, distinctOrganizations.Count());
        }

        /// <summary>
        /// Tests that OrganizationOptions.All array is not null
        /// </summary>
        [Fact]
        public void OrganizationOptions_All_ShouldNotBeNull()
        {
            // Act
            var allOrganizations = OrganizationOptions.All;

            // Assert
            Assert.NotNull(allOrganizations);
        }

        /// <summary>
        /// Tests that OrganizationOptions constants are not null or empty
        /// </summary>
        [Fact]
        public void OrganizationOptions_Constants_ShouldNotBeNullOrEmpty()
        {
            // Assert
            Assert.False(string.IsNullOrEmpty(OrganizationOptions.Kartverket));
            Assert.False(string.IsNullOrEmpty(OrganizationOptions.Politi));
            Assert.False(string.IsNullOrEmpty(OrganizationOptions.Ambulanse));
        }

        /// <summary>
        /// Tests that OrganizationOptions.All array can be used for validation
        /// </summary>
        [Fact]
        public void OrganizationOptions_All_ShouldBeUsable_ForValidation()
        {
            // Arrange
            var validOrganization = "Kartverket";
            var invalidOrganization = "InvalidOrg";

            // Act
            var isValid = OrganizationOptions.All.Contains(validOrganization);
            var isInvalid = OrganizationOptions.All.Contains(invalidOrganization);

            // Assert
            Assert.True(isValid);
            Assert.False(isInvalid);
        }

        /// <summary>
        /// Tests that OrganizationOptions.All array is case-sensitive
        /// </summary>
        [Fact]
        public void OrganizationOptions_All_ShouldBeCaseSensitive()
        {
            // Arrange
            var lowerCase = "kartverket";
            var upperCase = "KARTVERKET";

            // Act
            var containsLowerCase = OrganizationOptions.All.Contains(lowerCase);
            var containsUpperCase = OrganizationOptions.All.Contains(upperCase);

            // Assert
            Assert.False(containsLowerCase);
            Assert.False(containsUpperCase);
        }
    }
}

