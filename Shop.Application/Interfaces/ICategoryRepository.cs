using Shop.Domain.Entities;

namespace Shop.Application.Interfaces;

public interface ICategoryRepository : IRepository<Category>
{
    Task<Category?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Category>> GetRootCategoriesAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<int>> GetCategoryTreeIdsAsync(int categoryId, CancellationToken cancellationToken = default);
}
