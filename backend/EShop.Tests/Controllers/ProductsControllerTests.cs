using EShop.API.Controllers;
using EShop.Core.Entities;
using EShop.Tests.Helpers;
using Microsoft.AspNetCore.Mvc;

namespace EShop.Tests.Controllers;

public class ProductsControllerTests
{
    [Fact]
    public async Task GetProducts_ReturnsOkResult()
    {
        var context = TestDbContextFactory.Create("ProductsDb_GetAll");
        context.Products.Add(new Product
        {
            Name = "Laptop",
            Description = "Opis",
            Price = 2999.99m,
            ImageUrl = "img.jpg",
            Stock = 10,
            CategoryId = 1
        });
        await context.SaveChangesAsync();

        var controller = new ProductsController(context);
        var result = await controller.GetProducts(null, null, 1, 10);

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(okResult.Value);
    }

    [Fact]
    public async Task GetProduct_WithValidId_ReturnsOk()
    {
        var context = TestDbContextFactory.Create("ProductsDb_GetById");
        var product = new Product
        {
            Name = "Telefon",
            Description = "Opis",
            Price = 1999.99m,
            ImageUrl = "img.jpg",
            Stock = 5,
            CategoryId = 1
        };
        context.Products.Add(product);
        await context.SaveChangesAsync();

        var controller = new ProductsController(context);
        var result = await controller.GetProduct(product.Id);

        Assert.IsType<OkObjectResult>(result);
    }

    [Fact]
    public async Task GetProduct_WithInvalidId_ReturnsNotFound()
    {
        var context = TestDbContextFactory.Create("ProductsDb_NotFound");
        var controller = new ProductsController(context);

        var result = await controller.GetProduct(999);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task CreateProduct_ReturnsCreatedAtAction()
    {
        var context = TestDbContextFactory.Create("ProductsDb_Create");
        var controller = new ProductsController(context);

        var product = new Product
        {
            Name = "Słuchawki",
            Description = "Bezprzewodowe",
            Price = 299.99m,
            ImageUrl = "img.jpg",
            Stock = 20,
            CategoryId = 1
        };
        var result = await controller.CreateProduct(product);

        var createdResult = Assert.IsType<CreatedAtActionResult>(result.Result);
        Assert.Equal("GetProduct", createdResult.ActionName);
    }

    [Fact]
    public async Task DeleteProduct_WithValidId_ReturnsNoContent()
    {
        var context = TestDbContextFactory.Create("ProductsDb_Delete");
        var product = new Product
        {
            Name = "Klawiatura",
            Description = "Mechaniczna",
            Price = 399.99m,
            ImageUrl = "img.jpg",
            Stock = 15,
            CategoryId = 1
        };
        context.Products.Add(product);
        await context.SaveChangesAsync();

        var controller = new ProductsController(context);
        var result = await controller.DeleteProduct(product.Id);

        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task DeleteProduct_WithInvalidId_ReturnsNotFound()
    {
        var context = TestDbContextFactory.Create("ProductsDb_DeleteNotFound");
        var controller = new ProductsController(context);

        var result = await controller.DeleteProduct(999);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task GetProducts_WithCategoryFilter_ReturnsFilteredProducts()
    {
        var context = TestDbContextFactory.Create("ProductsDb_Filter");
        context.Products.AddRange(new Product
        {
            Name = "Laptop", 
            Description = "Opis", 
            Price = 2999m, 
            ImageUrl = "", 
            Stock = 5, 
            CategoryId = 1
        },
        new Product
        {
            Name = "Koszulka", 
            Description = "Opis", 
            Price = 99m, 
            ImageUrl = "", 
            Stock = 10, 
            CategoryId = 2
        });

        await context.SaveChangesAsync();

        var controller = new ProductsController(context);
        var result = await controller.GetProducts(1, null, 1, 10);
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(okResult.Value);
    }

    [Fact]
    public async Task GetProducts_WithSearchQuery_ReturnsFilteredProducts()
    {
        var context = TestDbContextFactory.Create("ProductsDb_Search");
        context.Products.AddRange(
            new Product { Name = "Laptop HP", Description = "D", Price = 2999m, ImageUrl = "", Stock = 5, CategoryId = 1 },
            new Product { Name = "Koszulka", Description = "D", Price = 99m, ImageUrl = "", Stock = 10, CategoryId = 2 }
        );
        await context.SaveChangesAsync();

        var controller = new ProductsController(context);
        var result = await controller.GetProducts(null, "Laptop", 1, 10);

        var okResult = Assert.IsType<OkObjectResult>(result);
        var value = okResult.Value!;
        var totalCount = (int)value.GetType().GetProperty("totalCount")!.GetValue(value)!;
        Assert.Equal(1, totalCount);
    }

    [Fact]
    public async Task GetProducts_Pagination_ReturnsCorrectPage()
    {
        var context = TestDbContextFactory.Create("ProductsDb_Pagination");
        for (int i = 1; i <= 15; i++)
            context.Products.Add(new Product { Name = $"Produkt {i}", Description = "D", Price = 10m * i, ImageUrl = "", Stock = 1, CategoryId = 1 });
        await context.SaveChangesAsync();

        var controller = new ProductsController(context);
        var result = await controller.GetProducts(null, null, 2, 5); // strona 2, po 5

        var okResult = Assert.IsType<OkObjectResult>(result);
        var value = okResult.Value!;
        var totalCount = (int)value.GetType().GetProperty("totalCount")!.GetValue(value)!;
        Assert.Equal(15, totalCount);

        var items = value.GetType().GetProperty("items")!.GetValue(value) as System.Collections.IList;
        Assert.Equal(5, items!.Count);
    }

    [Fact]
    public async Task GetProducts_ReturnsCorrectTotalCount_WithSmallPageSize()
    {
        var context = TestDbContextFactory.Create("ProductsDb_TotalCount");
        for (int i = 1; i <= 3; i++)
            context.Products.Add(new Product { Name = $"P{i}", Description = "D", Price = 10m, ImageUrl = "", Stock = 1, CategoryId = 1 });
        await context.SaveChangesAsync();

        var controller = new ProductsController(context);
        var result = await controller.GetProducts(null, null, 1, 1); // pageSize=1

        var okResult = Assert.IsType<OkObjectResult>(result);
        var value = okResult.Value!;
        var totalCount = (int)value.GetType().GetProperty("totalCount")!.GetValue(value)!;
        Assert.Equal(3, totalCount); // totalCount = 3 mimo pageSize=1

        var items = value.GetType().GetProperty("items")!.GetValue(value) as System.Collections.IList;
        Assert.Single(items!);
    }
}