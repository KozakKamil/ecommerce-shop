using EShop.API.DTOs;
using EShop.Core.Entities;
using EShop.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EShop.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public  class OrderController : ControllerBase
{
    private readonly AppDbContext _context;
    public OrderController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet("{userId}")]
    public async Task<ActionResult<IEnumerable<Order>>> GetOrders(string userId)
    {
        var orders = await _context.Orders
        .Include(o => o.Items)
            .ThenInclude(oi => oi.Product)
        .Where(o => o.UserId == userId)
        .OrderByDescending(o => o.OrderDate)
        .ToListAsync();

        return Ok(orders);
    }

    [HttpGet("{userId}/order/{id}")]
    public async Task<ActionResult<Order>> GetOrder(string userId, int id)
    {
        var order = await _context.Orders
        .Include(o => o.Items)
        .ThenInclude(oi => oi.Product)
        .ThenInclude(p => p.Category)
        .FirstOrDefaultAsync(o => o.Id == id && o.UserId == userId);

        if (order == null)
        {
            return NotFound();
        }

        return Ok(order);
    }

    [HttpPost]
    public async Task<ActionResult<Order>> CreateOrder(CreateOrderDto dto)
    {
        var cartItems = await _context.CartItems
        .Include(ci => ci.Product)
        .Where(ci => ci.UserId == dto.UserId)
        .ToListAsync();

        if(!cartItems.Any())
            return BadRequest("Koszyk jest pusty");

        var order = new Order
        {
            UserId = dto.UserId,
            OrderDate = DateTime.UtcNow,
            Status = OrderStatus.Pending,
            Items = cartItems.Select(ci => new OrderItem
            {
                ProductId = ci.ProductId,
                Quantity = ci.Quantity,
                UnitPrice = ci.Product.Price
            }).ToList(),
            TotalAmount = cartItems.Sum(ci => ci.Product.Price * ci.Quantity)
        };
        _context.Orders.Add(order);
        _context.CartItems.RemoveRange(cartItems);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetOrder), new { userId = order.UserId, id = order.Id }, order);
    }

    [HttpPut("{id}/status")]
    public async Task<IActionResult> UpdateOrderStatus(int id, UpdateOrderStatusDto dto)
    {
        var order = await _context.Orders.FindAsync(id);
        if (order == null)
        {
            return NotFound();
        }

        order.Status = dto.Status;
        await _context.SaveChangesAsync();

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> CancelOrder(int id)
    {
        var order = await _context.Orders
        .Include(o=>o.Items)
        .FirstOrDefaultAsync(o => o.Id == id);

        if (order == null)
        {
            return NotFound();
        }

        if(order.Status == OrderStatus.Shipped || order.Status == OrderStatus.Delivered)
            return BadRequest("Nie można anulować zamówienia, które zostało już wysłane lub dostarczone.");

        order.Status = OrderStatus.Cancelled;
        await _context.SaveChangesAsync();

        return NoContent();
    }
}