using Shop.Domain.Enums;

namespace Shop.web.Helpers;

public static class OrderStatusHelper
{
    public static string GetPersianLabel(OrderStatus status) => status switch
    {
        OrderStatus.Pending => "در انتظار پرداخت",
        OrderStatus.Paid => "پرداخت شده",
        OrderStatus.Completed => "تکمیل شده",
        OrderStatus.Cancelled => "لغو شده",
        _ => status.ToString()
    };

    public static string GetCssClass(OrderStatus status) => status switch
    {
        OrderStatus.Pending => "type-yellow",
        OrderStatus.Paid => "type-blue",
        OrderStatus.Completed => "type-success",
        OrderStatus.Cancelled => "type-danger",
        _ => "type-yellow"
    };
}
