namespace Shop.Application.DTOs;

public class CartSummaryDto
{
    public IReadOnlyList<CartItemDto> Items { get; set; } = Array.Empty<CartItemDto>();
    public int ItemCount => Items.Sum(i => i.Quantity);
    public decimal SubTotal => Items.Sum(i => i.LineTotal);
}
