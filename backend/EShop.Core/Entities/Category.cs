using System.Text.Json.Serialization;

namespace EShop.Core.Entities;

public class Category
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    //Navigation
    [JsonIgnore]
    public ICollection<Product> Products { get; set; } = new List<Product>();
}