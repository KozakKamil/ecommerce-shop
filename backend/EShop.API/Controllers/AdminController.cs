using EShop.API.DTOs;
using EShop.Core.Entities;
using EShop.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EShop.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class AdminController : ControllerBase
{
    private readonly AppDbContext _context;

    public AdminController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet("dashboard")]
    public async Task<ActionResult> GetDashboard()
    {
        var totalProducts = await _context.Products.CountAsync();
        var totalOrders = await _context.Orders.CountAsync();
        var totalRevenue = await _context.Orders
            .Where(o => o.Status != OrderStatus.Cancelled)
            .SumAsync(o => o.TotalAmount);

        var pendingOrders = await _context.Orders
            .CountAsync(o => o.Status == OrderStatus.Pending);

        return Ok(new
        {
            totalProducts,
            totalOrders,
            totalRevenue,
            pendingOrders
        });
    }

    [HttpGet("products")]
    public async Task<ActionResult> GetAllProducts()
    {
        var products = await _context.Products
            .Include(p => p.Category)
            .OrderByDescending(p => p.CreatedAt)
            .Select(p => new {
                p.Id, p.Name, p.Description, p.Price, 
                p.ImageUrl, p.Stock, p.CreatedAt, p.CategoryId,
                CategoryName = p.Category != null ? p.Category.Name : null
            })
            .ToListAsync();

        return Ok(products);
    }

    [HttpPost("products")]
    public async Task<ActionResult> CreateProduct(CreateProductDto dto)
    {
        var product = new Product
        {
            Name = dto.Name,
            Description = dto.Description,
            Price = dto.Price,
            ImageUrl = dto.ImageUrl,
            Stock = dto.Stock,
            CategoryId = dto.CategoryId
        };

        _context.Products.Add(product);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetAllProducts), new { id = product.Id }, product);
    }

    [HttpPut("products/{id}")]
    public async Task<ActionResult> UpdateProduct(int id, UpdateProductDto dto)
    {
        var product = await _context.Products.FindAsync(id);
        if (product == null) 
            return NotFound();

        product.Name = dto.Name;
        product.Description = dto.Description;
        product.Price = dto.Price;
        product.ImageUrl = dto.ImageUrl;
        product.Stock = dto.Stock;
        product.CategoryId = dto.CategoryId;

        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("products/{id}")]
    public async Task<IActionResult> DeleteProduct(int id)
    {
        var product = await _context.Products.FindAsync(id);
        if(product == null)
            return NotFound();

        _context.Products.Remove(product);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    [HttpGet("orders")]
    public async Task<ActionResult> GetAllOrders()
    {
        var orders = await _context.Orders
            .Include(o => o.Items)
                .ThenInclude(oi => oi.Product)
            .OrderByDescending(o => o.OrderDate)
            .ToListAsync();

        return Ok(orders);
    }

    [HttpPut("orders/{id}/status")]
    public async Task<ActionResult> UpdateOrderStatus(int id, UpdateOrderStatusDto dto)
    {
        var order = await _context.Orders.FindAsync(id);
        if (order == null)
            return NotFound();
        order.Status = dto.Status;
        await _context.SaveChangesAsync();
        
        return NoContent();
    }

    [HttpPost("products/{id}/image")]
    public async Task<ActionResult> UploadImage(int id,[FromForm] IFormFile file)
    {
        if(file == null || file.Length == 0)
            return BadRequest(new { message = "Brak pliku" });

        var allowed = new[] {".jpg", ".jpeg", ".png", ".webp"};
        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        if(!allowed.Contains(ext))
            return BadRequest(new { message = "Nieobsługiwany format pliku" });

        if(file.Length > 5 * 1024 * 1024)
            return BadRequest(new { message = "Plik jest zbyt duży (max 5MB)" });

        var product = await _context.Products.FindAsync(id);
        if(product == null) return NotFound();

        var uploadsDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "products");
        Directory.CreateDirectory(uploadsDir);

        var fileName = $"{id}_{Guid.NewGuid():N}{ext}";
        var filePath = Path.Combine(uploadsDir, fileName);

        using (var stream = new FileStream(filePath, FileMode.Create))
            await file.CopyToAsync(stream);

        if(!string.IsNullOrEmpty(product.ImageUrl) && product.ImageUrl.StartsWith("/images/"))
        {
            var oldPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", product.ImageUrl.TrimStart('/'));
            if(System.IO.File.Exists(oldPath))
                System.IO.File.Delete(oldPath);
        }

        product.ImageUrl = $"/images/products/{fileName}";
        await _context.SaveChangesAsync();

        return Ok(new { imageUrl = product.ImageUrl });
    }
}
