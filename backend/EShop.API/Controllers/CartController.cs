using EShop.API.DTOs;
using EShop.Core.Entities;
using EShop.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace EShop.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CartController : ControllerBase
{
    private readonly AppDbContext _context;

    public CartController(AppDbContext context)
    {
        _context = context;
    }

    private string GetUserId() => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

    [HttpGet]
    public async Task<ActionResult<IEnumerable<CartItem>>> GetCartItems()
    {
        var userId = GetUserId();
        var items = await _context.CartItems
            .Include(ci => ci.Product)
                .ThenInclude(p => p.Category)
            .Where(ci => ci.UserId == userId)
            .ToListAsync();

        return Ok(items);
    }

    [HttpGet("item/{id}")]
    public async Task<ActionResult<CartItem>> GetCartItem(int id)
    {
        var userId = GetUserId();
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
        var userId = GetUserId();
        var product = await _context.Products.FindAsync(dto.ProductId);
        if(product == null)
            return NotFound("Produkt nie został znaleziony.");

        var existingItem = await _context.CartItems
            .FirstOrDefaultAsync(ci => ci.ProductId == dto.ProductId && ci.UserId == userId);
        
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
            UserId = userId
        };

        _context.CartItems.Add(cartItem);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetCartItem), new { id = cartItem.Id }, cartItem);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateCartItem(int id, UpdateCartItemDto dto)
    {
        var userId = GetUserId();
        var item = await _context.CartItems
            .FirstOrDefaultAsync(ci => ci.Id == id && ci.UserId == userId);
        if(item == null)
            return NotFound();
        
        item.Quantity = dto.Quantity;
        await _context.SaveChangesAsync();

        return NoContent();
    }

    [HttpDelete("item/{id}")]
    public async Task<IActionResult> RemoveCartItem(int id)
    {
        var userId = GetUserId();
        var item = await _context.CartItems
            .FirstOrDefaultAsync(ci => ci.Id == id && ci.UserId == userId);
        
        if(item == null)
            return NotFound();

        _context.CartItems.Remove(item);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    [HttpDelete]
    public async Task<IActionResult> ClearCart()
    {
        var userId = GetUserId();
        var items = await _context.CartItems
            .Where(ci => ci.UserId == userId)
            .ToListAsync();
        
        _context.CartItems.RemoveRange(items);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}