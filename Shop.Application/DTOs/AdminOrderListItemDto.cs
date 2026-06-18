using Shop.Domain.Enums;

namespace Shop.Application.DTOs;

public class AdminOrderListItemDto
{
    public int Id { get; set; }
    public string OrderNumber { get; set; } = string.Empty;

    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;

    public OrderStatus Status { get; set; }
    public PaymentStatus PaymentStatus { get; set; }

    public DateTime CreatedAtUtc { get; set; }
    public DateTime? PaidAtUtc { get; set; }

    public decimal TotalAmount { get; set; }
}

