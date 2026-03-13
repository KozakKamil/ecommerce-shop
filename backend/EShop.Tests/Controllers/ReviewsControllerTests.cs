using EShop.API.Controllers;
using EShop.API.DTOs;
using EShop.Core.Entities;
using EShop.Tests.Helpers;
using Microsoft.AspNetCore.Mvc;
using EShop.Infrastructure.Data;

namespace EShop.Tests.Controllers;

public class ReviewsControllerTests
{
    private static (AppDbContext ctx, Product product, AppUser user) SetupDb(string dbName)
    {
        var ctx = TestDbContextFactory.Create(dbName);

        var category = new Category { Name = "Kat", Description = "" };
        ctx.Categories.Add(category);
        ctx.SaveChanges();

        var product = new Product { Name = "Prod", Description = "Opis", Price = 10m, ImageUrl = "", Stock = 5, CategoryId = category.Id };
        ctx.Products.Add(product);

        var user = new AppUser { Id = "user-1", FirstName = "Jan", LastName = "Kowalski", UserName = "jan@test.com", Email = "jan@test.com" };
        ctx.Users.Add(user);
        ctx.SaveChanges();

        return (ctx, product, user);
    }

    [Fact]
    public async Task GetReviews_NoReviews_ReturnsEmptyList()
    {
        var (ctx, product, _) = SetupDb("Reviews_GetEmpty");
        var controller = new ReviewsController(ctx);

        var result = await controller.GetReviews(product.Id);

        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(ok.Value);
    }

    [Fact]
    public async Task GetReviews_WithReviews_ReturnsReviews()
    {
        var (ctx, product, user) = SetupDb("Reviews_GetWithData");
        ctx.Reviews.Add(new Review { ProductId = product.Id, UserId = user.Id, Rating = 5, Comment = "Świetny!" });
        await ctx.SaveChangesAsync();

        var controller = new ReviewsController(ctx);

        var result = await controller.GetReviews(product.Id);

        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(ok.Value);
    }

    [Fact]
    public async Task CreateReview_ValidData_ReturnsCreated()
    {
        var (ctx, product, user) = SetupDb("Reviews_Create");
        var controller = new ReviewsController(ctx);
        ControllerTestHelper.SetUser(controller, user.Id);

        var result = await controller.CreateReview(product.Id, new CreateReviewDto { Rating = 4, Comment = "Dobry produkt" });

        Assert.IsType<CreatedAtActionResult>(result);
        Assert.Equal(1, ctx.Reviews.Count());
    }

    [Fact]
    public async Task CreateReview_InvalidRating_ReturnsBadRequest()
    {
        var (ctx, product, user) = SetupDb("Reviews_BadRating");
        var controller = new ReviewsController(ctx);
        ControllerTestHelper.SetUser(controller, user.Id);

        var result = await controller.CreateReview(product.Id, new CreateReviewDto { Rating = 6, Comment = "Test" });

        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task CreateReview_ProductNotFound_ReturnsNotFound()
    {
        var (ctx, _, user) = SetupDb("Reviews_NoProduct");
        var controller = new ReviewsController(ctx);
        ControllerTestHelper.SetUser(controller, user.Id);

        var result = await controller.CreateReview(9999, new CreateReviewDto { Rating = 3, Comment = "Test" });

        Assert.IsType<NotFoundObjectResult>(result);
    }

    [Fact]
    public async Task CreateReview_DuplicateReview_ReturnsBadRequest()
    {
        var (ctx, product, user) = SetupDb("Reviews_Duplicate");
        ctx.Reviews.Add(new Review { ProductId = product.Id, UserId = user.Id, Rating = 3, Comment = "Już dodane" });
        await ctx.SaveChangesAsync();

        var controller = new ReviewsController(ctx);
        ControllerTestHelper.SetUser(controller, user.Id);

        var result = await controller.CreateReview(product.Id, new CreateReviewDto { Rating = 5, Comment = "Druga próba" });

        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task DeleteReview_OwnReview_ReturnsNoContent()
    {
        var (ctx, product, user) = SetupDb("Reviews_Delete");
        var review = new Review { ProductId = product.Id, UserId = user.Id, Rating = 4, Comment = "Do usunięcia" };
        ctx.Reviews.Add(review);
        await ctx.SaveChangesAsync();

        var controller = new ReviewsController(ctx);
        ControllerTestHelper.SetUser(controller, user.Id);

        var result = await controller.DeleteReview(product.Id, review.Id);

        Assert.IsType<NoContentResult>(result);
        Assert.Equal(0, ctx.Reviews.Count());
    }

    [Fact]
    public async Task DeleteReview_OtherUsersReview_ReturnsNotFound()
    {
        var (ctx, product, user) = SetupDb("Reviews_DeleteForbidden");
        var review = new Review { ProductId = product.Id, UserId = "other-user", Rating = 4, Comment = "Nie moje" };
        ctx.Reviews.Add(review);
        await ctx.SaveChangesAsync();

        var controller = new ReviewsController(ctx);
        ControllerTestHelper.SetUser(controller, user.Id);

        var result = await controller.DeleteReview(product.Id, review.Id);

        Assert.IsType<NotFoundResult>(result);
    }
}