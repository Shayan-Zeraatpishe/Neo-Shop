using Microsoft.AspNetCore.Mvc;
using Shop.Application.Interfaces;
using Shop.Domain.Entities;

namespace Shop.web.Controllers;

public class ArticlesController : Controller
{
    private readonly IArticleRepository _articleRepository;

    public ArticlesController(IArticleRepository articleRepository)
    {
        _articleRepository = articleRepository;
    }

    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var articles = await _articleRepository.GetPublishedAsync(20, cancellationToken);
        return View(articles);
    }


    public async Task<IActionResult> Details(string id, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(id)) return NotFound();

        Article? article = null;

        // اگر id عددی بود، با Id جستجو کن
        if (int.TryParse(id, out int articleId))
        {
            article = await _articleRepository.GetByIdAsync(articleId, cancellationToken);
        }
        // در غیر این صورت، با Slug جستجو کن
        else
        {
            article = await _articleRepository.GetBySlugAsync(id, cancellationToken);
        }

        if (article == null) return NotFound();
        return View(article);
    }

}
