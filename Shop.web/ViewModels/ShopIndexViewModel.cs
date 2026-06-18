using Shop.Domain.Entities;

namespace Shop.web.ViewModels;

public class ShopIndexViewModel
{
    public IReadOnlyList<Product> Products { get; set; } = Array.Empty<Product>();
    public IReadOnlyList<Category> Categories { get; set; } = Array.Empty<Category>();
    public Category? CurrentCategory { get; set; }
    public string? SearchTerm { get; set; }
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
}
