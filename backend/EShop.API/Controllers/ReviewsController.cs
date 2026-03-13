using EShop.API.DTOs;
using EShop.Core.Entities;
using EShop.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace EShop.API.Controllers;

[ApiController]
[Route("api/products/{productId}/reviews")]
public class ReviewsController : ControllerBase
{
    private readonly AppDbContext _context;
    public ReviewsController(AppDbContext context)
    {
        _context = context;
    }

    private string? GetUserId() => User.FindFirstValue(ClaimTypes.NameIdentifier);

    [HttpGet]
    public async Task<ActionResult> GetReviews(int productId)
    {
        var reviews = await _context.Reviews
            .Include(r => r.User)
            .Where(r => r.ProductId == productId)
            .OrderByDescending(r => r.CreatedAt)
            .Select(r => new
            {
                r.Id,
                r.Rating,
                r.Comment,
                r.CreatedAt,
                UserName = r.User.FirstName + " " + r.User.LastName
            })
            .ToListAsync();
        var avgRating = reviews.Any() ? reviews.Average(r => r.Rating) : 0;

        return Ok(new {reviews, avgRating, totalReviews = reviews.Count});
    }

    [HttpPost]
    [Authorize]
    public async Task<ActionResult> CreateReview(int productId, CreateReviewDto dto)
    {
        if (dto.Rating < 1 || dto.Rating > 5)
            return BadRequest("Ocena musi być między 1 a 5");

        var userId = GetUserId()!;

        var product = await _context.Products.FindAsync(productId);
        if (product == null)
            return NotFound("Produkt nie istnieje");
        
        var existing = await _context.Reviews
            .AnyAsync(r => r.ProductId == productId && r.UserId == userId);
        if (existing)
            return BadRequest("Użytkownik już dodał recenzję dla tego produktu");

        var review = new Review
        {
            ProductId = productId,
            UserId = userId,
            Rating = dto.Rating,
            Comment = dto.Comment,
        };

        _context.Reviews.Add(review);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetReviews), new { productId }, review);
    }

    [HttpDelete("{reviewId}")]
    [Authorize]
    public async Task<ActionResult> DeleteReview(int productId, int reviewId)
    {
        var userId = GetUserId()!;
        var review = await _context.Reviews
            .FirstOrDefaultAsync(r => r.Id == reviewId && r.ProductId == productId && r.UserId == userId);

        if (review == null)
            return NotFound();
        
        _context.Reviews.Remove(review);
        await _context.SaveChangesAsync();
        return NoContent();
    }
}