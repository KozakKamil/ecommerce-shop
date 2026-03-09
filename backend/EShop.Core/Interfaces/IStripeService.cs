namespace EShop.Core.Interfaces;

public interface IStripeService
{
    Task<(string sessionId, string url)> CreateCheckoutSessionAsync(string userId);
    Task HandleWebhookAsync(string json, string signature);
}