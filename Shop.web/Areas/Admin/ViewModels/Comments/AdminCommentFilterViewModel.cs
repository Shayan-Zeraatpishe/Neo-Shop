using System.ComponentModel.DataAnnotations;
using Shop.Domain.Enums;

namespace Shop.web.Areas.Admin.ViewModels.Comments;

public class AdminCommentFilterViewModel
{
    [Display(Name = "شناسه دیدگاه")]
    public int? CommentId { get; set; }

    [Display(Name = "نام محصول")]
    [StringLength(120)]
    public string? ProductName { get; set; }

    [Display(Name = "نام کاربر")]
    [StringLength(120)]
    public string? UserName { get; set; }

    [Display(Name = "وضعیت")]
    public CommentStatus? Status { get; set; }

    [Display(Name = "از تاریخ")]
    [DataType(DataType.Date)]
    public DateOnly? FromDate { get; set; }

    [Display(Name = "تا تاریخ")]
    [DataType(DataType.Date)]
    public DateOnly? ToDate { get; set; }

    [Range(1, 200)]
    public int PageNumber { get; set; } = 1;

    [Range(5, 200)]
    public int PageSize { get; set; } = 20;

    public string SortBy { get; set; } = "CreatedAt";
    public bool SortDescending { get; set; } = true;
}
