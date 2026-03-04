using EShop.Core.Entities;

namespace EShop.API.DTOs;

public class UpdateOrderStatusDto
{
    public OrderStatus Status { get; set; }
}