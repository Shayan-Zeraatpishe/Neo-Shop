using Shop.Domain.Common;
using Shop.Domain.Enums;

namespace Shop.Domain.Entities;

public class OrderStatusHistory : BaseEntity
{
    public int OrderId { get; set; }

    public Order Order { get; set; } = null!;

    public OrderStatus OldStatus { get; set; }
    public OrderStatus NewStatus { get; set; }

    // Admin/user who triggered the change (if available).
    public string? ChangedByUserId { get; set; }

    public string? Note { get; set; }
}

