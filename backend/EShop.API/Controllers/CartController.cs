using EShop.Core.Entities;
using EShop.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EShop.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CartController : ControllerBase
{
    private readonly AppDbContext _context;

    public CartController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet("{userId}")]
    public async Task<ActionResult<List<CartItem>>> GetCart(string userId)
    {
        return await _context.CartItems
            .Include(c => c.Product)
            .Where(c => c.UserId == userId)
            .ToListAsync();
    }

    [HttpPost]
    public async Task<ActionResult<CartItem>> AddToCart(CartItem item)
    {
        var existing = await _context.CartItems
            .FirstOrDefaultAsync(c => c.ProductId == item.ProductId && c.UserId == item.UserId);

        if (existing != null)
            existing.Quantity += item.Quantity;
        else
            _context.CartItems.Add(item);

        await _context.SaveChangesAsync();
        return Ok(item);
    }
}