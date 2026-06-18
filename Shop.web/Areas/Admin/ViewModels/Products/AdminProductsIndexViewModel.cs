namespace Shop.web.Areas.Admin.ViewModels.Products;

public class AdminProductsIndexViewModel
{
    public AdminProductFilterViewModel Filter { get; set; } = new();

    public IReadOnlyList<AdminProductListItemViewModel> Products { get; set; } = Array.Empty<AdminProductListItemViewModel>();

    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => PageSize == 0 ? 0 : (int)Math.Ceiling(TotalCount / (double)PageSize);

    public bool HasPrevious => PageNumber > 1;
    public bool HasNext => PageNumber < TotalPages;

    public IReadOnlyList<Shop.Domain.Entities.Category> Categories { get; set; } = Array.Empty<Shop.Domain.Entities.Category>();
}

