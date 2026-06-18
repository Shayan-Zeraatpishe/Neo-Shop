using Shop.Domain.Enums;

namespace Shop.Application.DTOs;

public class AdminCommentFilterDto
{
    public int? CommentId { get; set; }
    public string? ProductName { get; set; }
    public string? UserName { get; set; }
    public CommentStatus? Status { get; set; }
    public DateTime? FromDateUtc { get; set; }
    public DateTime? ToDateUtc { get; set; }
    public string SortBy { get; set; } = "CreatedAt";
    public bool SortDescending { get; set; } = true;
}
