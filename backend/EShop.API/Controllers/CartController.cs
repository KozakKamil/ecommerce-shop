using EShop.API.DTOs;
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
    public async Task<ActionResult<IEnumerable<CartItem>>> GetCartItems(string userId)
    {
        var items = await _context.CartItems
            .Include(ci => ci.Product)
                .ThenInclude(p => p.Category)
            .Where(ci => ci.UserId == userId)
            .ToListAsync();

        return Ok(items);
    }

    [HttpGet("{userId}/item/{id}")]
    public async Task<ActionResult<CartItem>> GetCartItem(string userId, int id)
    {
        var item = await _context.CartItems
            .Include(ci => ci.Product)
                .ThenInclude(p => p.Category)
            .FirstOrDefaultAsync(ci => ci.Id == id && ci.UserId == userId);

        if(item == null)
            return NotFound();

        return Ok(item);
    }

    [HttpPost]
    public async Task<ActionResult<CartItem>> AddToCart(AddToCartDto dto)
    {
        var product = await _context.Products.FindAsync(dto.ProductId);
        if(product == null)
            return NotFound("Produkt nie został znaleziony.");

        var existingItem = await _context.CartItems
            .FirstOrDefaultAsync(ci => ci.ProductId == dto.ProductId && ci.UserId == dto.UserId);
        
        if(existingItem != null)
        {
            existingItem.Quantity += dto.Quantity;
            await _context.SaveChangesAsync();

            return Ok(existingItem);
        }

        var cartItem = new CartItem
        {
            ProductId = dto.ProductId,
            Quantity = dto.Quantity,
            UserId = dto.UserId
        };

        _context.CartItems.Add(cartItem);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetCartItem), new
        {
            userId = cartItem.UserId,
            id = cartItem.Id
        }, cartItem);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateCartItem(int id, UpdateCartItemDto dto)
    {
        var item = await _context.CartItems.FindAsync(id);
        if(item == null)
            return NotFound();
        
        item.Quantity = dto.Quantity;
        await _context.SaveChangesAsync();

        return NoContent();
    }

    [HttpDelete("{userId}/item/{id}")]
    public async Task<IActionResult> RemoveCartItem(string userId, int id)
    {
        var item = await _context.CartItems
            .FirstOrDefaultAsync(ci => ci.Id == id && ci.UserId == userId);
        
        if(item == null)
            return NotFound();

        _context.CartItems.Remove(item);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    [HttpDelete("{userId}")]
    public async Task<IActionResult> ClearCart(string userId)
    {
        var items = await _context.CartItems
            .Where(ci => ci.UserId == userId)
            .ToListAsync();
        
        _context.CartItems.RemoveRange(items);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}