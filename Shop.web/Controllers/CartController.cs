using Microsoft.AspNetCore.Mvc;
using Shop.Application.Services;
using Shop.web.Helpers;

namespace Shop.web.Controllers;

public class CartController : Controller
{
    private readonly ICartService _cartService;

    public CartController(ICartService cartService)
    {
        _cartService = cartService;
    }

    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var (userId, sessionId) = GetCartIdentity();
        var cart = await _cartService.GetCartAsync(userId, sessionId, cancellationToken);
        return View(cart);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Update(int productId, int quantity, CancellationToken cancellationToken)
    {
        var (userId, sessionId) = GetCartIdentity();
        await _cartService.UpdateQuantityAsync(userId, sessionId, productId, quantity, cancellationToken);
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Remove(int productId, CancellationToken cancellationToken)
    {
        var (userId, sessionId) = GetCartIdentity();
        await _cartService.RemoveFromCartAsync(userId, sessionId, productId, cancellationToken);
        return RedirectToAction(nameof(Index));
    }


    private (string? userId, string? sessionId) GetCartIdentity()
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            return (userId, null);
        }
        var sessionId = CartSessionHelper.GetOrCreateSessionId(HttpContext);
        return (null, sessionId);
    }
}
