using EShop.API.DTOs;
using EShop.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace EShop.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PaymentsController : ControllerBase
{
    private readonly IStripeService _stripeService;

    public PaymentsController(IStripeService stripeService)
    {
        _stripeService = stripeService;
    }

    private string GetUserId() => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

    [HttpPost("create-checkout-session")]
    public async Task<ActionResult> CreateCheckoutSession()
    {
        try
        {
            var userId = GetUserId();
            var (sessionId, url) = await _stripeService.CreateCheckoutSessionAsync(userId);

            return Ok(new CheckoutSessionResponseDto
            {
                SessionId = sessionId,
                Url = url
            });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPost("webhook")]
    [AllowAnonymous]
    public async Task<IActionResult> Webhook()
    {
        var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();
        var signature = Request.Headers["Stripe-Signature"];

        try
        {
            await _stripeService.HandleWebhookAsync(json, signature!);
            return Ok();
        }
        catch (Exception)
        {
            return BadRequest();
        }
    }
}