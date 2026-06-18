using Shop.Application.DTOs;
using Shop.Domain.Entities;
using Shop.Domain.Enums;

namespace Shop.Application.Interfaces;

public interface ICommentRepository : IRepository<Comment>
{
    Task<IReadOnlyList<ProductCommentDto>> GetApprovedByProductIdAsync(int productId, CancellationToken cancellationToken = default);
    Task<PagedResult<AdminCommentListItemDto>> GetAdminCommentsAsync(
        AdminCommentFilterDto filter,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default);
    Task<AdminCommentDetailsDto?> GetAdminCommentDetailsAsync(int commentId, CancellationToken cancellationToken = default);
    Task<bool> UpdateStatusAsync(int commentId, CommentStatus status, CancellationToken cancellationToken = default);
    Task<bool> SetAdminReplyAsync(int commentId, string? adminReply, CancellationToken cancellationToken = default);
}
