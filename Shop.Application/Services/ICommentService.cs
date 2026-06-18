using Shop.Application.DTOs;

namespace Shop.Application.Services;

public interface ICommentService
{
    Task<IReadOnlyList<ProductCommentDto>> GetApprovedProductCommentsAsync(int productId, CancellationToken cancellationToken = default);
    Task<(bool Success, string? Error)> CreateProductCommentAsync(CreateProductCommentDto dto, CancellationToken cancellationToken = default);
    Task<PagedResult<AdminCommentListItemDto>> GetAdminCommentsAsync(AdminCommentFilterDto filter, int pageNumber, int pageSize, CancellationToken cancellationToken = default);
    Task<AdminCommentDetailsDto?> GetAdminCommentDetailsAsync(int commentId, CancellationToken cancellationToken = default);
    Task<bool> ApproveCommentAsync(int commentId, CancellationToken cancellationToken = default);
    Task<bool> RejectCommentAsync(int commentId, CancellationToken cancellationToken = default);
    Task<bool> DeleteCommentAsync(int commentId, CancellationToken cancellationToken = default);
    Task<bool> ReplyToCommentAsync(int commentId, string reply, CancellationToken cancellationToken = default);
}
