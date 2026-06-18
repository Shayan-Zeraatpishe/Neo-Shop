using Microsoft.EntityFrameworkCore;
using Shop.Application.DTOs;
using Shop.Application.Interfaces;
using Shop.Domain.Entities;
using Shop.Domain.Enums;
using Shop.Infrastructure.Data;
using Shop.Infrastructure.Identity;

namespace Shop.Infrastructure.Repositories;

public class CommentRepository : Repository<Comment>, ICommentRepository
{
    public CommentRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IReadOnlyList<ProductCommentDto>> GetApprovedByProductIdAsync(int productId, CancellationToken cancellationToken = default)
        => await DbSet
            .AsNoTracking()
            .Where(c => c.ProductId == productId && c.Status == CommentStatus.Approved)
            .OrderByDescending(c => c.CreatedAt)
            .Select(c => new ProductCommentDto
            {
                Id = c.Id,
                AuthorName = c.AuthorName,
                Content = c.Content,
                AdminReply = c.AdminReply,
                CreatedAtUtc = c.CreatedAt
            })
            .ToListAsync(cancellationToken);

    public async Task<PagedResult<AdminCommentListItemDto>> GetAdminCommentsAsync(
        AdminCommentFilterDto filter,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        pageNumber = pageNumber < 1 ? 1 : pageNumber;
        pageSize = pageSize < 1 ? 20 : pageSize;

        var query = from comment in DbSet.AsNoTracking()
                    join product in Context.Products.AsNoTracking() on comment.ProductId equals product.Id
                    join user in Context.Set<ApplicationUser>().AsNoTracking() on comment.UserId equals user.Id into userJoin
                    from user in userJoin.DefaultIfEmpty()
                    select new { comment, product, user };

        if (filter.CommentId.HasValue)
            query = query.Where(x => x.comment.Id == filter.CommentId.Value);

        if (!string.IsNullOrWhiteSpace(filter.ProductName))
        {
            var term = filter.ProductName.Trim();
            query = query.Where(x => x.product.Name.Contains(term));
        }

        if (!string.IsNullOrWhiteSpace(filter.UserName))
        {
            var term = filter.UserName.Trim();
            query = query.Where(x =>
                x.comment.AuthorName.Contains(term) ||
                (x.user != null && x.user.FullName != null && x.user.FullName.Contains(term)) ||
                (x.user != null && x.user.UserName != null && x.user.UserName.Contains(term)));
        }

        if (filter.Status.HasValue)
            query = query.Where(x => x.comment.Status == filter.Status.Value);

        if (filter.FromDateUtc.HasValue)
            query = query.Where(x => x.comment.CreatedAt >= filter.FromDateUtc.Value);

        if (filter.ToDateUtc.HasValue)
            query = query.Where(x => x.comment.CreatedAt < filter.ToDateUtc.Value.AddDays(1));

        var totalCount = await query.CountAsync(cancellationToken);

        query = filter.SortBy switch
        {
            "ProductName" => filter.SortDescending
                ? query.OrderByDescending(x => x.product.Name)
                : query.OrderBy(x => x.product.Name),
            "AuthorName" => filter.SortDescending
                ? query.OrderByDescending(x => x.comment.AuthorName)
                : query.OrderBy(x => x.comment.AuthorName),
            "Status" => filter.SortDescending
                ? query.OrderByDescending(x => x.comment.Status)
                : query.OrderBy(x => x.comment.Status),
            _ => filter.SortDescending
                ? query.OrderByDescending(x => x.comment.CreatedAt)
                : query.OrderBy(x => x.comment.CreatedAt)
        };

        var items = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new AdminCommentListItemDto
            {
                Id = x.comment.Id,
                ProductId = x.product.Id,
                ProductName = x.product.Name,
                AuthorName = x.comment.AuthorName,
                AuthorEmail = x.comment.AuthorEmail,
                UserFullName = x.user != null ? x.user.FullName : null,
                UserPhone = x.user != null ? x.user.PhoneNumber : null,
                Content = x.comment.Content,
                Status = x.comment.Status,
                AdminReply = x.comment.AdminReply,
                CreatedAtUtc = x.comment.CreatedAt
            })
            .ToListAsync(cancellationToken);

        return new PagedResult<AdminCommentListItemDto>
        {
            Items = items,
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize
        };
    }

    public async Task<AdminCommentDetailsDto?> GetAdminCommentDetailsAsync(int commentId, CancellationToken cancellationToken = default)
    {
        var row = await (from comment in DbSet.AsNoTracking()
                         join product in Context.Products.AsNoTracking() on comment.ProductId equals product.Id
                         join user in Context.Set<ApplicationUser>().AsNoTracking() on comment.UserId equals user.Id into userJoin
                         from user in userJoin.DefaultIfEmpty()
                         where comment.Id == commentId
                         select new { comment, product, user })
            .FirstOrDefaultAsync(cancellationToken);

        if (row == null) return null;

        return new AdminCommentDetailsDto
        {
            Id = row.comment.Id,
            ProductId = row.product.Id,
            ProductName = row.product.Name,
            ProductSlug = row.product.Slug,
            UserId = row.comment.UserId,
            AuthorName = row.comment.AuthorName,
            AuthorEmail = row.comment.AuthorEmail,
            UserFullName = row.user?.FullName,
            UserEmail = row.user?.Email,
            UserPhone = row.user?.PhoneNumber,
            Content = row.comment.Content,
            Status = row.comment.Status,
            AdminReply = row.comment.AdminReply,
            CreatedAtUtc = row.comment.CreatedAt,
            UpdatedAtUtc = row.comment.UpdatedAt
        };
    }

    public async Task<bool> UpdateStatusAsync(int commentId, CommentStatus status, CancellationToken cancellationToken = default)
    {
        var comment = await DbSet.FirstOrDefaultAsync(c => c.Id == commentId, cancellationToken);
        if (comment == null) return false;

        comment.Status = status;
        comment.UpdatedAt = DateTime.UtcNow;
        return await SaveChangesAsync(cancellationToken) > 0;
    }

    public async Task<bool> SetAdminReplyAsync(int commentId, string? adminReply, CancellationToken cancellationToken = default)
    {
        var comment = await DbSet.FirstOrDefaultAsync(c => c.Id == commentId, cancellationToken);
        if (comment == null) return false;

        comment.AdminReply = adminReply;
        comment.UpdatedAt = DateTime.UtcNow;
        return await SaveChangesAsync(cancellationToken) > 0;
    }
}
