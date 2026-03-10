using EShop.API.Controllers;
using EShop.Core.Entities;
using EShop.Tests.Helpers;
using Microsoft.AspNetCore.Mvc;

namespace EShop.Tests.Controllers;

public class OrderControllerTests
{
    [Fact]
    public async Task GetOrders_ReturnsOkWithOrders()
    {
        var context = TestDbContextFactory.Create("OrderDb_GetAll");
        context.Orders.Add(new Order { UserId = "user-1", TotalAmount = 100m, Status = OrderStatus.Pending, Items = new List<OrderItem>() });
        await context.SaveChangesAsync();

        var controller = new OrderController(context);
        ControllerTestHelper.SetUser(controller, "user-1");

        var result = await controller.GetOrders();

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var orders = Assert.IsAssignableFrom<IEnumerable<Order>>(okResult.Value);
        Assert.Single(orders);
    }

    [Fact]
    public async Task GetOrders_NoOrders_ReturnsEmptyList()
    {
        var context = TestDbContextFactory.Create("OrderDb_Empty");
        var controller = new OrderController(context);
        ControllerTestHelper.SetUser(controller, "user-none");

        var result = await controller.GetOrders();

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var orders = Assert.IsAssignableFrom<IEnumerable<Order>>(okResult.Value);
        Assert.Empty(orders);
    }

    [Fact]
    public async Task CreateOrder_WithCartItems_ReturnsCreatedAtAction()
    {
        var context = TestDbContextFactory.Create("OrderDb_Create");
        var product = new Product { Name = "Test", Description = "Opis", Price = 50m, ImageUrl = "", Stock = 10, CategoryId = 1 };
        context.Products.Add(product);
        await context.SaveChangesAsync();
        context.CartItems.Add(new CartItem { ProductId = product.Id, Quantity = 2, UserId = "user-1" });
        await context.SaveChangesAsync();

        var controller = new OrderController(context);
        ControllerTestHelper.SetUser(controller, "user-1");

        var result = await controller.CreateOrder();

        var createdResult = Assert.IsType<CreatedAtActionResult>(result.Result);
        var order = Assert.IsType<Order>(createdResult.Value);
        Assert.Equal(100m, order.TotalAmount);
        Assert.Single(order.Items);
    }

    [Fact]
    public async Task CreateOrder_EmptyCart_ReturnsBadRequest()
    {
        var context = TestDbContextFactory.Create("OrderDb_EmptyCart");
        var controller = new OrderController(context);
        ControllerTestHelper.SetUser(controller, "user-1");

        var result = await controller.CreateOrder();

        Assert.IsType<BadRequestObjectResult>(result.Result);
    }

    [Fact]
    public async Task CreateOrder_ClearsCartAfterOrder()
    {
        var context = TestDbContextFactory.Create("OrderDb_ClearsCart");
        var product = new Product { Name = "Test", Description = "Opis", Price = 50m, ImageUrl = "", Stock = 10, CategoryId = 1 };
        context.Products.Add(product);
        await context.SaveChangesAsync();
        context.CartItems.Add(new CartItem { ProductId = product.Id, Quantity = 1, UserId = "user-1" });
        await context.SaveChangesAsync();

        var controller = new OrderController(context);
        ControllerTestHelper.SetUser(controller, "user-1");

        await controller.CreateOrder();

        Assert.Empty(context.CartItems.Where(ci => ci.UserId == "user-1"));
    }

    [Fact]
    public async Task CancelOrder_PendingOrder_ReturnsNoContent()
    {
        var context = TestDbContextFactory.Create("OrderDb_Cancel");
        var order = new Order { UserId = "user-1", TotalAmount = 100m, Status = OrderStatus.Pending, Items = new List<OrderItem>() };
        context.Orders.Add(order);
        await context.SaveChangesAsync();

        var controller = new OrderController(context);
        ControllerTestHelper.SetUser(controller, "user-1");

        var result = await controller.CancelOrder(order.Id);

        Assert.IsType<NoContentResult>(result);
        Assert.Equal(OrderStatus.Cancelled, context.Orders.Find(order.Id)!.Status);
    }

    [Fact]
    public async Task CancelOrder_ShippedOrder_ReturnsBadRequest()
    {
        var context = TestDbContextFactory.Create("OrderDb_CancelShipped");
        var order = new Order { UserId = "user-1", TotalAmount = 100m, Status = OrderStatus.Shipped, Items = new List<OrderItem>() };
        context.Orders.Add(order);
        await context.SaveChangesAsync();

        var controller = new OrderController(context);
        ControllerTestHelper.SetUser(controller, "user-1");

        var result = await controller.CancelOrder(order.Id);

        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task CancelOrder_InvalidId_ReturnsNotFound()
    {
        var context = TestDbContextFactory.Create("OrderDb_CancelNotFound");
        var controller = new OrderController(context);
        ControllerTestHelper.SetUser(controller, "user-1");

        var result = await controller.CancelOrder(999);

        Assert.IsType<NotFoundResult>(result);
    }
}