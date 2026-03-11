using EShop.Core.Entities;
using EShop.Core.Interfaces;
using EShop.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Stripe;
using Stripe.Checkout;

namespace EShop.Infrastructure.Services;

public class StripeService : IStripeService
{
    private readonly AppDbContext _context;
    private readonly IConfiguration _configuration;

    public StripeService(AppDbContext context, IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;
        StripeConfiguration.ApiKey = _configuration["Stripe:SecretKey"];
    }

    public async Task<(string sessionId, string url)> CreateCheckoutSessionAsync(string userId)
    {
        var cartItems = await _context.CartItems
            .Include(ci => ci.Product)
            .Where(ci => ci.UserId == userId)
            .ToListAsync();

        if (!cartItems.Any())
            throw new InvalidOperationException("Koszyk jest pusty");

        var totalAmount = cartItems.Sum(ci => ci.Product.Price * ci.Quantity);
        if (totalAmount < 2.00m)
            throw new InvalidOperationException("Minimalna kwota zamówienia to 2,00 zł.");


        // 1. Najpierw utwórz sesję Stripe (bez dotykania DB)
        var lineItems = cartItems.Select(ci => new SessionLineItemOptions
        {
            PriceData = new SessionLineItemPriceDataOptions
            {
                UnitAmountDecimal = ci.Product.Price * 100,
                Currency = "pln",
                ProductData = new SessionLineItemPriceDataProductDataOptions
                {
                    Name = ci.Product.Name,
                    Description = ci.Product.Description,
                    Images = string.IsNullOrEmpty(ci.Product.ImageUrl)
                        ? null
                        : new List<string> { ci.Product.ImageUrl }
                }
            },
            Quantity = ci.Quantity
        }).ToList();

        var options = new SessionCreateOptions
        {
            PaymentMethodTypes = new List<string> { "card" },
            LineItems = lineItems,
            Mode = "payment",
            SuccessUrl = _configuration["Stripe:SuccessUrl"] + "?orderId={CHECKOUT_SESSION_ID}",
            CancelUrl = _configuration["Stripe:CancelUrl"],
            Metadata = new Dictionary<string, string>
            {
                { "userId", userId }
            }
        };

        var service = new SessionService();
        var session = await service.CreateAsync(options); // jeśli Stripe rzuci wyjątek, koszyk zostaje

        // 2. Dopiero po sukcesie Stripe: utwórz zamówienie i wyczyść koszyk
        var order = new Order
        {
            UserId = userId,
            OrderDate = DateTime.UtcNow,
            Status = OrderStatus.Pending,
            StripePaymentIntentId = session.Id,
            Items = cartItems.Select(ci => new OrderItem
            {
                ProductId = ci.ProductId,
                Quantity = ci.Quantity,
                UnitPrice = ci.Product.Price
            }).ToList(),
            TotalAmount = cartItems.Sum(ci => ci.Product.Price * ci.Quantity)
        };

        _context.Orders.Add(order);
        _context.CartItems.RemoveRange(cartItems);
        await _context.SaveChangesAsync();

        return (session.Id, session.Url);
    }

    public async Task HandleWebhookAsync(string json, string signature)
    {
        var webhookSecret = _configuration["Stripe:WebhookSecret"];
        var stripeEvent = EventUtility.ConstructEvent(json, signature, webhookSecret);

        if (stripeEvent.Type == EventTypes.CheckoutSessionCompleted)
        {
            var session = stripeEvent.Data.Object as Session;
            if (session != null)
            {
                var order = await _context.Orders
                    .FirstOrDefaultAsync(o => o.StripePaymentIntentId == session.Id);

                if (order != null)
                {
                    order.Status = OrderStatus.PaymentReceived;
                    await _context.SaveChangesAsync();
                }
            }
        }
    }
}