using Microsoft.AspNetCore.Mvc;
using Shop.Application.Interfaces;
using Shop.web.ViewModels;

namespace Shop.web.Controllers;

public class HomeController : Controller
{
    private readonly IProductRepository _productRepository;
    private readonly ICategoryRepository _categoryRepository;
    private readonly IArticleRepository _articleRepository;

    public HomeController(
        IProductRepository productRepository,
        ICategoryRepository categoryRepository,
        IArticleRepository articleRepository)
    {
        _productRepository = productRepository;
        _categoryRepository = categoryRepository;
        _articleRepository = articleRepository;
    }

    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var featured = await _productRepository.GetFeaturedAsync(12, cancellationToken);
        var model = new HomeIndexViewModel
        {
            Categories = await _categoryRepository.GetRootCategoriesAsync(cancellationToken),
            FeaturedProducts = featured,
            SpecialOfferProducts = featured.Where(p => p.IsSpecialOffer || p.DiscountPrice.HasValue).Take(6).ToList(),
            LatestArticles = await _articleRepository.GetPublishedAsync(3, cancellationToken)
        };
        return View(model);
    }

    [HttpGet]
    public IActionResult Search(string? q)
    {
        var term = string.IsNullOrWhiteSpace(q) ? null : q.Trim();
        return RedirectToAction("Index", "Shop", new { q = term });
    }

    public IActionResult About() => View();

    public IActionResult Contact() => View();

    public IActionResult Privacy() => View();

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error() => View(new Models.ErrorViewModel { RequestId = System.Diagnostics.Activity.Current?.Id ?? HttpContext.TraceIdentifier });
}
