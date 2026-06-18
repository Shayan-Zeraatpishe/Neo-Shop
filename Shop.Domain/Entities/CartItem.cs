using Shop.Domain.Common;

namespace Shop.Domain.Entities;

public class CartItem : BaseEntity
{
    public string? UserId { get; set; }
    public string? SessionId { get; set; }
    public int ProductId { get; set; }
    public int Quantity { get; set; } = 1;

    public Product Product { get; set; } = null!;
}
