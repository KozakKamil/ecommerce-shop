using EShop.Core.Entities;
using EShop.Infrastructure.Services;
using EShop.Tests.Helpers;
using Microsoft.Extensions.Configuration;

namespace EShop.Tests.Services;

public class StripeServiceTests
{
    private IConfiguration CreateTestConfiguration()
    {
        var inMemorySettings = new Dictionary<string, string?>
        {
            { "Stripe:SecretKey", "sk_test_fake_key" },
            { "Stripe:WebhookSecret", "whsec_fake_secret" },
            { "Stripe:SuccessUrl", "http://localhost:4200/payment/success" },
            { "Stripe:CancelUrl", "http://localhost:4200/payment/cancel" }
        };

        return new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings)
            .Build();
    }

    [Fact]
    public async Task CreateCheckoutSession_EmptyCart_ThrowsInvalidOperationException()
    {
        var context = TestDbContextFactory.Create("StripeDb_EmptyCart");
        var config = CreateTestConfiguration();
        var service = new StripeService(context, config);

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.CreateCheckoutSessionAsync("user-no-cart")
        );
    }

    [Fact]
    public async Task CreateCheckoutSession_WithCartItems_StripeFailure_CartNotCleared()
    {
        // Po naszej zmianie: jeśli Stripe rzuci wyjątek, koszyk zostaje nienaruszony
        var context = TestDbContextFactory.Create("StripeDb_CreateOrder");
        var config = CreateTestConfiguration();

        var product = new Product
        {
            Name = "Test Product",
            Description = "Opis",
            Price = 99.99m,
            ImageUrl = "img.jpg",
            Stock = 10,
            CategoryId = 1
        };

        context.Products.Add(product);
        await context.SaveChangesAsync();

        context.CartItems.Add(new CartItem
        {
            ProductId = product.Id,
            Quantity = 2,
            UserId = "user-stripe"
        });
        await context.SaveChangesAsync();

        var service = new StripeService(context, config);

        // Stripe rzuci wyjątek (fałszywy klucz) - koszyk powinien zostać
        await Assert.ThrowsAnyAsync<Exception>(
            () => service.CreateCheckoutSessionAsync("user-stripe")
        );

        // Koszyk NIE powinien być wyczyszczony
        var cartItems = context.CartItems.Where(ci => ci.UserId == "user-stripe").ToList();
        Assert.Single(cartItems);

        // Zamówienie NIE powinno być stworzone
        var orders = context.Orders.Where(o => o.UserId == "user-stripe").ToList();
        Assert.Empty(orders);
    }

    [Fact]
    public async Task CreateCheckoutSession_SetsCorrectOrderItems_StripeFailure_CartPreserved()
    {
        var context = TestDbContextFactory.Create("StripeDb_OrderItems");
        var config = CreateTestConfiguration();

        var product1 = new Product
        {
            Name = "P1",
            Description = "O1",
            Price = 50m,
            ImageUrl = "",
            Stock = 5,
            CategoryId = 1
        };

        var product2 = new Product
        {
            Name = "P2",
            Description = "O2",
            Price = 30m,
            ImageUrl = "",
            Stock = 3,
            CategoryId = 1
        };

        context.Products.AddRange(product1, product2);
        await context.SaveChangesAsync();

        context.CartItems.AddRange(
            new CartItem { ProductId = product1.Id, Quantity = 1, UserId = "user-items" },
            new CartItem { ProductId = product2.Id, Quantity = 3, UserId = "user-items" }
        );
        await context.SaveChangesAsync();

        var service = new StripeService(context, config);

        // Stripe rzuci wyjątek - oba produkty w koszyku powinny zostać
        await Assert.ThrowsAnyAsync<Exception>(
            () => service.CreateCheckoutSessionAsync("user-items")
        );

        var cartItems = context.CartItems.Where(ci => ci.UserId == "user-items").ToList();
        Assert.Equal(2, cartItems.Count);

        var orders = context.Orders.Where(o => o.UserId == "user-items").ToList();
        Assert.Empty(orders);
    }
}