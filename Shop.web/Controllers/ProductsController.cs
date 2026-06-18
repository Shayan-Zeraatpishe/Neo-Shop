using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Shop.Application.DTOs;
using Shop.Application.Interfaces;
using Shop.Application.Services;
using Shop.Infrastructure.Identity;
using Shop.web.ViewModels;

namespace Shop.web.Controllers;

public class ProductsController : Controller
{
    private readonly IProductRepository _productRepository;
    private readonly ICommentService _commentService;
    private readonly UserManager<ApplicationUser> _userManager;

    public ProductsController(
        IProductRepository productRepository,
        ICommentService commentService,
        UserManager<ApplicationUser> userManager)
    {
        _productRepository = productRepository;
        _commentService = commentService;
        _userManager = userManager;
    }

    public async Task<IActionResult> Details(string id, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(id)) return NotFound();

        var product = await _productRepository.GetBySlugAsync(id, cancellationToken);
        if (product == null) return NotFound();

        var related = await _productRepository.SearchAsync(null, new[] { product.CategoryId }, null, null, cancellationToken);
        var comments = await _commentService.GetApprovedProductCommentsAsync(product.Id, cancellationToken);

        ApplicationUser? user = null;
        if (User.Identity?.IsAuthenticated == true)
            user = await _userManager.GetUserAsync(User);

        var model = new ProductDetailsViewModel
        {
            Product = product,
            RelatedProducts = related.Where(p => p.Id != product.Id).Take(4).ToList(),
            Comments = comments,
            NewComment = new AddProductCommentViewModel
            {
                ProductId = product.Id,
                ProductSlug = product.Slug,
                AuthorName = user?.FullName ?? user?.UserName ?? string.Empty,
                AuthorEmail = user?.Email ?? string.Empty
            }
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddComment([Bind(Prefix = "NewComment")] AddProductCommentViewModel model, CancellationToken cancellationToken)
    {
        if (model.ProductId <= 0)
            return NotFound();

        var product = await _productRepository.GetByIdAsync(model.ProductId, cancellationToken);
        if (product == null || !product.IsActive)
            return NotFound();

        if (!ModelState.IsValid)
        {
            var related = await _productRepository.SearchAsync(null, new[] { product.CategoryId }, null, null, cancellationToken);
            var comments = await _commentService.GetApprovedProductCommentsAsync(product.Id, cancellationToken);
            var invalidModel = new ProductDetailsViewModel
            {
                Product = product,
                RelatedProducts = related.Where(p => p.Id != product.Id).Take(4).ToList(),
                Comments = comments,
                NewComment = model
            };
            return View("Details", invalidModel);
        }

        string? userId = null;
        if (User.Identity?.IsAuthenticated == true)
        {
            userId = _userManager.GetUserId(User);
            var user = await _userManager.GetUserAsync(User);
            if (user != null)
            {
                model.AuthorName = user.FullName ?? user.UserName ?? model.AuthorName;
                model.AuthorEmail = user.Email ?? model.AuthorEmail;
            }
        }

        var dto = new CreateProductCommentDto
        {
            ProductId = model.ProductId,
            UserId = userId,
            AuthorName = model.AuthorName,
            AuthorEmail = model.AuthorEmail,
            Content = model.Content
        };

        var (success, error) = await _commentService.CreateProductCommentAsync(dto, cancellationToken);
        if (!success)
        {
            ModelState.AddModelError(string.Empty, error ?? "ثبت دیدگاه با خطا مواجه شد.");
            var related = await _productRepository.SearchAsync(null, new[] { product.CategoryId }, null, null, cancellationToken);
            var comments = await _commentService.GetApprovedProductCommentsAsync(product.Id, cancellationToken);
            return View("Details", new ProductDetailsViewModel
            {
                Product = product,
                RelatedProducts = related.Where(p => p.Id != product.Id).Take(4).ToList(),
                Comments = comments,
                NewComment = model
            });
        }

        TempData["CommentSuccess"] = "دیدگاه شما ثبت شد و پس از تأیید نمایش داده می‌شود.";
        return RedirectToAction(nameof(Details), new { id = model.ProductSlug });
    }
}
