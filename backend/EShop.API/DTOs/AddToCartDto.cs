namespace EShop.API.DTOs;

public class AddToCartDto
{
    public int ProductId {get;set;}
    public int Quantity {get;set;} = 1;
    public string UserId {get;set;} = string.Empty;
}