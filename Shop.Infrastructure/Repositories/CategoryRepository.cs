using Microsoft.EntityFrameworkCore;
using Shop.Application.Interfaces;
using Shop.Domain.Entities;
using Shop.Infrastructure.Data;

namespace Shop.Infrastructure.Repositories;

public class CategoryRepository : Repository<Category>, ICategoryRepository
{
    public CategoryRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<Category?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default)
        => await DbSet
            .Include(c => c.SubCategories.Where(s => s.IsActive))
            .FirstOrDefaultAsync(c => c.Slug == slug && c.IsActive, cancellationToken);

    public async Task<IReadOnlyList<Category>> GetRootCategoriesAsync(CancellationToken cancellationToken = default)
        => await DbSet
            .AsNoTracking()
            .Where(c => c.ParentCategoryId == null && c.IsActive)
            .Include(c => c.SubCategories.Where(s => s.IsActive))
            .OrderBy(c => c.DisplayOrder)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<int>> GetCategoryTreeIdsAsync(int categoryId, CancellationToken cancellationToken = default)
    {
        var all = await DbSet.AsNoTracking().Where(c => c.IsActive).ToListAsync(cancellationToken);
        var ids = new List<int> { categoryId };
        CollectDescendants(categoryId, all, ids);
        return ids;
    }

    private static void CollectDescendants(int parentId, List<Category> all, List<int> ids)
    {
        foreach (var child in all.Where(c => c.ParentCategoryId == parentId))
        {
            ids.Add(child.Id);
            CollectDescendants(child.Id, all, ids);
        }
    }
}
