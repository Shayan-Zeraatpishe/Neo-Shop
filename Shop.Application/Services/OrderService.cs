using Shop.Application.DTOs;
using Shop.Application.Interfaces;
using Shop.Domain.Entities;
using Shop.Domain.Enums;

namespace Shop.Application.Services;

public class OrderService : IOrderService
{
    private readonly IOrderRepository _orderRepository;
    private readonly IDownloadTokenRepository _downloadTokenRepository;
    private readonly IProductRepository _productRepository;

    public OrderService(
        IOrderRepository orderRepository,
        IDownloadTokenRepository downloadTokenRepository,
        IProductRepository productRepository)
    {
        _orderRepository = orderRepository;
        _downloadTokenRepository = downloadTokenRepository;
        _productRepository = productRepository;
    }

    public async Task<Order> CreateBuyNowOrderAsync(string userId, Product product, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(userId))
            throw new InvalidOperationException("برای ثبت سفارش باید وارد حساب کاربری شوید.");

        if (!product.IsActive)
            throw new InvalidOperationException("این محصول در حال حاضر فعال نیست.");

        if (product.Stock < 1)
            throw new InvalidOperationException($"موجودی {product.Name} کافی نیست.");

        var unitPrice = product.EffectivePrice;
        product.Stock -= 1;
        await _productRepository.UpdateAsync(product, cancellationToken);

        var order = new Order
        {
            OrderNumber = GenerateOrderNumber(),
            UserId = userId,
            FullName = string.Empty,
            Email = string.Empty,
            PhoneNumber = string.Empty,
            Address = null,
            ShippingAddress = null,
            BillingAddress = null,
            Notes = null,
            SubTotal = unitPrice,
            DiscountAmount = 0,
            TotalAmount = unitPrice,
            Status = OrderStatus.Pending,
            Items = new List<OrderItem>
            {
                new()
                {
                    ProductId = product.Id,
                    ProductName = product.Name,
                    Quantity = 1,
                    UnitPrice = unitPrice,
                    TotalPrice = unitPrice
                }
            }
        };

        await _orderRepository.AddAsync(order, cancellationToken);
        await _orderRepository.SaveChangesAsync(cancellationToken);

        return order;
    }

    public async Task<Order> UpdateCheckoutDetailsAsync(int orderId, string userId, CheckoutDto checkout, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(userId))
            throw new InvalidOperationException("برای ادامه باید وارد حساب کاربری شوید.");

        var order = await _orderRepository.GetWithItemsAsync(orderId, cancellationToken)
            ?? throw new InvalidOperationException("سفارش یافت نشد.");

        if (!string.Equals(order.UserId, userId, StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("شما اجازه دسترسی به این سفارش را ندارید.");

        order.FullName = checkout.FullName;
        order.Email = checkout.Email;
        order.PhoneNumber = checkout.PhoneNumber;
        order.Address = checkout.Address;
        order.ShippingAddress = checkout.Address;
        order.BillingAddress = checkout.Address;
        order.Notes = checkout.Notes;
        order.UpdatedAt = DateTime.UtcNow;

        await _orderRepository.UpdateAsync(order, cancellationToken);
        await _orderRepository.SaveChangesAsync(cancellationToken);

        return order;
    }

    public async Task<Order?> GetOrderAsync(int id, string? userId, CancellationToken cancellationToken = default)
    {
        var order = await _orderRepository.GetWithItemsAsync(id, cancellationToken);
        if (order == null) return null;
        // Admins (userId == null) can see all orders
        if (!string.IsNullOrEmpty(userId) && order.UserId != userId) return null;
        return order;
    }

    public Task<IReadOnlyList<Order>> GetUserOrdersAsync(string userId, CancellationToken cancellationToken = default)
        => _orderRepository.GetByUserIdAsync(userId, cancellationToken);

    public async Task CompletePaymentAsync(int orderId, CancellationToken cancellationToken = default)
    {
        // Backward-compatible call (no payment metadata).
        await CompletePaymentWithMetadataAsync(orderId, paymentReferenceId: null, paymentProvider: "ZarinPal", cancellationToken);
    }

    public async Task CompletePaymentWithMetadataAsync(
        int orderId,
        string? paymentReferenceId,
        string? paymentProvider,
        CancellationToken cancellationToken = default)
    {
        var order = await _orderRepository.GetWithItemsAsync(orderId, cancellationToken)
            ?? throw new InvalidOperationException("سفارش یافت نشد.");

        // Store payment metadata for Admin.
        order.PaymentReferenceId = paymentReferenceId;
        order.PaymentProvider = paymentProvider;

        // Transition: Pending -> Paid (or Paid -> Paid if already marked).
        if (order.Status != OrderStatus.Paid)
            AddStatusHistory(order, order.Status, OrderStatus.Paid, changedByUserId: null, note: "پرداخت تایید شد.");

        order.Status = OrderStatus.Paid;
        order.PaidAt = DateTime.UtcNow;
        order.UpdatedAt = DateTime.UtcNow;

        // Only generate download tokens for digital products
        foreach (var item in order.Items)
        {
            var product = await _productRepository.GetByIdAsync(item.ProductId, cancellationToken);
            if (product == null || string.IsNullOrEmpty(product.DigitalFileName))
                continue;

            // Avoid duplicate tokens
            if (item.DownloadToken != null)
                continue;

            var token = new DownloadToken
            {
                OrderItemId = item.Id,
                Token = Guid.NewGuid().ToString("N"),
                ExpiresAt = DateTime.UtcNow.AddDays(30),
                MaxDownloads = 5
            };
            await _downloadTokenRepository.AddAsync(token, cancellationToken);
        }

        // Transition: Paid -> Completed
        if (order.Status != OrderStatus.Completed)
            AddStatusHistory(order, order.Status, OrderStatus.Completed, changedByUserId: null, note: "پرداخت کامل شد.");

        order.Status = OrderStatus.Completed;
        await _orderRepository.UpdateAsync(order, cancellationToken);

        // Single SaveChanges covers both order update and new tokens
        await _orderRepository.SaveChangesAsync(cancellationToken);
    }

    public Task<IReadOnlyList<Order>> GetAllOrdersAsync(CancellationToken cancellationToken = default)
        => _orderRepository.GetAllAsync(cancellationToken);

    public async Task UpdateOrderStatusAsync(int orderId, OrderStatus status, CancellationToken cancellationToken = default)
    {
        await UpdateOrderStatusWithHistoryAsync(orderId, status, changedByUserId: null, note: null, cancellationToken);
    }

    public async Task UpdateOrderStatusWithHistoryAsync(
        int orderId,
        OrderStatus status,
        string? changedByUserId,
        string? note,
        CancellationToken cancellationToken = default)
    {
        var order = await _orderRepository.GetByIdAsync(orderId, cancellationToken)
            ?? throw new InvalidOperationException("سفارش یافت نشد.");

        if (order.Status != status)
        {
            AddStatusHistory(order, order.Status, status, changedByUserId, note);
        }

        order.Status = status;
        order.UpdatedAt = DateTime.UtcNow;
        if (status == OrderStatus.Paid || status == OrderStatus.Completed)
            order.PaidAt ??= DateTime.UtcNow;

        await _orderRepository.UpdateAsync(order, cancellationToken);
        await _orderRepository.SaveChangesAsync(cancellationToken);
    }

    private static void AddStatusHistory(
        Order order,
        OrderStatus oldStatus,
        OrderStatus newStatus,
        string? changedByUserId,
        string? note)
    {
        // Navigation collection is initialized in the entity.
        order.StatusHistory.Add(new OrderStatusHistory
        {
            OrderId = order.Id,
            OldStatus = oldStatus,
            NewStatus = newStatus,
            ChangedByUserId = changedByUserId,
            Note = note
        });
    }

    private static string GenerateOrderNumber()
        => $"ORD-{DateTime.UtcNow:yyyyMMdd}-{Random.Shared.Next(10000, 99999)}";
}
