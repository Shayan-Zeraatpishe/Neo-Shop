using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shop.Application.Interfaces;
using Shop.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Shop.web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public class DashboardController : Controller
{
    private readonly ApplicationDbContext _context;

    public DashboardController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        ViewBag.ProductCount = await _context.Products.CountAsync(cancellationToken);
        ViewBag.OrderCount = await _context.Orders.CountAsync(cancellationToken);
        ViewBag.UserCount = await _context.Users.CountAsync(cancellationToken);
        ViewBag.Revenue = await _context.Orders
            .Where(o => o.Status == Domain.Enums.OrderStatus.Completed || o.Status == Domain.Enums.OrderStatus.Paid)
            .SumAsync(o => o.TotalAmount, cancellationToken);
        return View();
    }
}
