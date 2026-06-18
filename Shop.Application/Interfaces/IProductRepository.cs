using Shop.Domain.Entities;
using Shop.Application.DTOs;

namespace Shop.Application.Interfaces;

public interface IProductRepository : IRepository<Product>
{
    Task<Product?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Product>> GetFeaturedAsync(int count, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Product>> SearchAsync(string? term, IEnumerable<int>? categoryIds, decimal? minPrice, decimal? maxPrice, CancellationToken cancellationToken = default);

    // Admin endpoints
    Task<PagedResult<AdminProductListItemDto>> GetAdminProductsAsync(
        AdminProductFilterDto filter,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default);

    Task<AdminProductDetailsDto?> GetAdminProductDetailsAsync(int productId, CancellationToken cancellationToken = default);

    // Used for slug uniqueness during admin create/edit.
    Task<Product?> GetBySlugIncludingInactiveAsync(string slug, CancellationToken cancellationToken = default);

    Task DeleteProductImagesAsync(IEnumerable<int> imageIds, CancellationToken cancellationToken = default);
}
