using Shop.Domain.Enums;

namespace Shop.web.Areas.Admin.ViewModels.Orders;

public class AdminOrderDetailsViewModel
{
    public int Id { get; set; }
    public string OrderNumber { get; set; } = string.Empty;

    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;

    public string? ShippingAddress { get; set; }
    public string? BillingAddress { get; set; }
    public string? AddressFallback { get; set; }

    public decimal SubTotal { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal TotalAmount { get; set; }

    public OrderStatus Status { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public DateTime? PaidAtUtc { get; set; }

    public PaymentStatus PaymentStatus { get; set; }
    public string? PaymentProvider { get; set; }
    public string? PaymentReferenceId { get; set; }

    public string? Notes { get; set; }

    public IReadOnlyList<AdminOrderItemViewModel> Items { get; set; } = Array.Empty<AdminOrderItemViewModel>();
    public IReadOnlyList<OrderStatusHistoryItemViewModel> StatusHistory { get; set; } = Array.Empty<OrderStatusHistoryItemViewModel>();

    // For status update form
    public OrderStatus? StatusToUpdate { get; set; }
}

