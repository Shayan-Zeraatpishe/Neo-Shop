namespace Shop.web.Areas.Admin.ViewModels.Products;

public class AdminProductListItemViewModel
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? Brand { get; set; }

    public decimal Price { get; set; }
    public decimal? DiscountPrice { get; set; }
    public decimal EffectivePrice { get; set; }

    public int Stock { get; set; }
    public bool IsActive { get; set; }
    public bool IsFeatured { get; set; }
    public bool IsSpecialOffer { get; set; }

    public string? CategoryName { get; set; }
    public string? MainImageUrl { get; set; }
    public DateTime CreatedAtUtc { get; set; }
}

