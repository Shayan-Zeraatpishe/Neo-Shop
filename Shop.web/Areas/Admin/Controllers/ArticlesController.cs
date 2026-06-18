using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shop.Application.Interfaces;
using Shop.Domain.Entities;

namespace Shop.web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public class ArticlesController : Controller
{
    private readonly IArticleRepository _articleRepository;
    private readonly IWebHostEnvironment _environment;

    public ArticlesController(IArticleRepository articleRepository, IWebHostEnvironment environment)
    {
        _articleRepository = articleRepository;
        _environment = environment;
    }

    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var articles = await _articleRepository.GetAllAsync(cancellationToken);
        return View(articles.OrderByDescending(a => a.CreatedAt).ToList());
    }

    public IActionResult Create() => View(new Article { IsPublished = true });

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Article article, IFormFile? imageFile, CancellationToken cancellationToken)
    {
        ModelState.Remove("Slug");
        if (!ModelState.IsValid) return View(article);

        article.Slug = await GenerateUniqueSlugAsync(article.Title, null, cancellationToken);
        article.ImageUrl = await SaveImageAsync(imageFile) ?? article.ImageUrl;
        article.PublishedAt = article.IsPublished ? DateTime.UtcNow : null;

        await _articleRepository.AddAsync(article, cancellationToken);
        await _articleRepository.SaveChangesAsync(cancellationToken);
        TempData["Success"] = "مقاله با موفقیت ایجاد شد.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id, CancellationToken cancellationToken)
    {
        var article = await _articleRepository.GetByIdAsync(id, cancellationToken);
        if (article == null) return NotFound();
        return View(article);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Article article, IFormFile? imageFile, CancellationToken cancellationToken)
    {
        if (id != article.Id) return NotFound();
        ModelState.Remove("Slug");

        if (!ModelState.IsValid) return View(article);

        var existing = await _articleRepository.GetByIdAsync(id, cancellationToken);
        if (existing == null) return NotFound();

        if (existing.Title != article.Title)
            existing.Slug = await GenerateUniqueSlugAsync(article.Title, id, cancellationToken);

        existing.Title = article.Title;
        existing.Summary = article.Summary;
        existing.Content = article.Content;
        existing.IsPublished = article.IsPublished;
        existing.PublishedAt = article.IsPublished ? (existing.PublishedAt ?? DateTime.UtcNow) : null;

        if (imageFile != null && imageFile.Length > 0)
            existing.ImageUrl = await SaveImageAsync(imageFile);

        await _articleRepository.UpdateAsync(existing, cancellationToken);
        await _articleRepository.SaveChangesAsync(cancellationToken);
        TempData["Success"] = "مقاله با موفقیت ویرایش شد.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var article = await _articleRepository.GetByIdAsync(id, cancellationToken);
        if (article == null) return NotFound();
        await _articleRepository.DeleteAsync(article, cancellationToken);
        await _articleRepository.SaveChangesAsync(cancellationToken);
        TempData["Success"] = "مقاله حذف شد.";
        return RedirectToAction(nameof(Index));
    }

    private async Task<string?> SaveImageAsync(IFormFile? file)
    {
        if (file == null || file.Length == 0) return null;
        var allowed = new[] { ".jpg", ".jpeg", ".png", ".webp", ".gif" };
        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!allowed.Contains(ext)) return null;

        var uploads = Path.Combine(_environment.WebRootPath, "images", "articles");
        Directory.CreateDirectory(uploads);
        var fileName = $"{Guid.NewGuid():N}{ext}";
        var path = Path.Combine(uploads, fileName);
        await using var stream = new FileStream(path, FileMode.Create);
        await file.CopyToAsync(stream);
        return $"/images/articles/{fileName}";
    }

    private async Task<string> GenerateUniqueSlugAsync(string title, int? excludeId, CancellationToken cancellationToken)
    {
        var baseSlug = title.Trim().Replace(" ", "-").ToLowerInvariant();
        var slug = baseSlug;
        var counter = 1;
        while (true)
        {
            var existing = (await _articleRepository.FindAsync(a => a.Slug == slug, cancellationToken)).FirstOrDefault();
            if (existing == null || existing.Id == excludeId)
                return slug;
            slug = $"{baseSlug}-{counter++}";
        }
    }
}
