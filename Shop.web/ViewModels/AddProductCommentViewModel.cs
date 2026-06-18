using System.ComponentModel.DataAnnotations;

namespace Shop.web.ViewModels;

public class AddProductCommentViewModel
{
    public int ProductId { get; set; }
    public string ProductSlug { get; set; } = string.Empty;

    [Required(ErrorMessage = "نام الزامی است.")]
    [StringLength(120, MinimumLength = 2, ErrorMessage = "نام باید بین ۲ تا ۱۲۰ کاراکتر باشد.")]
    [Display(Name = "نام")]
    public string AuthorName { get; set; } = string.Empty;

    [Required(ErrorMessage = "ایمیل الزامی است.")]
    [EmailAddress(ErrorMessage = "ایمیل معتبر نیست.")]
    [StringLength(256)]
    [Display(Name = "ایمیل")]
    public string AuthorEmail { get; set; } = string.Empty;

    [Required(ErrorMessage = "متن دیدگاه الزامی است.")]
    [StringLength(2000, MinimumLength = 3, ErrorMessage = "متن دیدگاه باید بین ۳ تا ۲۰۰۰ کاراکتر باشد.")]
    [Display(Name = "دیدگاه")]
    public string Content { get; set; } = string.Empty;
}
