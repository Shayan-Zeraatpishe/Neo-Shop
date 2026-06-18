using Shop.Application.DTOs;
using Shop.Application.Interfaces;
using Shop.Application.Services;
using Shop.Domain.Entities;
using Shop.Domain.Enums;

namespace Shop.Application.Services;

public class CommentService : ICommentService
{
    private readonly ICommentRepository _commentRepository;
    private readonly IProductRepository _productRepository;

    public CommentService(ICommentRepository commentRepository, IProductRepository productRepository)
    {
        _commentRepository = commentRepository;
        _productRepository = productRepository;
    }

    public Task<IReadOnlyList<ProductCommentDto>> GetApprovedProductCommentsAsync(int productId, CancellationToken cancellationToken = default)
        => _commentRepository.GetApprovedByProductIdAsync(productId, cancellationToken);

    public async Task<(bool Success, string? Error)> CreateProductCommentAsync(CreateProductCommentDto dto, CancellationToken cancellationToken = default)
    {
        var authorName = dto.AuthorName?.Trim() ?? string.Empty;
        var content = dto.Content?.Trim() ?? string.Empty;
        var email = string.IsNullOrWhiteSpace(dto.AuthorEmail) ? null : dto.AuthorEmail.Trim();

        if (authorName.Length < 2)
            return (false, "نام باید حداقل ۲ کاراکتر باشد.");

        if (content.Length < 3)
            return (false, "متن دیدگاه باید حداقل ۳ کاراکتر باشد.");

        if (content.Length > 2000)
            return (false, "متن دیدگاه نباید بیشتر از ۲۰۰۰ کاراکتر باشد.");

        var product = await _productRepository.GetByIdAsync(dto.ProductId, cancellationToken);
        if (product == null || !product.IsActive)
            return (false, "محصول یافت نشد.");

        var comment = new Comment
        {
            ProductId = dto.ProductId,
            UserId = dto.UserId,
            AuthorName = authorName,
            AuthorEmail = email,
            Content = content,
            Status = CommentStatus.Pending
        };

        await _commentRepository.AddAsync(comment, cancellationToken);
        await _commentRepository.SaveChangesAsync(cancellationToken);

        return (true, null);
    }

    public Task<PagedResult<AdminCommentListItemDto>> GetAdminCommentsAsync(
        AdminCommentFilterDto filter,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default)
        => _commentRepository.GetAdminCommentsAsync(filter, pageNumber, pageSize, cancellationToken);

    public Task<AdminCommentDetailsDto?> GetAdminCommentDetailsAsync(int commentId, CancellationToken cancellationToken = default)
        => _commentRepository.GetAdminCommentDetailsAsync(commentId, cancellationToken);

    public Task<bool> ApproveCommentAsync(int commentId, CancellationToken cancellationToken = default)
        => _commentRepository.UpdateStatusAsync(commentId, CommentStatus.Approved, cancellationToken);

    public Task<bool> RejectCommentAsync(int commentId, CancellationToken cancellationToken = default)
        => _commentRepository.UpdateStatusAsync(commentId, CommentStatus.Rejected, cancellationToken);

    public async Task<bool> DeleteCommentAsync(int commentId, CancellationToken cancellationToken = default)
    {
        var comment = await _commentRepository.GetByIdAsync(commentId, cancellationToken);
        if (comment == null) return false;

        await _commentRepository.DeleteAsync(comment, cancellationToken);
        return await _commentRepository.SaveChangesAsync(cancellationToken) > 0;
    }

    public async Task<bool> ReplyToCommentAsync(int commentId, string reply, CancellationToken cancellationToken = default)
    {
        var trimmed = reply?.Trim();
        if (string.IsNullOrWhiteSpace(trimmed))
            return false;

        return await _commentRepository.SetAdminReplyAsync(commentId, trimmed, cancellationToken);
    }
}
