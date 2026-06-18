using Microsoft.EntityFrameworkCore;
using Shop.Application.Interfaces;
using Shop.Application.DTOs;
using Shop.Domain.Entities;
using Shop.Infrastructure.Data;

namespace Shop.Infrastructure.Repositories;

public class ProductRepository : Repository<Product>, IProductRepository
{
    public ProductRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<Product?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default)
        => await DbSet
            .Include(p => p.Category)
            .Include(p => p.Images.OrderBy(i => i.DisplayOrder))
            .FirstOrDefaultAsync(p => p.Slug == slug && p.IsActive, cancellationToken);

    public async Task<Product?> GetBySlugIncludingInactiveAsync(string slug, CancellationToken cancellationToken = default)
        => await DbSet
            .Include(p => p.Category)
            .Include(p => p.Images.OrderBy(i => i.DisplayOrder))
            .FirstOrDefaultAsync(p => p.Slug == slug, cancellationToken);

    public override async Task<Product?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        => await DbSet
            .Include(p => p.Category)
            .Include(p => p.Images.OrderBy(i => i.DisplayOrder))
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

    public async Task<IReadOnlyList<Product>> GetFeaturedAsync(int count, CancellationToken cancellationToken = default)
        => await DbSet
            .AsNoTracking()
            .Where(p => p.IsActive && p.IsFeatured)
            .OrderByDescending(p => p.CreatedAt)
            .Take(count)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<Product>> SearchAsync(string? term, IEnumerable<int>? categoryIds, decimal? minPrice, decimal? maxPrice, CancellationToken cancellationToken = default)
    {
        var query = DbSet.AsNoTracking().Where(p => p.IsActive);

        if (!string.IsNullOrWhiteSpace(term))
        {
            var trimmedTerm = term.Trim();
            query = query.Where(p =>
                p.Name.Contains(trimmedTerm) ||
                (p.ShortDescription != null && p.ShortDescription.Contains(trimmedTerm)) ||
                (p.Description != null && p.Description.Contains(trimmedTerm)) ||
                (p.Category != null && p.Category.Name.Contains(trimmedTerm)));
        }

        if (categoryIds != null)
        {
            var idList = categoryIds.ToList();
            if (idList.Count > 0)
                query = query.Where(p => idList.Contains(p.CategoryId));
        }

        if (minPrice.HasValue)
            query = query.Where(p => (p.DiscountPrice ?? p.Price) >= minPrice.Value);

        if (maxPrice.HasValue)
            query = query.Where(p => (p.DiscountPrice ?? p.Price) <= maxPrice.Value);

        return await query.OrderByDescending(p => p.CreatedAt).ToListAsync(cancellationToken);
    }

    public async Task<PagedResult<AdminProductListItemDto>> GetAdminProductsAsync(
        AdminProductFilterDto filter,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        pageNumber = pageNumber < 1 ? 1 : pageNumber;
        pageSize = pageSize < 1 ? 20 : pageSize;

        var query = DbSet.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
        {
            var term = filter.SearchTerm.Trim();
            query = query.Where(p =>
                p.Name.Contains(term) ||
                (p.Description != null && p.Description.Contains(term)) ||
                p.Slug.Contains(term));
        }

        if (filter.IsActive.HasValue)
            query = query.Where(p => p.IsActive == filter.IsActive.Value);

        if (filter.CategoryId.HasValue)
            query = query.Where(p => p.CategoryId == filter.CategoryId.Value);

        var totalCount = await query.CountAsync(cancellationToken);

        query = ApplyAdminProductSort(query, filter);

        var skip = (pageNumber - 1) * pageSize;

        var items = await query
            .Skip(skip)
            .Take(pageSize)
            .Select(p => new AdminProductListItemDto
            {
                Id = p.Id,
                Name = p.Name,
                Slug = p.Slug,
                Price = p.Price,
                DiscountPrice = p.DiscountPrice,
                EffectivePrice = p.DiscountPrice ?? p.Price,
                Stock = p.Stock,
                IsActive = p.IsActive,
                IsFeatured = p.IsFeatured,
                IsSpecialOffer = p.IsSpecialOffer,
                Brand = p.Brand,
                CategoryName = p.Category != null ? p.Category.Name : null,
                MainImageUrl = p.MainImageUrl,
                CreatedAtUtc = p.CreatedAt
            })
            .ToListAsync(cancellationToken);

        return new PagedResult<AdminProductListItemDto>
        {
            Items = items,
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize
        };
    }

    public async Task<AdminProductDetailsDto?> GetAdminProductDetailsAsync(int productId, CancellationToken cancellationToken = default)
    {
        var product = await DbSet
            .AsNoTracking()
            .Include(p => p.Category)
            .Include(p => p.Images.OrderBy(i => i.DisplayOrder))
            .FirstOrDefaultAsync(p => p.Id == productId, cancellationToken);

        if (product == null) return null;

        return new AdminProductDetailsDto
        {
            Id = product.Id,
            Name = product.Name,
            Slug = product.Slug,
            ShortDescription = product.ShortDescription,
            Description = product.Description,
            Price = product.Price,
            DiscountPrice = product.DiscountPrice,
            Brand = product.Brand,
            Specifications = product.Specifications,
            Stock = product.Stock,
            IsActive = product.IsActive,
            IsFeatured = product.IsFeatured,
            IsSpecialOffer = product.IsSpecialOffer,
            CategoryId = product.CategoryId,
            CategoryName = product.Category?.Name,
            MainImageUrl = product.MainImageUrl,
            Images = product.Images
                .Select(i => new AdminProductImageDto
                {
                    Id = i.Id,
                    ImageUrl = i.ImageUrl,
                    DisplayOrder = i.DisplayOrder
                })
                .ToList(),
            SeoTitle = product.SeoTitle,
            SeoDescription = product.SeoDescription,
            SeoKeywords = product.SeoKeywords,
            SeoCanonicalUrl = product.SeoCanonicalUrl,
            DigitalFileOriginalName = product.DigitalFileOriginalName,
            DigitalFileName = product.DigitalFileName,
            CreatedAtUtc = product.CreatedAt
        };
    }

    private static IQueryable<Product> ApplyAdminProductSort(IQueryable<Product> query, AdminProductFilterDto filter)
    {
        var desc = filter.SortDescending;
        return filter.SortBy switch
        {
            "Name" => desc ? query.OrderByDescending(p => p.Name) : query.OrderBy(p => p.Name),
            "Price" => desc ? query.OrderByDescending(p => p.Price) : query.OrderBy(p => p.Price),
            "Stock" => desc ? query.OrderByDescending(p => p.Stock) : query.OrderBy(p => p.Stock),
            _ => desc ? query.OrderByDescending(p => p.CreatedAt) : query.OrderBy(p => p.CreatedAt)
        };
    }

    public async Task DeleteProductImagesAsync(IEnumerable<int> imageIds, CancellationToken cancellationToken = default)
    {
        var ids = imageIds?.Distinct().ToList() ?? new List<int>();
        if (ids.Count == 0) return;

        var images = await Context.Set<ProductImage>()
            .Where(i => ids.Contains(i.Id))
            .ToListAsync(cancellationToken);

        if (images.Count == 0) return;

        Context.RemoveRange(images);
        // SaveChanges is still controlled by caller (controller).
    }
}
