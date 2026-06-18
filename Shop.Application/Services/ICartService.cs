using Shop.Application.DTOs;

namespace Shop.Application.Services;

public interface ICartService
{
    Task<CartSummaryDto> GetCartAsync(string? userId, string? sessionId, CancellationToken cancellationToken = default);
    Task AddToCartAsync(string? userId, string? sessionId, int productId, int quantity, CancellationToken cancellationToken = default);
    Task UpdateQuantityAsync(string? userId, string? sessionId, int productId, int quantity, CancellationToken cancellationToken = default);
    Task RemoveFromCartAsync(string? userId, string? sessionId, int productId, CancellationToken cancellationToken = default);
    Task ClearCartAsync(string? userId, string? sessionId, CancellationToken cancellationToken = default);
    Task MergeSessionCartAsync(string sessionId, string userId, CancellationToken cancellationToken = default);
}
