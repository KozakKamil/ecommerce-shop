using EShop.API.Controllers;
using EShop.API.DTOs;
using EShop.Core.Interfaces;
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
    }

    [Fact]
    public async Task CreateCheckoutSession_WithValidUser_ReturnsOkWithSession()
    {
        // Arrange
        _mockStripeService
            .Setup(s => s.CreateCheckoutSessionAsync("user-1"))
            .ReturnsAsync(("session_123", "https://checkout.stripe.com/pay/session_123"));

        var dto = new CreateCheckoutSessionDto { UserId = "user-1" };

        // Act
        var result = await _controller.CreateCheckoutSession(dto);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<CheckoutSessionResponseDto>(okResult.Value);
        Assert.Equal("session_123", response.SessionId);
        Assert.Contains("checkout.stripe.com", response.Url);
    }

    [Fact]
    public async Task CreateCheckoutSession_EmptyCart_ReturnsBadRequest()
    {
        _mockStripeService
            .Setup(s => s.CreateCheckoutSessionAsync("user-empty"))
            .ThrowsAsync(new InvalidOperationException("Koszyk jest pusty"));

        var dto = new CreateCheckoutSessionDto { UserId = "user-empty" };

        var result = await _controller.CreateCheckoutSession(dto);

        Assert.IsType<BadRequestObjectResult>(result.Result);
    }

    [Fact]
    public async Task CreateCheckoutSession_CallsStripeServiceOnce()
    {
        _mockStripeService
            .Setup(s => s.CreateCheckoutSessionAsync(It.IsAny<string>()))
            .ReturnsAsync(("session_1", "https://url.com"));

        var dto = new CreateCheckoutSessionDto { UserId = "user-1" };

        await _controller.CreateCheckoutSession(dto);

        _mockStripeService.Verify(s => s.CreateCheckoutSessionAsync("user-1"), Times.Once);
    }
}