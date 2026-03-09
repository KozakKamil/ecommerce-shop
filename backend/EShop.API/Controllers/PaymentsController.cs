using EShop.API.DTOs;
using EShop.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace EShop.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PaymentsController : ControllerBase
{
    private readonly IStripeService _stripeService;

    public PaymentsController(IStripeService stripeService)
    {
        _stripeService = stripeService;
    }

    [HttpPost("create-checkout-session")]
    public async Task<ActionResult<CheckoutSessionResponseDto>> CreateCheckoutSession(CreateCheckoutSessionDto dto)
    {
        try
        {
            var (sessionId, url) = await _stripeService.CreateCheckoutSessionAsync(dto.UserId);

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