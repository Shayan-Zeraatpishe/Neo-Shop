using System.ComponentModel.DataAnnotations;
using Shop.Domain.Entities;

namespace Shop.web.Areas.Admin.ViewModels.Products;

public class AdminProductFilterViewModel
{
    [Display(Name = "جستجو")]
    [StringLength(120)]
    public string? SearchTerm { get; set; }

    [Display(Name = "وضعیت انتشار")]
    public bool? IsActive { get; set; }

    [Display(Name = "دسته‌بندی")]
    public int? CategoryId { get; set; }

    // Paging
    [Range(1, 200)]
    public int PageNumber { get; set; } = 1;

    [Range(5, 200)]
    public int PageSize { get; set; } = 20;

    // Sorting
    public string SortBy { get; set; } = "CreatedAt";
    public bool SortDescending { get; set; } = true;
}

