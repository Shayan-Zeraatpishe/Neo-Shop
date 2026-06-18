using Shop.Domain.Entities;

namespace Shop.Application.Interfaces;

public interface IDownloadTokenRepository
{
    Task<DownloadToken?> GetByTokenAsync(string token, CancellationToken cancellationToken = default);
    Task AddAsync(DownloadToken token, CancellationToken cancellationToken = default);
    Task UpdateAsync(DownloadToken token, CancellationToken cancellationToken = default);
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
