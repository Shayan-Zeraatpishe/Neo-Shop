using Microsoft.EntityFrameworkCore;
using Shop.Application.Interfaces;
using Shop.Domain.Entities;
using Shop.Infrastructure.Data;

namespace Shop.Infrastructure.Repositories;

public class CartRepository : ICartRepository
{
    private readonly ApplicationDbContext _context;

    public CartRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<CartItem>> GetCartItemsAsync(string? userId, string? sessionId, CancellationToken cancellationToken = default)
    {
        var query = _context.CartItems.Include(c => c.Product).AsQueryable();

        if (!string.IsNullOrEmpty(userId))
            query = query.Where(c => c.UserId == userId);
        else if (!string.IsNullOrEmpty(sessionId))
            query = query.Where(c => c.SessionId == sessionId);
        else
            return Array.Empty<CartItem>();

        return await query.ToListAsync(cancellationToken);
    }

    public async Task<CartItem?> GetCartItemAsync(string? userId, string? sessionId, int productId, CancellationToken cancellationToken = default)
    {
        var query = _context.CartItems.AsQueryable();

        if (!string.IsNullOrEmpty(userId))
            query = query.Where(c => c.UserId == userId);
        else if (!string.IsNullOrEmpty(sessionId))
            query = query.Where(c => c.SessionId == sessionId);
        else
            return null;

        return await query.FirstOrDefaultAsync(c => c.ProductId == productId, cancellationToken);
    }

    public async Task AddOrUpdateAsync(CartItem item, CancellationToken cancellationToken = default)
    {
        if (item.Id == 0)
            await _context.CartItems.AddAsync(item, cancellationToken);
        else
            _context.CartItems.Update(item);
    }

    public Task RemoveAsync(CartItem item, CancellationToken cancellationToken = default)
    {
        _context.CartItems.Remove(item);
        return Task.CompletedTask;
    }

    public async Task ClearAsync(string? userId, string? sessionId, CancellationToken cancellationToken = default)
    {
        var items = await GetCartItemsAsync(userId, sessionId, cancellationToken);
        _context.CartItems.RemoveRange(items);
    }

    public async Task MergeSessionCartAsync(string sessionId, string userId, CancellationToken cancellationToken = default)
    {
        var sessionItems = await _context.CartItems
            .Where(c => c.SessionId == sessionId)
            .ToListAsync(cancellationToken);

        foreach (var sessionItem in sessionItems)
        {
            var userItem = await _context.CartItems
                .FirstOrDefaultAsync(c => c.UserId == userId && c.ProductId == sessionItem.ProductId, cancellationToken);

            if (userItem != null)
            {
                userItem.Quantity += sessionItem.Quantity;
                _context.CartItems.Remove(sessionItem);
            }
            else
            {
                sessionItem.UserId = userId;
                sessionItem.SessionId = null;
            }
        }

        await _context.SaveChangesAsync(cancellationToken);
    }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        => _context.SaveChangesAsync(cancellationToken);
}
