using Shop.Domain.Enums;

namespace Shop.Application.DTOs;

public class AdminCommentDetailsDto
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string ProductSlug { get; set; } = string.Empty;
    public string? UserId { get; set; }
    public string AuthorName { get; set; } = string.Empty;
    public string? AuthorEmail { get; set; }
    public string? UserFullName { get; set; }
    public string? UserEmail { get; set; }
    public string? UserPhone { get; set; }
    public string Content { get; set; } = string.Empty;
    public CommentStatus Status { get; set; }
    public string? AdminReply { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public DateTime? UpdatedAtUtc { get; set; }
}
