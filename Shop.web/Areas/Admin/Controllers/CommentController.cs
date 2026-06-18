using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shop.Application.DTOs;
using Shop.Application.Services;
using Shop.web.Areas.Admin.ViewModels.Comments;
using Shop.web.Helpers;

namespace Shop.web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public class CommentController : Controller
{
    private readonly ICommentService _commentService;

    public CommentController(ICommentService commentService)
    {
        _commentService = commentService;
    }

    [HttpGet]
    public async Task<IActionResult> Index([FromQuery] AdminCommentFilterViewModel filter, CancellationToken cancellationToken)
    {
        var filterDto = new AdminCommentFilterDto
        {
            CommentId = filter.CommentId,
            ProductName = filter.ProductName,
            UserName = filter.UserName,
            Status = filter.Status,
            FromDateUtc = filter.FromDate?.ToDateTime(TimeOnly.MinValue),
            ToDateUtc = filter.ToDate?.ToDateTime(TimeOnly.MinValue),
            SortBy = filter.SortBy,
            SortDescending = filter.SortDescending
        };

        var paged = await _commentService.GetAdminCommentsAsync(filterDto, filter.PageNumber, filter.PageSize, cancellationToken);

        var vm = new AdminCommentsIndexViewModel
        {
            Filter = filter,
            Comments = paged.Items.Select(x => new AdminCommentListItemViewModel
            {
                Id = x.Id,
                ProductId = x.ProductId,
                ProductName = x.ProductName,
                AuthorName = x.AuthorName,
                AuthorEmail = x.AuthorEmail,
                UserFullName = x.UserFullName,
                UserPhone = x.UserPhone,
                Content = x.Content,
                Status = x.Status,
                AdminReply = x.AdminReply,
                CreatedAtUtc = x.CreatedAtUtc
            }).ToList(),
            TotalCount = paged.TotalCount,
            PageNumber = paged.PageNumber,
            PageSize = paged.PageSize
        };

        return View(vm);
    }

    [HttpGet]
    public async Task<IActionResult> Details(int id, CancellationToken cancellationToken)
    {
        var dto = await _commentService.GetAdminCommentDetailsAsync(id, cancellationToken);
        if (dto == null) return NotFound();

        var vm = new AdminCommentDetailsViewModel
        {
            Id = dto.Id,
            ProductId = dto.ProductId,
            ProductName = dto.ProductName,
            ProductSlug = dto.ProductSlug,
            UserId = dto.UserId,
            AuthorName = dto.AuthorName,
            AuthorEmail = dto.AuthorEmail,
            UserFullName = dto.UserFullName,
            UserEmail = dto.UserEmail,
            UserPhone = dto.UserPhone,
            Content = dto.Content,
            Status = dto.Status,
            AdminReply = dto.AdminReply,
            CreatedAtUtc = dto.CreatedAtUtc,
            UpdatedAtUtc = dto.UpdatedAtUtc,
            ReplyText = dto.AdminReply
        };

        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Approve(int id, CancellationToken cancellationToken)
    {
        if (!await _commentService.ApproveCommentAsync(id, cancellationToken))
            return NotFound();

        TempData["Success"] = "دیدگاه تأیید شد.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Reject(int id, CancellationToken cancellationToken)
    {
        if (!await _commentService.RejectCommentAsync(id, cancellationToken))
            return NotFound();

        TempData["Success"] = "دیدگاه رد شد.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        if (!await _commentService.DeleteCommentAsync(id, cancellationToken))
            return NotFound();

        TempData["Success"] = "دیدگاه حذف شد.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Reply(AdminCommentDetailsViewModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
            return View("Details", model);

        if (!await _commentService.ReplyToCommentAsync(model.Id, model.ReplyText ?? string.Empty, cancellationToken))
            return NotFound();

        TempData["Success"] = "پاسخ ثبت شد.";
        return RedirectToAction(nameof(Details), new { id = model.Id });
    }
}
