using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shop.Application.DTOs;
using Shop.Application.Services;

namespace Shop.web.Controllers;

[Authorize]
public class PaymentController : Controller
{
    private readonly IPaymentService _paymentService;
    private readonly IOrderService _orderService;

    public PaymentController(IPaymentService paymentService, IOrderService orderService)
    {
        _paymentService = paymentService;
        _orderService = orderService;
    }

    [HttpGet]
    public async Task<IActionResult> Gateway(string authority, int orderId, CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        var order = await _orderService.GetOrderAsync(orderId, userId, cancellationToken);
        if (order == null) return NotFound();

        ViewBag.Authority = authority;
        ViewBag.OrderId = orderId;
        return View(order);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Gateway(string authority, int orderId)
    {
        return RedirectToAction(nameof(Callback), new { Authority = authority, Status = "OK", orderId });
    }

    [HttpGet]
    public async Task<IActionResult> Callback(string authority, string? status, int orderId, CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        var order = await _orderService.GetOrderAsync(orderId, userId, cancellationToken);
        if (order == null) return NotFound();

        if (!string.Equals(status, "OK", StringComparison.OrdinalIgnoreCase))
        {
            TempData["Error"] = "پرداخت لغو شد.";
            return RedirectToAction("Index", "Orders");
        }

        var verify = await _paymentService.VerifyPaymentAsync(authority, order.TotalAmount, cancellationToken);
        if (!verify.Success)
        {
            TempData["Error"] = verify.ErrorMessage ?? "تایید پرداخت ناموفق بود.";
            return RedirectToAction("Index", "Orders");
        }

        await _orderService.CompletePaymentWithMetadataAsync(orderId, verify.ReferenceId, paymentProvider: "ZarinPal", cancellationToken);
        TempData["PaymentRef"] = verify.ReferenceId;
        return RedirectToAction("Invoice", "Checkout", new { id = orderId });
    }

    private string? GetUserId()
        => User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
}
