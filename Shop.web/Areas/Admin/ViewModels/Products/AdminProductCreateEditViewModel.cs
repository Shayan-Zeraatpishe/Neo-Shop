using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace Shop.web.Areas.Admin.ViewModels.Products;

public class AdminProductCreateEditViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "نام محصول الزامی است")]
    [StringLength(200)]
    public string Name { get; set; } = string.Empty;

    [StringLength(500)]
    public string? ShortDescription { get; set; }

    public string? Description { get; set; }

    [Range(0, 999999999999)]
    public decimal Price { get; set; }

    [Range(0, 999999999999)]
    public decimal? DiscountPrice { get; set; }

    [StringLength(200)]
    public string? Brand { get; set; }

    [Required(ErrorMessage = "دسته‌بندی الزامی است")]
    public int CategoryId { get; set; }

    public bool IsActive { get; set; } = true;
    public bool IsFeatured { get; set; }
    public bool IsSpecialOffer { get; set; }

    [Range(0, int.MaxValue)]
    public int Stock { get; set; } = 0;

    [StringLength(2000)]
    public string? Specifications { get; set; }

    // SEO fields
    [StringLength(200)]
    public string? SeoTitle { get; set; }

    [StringLength(500)]
    public string? SeoDescription { get; set; }

    [StringLength(800)]
    public string? SeoKeywords { get; set; }

    [StringLength(500)]
    public string? SeoCanonicalUrl { get; set; }

    // Uploads
    public IFormFile? MainImageFile { get; set; }
    public List<IFormFile> GalleryImageFiles { get; set; } = new();
    public IFormFile? DigitalFile { get; set; }

    // Edit-only
    public string? MainImageUrl { get; set; }
    public string? DigitalFileOriginalName { get; set; }
    public string? DigitalFileName { get; set; }

    public List<AdminProductImageViewModel> ExistingImages { get; set; } = new();

    // Edit-only: image ids to delete
    public List<int> DeleteImageIds { get; set; } = new();
}

