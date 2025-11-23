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
        
        [Fact]
        public void FeedbackForm_Get_ReturnsViewWithModel()
        {
            var result = _controller.FeedbackForm() as ViewResult;
            
            Assert.NotNull(result);
            Assert.IsType<AdviceViewModel>(result.Model);
        }
        
        
        [Fact]
        public async Task FeedbackForm_Post_ValidModel_RedirectsToThankForm()
        {
            var model = new AdviceViewModel
            {
                ViewadviceMessage = "Great app!",
                ViewEmail = "user@test.com"
            };
            
            var result = await _controller.FeedbackForm(model) as RedirectToActionResult;
            
            Assert.NotNull(result);
            Assert.Equal("ThankForm", result.ActionName);
            Assert.Equal("user@test.com", result.RouteValues["email"]);
            Assert.Equal("Great app!", result.RouteValues["message"]);
            
            _mockRepo.Verify(r => r.AddAdvice(It.IsAny<Advice>()), Times.Once);
        }
        
        
        [Fact]
        public async Task FeedbackForm_Post_InvalidModel_ReturnsSameView()
        {
            var model = new AdviceViewModel();
            _controller.ModelState.AddModelError("Email", "Required");
            
            var result = await _controller.FeedbackForm(model) as ViewResult;
            
            Assert.NotNull(result);
            Assert.Equal(model, result.Model);
            
            _mockRepo.Verify(r => r.AddAdvice(It.IsAny<Advice>()), Times.Never);
        }
    }
}
