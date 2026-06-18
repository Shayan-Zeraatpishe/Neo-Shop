using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shop.Application.DTOs;
using Shop.Application.Interfaces;
using Shop.Application.Services;
using Shop.web.ViewModels;

namespace Shop.web.Controllers;

[Authorize]
public class OrdersController : Controller
{
    private readonly IOrderService _orderService;
    private readonly IProductRepository _productRepository;
    private readonly IPaymentService _paymentService;

    public OrdersController(IOrderService orderService, IProductRepository productRepository, IPaymentService paymentService)
    {
        _orderService = orderService;
        _productRepository = productRepository;
        _paymentService = paymentService;
    }

    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        if (string.IsNullOrEmpty(userId)) return Challenge();

        var orders = await _orderService.GetUserOrdersAsync(userId, cancellationToken);
        return View(orders);
    }

    [HttpGet("Orders/BuyNow/{productId}")]
    public Task<IActionResult> BuyNowGet(int productId, CancellationToken cancellationToken)
        => BuyNowCore(productId, cancellationToken);

    [HttpPost("Orders/BuyNow/{productId}")]
    [ValidateAntiForgeryToken]
    public Task<IActionResult> BuyNow(int productId, CancellationToken cancellationToken)
        => BuyNowCore(productId, cancellationToken);

    private async Task<IActionResult> BuyNowCore(int productId, CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        if (string.IsNullOrWhiteSpace(userId))
            return Challenge();

        var product = await _productRepository.GetByIdAsync(productId, cancellationToken);
        if (product == null)
            return NotFound();

        try
        {
            var order = await _orderService.CreateBuyNowOrderAsync(userId, product, cancellationToken);
            return RedirectToAction(nameof(Checkout), new { orderId = order.Id });
        }
        catch (InvalidOperationException ex)
        {
            TempData["Error"] = ex.Message;
            return RedirectToAction("Details", "Products", new { id = product.Slug });
        }
    }

    [HttpGet("Orders/Checkout/{orderId}")]
    public async Task<IActionResult> Checkout(int orderId, CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        if (string.IsNullOrWhiteSpace(userId))
            return Challenge();

        var order = await _orderService.GetOrderAsync(orderId, userId, cancellationToken);
        if (order == null)
            return NotFound();

        var viewModel = new OrderCheckoutViewModel
        {
            Order = order,
            Checkout = new CheckoutDto
            {
                FullName = User.Identity?.Name ?? string.Empty,
                Email = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value ?? string.Empty,
                PhoneNumber = User.FindFirst(System.Security.Claims.ClaimTypes.MobilePhone)?.Value ?? string.Empty
            }
        };

        return View("~/Views/Orders/Checkout.cshtml", viewModel);
    }

    [HttpPost("Orders/Checkout/{orderId}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Checkout(int orderId, OrderCheckoutViewModel model, CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        if (string.IsNullOrWhiteSpace(userId))
            return Challenge();

        var order = await _orderService.GetOrderAsync(orderId, userId, cancellationToken);
        if (order == null)
            return NotFound();

        model.Order = order;

        if (!ModelState.IsValid)
        {
            return View("~/Views/Orders/Checkout.cshtml", model);
        }

        try
        {
            order = await _orderService.UpdateCheckoutDetailsAsync(orderId, userId, model.Checkout, cancellationToken);
        }
        catch (InvalidOperationException ex)
        {
            TempData["Error"] = ex.Message;
            return View("~/Views/Orders/Checkout.cshtml", model);
        }

        var callbackUrl = Url.Action(nameof(PaymentController.Callback), "Payment", new { orderId = order.Id }, Request.Scheme)!;
        var payment = await _paymentService.InitiatePaymentAsync(new PaymentRequestDto
        {
            OrderId = order.Id,
            OrderNumber = order.OrderNumber,
            Amount = order.TotalAmount,
            Description = $"پرداخت سفارش {order.OrderNumber}",
            CallbackUrl = callbackUrl
        }, cancellationToken);

        if (!payment.Success || string.IsNullOrWhiteSpace(payment.GatewayUrl))
        {
            TempData["Error"] = payment.ErrorMessage ?? "خطا در اتصال به درگاه پرداخت";
            model.Order = order;
            return View("~/Views/Orders/Checkout.cshtml", model);
        }

        return Redirect(payment.GatewayUrl);
    }

    public async Task<IActionResult> Details(int id, CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        var order = await _orderService.GetOrderAsync(id, userId, cancellationToken);
        if (order == null) return NotFound();
        return View(order);
    }

    private string? GetUserId()
        => User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
}
