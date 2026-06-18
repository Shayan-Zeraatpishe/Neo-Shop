using System.ComponentModel.DataAnnotations;
using Shop.Domain.Entities;

namespace Shop.web.ViewModels;

public class LoginViewModel
{
    [Required(ErrorMessage = "شماره موبایل یا ایمیل الزامی است")]
    [Display(Name = "شماره موبایل یا ایمیل")]
    public string UserName { get; set; } = string.Empty;

    [Required(ErrorMessage = "رمز عبور الزامی است")]
    [DataType(DataType.Password)]
    [Display(Name = "رمز عبور")]
    public string Password { get; set; } = string.Empty;

    [Display(Name = "مرا به خاطر بسپار")]
    public bool RememberMe { get; set; } = true;
}

public class RegisterViewModel
{
    [Required(ErrorMessage = "نام و نام خانوادگی الزامی است")]
    [Display(Name = "نام و نام خانوادگی")]
    public string FullName { get; set; } = string.Empty;

    [Required(ErrorMessage = "شماره موبایل الزامی است")]
    [Phone(ErrorMessage = "شماره موبایل معتبر نیست")]
    [Display(Name = "شماره موبایل")]
    public string PhoneNumber { get; set; } = string.Empty;

    [EmailAddress(ErrorMessage = "ایمیل معتبر نیست")]
    [Display(Name = "ایمیل")]
    public string? Email { get; set; }

    [Required(ErrorMessage = "رمز عبور الزامی است")]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "رمز عبور باید حداقل ۶ کاراکتر باشد")]
    [DataType(DataType.Password)]
    [Display(Name = "رمز عبور")]
    public string Password { get; set; } = string.Empty;

    [DataType(DataType.Password)]
    [Compare(nameof(Password), ErrorMessage = "رمز عبور و تکرار آن یکسان نیست")]
    [Display(Name = "تکرار رمز عبور")]
    public string ConfirmPassword { get; set; } = string.Empty;
}

public class AccountDashboardViewModel
{
    public string FullName { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? PhoneNumber { get; set; }
    public IReadOnlyList<Order> Orders { get; set; } = Array.Empty<Order>();
}

public class ProfileViewModel
{
    [Required(ErrorMessage = "نام و نام خانوادگی الزامی است")]
    [Display(Name = "نام و نام خانوادگی")]
    public string FullName { get; set; } = string.Empty;

    [EmailAddress(ErrorMessage = "ایمیل معتبر نیست")]
    [Display(Name = "ایمیل")]
    public string? Email { get; set; }

    [Required(ErrorMessage = "شماره موبایل الزامی است")]
    [Phone(ErrorMessage = "شماره موبایل معتبر نیست")]
    [Display(Name = "شماره موبایل")]
    public string PhoneNumber { get; set; } = string.Empty;
}
