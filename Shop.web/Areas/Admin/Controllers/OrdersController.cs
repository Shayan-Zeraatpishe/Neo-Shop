using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Logging;
using Shop.Application.Interfaces;
using Shop.Application.Services;
using Shop.Application.DTOs;
using Shop.Domain.Entities;
using Shop.Domain.Enums;
using Shop.web.Areas.Admin.ViewModels.Orders;

namespace Shop.web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public class OrdersController : Controller
{
    private readonly IOrderRepository _orderRepository;
    private readonly IOrderService _orderService;
    private readonly ILogger<OrdersController> _logger;

    public OrdersController(IOrderRepository orderRepository, IOrderService orderService, ILogger<OrdersController> logger)
    {
        _orderRepository = orderRepository;
        _orderService = orderService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> Index([FromQuery] AdminOrderFilterViewModel filter, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            filter.PageNumber = 1;
            filter.PageSize = 20;
            filter.SortBy = "CreatedAt";
            filter.SortDescending = true;
        }

        var filterDto = new AdminOrderFilterDto
        {
            OrderId = filter.OrderId,
            Customer = filter.Customer,
            Status = filter.Status,
            PaymentStatus = filter.PaymentStatus,
            FromDateUtc = filter.FromDate?.ToDateTime(TimeOnly.MinValue),
            ToDateUtc = filter.ToDate?.ToDateTime(TimeOnly.MinValue),
            MinTotalAmount = filter.MinTotalAmount,
            MaxTotalAmount = filter.MaxTotalAmount,
            SortBy = filter.SortBy,
            SortDescending = filter.SortDescending
        };

        var paged = await _orderRepository.GetAdminOrdersAsync(filterDto, filter.PageNumber, filter.PageSize, cancellationToken);

        var vm = new AdminOrdersIndexViewModel
        {
            Filter = filter,
            Orders = paged.Items
                .Select(x => new AdminOrderListItemViewModel
                {
                    Id = x.Id,
                    OrderNumber = x.OrderNumber,
                    FullName = x.FullName,
                    Email = x.Email,
                    PhoneNumber = x.PhoneNumber,
                    Status = x.Status,
                    PaymentStatus = x.PaymentStatus,
                    CreatedAtUtc = x.CreatedAtUtc,
                    PaidAtUtc = x.PaidAtUtc,
                    TotalAmount = x.TotalAmount
                })
                .ToList(),
            TotalCount = paged.TotalCount,
            PageNumber = paged.PageNumber,
            PageSize = paged.PageSize
        };

        return View(vm);
    }

    [HttpGet]
    public async Task<IActionResult> Details(int id, CancellationToken cancellationToken)
    {
        var dto = await _orderRepository.GetAdminOrderDetailsAsync(id, cancellationToken);
        if (dto == null) return NotFound();

        var vm = new AdminOrderDetailsViewModel
        {
            Id = dto.Id,
            OrderNumber = dto.OrderNumber,
            FullName = dto.FullName,
            Email = dto.Email,
            PhoneNumber = dto.PhoneNumber,
            ShippingAddress = dto.ShippingAddress ?? dto.AddressFallback,
            BillingAddress = dto.BillingAddress ?? dto.AddressFallback,
            AddressFallback = dto.AddressFallback,
            Notes = dto.Notes,
            SubTotal = dto.SubTotal,
            DiscountAmount = dto.DiscountAmount,
            TotalAmount = dto.TotalAmount,
            Status = dto.Status,
            CreatedAtUtc = dto.CreatedAtUtc,
            PaidAtUtc = dto.PaidAtUtc,
            PaymentStatus = dto.PaymentStatus,
            PaymentProvider = dto.PaymentProvider,
            PaymentReferenceId = dto.PaymentReferenceId,
            Items = dto.Items
                .Select(i => new AdminOrderItemViewModel
                {
                    ProductName = i.ProductName,
                    Quantity = i.Quantity,
                    UnitPrice = i.UnitPrice,
                    TotalPrice = i.TotalPrice
                })
                .ToList(),
            StatusHistory = dto.StatusHistory
                .Select(h => new OrderStatusHistoryItemViewModel
                {
                    OldStatus = h.OldStatus,
                    NewStatus = h.NewStatus,
                    ChangedAtUtc = h.ChangedAtUtc,
                    ChangedByUserId = h.ChangedByUserId,
                    Note = h.Note
                })
                .ToList(),
            StatusToUpdate = dto.Status
        };

        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateStatus(int orderId, OrderStatus status, string? note, CancellationToken cancellationToken)
    {
        if (!Enum.IsDefined(typeof(OrderStatus), status))
        {
            TempData["Error"] = "وضعیت سفارش نامعتبر است.";
            return RedirectToAction(nameof(Details), new { id = orderId });
        }

        var changedByUserId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

        try
        {
            await _orderService.UpdateOrderStatusWithHistoryAsync(orderId, status, changedByUserId, note, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Admin order status update failed. OrderId={OrderId}, Status={Status}", orderId, status);
            TempData["Error"] = "خطا در به‌روزرسانی وضعیت سفارش.";
        }

        return RedirectToAction(nameof(Details), new { id = orderId });
    }


}
