using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shop.Application.Services;

namespace Shop.web.Controllers;

[Authorize]
public class CheckoutController : Controller
{
    private readonly IOrderService _orderService;

    public CheckoutController(IOrderService orderService)
    {
        _orderService = orderService;
    }

    [HttpGet("Checkout")]
    [HttpGet("Checkout/Index")]
    public IActionResult Index(int? orderId = null)
    {
        if (orderId.HasValue)
            return RedirectToAction("Checkout", "Orders", new { orderId = orderId.Value });

        return RedirectToAction("Index", "Orders");
    }

    public async Task<IActionResult> Invoice(int id, CancellationToken cancellationToken)
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        var order = await _orderService.GetOrderAsync(id, userId, cancellationToken);
        if (order == null) return NotFound();
        return View(order);
    }
}
