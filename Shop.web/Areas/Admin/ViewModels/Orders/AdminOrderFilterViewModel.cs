using System.ComponentModel.DataAnnotations;
using Shop.Domain.Enums;

namespace Shop.web.Areas.Admin.ViewModels.Orders;

public class AdminOrderFilterViewModel
{
    [Display(Name = "شناسه سفارش")]
    public int? OrderId { get; set; }

    [Display(Name = "مشتری (نام/ایمیل/موبایل)")]
    [StringLength(120)]
    public string? Customer { get; set; }

    [Display(Name = "وضعیت سفارش")]
    public OrderStatus? Status { get; set; }

    [Display(Name = "وضعیت پرداخت")]
    public PaymentStatus? PaymentStatus { get; set; }

    [Display(Name = "از تاریخ")]
    [DataType(DataType.Date)]
    public DateOnly? FromDate { get; set; }

    [Display(Name = "تا تاریخ")]
    [DataType(DataType.Date)]
    public DateOnly? ToDate { get; set; }

    [Display(Name = "حداقل مبلغ")]
    [Range(0, double.MaxValue)]
    public decimal? MinTotalAmount { get; set; }

    [Display(Name = "حداکثر مبلغ")]
    [Range(0, double.MaxValue)]
    public decimal? MaxTotalAmount { get; set; }

    // Paging
    [Range(1, 200)]
    public int PageNumber { get; set; } = 1;

    [Range(5, 200)]
    public int PageSize { get; set; } = 20;

    // Sorting
    public string SortBy { get; set; } = "CreatedAt";
    public bool SortDescending { get; set; } = true;
}

