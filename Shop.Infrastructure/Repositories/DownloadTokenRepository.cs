using Microsoft.EntityFrameworkCore;
using Shop.Application.Interfaces;
using Shop.Domain.Entities;
using Shop.Infrastructure.Data;

namespace Shop.Infrastructure.Repositories;

public class DownloadTokenRepository : IDownloadTokenRepository
{
    private readonly ApplicationDbContext _context;

    public DownloadTokenRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<DownloadToken?> GetByTokenAsync(string token, CancellationToken cancellationToken = default)
        => await _context.DownloadTokens
            .Include(t => t.OrderItem)
            .ThenInclude(i => i.Order)
            .Include(t => t.OrderItem)
            .ThenInclude(i => i.Product)
            .FirstOrDefaultAsync(t => t.Token == token, cancellationToken);

    public async Task AddAsync(DownloadToken token, CancellationToken cancellationToken = default)
        => await _context.DownloadTokens.AddAsync(token, cancellationToken);

    public Task UpdateAsync(DownloadToken token, CancellationToken cancellationToken = default)
    {
        token.UpdatedAt = DateTime.UtcNow;
        _context.DownloadTokens.Update(token);
        return Task.CompletedTask;
    }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        => _context.SaveChangesAsync(cancellationToken);
}
