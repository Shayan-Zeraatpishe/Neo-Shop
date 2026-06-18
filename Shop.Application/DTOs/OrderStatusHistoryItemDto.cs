using Shop.Domain.Enums;

namespace Shop.Application.DTOs;

public class OrderStatusHistoryItemDto
{
    public OrderStatus OldStatus { get; set; }
    public OrderStatus NewStatus { get; set; }
    public DateTime ChangedAtUtc { get; set; }
    public string? ChangedByUserId { get; set; }
    public string? Note { get; set; }
}

