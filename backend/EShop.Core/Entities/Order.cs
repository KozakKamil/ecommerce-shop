namespace EShop.Core.Entities;

public class Order
{
    public int Id{get;set;}
    public string UserId {get;set;} = string.Empty;
    public DateTime OrderDate {get;set;} = DateTime.UtcNow;
    public decimal TotalAmount {get;set;}
    public OrderStatus Status {get;set;} = OrderStatus.Pending;
    public string? StripePaymentIntentId {get;set;}

    public ICollection<OrderItem> Items {get;set;} = new List<OrderItem>();
}

public class OrderItem
{
    public int Id {get;set;}
    public int Quantity {get;set;}
    public decimal UnitPrice {get;set;}
    public int OrderId {get;set;}
    public Order Order {get;set;} = null!;
    public int ProductId {get;set;}
    public Product Product {get;set;} = null!;
}

public enum OrderStatus
{
    Pending,
    PaymentReceived,
    Processing,
    Shipped,
    Delivered,
    Cancelled
}
