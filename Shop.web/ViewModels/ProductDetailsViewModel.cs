using Shop.Application.DTOs;
using Shop.Domain.Entities;

namespace Shop.web.ViewModels;

public class ProductDetailsViewModel
{
    public Product Product { get; set; } = null!;
    public IReadOnlyList<Product> RelatedProducts { get; set; } = Array.Empty<Product>();
    public IReadOnlyList<ProductCommentDto> Comments { get; set; } = Array.Empty<ProductCommentDto>();
    public AddProductCommentViewModel NewComment { get; set; } = new();
}
