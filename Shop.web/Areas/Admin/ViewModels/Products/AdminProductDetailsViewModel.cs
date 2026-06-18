namespace Shop.web.Areas.Admin.ViewModels.Products;

public class AdminProductDetailsViewModel
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? Brand { get; set; }
    public int CategoryId { get; set; }
    public string? CategoryName { get; set; }

    public decimal Price { get; set; }
    public decimal? DiscountPrice { get; set; }
    public decimal EffectivePrice { get; set; }
    public int Stock { get; set; }

    public bool IsActive { get; set; }
    public bool IsFeatured { get; set; }
    public bool IsSpecialOffer { get; set; }

    public string? ShortDescription { get; set; }
    public string? Description { get; set; }
    public string? Specifications { get; set; }

    public string? MainImageUrl { get; set; }
    public IReadOnlyList<AdminProductImageViewModel> Images { get; set; } = Array.Empty<AdminProductImageViewModel>();

    public string? SeoTitle { get; set; }
    public string? SeoDescription { get; set; }
    public string? SeoKeywords { get; set; }
    public string? SeoCanonicalUrl { get; set; }

    public string? DigitalFileOriginalName { get; set; }
    public string? DigitalFileName { get; set; }

    public DateTime CreatedAtUtc { get; set; }
}

