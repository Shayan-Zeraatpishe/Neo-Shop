using Shop.Domain.Common;
using Shop.Domain.Enums;

namespace Shop.Domain.Entities;

public class Comment : BaseEntity
{
    public int ProductId { get; set; }
    public string? UserId { get; set; }
    public string AuthorName { get; set; } = string.Empty;
    public string? AuthorEmail { get; set; }
    public string Content { get; set; } = string.Empty;
    public CommentStatus Status { get; set; } = CommentStatus.Pending;
    public string? AdminReply { get; set; }

    public Product Product { get; set; } = null!;
}
