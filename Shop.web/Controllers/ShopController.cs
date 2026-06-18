using Microsoft.AspNetCore.Mvc;
using Shop.Application.Interfaces;
using Shop.Domain.Entities;
using Shop.web.ViewModels;

namespace Shop.web.Controllers;

public class ShopController : Controller
{
    private readonly IProductRepository _productRepository;
    private readonly ICategoryRepository _categoryRepository;

    public ShopController(IProductRepository productRepository, ICategoryRepository categoryRepository)
    {
        _productRepository = productRepository;
        _categoryRepository = categoryRepository;
    }

    public async Task<IActionResult> Index(string? category, string? q, decimal? minPrice, decimal? maxPrice, CancellationToken cancellationToken)
    {
        q = string.IsNullOrWhiteSpace(q) ? null : q.Trim();

        Category? currentCategory = null;
        IReadOnlyList<int>? categoryIds = null;

        if (!string.IsNullOrEmpty(category))
        {
            currentCategory = await _categoryRepository.GetBySlugAsync(category, cancellationToken);
            if (currentCategory != null)
                categoryIds = await _categoryRepository.GetCategoryTreeIdsAsync(currentCategory.Id, cancellationToken);
        }

        var model = new ShopIndexViewModel
        {
            Products = await _productRepository.SearchAsync(q, categoryIds, minPrice, maxPrice, cancellationToken),
            Categories = await _categoryRepository.GetRootCategoriesAsync(cancellationToken),
            CurrentCategory = currentCategory,
            SearchTerm = q,
            MinPrice = minPrice,
            MaxPrice = maxPrice
        };

        return View(model);
    }
}
