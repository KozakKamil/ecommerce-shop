using EShop.API.Controllers;
using EShop.Core.Entities;
using EShop.Tests.Helpers;
using Microsoft.AspNetCore.Mvc;

namespace EShop.Tests.Controllers;

public class CategoriesControllerTests
{
    [Fact]
    public async Task GetCategories_ReturnsAllCategories()
    {
        var context = TestDbContextFactory.Create("CatDb_GetAll");
        context.Categories.AddRange(
            new Category { Name = "Elektronika", Description = "D" },
            new Category { Name = "Odzież", Description = "D" }
        );
        await context.SaveChangesAsync();

        var controller = new CategoriesController(context);
        var result = await controller.GetCaregories();

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var categories = Assert.IsAssignableFrom<IEnumerable<Category>>(okResult.Value);
        Assert.Equal(7, categories.Count());
    }

    [Fact]
    public async Task GetCategories_ReturnsSeededCategories()
    {
        var context = TestDbContextFactory.Create("CatDb_Seeded");

        var controller = new CategoriesController(context);
        var result = await controller.GetCaregories();

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var categories = Assert.IsAssignableFrom<IEnumerable<Category>>(okResult.Value);
        Assert.Equal(5, categories.Count()); // 5 kategorii z seed data
    }

    [Fact]
    public async Task GetCategories_ReturnsOnlyExpectedCategories_NoExtras()
    {
        var context = TestDbContextFactory.Create("CatDb_NoExtras");

        var controller = new CategoriesController(context);
        var result = await controller.GetCaregories();

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var categories = Assert.IsAssignableFrom<IEnumerable<Category>>(okResult.Value);
        Assert.Equal(5, categories.Count());
    }

    [Fact]
    public async Task GetCategory_ValidId_ReturnsOk()
    {
        var context = TestDbContextFactory.Create("CatDb_GetById");
        var category = new Category { Name = "Elektronika", Description = "D" };
        context.Categories.Add(category);
        await context.SaveChangesAsync();

        var controller = new CategoriesController(context);
        var result = await controller.GetCategory(category.Id);

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var cat = Assert.IsType<Category>(okResult.Value);
        Assert.Equal("Elektronika", cat.Name);
    }

    [Fact]
    public async Task GetCategory_InvalidId_ReturnsNotFound()
    {
        var context = TestDbContextFactory.Create("CatDb_NotFound");
        var controller = new CategoriesController(context);

        var result = await controller.GetCategory(999);

        Assert.IsType<NotFoundResult>(result.Result);
    }

    [Fact]
    public async Task GetCategory_IncludesProducts()
    {
        var context = TestDbContextFactory.Create("CatDb_WithProducts");
        var category = new Category { Name = "Elektronika", Description = "D" };
        context.Categories.Add(category);
        await context.SaveChangesAsync();
        context.Products.Add(new Product { Name = "Laptop", Description = "D", Price = 100m, ImageUrl = "", Stock = 5, CategoryId = category.Id });
        await context.SaveChangesAsync();

        var controller = new CategoriesController(context);
        var result = await controller.GetCategory(category.Id);

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var cat = Assert.IsType<Category>(okResult.Value);
        Assert.Single(cat.Products!);
    }
}