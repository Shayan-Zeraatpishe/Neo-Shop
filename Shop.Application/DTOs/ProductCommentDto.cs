namespace Shop.Application.DTOs;

public class ProductCommentDto
{
    public int Id { get; set; }
    public string AuthorName { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string? AdminReply { get; set; }
    public DateTime CreatedAtUtc { get; set; }
}
