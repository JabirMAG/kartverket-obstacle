using FirstWebApplication.Controllers;
using FirstWebApplication.Models;
using FirstWebApplication.Models.ViewModel;
using FirstWebApplication.Repositories;
using Kartverket.Tests.Helpers;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace Kartverket.Tests.Controllers
{
    /// <summary>
    /// Tests for AdviceController
    /// </summary>
    public class AdviceControllerTest
    {
        private readonly Mock<IAdviceRepository> _adviceRepositoryMock;
        private readonly AdviceController _controller;

        public AdviceControllerTest()
        {
            _adviceRepositoryMock = new Mock<IAdviceRepository>();
            _controller = new AdviceController(_adviceRepositoryMock.Object);
        }

        /// <summary>
        /// Tests that FeedbackForm GET returns a view with status 200
        /// </summary>
        [Fact]
        public void FeedbackForm_Get_ShouldReturnView_WithStatus200()
        {
            // Act
            var result = _controller.FeedbackForm();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.NotNull(viewResult);
        }

        /// <summary>
        /// Tests that FeedbackForm POST returns view with status 200 when model is invalid
        /// </summary>
        [Fact]
        public async Task FeedbackForm_Post_ShouldReturnView_WhenModelIsInvalid()
        {
            // Arrange
            var viewModel = new AdviceViewModel
            {
                ViewadviceMessage = string.Empty,
                ViewEmail = "invalid-email"
            };
            _controller.ModelState.AddModelError("ViewEmail", "Invalid email");

            // Act
            var result = await _controller.FeedbackForm(viewModel);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.NotNull(viewResult);
            Assert.False(_controller.ModelState.IsValid);
            _adviceRepositoryMock.Verify(x => x.AddAdvice(It.IsAny<Advice>()), Times.Never);
        }

        /// <summary>
        /// Tests that FeedbackForm POST saves advice and redirects with status 302 when valid
        /// </summary>
        [Fact]
        public async Task FeedbackForm_Post_ShouldSaveAdvice_AndRedirect_WhenValid()
        {
            // Arrange
            var viewModel = new AdviceViewModel
            {
                ViewadviceMessage = "Test feedback message",
                ViewEmail = "test@example.com"
            };
            var savedAdvice = TestDataBuilder.CreateValidAdvice("test@example.com", "Test feedback message");
            savedAdvice.adviceID = 1;

            _adviceRepositoryMock.Setup(x => x.AddAdvice(It.IsAny<Advice>()))
                .ReturnsAsync(savedAdvice);

            // Act
            var result = await _controller.FeedbackForm(viewModel);

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.NotNull(redirectResult);
            Assert.Equal("ThankForm", redirectResult.ActionName);
            _adviceRepositoryMock.Verify(x => x.AddAdvice(It.Is<Advice>(a => 
                a.Email == viewModel.ViewEmail && 
                a.adviceMessage == viewModel.ViewadviceMessage)), Times.Once);
        }

        /// <summary>
        /// Tests that ThankForm returns a view with status 200 and advice data
        /// </summary>
        [Fact]
        public void ThankForm_ShouldReturnView_WithAdviceData()
        {
            // Arrange
            var advice = TestDataBuilder.CreateValidAdvice();
            var email = advice.Email;
            var message = advice.adviceMessage;

            // Act
            var result = _controller.ThankForm(email, message);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.NotNull(viewResult);
            var model = Assert.IsType<Advice>(viewResult.Model);
            Assert.Equal(email, model.Email);
            Assert.Equal(message, model.adviceMessage);
        }

        /// <summary>
        /// Tests that ThankForm redirects to FeedbackForm when email is null
        /// </summary>
        [Fact]
        public void ThankForm_ShouldRedirect_WhenEmailIsNull()
        {
            // Arrange
            string email = null;
            string message = "Test message";

            // Act
            var result = _controller.ThankForm(email, message);

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("FeedbackForm", redirectResult.ActionName);
        }

        /// <summary>
        /// Tests that ThankForm redirects to FeedbackForm when message is null
        /// </summary>
        [Fact]
        public void ThankForm_ShouldRedirect_WhenMessageIsNull()
        {
            // Arrange
            string email = "test@example.com";
            string message = null;

            // Act
            var result = _controller.ThankForm(email, message);

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("FeedbackForm", redirectResult.ActionName);
        }

        /// <summary>
        /// Tests that ThankForm redirects to FeedbackForm when email is empty
        /// </summary>
        [Fact]
        public void ThankForm_ShouldRedirect_WhenEmailIsEmpty()
        {
            // Arrange
            string email = string.Empty;
            string message = "Test message";

            // Act
            var result = _controller.ThankForm(email, message);

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("FeedbackForm", redirectResult.ActionName);
        }

        /// <summary>
        /// Tests that ThankForm redirects to FeedbackForm when message is empty
        /// </summary>
        [Fact]
        public void ThankForm_ShouldRedirect_WhenMessageIsEmpty()
        {
            // Arrange
            string email = "test@example.com";
            string message = string.Empty;

            // Act
            var result = _controller.ThankForm(email, message);

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("FeedbackForm", redirectResult.ActionName);
        }
    }

}

