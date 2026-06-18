using System.ComponentModel.DataAnnotations;

namespace Shop.Application.DTOs;

public class CheckoutDto
{
    [Required(ErrorMessage = "وارد کردن نام اجباری است")]
    [Display(Name = "نام و نام خانوادگی")]
    public string FullName { get; set; } = string.Empty;

    [Required(ErrorMessage = "وارد کردن ایمیل اجباری است")]
    [EmailAddress(ErrorMessage = "ایمیل معتبر نیست")]
    [Display(Name = "ایمیل")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "وارد کردن شماره موبایل اجباری است")]
    [Phone(ErrorMessage = "شماره موبایل معتبر نیست")]
    [Display(Name = "شماره موبایل")]
    public string PhoneNumber { get; set; } = string.Empty;

    [Display(Name = "آدرس")]
    public string? Address { get; set; }

    [Display(Name = "یادداشت")]
    public string? Notes { get; set; }
}
