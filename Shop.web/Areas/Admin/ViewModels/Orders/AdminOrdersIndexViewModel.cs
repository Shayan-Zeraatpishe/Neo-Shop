using Shop.Domain.Enums;

namespace Shop.web.Areas.Admin.ViewModels.Orders;

public class AdminOrdersIndexViewModel
{
    public AdminOrderFilterViewModel Filter { get; set; } = new();
    public IReadOnlyList<AdminOrderListItemViewModel> Orders { get; set; } = Array.Empty<AdminOrderListItemViewModel>();
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => PageSize == 0 ? 0 : (int)Math.Ceiling(TotalCount / (double)PageSize);

    public bool HasPrevious => PageNumber > 1;
    public bool HasNext => PageNumber < TotalPages;
}

