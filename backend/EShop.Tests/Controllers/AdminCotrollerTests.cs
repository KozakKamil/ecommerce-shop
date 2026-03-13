using EShop.API.Controllers;
using EShop.API.DTOs;
using EShop.Core.Entities;
using EShop.Tests.Helpers;
using Microsoft.AspNetCore.Mvc;

namespace EShop.Tests.Controllers;

public class AdminControllerTests
{
    [Fact]
    public async Task GetDashboard_ReturnsCorrectStats()
    {
        var context = TestDbContextFactory.Create("AdminDb_Dashboard");
        context.Products.AddRange(
            new Product { Name = "P1", Description = "D", Price = 100m, ImageUrl = "", Stock = 5, CategoryId = 1 },
            new Product { Name = "P2", Description = "D", Price = 200m, ImageUrl = "", Stock = 3, CategoryId = 1 }
        );
        context.Orders.AddRange(
            new Order { UserId = "u1", TotalAmount = 100m, Status = OrderStatus.Pending, Items = new List<OrderItem>() },
            new Order { UserId = "u2", TotalAmount = 50m, Status = OrderStatus.Cancelled, Items = new List<OrderItem>() }
        );
        await context.SaveChangesAsync();

        var controller = new AdminController(context);
        var result = await controller.GetDashboard();

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(okResult.Value);
    }

    [Fact]
    public async Task CreateProduct_ValidDto_ReturnsCreatedAtAction()
    {
        var context = TestDbContextFactory.Create("AdminDb_Create");
        var controller = new AdminController(context);

        var dto = new CreateProductDto
        {
            Name = "Nowy produkt",
            Description = "Opis",
            Price = 99.99m,
            ImageUrl = "img.jpg",
            Stock = 10,
            CategoryId = 1
        };

        var result = await controller.CreateProduct(dto);
        Assert.IsType<CreatedAtActionResult>(result);
    }

    [Fact]
    public async Task UpdateProduct_ValidId_ReturnsNoContent()
    {
        var context = TestDbContextFactory.Create("AdminDb_Update");
        var product = new Product { Name = "Stary", Description = "D", Price = 10m, ImageUrl = "", Stock = 5, CategoryId = 1 };
        context.Products.Add(product);
        await context.SaveChangesAsync();

        var controller = new AdminController(context);
        var dto = new UpdateProductDto { Name = "Nowy", Description = "Nowy opis", Price = 20m, ImageUrl = "", Stock = 3, CategoryId = 1 };

        var result = await controller.UpdateProduct(product.Id, dto);

        Assert.IsType<NoContentResult>(result);
        Assert.Equal("Nowy", context.Products.Find(product.Id)!.Name);
    }

    [Fact]
    public async Task UpdateProduct_InvalidId_ReturnsNotFound()
    {
        var context = TestDbContextFactory.Create("AdminDb_UpdateNotFound");
        var controller = new AdminController(context);

        var result = await controller.UpdateProduct(999, new UpdateProductDto { Name = "X", Description = "D", Price = 1m, ImageUrl = "", Stock = 1, CategoryId = 1 });

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task DeleteProduct_ValidId_ReturnsNoContent()
    {
        var context = TestDbContextFactory.Create("AdminDb_Delete");
        var product = new Product { Name = "P", Description = "D", Price = 10m, ImageUrl = "", Stock = 1, CategoryId = 1 };
        context.Products.Add(product);
        await context.SaveChangesAsync();

        var controller = new AdminController(context);
        var result = await controller.DeleteProduct(product.Id);

        Assert.IsType<NoContentResult>(result);
        Assert.Null(context.Products.Find(product.Id));
    }

    [Fact]
    public async Task GetAllOrders_ReturnsAllOrders()
    {
        var context = TestDbContextFactory.Create("AdminDb_Orders");
        context.Orders.AddRange(
            new Order { UserId = "u1", TotalAmount = 100m, Status = OrderStatus.Pending, Items = new List<OrderItem>() },
            new Order { UserId = "u2", TotalAmount = 200m, Status = OrderStatus.Shipped, Items = new List<OrderItem>() }
        );
        await context.SaveChangesAsync();

        var controller = new AdminController(context);
        var result = await controller.GetAllOrders();

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(okResult.Value);
    }

    [Fact]
    public async Task UpdateOrderStatus_ValidId_ChangesStatus()
    {
        var context = TestDbContextFactory.Create("AdminDb_UpdateStatus");
        var order = new Order { UserId = "u1", TotalAmount = 100m, Status = OrderStatus.Pending, Items = new List<OrderItem>() };
        context.Orders.Add(order);
        await context.SaveChangesAsync();

        var controller = new AdminController(context);
        var result = await controller.UpdateOrderStatus(order.Id, new UpdateOrderStatusDto { Status = OrderStatus.Shipped });

        Assert.IsType<NoContentResult>(result);
        Assert.Equal(OrderStatus.Shipped, context.Orders.Find(order.Id)!.Status);
    }

    [Fact]
    public async Task UpdateOrderStatus_InvalidId_ReturnsNotFound()
    {
        var context = TestDbContextFactory.Create("AdminDb_StatusNotFound");
        var controller = new AdminController(context);

        var result = await controller.UpdateOrderStatus(999, new UpdateOrderStatusDto { Status = OrderStatus.Shipped });

        Assert.IsType<NotFoundResult>(result);
    }
}