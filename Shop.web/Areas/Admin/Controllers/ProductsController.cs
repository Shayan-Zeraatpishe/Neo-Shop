using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Shop.Application.DTOs;
using Shop.Application.Interfaces;
using Shop.Domain.Entities;
using Shop.web.Areas.Admin.ViewModels.Products;

namespace Shop.web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public class ProductsController : Controller
{
    private readonly IProductRepository _productRepository;
    private readonly ICategoryRepository _categoryRepository;
    private readonly IWebHostEnvironment _environment;
    private readonly IConfiguration _configuration;
    private readonly ILogger<ProductsController> _logger;

    private const long MaxImageBytes = 3 * 1024 * 1024; // ~3MB
    private const long MaxDigitalBytes = 50 * 1024 * 1024; // ~50MB

    private static readonly string[] AllowedImageExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp", ".gif" };

    public ProductsController(
        IProductRepository productRepository,
        ICategoryRepository categoryRepository,
        IWebHostEnvironment environment,
        IConfiguration configuration,
        ILogger<ProductsController> logger)
    {
        _productRepository = productRepository;
        _categoryRepository = categoryRepository;
        _environment = environment;
        _configuration = configuration;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> Index([FromQuery] AdminProductFilterViewModel filter, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            filter.PageNumber = 1;
            filter.PageSize = 20;
            filter.SortBy = "CreatedAt";
            filter.SortDescending = true;
        }

        var categories = await _categoryRepository.GetAllAsync(cancellationToken);

        var filterDto = new AdminProductFilterDto
        {
            SearchTerm = filter.SearchTerm,
            IsActive = filter.IsActive,
            CategoryId = filter.CategoryId,
            SortBy = filter.SortBy,
            SortDescending = filter.SortDescending
        };

        var paged = await _productRepository.GetAdminProductsAsync(filterDto, filter.PageNumber, filter.PageSize, cancellationToken);

        var vm = new AdminProductsIndexViewModel
        {
            Filter = filter,
            Categories = categories,
            Products = paged.Items.Select(p => new AdminProductListItemViewModel
            {
                Id = p.Id,
                Name = p.Name,
                Slug = p.Slug,
                Brand = p.Brand,
                Price = p.Price,
                DiscountPrice = p.DiscountPrice,
                EffectivePrice = p.EffectivePrice,
                Stock = p.Stock,
                IsActive = p.IsActive,
                IsFeatured = p.IsFeatured,
                IsSpecialOffer = p.IsSpecialOffer,
                CategoryName = p.CategoryName,
                MainImageUrl = p.MainImageUrl,
                CreatedAtUtc = p.CreatedAtUtc
            }).ToList(),
            TotalCount = paged.TotalCount,
            PageNumber = paged.PageNumber,
            PageSize = paged.PageSize
        };

        return View(vm);
    }

    [HttpGet]
    public async Task<IActionResult> Create(CancellationToken cancellationToken)
    {
        await PopulateCategoriesAsync(cancellationToken);
        return View(new AdminProductCreateEditViewModel { IsActive = true, Stock = 0 });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(AdminProductCreateEditViewModel model, CancellationToken cancellationToken)
    {
        await PopulateCategoriesAsync(cancellationToken);

        if (!ModelState.IsValid)
            return View(model);

        if (model.DiscountPrice.HasValue && model.DiscountPrice.Value > model.Price)
            ModelState.AddModelError(nameof(model.DiscountPrice), "قیمت تخفیف نمی‌تواند بیشتر از قیمت باشد.");

        var mainImageUrl = await SaveImageIfValidAsync(model.MainImageFile, cancellationToken, required: true);
        if (mainImageUrl == null)
        {
            ModelState.AddModelError("", "تصویر اصلی معتبر الزامی است.");
            return View(model);
        }

        if (!ValidateGalleryImages(model.GalleryImageFiles, out var galleryError))
        {
            ModelState.AddModelError("", galleryError);
            return View(model);
        }

        var product = new Product
        {
            Name = model.Name.Trim(),
            ShortDescription = model.ShortDescription,
            Description = model.Description,
            Price = model.Price,
            DiscountPrice = model.DiscountPrice,
            Brand = model.Brand,
            CategoryId = model.CategoryId,
            IsActive = model.IsActive,
            IsFeatured = model.IsFeatured,
            IsSpecialOffer = model.IsSpecialOffer,
            Stock = model.Stock,
            Specifications = model.Specifications,
            MainImageUrl = mainImageUrl,

            SeoTitle = model.SeoTitle,
            SeoDescription = model.SeoDescription,
            SeoKeywords = model.SeoKeywords,
            SeoCanonicalUrl = model.SeoCanonicalUrl
        };

        product.Slug = await GenerateUniqueSlugAsync(product.Name, excludeId: null, cancellationToken);

        if (model.DigitalFile != null && model.DigitalFile.Length > 0)
        {
            if (model.DigitalFile.Length > MaxDigitalBytes)
            {
                ModelState.AddModelError(nameof(model.DigitalFile), "حجم فایل دیجیتال بیش از حد مجاز است.");
                return View(model);
            }
            await SaveDigitalFileAsync(product, model.DigitalFile, cancellationToken);
        }

        // Gallery images => ProductImages
        var displayOrder = 1;
        foreach (var file in model.GalleryImageFiles.Where(f => f != null && f.Length > 0))
        {
            var url = await SaveImageIfValidAsync(file, cancellationToken, required: false);
            if (url == null) continue;
            product.Images.Add(new ProductImage
            {
                ImageUrl = url,
                DisplayOrder = displayOrder++,
            });
        }

        await _productRepository.AddAsync(product, cancellationToken);
        await _productRepository.SaveChangesAsync(cancellationToken);
        TempData["Success"] = "محصول با موفقیت اضافه شد.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id, CancellationToken cancellationToken)
    {
        var dto = await _productRepository.GetAdminProductDetailsAsync(id, cancellationToken);
        if (dto == null) return NotFound();

        await PopulateCategoriesAsync(cancellationToken);

        var vm = new AdminProductCreateEditViewModel
        {
            Id = dto.Id,
            Name = dto.Name,
            ShortDescription = dto.ShortDescription,
            Description = dto.Description,
            Price = dto.Price,
            DiscountPrice = dto.DiscountPrice,
            Brand = dto.Brand,
            CategoryId = dto.CategoryId,
            IsActive = dto.IsActive,
            IsFeatured = dto.IsFeatured,
            IsSpecialOffer = dto.IsSpecialOffer,
            Stock = dto.Stock,
            Specifications = dto.Specifications,

            SeoTitle = dto.SeoTitle,
            SeoDescription = dto.SeoDescription,
            SeoKeywords = dto.SeoKeywords,
            SeoCanonicalUrl = dto.SeoCanonicalUrl,

            MainImageUrl = dto.MainImageUrl,
            ExistingImages = dto.Images
                .Select(i => new AdminProductImageViewModel
                {
                    Id = i.Id,
                    ImageUrl = i.ImageUrl,
                    DisplayOrder = i.DisplayOrder
                }).ToList(),
            DigitalFileOriginalName = dto.DigitalFileOriginalName,
            DigitalFileName = dto.DigitalFileName
        };

        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(AdminProductCreateEditViewModel model, CancellationToken cancellationToken)
    {
        await PopulateCategoriesAsync(cancellationToken);

        if (model.Id <= 0)
            return BadRequest();

        if (!ModelState.IsValid)
            return View(model);

        if (model.DiscountPrice.HasValue && model.DiscountPrice.Value > model.Price)
            ModelState.AddModelError(nameof(model.DiscountPrice), "قیمت تخفیف نمی‌تواند بیشتر از قیمت باشد.");

        if (!ValidateGalleryImages(model.GalleryImageFiles, out var galleryError))
        {
            ModelState.AddModelError("", galleryError);
            return View(model);
        }

        var existing = await _productRepository.GetByIdAsync(model.Id, cancellationToken);
        if (existing == null) return NotFound();

        // Slug regen only when Name changed.
        if (!string.Equals(existing.Name, model.Name.Trim(), StringComparison.Ordinal))
            existing.Slug = await GenerateUniqueSlugAsync(model.Name, excludeId: existing.Id, cancellationToken);

        existing.Name = model.Name.Trim();
        existing.ShortDescription = model.ShortDescription;
        existing.Description = model.Description;
        existing.Price = model.Price;
        existing.DiscountPrice = model.DiscountPrice;
        existing.Brand = model.Brand;
        existing.CategoryId = model.CategoryId;
        existing.IsActive = model.IsActive;
        existing.IsFeatured = model.IsFeatured;
        existing.IsSpecialOffer = model.IsSpecialOffer;
        existing.Stock = model.Stock;
        existing.Specifications = model.Specifications;

        existing.SeoTitle = model.SeoTitle;
        existing.SeoDescription = model.SeoDescription;
        existing.SeoKeywords = model.SeoKeywords;
        existing.SeoCanonicalUrl = model.SeoCanonicalUrl;

        if (model.MainImageFile != null && model.MainImageFile.Length > 0)
        {
            var mainUrl = await SaveImageIfValidAsync(model.MainImageFile, cancellationToken, required: false);
            if (mainUrl != null)
                existing.MainImageUrl = mainUrl;
        }

        if (model.DigitalFile != null && model.DigitalFile.Length > 0)
        {
            if (model.DigitalFile.Length > MaxDigitalBytes)
            {
                ModelState.AddModelError(nameof(model.DigitalFile), "حجم فایل دیجیتال بیش از حد مجاز است.");
                return View(model);
            }
            await SaveDigitalFileAsync(existing, model.DigitalFile, cancellationToken);
        }

        if (model.DeleteImageIds.Count > 0)
            await _productRepository.DeleteProductImagesAsync(model.DeleteImageIds, cancellationToken);

        // Add new gallery images
        var deleteIds = model.DeleteImageIds?.Distinct().ToHashSet() ?? new HashSet<int>();
        var nextDisplayOrder = existing.Images
            .Where(i => !deleteIds.Contains(i.Id))
            .Select(i => i.DisplayOrder)
            .DefaultIfEmpty(0)
            .Max() + 1;

        foreach (var file in model.GalleryImageFiles.Where(f => f != null && f.Length > 0))
        {
            var url = await SaveImageIfValidAsync(file, cancellationToken, required: false);
            if (url == null) continue;

            existing.Images.Add(new ProductImage
            {
                ProductId = existing.Id,
                ImageUrl = url,
                DisplayOrder = nextDisplayOrder++
            });
        }

        await _productRepository.UpdateAsync(existing, cancellationToken);
        await _productRepository.SaveChangesAsync(cancellationToken);
        TempData["Success"] = "محصول با موفقیت ویرایش شد.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Details(int id, CancellationToken cancellationToken)
    {
        var dto = await _productRepository.GetAdminProductDetailsAsync(id, cancellationToken);
        if (dto == null) return NotFound();

        var vm = new AdminProductDetailsViewModel
        {
            Id = dto.Id,
            Name = dto.Name,
            Slug = dto.Slug,
            Brand = dto.Brand,
            CategoryId = dto.CategoryId,
            CategoryName = dto.CategoryName,
            Price = dto.Price,
            DiscountPrice = dto.DiscountPrice,
            EffectivePrice = dto.DiscountPrice ?? dto.Price,
            Stock = dto.Stock,
            IsActive = dto.IsActive,
            IsFeatured = dto.IsFeatured,
            IsSpecialOffer = dto.IsSpecialOffer,
            ShortDescription = dto.ShortDescription,
            Description = dto.Description,
            Specifications = dto.Specifications,
            MainImageUrl = dto.MainImageUrl,
            Images = dto.Images.Select(i => new AdminProductImageViewModel
            {
                Id = i.Id,
                ImageUrl = i.ImageUrl,
                DisplayOrder = i.DisplayOrder
            }).ToList(),
            SeoTitle = dto.SeoTitle,
            SeoDescription = dto.SeoDescription,
            SeoKeywords = dto.SeoKeywords,
            SeoCanonicalUrl = dto.SeoCanonicalUrl,
            DigitalFileOriginalName = dto.DigitalFileOriginalName,
            DigitalFileName = dto.DigitalFileName,
            CreatedAtUtc = dto.CreatedAtUtc
        };

        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var product = await _productRepository.GetByIdAsync(id, cancellationToken);
        if (product == null) return NotFound();

        // Safe delete: keep record, hide from storefront.
        product.IsActive = false;
        await _productRepository.UpdateAsync(product, cancellationToken);
        await _productRepository.SaveChangesAsync(cancellationToken);

        TempData["Success"] = "محصول حذف شد (حذف ایمن).";
        return RedirectToAction(nameof(Index));
    }

    private async Task PopulateCategoriesAsync(CancellationToken cancellationToken)
    {
        var categories = await _categoryRepository.GetAllAsync(cancellationToken);
        ViewBag.Categories = new SelectList(categories, "Id", "Name");
    }

    private bool ValidateGalleryImages(IEnumerable<IFormFile> files, out string? error)
    {
        error = null;
        foreach (var file in files ?? Enumerable.Empty<IFormFile>())
        {
            if (file == null || file.Length == 0) continue;
            if (file.Length > MaxImageBytes)
            {
                error = $"حجم بعضی از تصاویر بیشتر از حد مجاز است (حداکثر {MaxImageBytes / (1024 * 1024)}MB).";
                return false;
            }
            var ext = Path.GetExtension(file.FileName)?.ToLowerInvariant();
            if (string.IsNullOrWhiteSpace(ext) || !AllowedImageExtensions.Contains(ext))
            {
                error = "فرمت یکی از تصاویر مجاز نیست.";
                return false;
            }
        }
        return true;
    }

    private async Task<string?> SaveImageIfValidAsync(IFormFile? file, CancellationToken cancellationToken, bool required)
    {
        if (file == null || file.Length == 0)
            return required ? null : null;

        if (file.Length > MaxImageBytes)
            return null;

        var ext = Path.GetExtension(file.FileName)?.ToLowerInvariant();
        if (string.IsNullOrWhiteSpace(ext) || !AllowedImageExtensions.Contains(ext))
            return null;

        var uploads = Path.Combine(_environment.WebRootPath, "images", "products");
        Directory.CreateDirectory(uploads);

        var fileName = $"{Guid.NewGuid():N}{ext}";
        var path = Path.Combine(uploads, fileName);

        await using var stream = new FileStream(path, FileMode.Create);
        await file.CopyToAsync(stream, cancellationToken);

        return $"/images/products/{fileName}";
    }

    private async Task SaveDigitalFileAsync(Product product, IFormFile file, CancellationToken cancellationToken)
    {
        var storagePath = _configuration["DigitalProducts:StoragePath"] ?? "App_Data/DigitalProducts";
        Directory.CreateDirectory(storagePath);

        var ext = Path.GetExtension(file.FileName);
        if (string.IsNullOrWhiteSpace(ext)) ext = ".bin";

        var fileName = $"{Guid.NewGuid():N}{ext.ToLowerInvariant()}";
        var path = Path.Combine(storagePath, fileName);

        await using var stream = new FileStream(path, FileMode.Create);
        await file.CopyToAsync(stream, cancellationToken);

        product.DigitalFileName = fileName;
        product.DigitalFileOriginalName = file.FileName;
    }

    private async Task<string> GenerateUniqueSlugAsync(string name, int? excludeId, CancellationToken cancellationToken)
    {
        var baseSlug = name.Trim().Replace(" ", "-").ToLowerInvariant();
        if (string.IsNullOrWhiteSpace(baseSlug))
            baseSlug = "product";

        var slug = baseSlug;
        var counter = 1;
        while (true)
        {
            var existing = await _productRepository.GetBySlugIncludingInactiveAsync(slug, cancellationToken);
            if (existing == null || existing.Id == excludeId)
                return slug;

            slug = $"{baseSlug}-{counter++}";
        }
    }
}

