using Shop.Domain.Entities;

namespace Shop.web.ViewModels;

public class HomeIndexViewModel
{
    public IReadOnlyList<Category> Categories { get; set; } = Array.Empty<Category>();
    public IReadOnlyList<Product> FeaturedProducts { get; set; } = Array.Empty<Product>();
    public IReadOnlyList<Product> SpecialOfferProducts { get; set; } = Array.Empty<Product>();
    public IReadOnlyList<Article> LatestArticles { get; set; } = Array.Empty<Article>();
}
