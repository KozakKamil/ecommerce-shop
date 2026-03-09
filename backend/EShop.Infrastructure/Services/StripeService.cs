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

        if(!cartItems.Any())
            throw new InvalidOperationException("Koszyk jest pusty");

        var order = new Order
        {
            UserId = userId,
            OrderDate = DateTime.UtcNow,
            Status = OrderStatus.Pending,
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


        var lineItems = order.Items.Select(item =>
        {
            var product = cartItems.First(ci => ci.ProductId == item.ProductId).Product;
            return new SessionLineItemOptions
            {
                PriceData = new SessionLineItemPriceDataOptions
                {
                    UnitAmountDecimal = item.UnitPrice * 100,
                    Currency = "pln",
                    ProductData = new SessionLineItemPriceDataProductDataOptions
                    {
                        Name = product.Name,
                        Description = product.Description,
                        Images = string.IsNullOrEmpty(product.ImageUrl)
                            ? null
                            : new List<string> { product.ImageUrl }
                    }
                },
                Quantity = item.Quantity
            };
        }).ToList();

        var options = new SessionCreateOptions
        {
            PaymentMethodTypes = new List<string> { "card" },
            LineItems = lineItems,
            Mode = "payment",
            SuccessUrl = _configuration["Stripe:SuccessUrl"] + "?orderId=" + order.Id,
            CancelUrl = _configuration["Stripe:CancelUrl"] + "?orderId=" + order.Id,
            Metadata = new Dictionary<string, string>
            {
                {"orderId", order.Id.ToString()}
            }
        };

        var service = new SessionService();
        var session = await service.CreateAsync(options);

        order.StripePaymentIntentId = session.PaymentIntentId;
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
            if (session?.Metadata.TryGetValue("orderId", out var orderIdStr) == true
                && int.TryParse(orderIdStr, out var orderId))
            {
                var order = await _context.Orders.FindAsync(orderId);
                if (order != null)
                {
                    order.Status = OrderStatus.PaymentReceived;
                    order.StripePaymentIntentId = session.PaymentIntentId;
                    await _context.SaveChangesAsync();
                }
            }
        }
    }
}