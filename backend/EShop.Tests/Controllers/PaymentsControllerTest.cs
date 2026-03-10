using EShop.API.Controllers;
using EShop.Core.Interfaces;
using EShop.Tests.Helpers;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace EShop.Tests.Controllers;

public class PaymentsControllerTests
{
    private readonly Mock<IStripeService> _mockStripeService;
    private readonly PaymentsController _controller;

    public PaymentsControllerTests()
    {
        _mockStripeService = new Mock<IStripeService>();
        _controller = new PaymentsController(_mockStripeService.Object);
        ControllerTestHelper.SetUser(_controller, "user-1");
    }

    [Fact]
    public async Task CreateCheckoutSession_WithValidUser_ReturnsOk()
    {
        _mockStripeService
            .Setup(s => s.CreateCheckoutSessionAsync("user-1"))
            .ReturnsAsync(("session_123", "https://checkout.stripe.com/pay/session_123"));

        var result = await _controller.CreateCheckoutSession();

        Assert.IsType<OkObjectResult>(result);
    }

    [Fact]
    public async Task CreateCheckoutSession_EmptyCart_ReturnsBadRequest()
    {
        ControllerTestHelper.SetUser(_controller, "user-empty");

        _mockStripeService
            .Setup(s => s.CreateCheckoutSessionAsync("user-empty"))
            .ThrowsAsync(new InvalidOperationException("Koszyk jest pusty"));

        var result = await _controller.CreateCheckoutSession();

        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task CreateCheckoutSession_CallsStripeServiceOnce()
    {
        _mockStripeService
            .Setup(s => s.CreateCheckoutSessionAsync(It.IsAny<string>()))
            .ReturnsAsync(("session_1", "https://url.com"));

        await _controller.CreateCheckoutSession();

        _mockStripeService.Verify(s => s.CreateCheckoutSessionAsync("user-1"), Times.Once);
    }
}