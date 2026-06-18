using Shop.Domain.Enums;

namespace Shop.Application.DTOs;

public class AdminOrderDetailsDto
{
    public int Id { get; set; }
    public string OrderNumber { get; set; } = string.Empty;

    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;

    public string? ShippingAddress { get; set; }
    public string? BillingAddress { get; set; }
    public string? AddressFallback { get; set; }
    public string? Notes { get; set; }

    public decimal SubTotal { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal TotalAmount { get; set; }

    public OrderStatus Status { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public DateTime? PaidAtUtc { get; set; }

    public PaymentStatus PaymentStatus { get; set; }
    public string? PaymentProvider { get; set; }
    public string? PaymentReferenceId { get; set; }

    public IReadOnlyList<AdminOrderItemDto> Items { get; set; } = Array.Empty<AdminOrderItemDto>();
    public IReadOnlyList<OrderStatusHistoryItemDto> StatusHistory { get; set; } = Array.Empty<OrderStatusHistoryItemDto>();
}

