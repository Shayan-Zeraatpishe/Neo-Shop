using Shop.Domain.Entities;

namespace Shop.Application.Interfaces;

public interface ICartRepository
{
    Task<IReadOnlyList<CartItem>> GetCartItemsAsync(string? userId, string? sessionId, CancellationToken cancellationToken = default);
    Task<CartItem?> GetCartItemAsync(string? userId, string? sessionId, int productId, CancellationToken cancellationToken = default);
    Task AddOrUpdateAsync(CartItem item, CancellationToken cancellationToken = default);
    Task RemoveAsync(CartItem item, CancellationToken cancellationToken = default);
    Task ClearAsync(string? userId, string? sessionId, CancellationToken cancellationToken = default);
    Task MergeSessionCartAsync(string sessionId, string userId, CancellationToken cancellationToken = default);
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
