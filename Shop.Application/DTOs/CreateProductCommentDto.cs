namespace Shop.Application.DTOs;

public class CreateProductCommentDto
{
    public int ProductId { get; set; }
    public string? UserId { get; set; }
    public string AuthorName { get; set; } = string.Empty;
    public string? AuthorEmail { get; set; }
    public string Content { get; set; } = string.Empty;
}
