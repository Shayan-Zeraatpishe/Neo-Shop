using Shop.Application.DTOs;
using Shop.Application.Interfaces;

namespace Shop.Application.Services;

public class CartService : ICartService
{
    private readonly ICartRepository _cartRepository;
    private readonly IProductRepository _productRepository;

    public CartService(ICartRepository cartRepository, IProductRepository productRepository)
    {
        _cartRepository = cartRepository;
        _productRepository = productRepository;
    }

    public async Task<CartSummaryDto> GetCartAsync(string? userId, string? sessionId, CancellationToken cancellationToken = default)
    {
        var items = await _cartRepository.GetCartItemsAsync(userId, sessionId, cancellationToken);
        return new CartSummaryDto
        {
            Items = items.Select(i => new CartItemDto
            {
                ProductId = i.ProductId,
                ProductName = i.Product.Name,
                ImageUrl = i.Product.MainImageUrl,
                Slug = i.Product.Slug,
                Quantity = i.Quantity,
                UnitPrice = i.Product.EffectivePrice,
                OriginalPrice = i.Product.DiscountPrice.HasValue ? i.Product.Price : null
            }).ToList()
        };
    }

    public async Task AddToCartAsync(string? userId, string? sessionId, int productId, int quantity, CancellationToken cancellationToken = default)
    {
        var product = await _productRepository.GetByIdAsync(productId, cancellationToken)
            ?? throw new InvalidOperationException("محصول یافت نشد.");

        if (!product.IsActive)
            throw new InvalidOperationException("محصول غیرفعال است.");

        var existing = await _cartRepository.GetCartItemAsync(userId, sessionId, productId, cancellationToken);
        if (existing != null)
        {
            existing.Quantity += quantity;
            existing.UpdatedAt = DateTime.UtcNow;
            await _cartRepository.AddOrUpdateAsync(existing, cancellationToken);
        }
        else
        {
            await _cartRepository.AddOrUpdateAsync(new Domain.Entities.CartItem
            {
                UserId = userId,
                SessionId = sessionId,
                ProductId = productId,
                Quantity = quantity
            }, cancellationToken);
        }

        await _cartRepository.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateQuantityAsync(string? userId, string? sessionId, int productId, int quantity, CancellationToken cancellationToken = default)
    {
        if (quantity <= 0)
        {
            await RemoveFromCartAsync(userId, sessionId, productId, cancellationToken);
            return;
        }

        var item = await _cartRepository.GetCartItemAsync(userId, sessionId, productId, cancellationToken)
            ?? throw new InvalidOperationException("آیتم در سبد خرید یافت نشد.");

        item.Quantity = quantity;
        item.UpdatedAt = DateTime.UtcNow;
        await _cartRepository.AddOrUpdateAsync(item, cancellationToken);
        await _cartRepository.SaveChangesAsync(cancellationToken);
    }

    public async Task RemoveFromCartAsync(string? userId, string? sessionId, int productId, CancellationToken cancellationToken = default)
    {
        var item = await _cartRepository.GetCartItemAsync(userId, sessionId, productId, cancellationToken);
        if (item == null) return;

        await _cartRepository.RemoveAsync(item, cancellationToken);
        await _cartRepository.SaveChangesAsync(cancellationToken);
    }

    public async Task ClearCartAsync(string? userId, string? sessionId, CancellationToken cancellationToken = default)
    {
        await _cartRepository.ClearAsync(userId, sessionId, cancellationToken);
        await _cartRepository.SaveChangesAsync(cancellationToken);
    }

    public Task MergeSessionCartAsync(string sessionId, string userId, CancellationToken cancellationToken = default)
        => _cartRepository.MergeSessionCartAsync(sessionId, userId, cancellationToken);
}
