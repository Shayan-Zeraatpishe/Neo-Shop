using Shop.Domain.Enums;

namespace Shop.Application.DTOs;

public class AdminOrderFilterDto
{
    public int? OrderId { get; set; }
    public string? Customer { get; set; }
    public OrderStatus? Status { get; set; }
    public PaymentStatus? PaymentStatus { get; set; }

    public DateTime? FromDateUtc { get; set; }
    public DateTime? ToDateUtc { get; set; }

    public decimal? MinTotalAmount { get; set; }
    public decimal? MaxTotalAmount { get; set; }

    // Sorting
    public string SortBy { get; set; } = "CreatedAt";
    public bool SortDescending { get; set; } = true;
}

