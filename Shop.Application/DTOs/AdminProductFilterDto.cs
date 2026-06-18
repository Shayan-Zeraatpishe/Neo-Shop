namespace Shop.Application.DTOs;

public class AdminProductFilterDto
{
    public string? SearchTerm { get; set; }
    public bool? IsActive { get; set; }
    public int? CategoryId { get; set; }

    // Sorting
    public string SortBy { get; set; } = "CreatedAt";
    public bool SortDescending { get; set; } = true;
}

