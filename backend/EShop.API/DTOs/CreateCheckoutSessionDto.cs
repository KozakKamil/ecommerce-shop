namespace EShop.API.DTOs;

public class CreateCheckoutSessionDto
{
    public string UserId { get; set; } = string.Empty;
}

public class CheckoutSessionResponseDto
{
    public string SessionId { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
}