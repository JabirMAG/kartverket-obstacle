using FirstWebApplication.Controllers;
using FirstWebApplication.Models;
using FirstWebApplication.Models.ViewModel;
using FirstWebApplication.Repositories;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace FirstWebApplication.Kartverket.Tests.Controllers
{
    public class AdviceControllerTests
    {
        private readonly Mock<IAdviceRepository> _mockRepo;
        private readonly AdviceController _controller;

        public AdviceControllerTests()
        {
            _mockRepo = new Mock<IAdviceRepository>();
            _controller = new AdviceController(_mockRepo.Object);
        }

        // -----------------------------
        // 1. GET FeedbackForm
        // -----------------------------
        [Fact]
        public void FeedbackForm_Get_ReturnsViewWithModel()
        {
            // Act
            var result = _controller.FeedbackForm() as ViewResult;

            // Assert
            Assert.NotNull(result);
            Assert.IsType<AdviceViewModel>(result.Model);
        }

        // -----------------------------
        // 2. POST FeedbackForm (valid)
        // -----------------------------
        [Fact]
        public async Task FeedbackForm_Post_ValidModel_RedirectsToThankForm()
        {
            // Arrange
            var model = new AdviceViewModel
            {
                ViewadviceMessage = "Great app!",
                ViewEmail = "user@test.com"
            };

            // Act
            var result = await _controller.FeedbackForm(model) as RedirectToActionResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal("ThankForm", result.ActionName);
            Assert.Equal("user@test.com", result.RouteValues["email"]);
            Assert.Equal("Great app!", result.RouteValues["message"]);

            // IMPORTANT: repo should be called once
            _mockRepo.Verify(r => r.AddAdvice(It.IsAny<Advice>()), Times.Once);
        }

        // -----------------------------
        // 3. POST FeedbackForm (invalid model)
        // -----------------------------
        [Fact]
        public async Task FeedbackForm_Post_InvalidModel_ReturnsSameView()
        {
            // Arrange
            var model = new AdviceViewModel();
            _controller.ModelState.AddModelError("Email", "Required");

            // Act
            var result = await _controller.FeedbackForm(model) as ViewResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(model, result.Model);

            // Repo should NOT be called
            _mockRepo.Verify(r => r.AddAdvice(It.IsAny<Advice>()), Times.Never);
        }
    }
}
