using FirstWebApplication.Models;
using System.ComponentModel.DataAnnotations;

namespace Kartverket.Tests.Models
{
    /// <summary>
    /// Tests for Advice domain model
    /// </summary>
    public class AdviceTest
    {
        /// <summary>
        /// Tests that Advice can be instantiated with default values
        /// </summary>
        [Fact]
        public void Advice_ShouldBeInstantiable_WithDefaultValues()
        {
            // Arrange & Act
            var advice = new Advice();

            // Assert
            Assert.NotNull(advice);
            Assert.Equal(0, advice.adviceID);
            Assert.Null(advice.adviceMessage);
            Assert.Null(advice.Email);
        }

        /// <summary>
        /// Tests that Advice can be instantiated with all properties set
        /// </summary>
        [Fact]
        public void Advice_ShouldBeInstantiable_WithAllPropertiesSet()
        {
            // Arrange
            var adviceId = 1;
            var message = "This is a test feedback message";
            var email = "test@example.com";

            // Act
            var advice = new Advice
            {
                adviceID = adviceId,
                adviceMessage = message,
                Email = email
            };

            // Assert
            Assert.NotNull(advice);
            Assert.Equal(adviceId, advice.adviceID);
            Assert.Equal(message, advice.adviceMessage);
            Assert.Equal(email, advice.Email);
        }

        /// <summary>
        /// Tests that Advice properties can be modified after instantiation
        /// </summary>
        [Fact]
        public void Advice_ShouldAllowPropertyModification_AfterInstantiation()
        {
            // Arrange
            var advice = new Advice
            {
                adviceID = 1,
                adviceMessage = "Initial message",
                Email = "initial@example.com"
            };

            // Act
            advice.adviceID = 2;
            advice.adviceMessage = "Updated message";
            advice.Email = "updated@example.com";

            // Assert
            Assert.Equal(2, advice.adviceID);
            Assert.Equal("Updated message", advice.adviceMessage);
            Assert.Equal("updated@example.com", advice.Email);
        }

        /// <summary>
        /// Tests that Advice can handle null values for string properties
        /// </summary>
        [Fact]
        public void Advice_ShouldHandleNullValues_ForStringProperties()
        {
            // Arrange & Act
            var advice = new Advice
            {
                adviceID = 1,
                adviceMessage = null,
                Email = null
            };

            // Assert
            Assert.Null(advice.adviceMessage);
            Assert.Null(advice.Email);
        }

        /// <summary>
        /// Tests that Advice can handle empty string values
        /// </summary>
        [Fact]
        public void Advice_ShouldHandleEmptyStringValues()
        {
            // Arrange & Act
            var advice = new Advice
            {
                adviceID = 1,
                adviceMessage = string.Empty,
                Email = string.Empty
            };

            // Assert
            Assert.Equal(string.Empty, advice.adviceMessage);
            Assert.Equal(string.Empty, advice.Email);
        }

        /// <summary>
        /// Tests that Advice can handle long message content
        /// </summary>
        [Fact]
        public void Advice_ShouldHandleLongMessageContent()
        {
            // Arrange
            var longMessage = new string('A', 1000);

            // Act
            var advice = new Advice
            {
                adviceID = 1,
                adviceMessage = longMessage,
                Email = "test@example.com"
            };

            // Assert
            Assert.Equal(longMessage, advice.adviceMessage);
            Assert.Equal(1000, advice.adviceMessage.Length);
        }
    }
}

