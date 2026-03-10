using EShop.API.Controllers;
using EShop.API.DTOs;
using EShop.Core.Entities;
using EShop.Tests.Helpers;
using Microsoft.AspNetCore.Mvc;

namespace EShop.Tests.Controllers;

public class CartControllerTests
{
    [Fact]
    public async Task GetCartItems_ReturnsOkWithItems()
    {
        var context = TestDbContextFactory.Create("CartDb_GetItems");
        var product = new Product { Name = "Test", Description = "Opis", Price = 100m, ImageUrl = "", Stock = 5, CategoryId = 1 };
        context.Products.Add(product);
        await context.SaveChangesAsync();

        context.CartItems.Add(new CartItem { ProductId = product.Id, Quantity = 2, UserId = "user-1" });
        await context.SaveChangesAsync();

        var controller = new CartController(context);
        ControllerTestHelper.SetUser(controller, "user-1");

        var result = await controller.GetCartItems();

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var items = Assert.IsAssignableFrom<IEnumerable<CartItem>>(okResult.Value);
        Assert.Single(items);
    }

    [Fact]
    public async Task GetCartItems_EmptyCart_ReturnsEmptyList()
    {
        var context = TestDbContextFactory.Create("CartDb_Empty");
        var controller = new CartController(context);
        ControllerTestHelper.SetUser(controller, "user-empty");

        var result = await controller.GetCartItems();

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var items = Assert.IsAssignableFrom<IEnumerable<CartItem>>(okResult.Value);
        Assert.Empty(items);
    }

    [Fact]
    public async Task AddToCart_NewItem_ReturnsCreatedAtAction()
    {
        var context = TestDbContextFactory.Create("CartDb_AddNew");
        context.Products.Add(new Product { Id = 1, Name = "Test", Description = "Opis", Price = 100m, ImageUrl = "", Stock = 5, CategoryId = 1 });
        await context.SaveChangesAsync();

        var controller = new CartController(context);
        ControllerTestHelper.SetUser(controller, "user-1");

        var dto = new AddToCartDto { ProductId = 1, Quantity = 1 };
        var result = await controller.AddToCart(dto);

        Assert.IsType<CreatedAtActionResult>(result.Result);
    }

    [Fact]
    public async Task AddToCart_ExistingItem_IncreasesQuantity()
    {
        var context = TestDbContextFactory.Create("CartDb_AddExisting");
        context.Products.Add(new Product { Id = 1, Name = "Test", Description = "Opis", Price = 100m, ImageUrl = "", Stock = 5, CategoryId = 1 });
        context.CartItems.Add(new CartItem { ProductId = 1, Quantity = 2, UserId = "user-1" });
        await context.SaveChangesAsync();

        var controller = new CartController(context);
        ControllerTestHelper.SetUser(controller, "user-1");

        var dto = new AddToCartDto { ProductId = 1, Quantity = 3 };
        var result = await controller.AddToCart(dto);

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var item = Assert.IsType<CartItem>(okResult.Value);
        Assert.Equal(5, item.Quantity);
    }

    [Fact]
    public async Task AddToCart_InvalidProduct_ReturnsNotFound()
    {
        var context = TestDbContextFactory.Create("CartDb_InvalidProduct");
        var controller = new CartController(context);
        ControllerTestHelper.SetUser(controller, "user-1");

        var dto = new AddToCartDto { ProductId = 999, Quantity = 1 };
        var result = await controller.AddToCart(dto);

        Assert.IsType<NotFoundObjectResult>(result.Result);
    }

    [Fact]
    public async Task UpdateCartItem_ValidId_ReturnsNoContent()
    {
        var context = TestDbContextFactory.Create("CartDb_Update");
        context.CartItems.Add(new CartItem { Id = 1, ProductId = 1, Quantity = 1, UserId = "user-1" });
        await context.SaveChangesAsync();

        var controller = new CartController(context);
        ControllerTestHelper.SetUser(controller, "user-1");

        var result = await controller.UpdateCartItem(1, new UpdateCartItemDto { Quantity = 5 });
        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task UpdateCartItem_InvalidId_ReturnsNotFound()
    {
        var context = TestDbContextFactory.Create("CartDb_UpdateInvalid");
        var controller = new CartController(context);
        ControllerTestHelper.SetUser(controller, "user-1");

        var result = await controller.UpdateCartItem(999, new UpdateCartItemDto { Quantity = 5 });
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task ClearCart_RemovesAllItems()
    {
        var context = TestDbContextFactory.Create("CartDb_Clear");
        context.CartItems.AddRange(
            new CartItem { ProductId = 1, Quantity = 1, UserId = "user-1" },
            new CartItem { ProductId = 2, Quantity = 2, UserId = "user-1" }
        );
        await context.SaveChangesAsync();

        var controller = new CartController(context);
        ControllerTestHelper.SetUser(controller, "user-1");

        var result = await controller.ClearCart();
        Assert.IsType<NoContentResult>(result);
        Assert.Empty(context.CartItems.Where(ci => ci.UserId == "user-1"));
    }
}