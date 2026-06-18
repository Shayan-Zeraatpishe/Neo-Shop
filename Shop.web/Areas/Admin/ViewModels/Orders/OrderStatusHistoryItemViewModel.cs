using Shop.Domain.Enums;

namespace Shop.web.Areas.Admin.ViewModels.Orders;

public class OrderStatusHistoryItemViewModel
{
    public OrderStatus OldStatus { get; set; }
    public OrderStatus NewStatus { get; set; }
    public DateTime ChangedAtUtc { get; set; }
    public string? ChangedByUserId { get; set; }
    public string? Note { get; set; }
}

