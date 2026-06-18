using Shop.Application.DTOs;
using Shop.Domain.Entities;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace Shop.web.ViewModels;

public class OrderCheckoutViewModel
{
    [ValidateNever]
    public Order? Order { get; set; }
    public CheckoutDto Checkout { get; set; } = new();
}