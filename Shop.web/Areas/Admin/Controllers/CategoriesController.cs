using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Shop.Application.Interfaces;
using Shop.Domain.Entities;

namespace Shop.web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public class CategoriesController : Controller
{
    private readonly ICategoryRepository _categoryRepository;

    public CategoriesController(ICategoryRepository categoryRepository)
    {
        _categoryRepository = categoryRepository;
    }

    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var categories = await _categoryRepository.GetAllAsync(cancellationToken);
        await PopulateParentCategories(null, cancellationToken);
        return View(categories);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Category category, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(category.Name))
        {
            TempData["Error"] = "نام دسته‌بندی الزامی است.";
            return RedirectToAction(nameof(Index));
        }

        var baseSlug = category.Name.Trim().Replace(" ", "-").ToLowerInvariant();
        category.Slug = await GenerateUniqueSlugAsync(baseSlug, null, cancellationToken);

        await _categoryRepository.AddAsync(category, cancellationToken);
        await _categoryRepository.SaveChangesAsync(cancellationToken);
        TempData["Success"] = "دسته‌بندی اضافه شد.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var category = await _categoryRepository.GetByIdAsync(id, cancellationToken);
        if (category == null) return NotFound();
        await _categoryRepository.DeleteAsync(category, cancellationToken);
        await _categoryRepository.SaveChangesAsync(cancellationToken);
        TempData["Success"] = "دسته‌بندی حذف شد.";
        return RedirectToAction(nameof(Index));
    }

    private async Task PopulateParentCategories(int? selectedId, CancellationToken cancellationToken)
    {
        var categories = await _categoryRepository.GetRootCategoriesAsync(cancellationToken);
        ViewBag.ParentCategories = new SelectList(categories, "Id", "Name", selectedId);
    }

    private async Task<string> GenerateUniqueSlugAsync(string baseSlug, int? excludeId, CancellationToken cancellationToken)
    {
        var slug = baseSlug;
        var counter = 1;
        while (true)
        {
            var existing = await _categoryRepository.GetBySlugAsync(slug, cancellationToken);
            if (existing == null || existing.Id == excludeId)
                return slug;
            slug = $"{baseSlug}-{counter++}";
        }
    }
}
