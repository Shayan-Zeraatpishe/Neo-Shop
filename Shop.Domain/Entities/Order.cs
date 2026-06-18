using Shop.Domain.Common;
using Shop.Domain.Enums;

namespace Shop.Domain.Entities;

public class Order : BaseEntity
{
    public string OrderNumber { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string? Address { get; set; }
    // Split addresses for admin display. Checkout provides only one address,
    // so we map it to both shipping and billing.
    public string? ShippingAddress { get; set; }
    public string? BillingAddress { get; set; }
    public decimal SubTotal { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public OrderStatus Status { get; set; } = OrderStatus.Pending;
    public DateTime? PaidAt { get; set; }
    // Payment metadata (provider/gateway and reference id when available).
    public string? PaymentProvider { get; set; }
    public string? PaymentReferenceId { get; set; }
    public string? Notes { get; set; }

    public ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();

    public ICollection<OrderStatusHistory> StatusHistory { get; set; } = new List<OrderStatusHistory>();
}
