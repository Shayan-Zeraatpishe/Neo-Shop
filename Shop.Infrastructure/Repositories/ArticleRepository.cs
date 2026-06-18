using Microsoft.EntityFrameworkCore;
using Shop.Application.Interfaces;
using Shop.Domain.Entities;
using Shop.Infrastructure.Data;

namespace Shop.Infrastructure.Repositories;

public class ArticleRepository : Repository<Article>, IArticleRepository
{
    public ArticleRepository(ApplicationDbContext context) : base(context)
    {
    }

    //public async Task<Article?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default)
    //    => await DbSet.FirstOrDefaultAsync(a => a.Slug == slug && a.IsPublished, cancellationToken);



    // برای جستجو با Id
    public async Task<Article?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        => await DbSet.FirstOrDefaultAsync(a => a.Id == id && a.IsPublished, cancellationToken);

    // برای جستجو با Slug
    public async Task<Article?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default)
        => await DbSet.FirstOrDefaultAsync(a => a.Slug == slug && a.IsPublished, cancellationToken);




    public async Task<IReadOnlyList<Article>> GetPublishedAsync(int count, CancellationToken cancellationToken = default)
        => await DbSet
            .AsNoTracking()
            .Where(a => a.IsPublished)
            .OrderByDescending(a => a.PublishedAt ?? a.CreatedAt)
            .Take(count)
            .ToListAsync(cancellationToken);
}
