using Shop.Domain.Entities;

namespace Shop.Application.Interfaces;

public interface IArticleRepository : IRepository<Article>
{
    Task<Article?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default);

   // Task<Article?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    //GetByIdAsync

    Task<Article?> GetByIdAsync(int id, CancellationToken cancellationToken = default);


    Task<IReadOnlyList<Article>> GetPublishedAsync(int count, CancellationToken cancellationToken = default);
}
