using Shop.Domain.Common;

namespace Shop.Domain.Entities;

public class Product : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? ShortDescription { get; set; }
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public decimal? DiscountPrice { get; set; }
    public string? MainImageUrl { get; set; }
    public string? Brand { get; set; }
    public string? Specifications { get; set; }
    public string? DigitalFileName { get; set; }
    public string? DigitalFileOriginalName { get; set; }

    // SEO fields (Admin-managed).
    public string? SeoTitle { get; set; }
    public string? SeoDescription { get; set; }
    public string? SeoKeywords { get; set; }
    public string? SeoCanonicalUrl { get; set; }

    public bool IsActive { get; set; } = true;
    public bool IsFeatured { get; set; }
    public bool IsSpecialOffer { get; set; }
    public int Stock { get; set; } = 999;
    public int CategoryId { get; set; }

    public Category Category { get; set; } = null!;
    public ICollection<ProductImage> Images { get; set; } = new List<ProductImage>();
    public ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();
    public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    public ICollection<Comment> Comments { get; set; } = new List<Comment>();

    public decimal EffectivePrice => DiscountPrice ?? Price;
}
