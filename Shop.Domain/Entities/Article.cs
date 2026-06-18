using System.ComponentModel.DataAnnotations;
using Shop.Domain.Common;

namespace Shop.Domain.Entities;

public class Article : BaseEntity
{
    [Required(ErrorMessage = "عنوان الزامی است")]
    public string Title { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? Summary { get; set; }
    [Required(ErrorMessage = "محتوا الزامی است")]
    public string Content { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
    public bool IsPublished { get; set; } = true;
    public DateTime? PublishedAt { get; set; }
}
