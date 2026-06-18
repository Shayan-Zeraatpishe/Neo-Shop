using Shop.Application.DTOs;
using Shop.Domain.Entities;

namespace Shop.Application.Services;

public interface IOrderService
{
    Task<Order> CreateBuyNowOrderAsync(string userId, Product product, CancellationToken cancellationToken = default);
    Task<Order> UpdateCheckoutDetailsAsync(int orderId, string userId, CheckoutDto checkout, CancellationToken cancellationToken = default);
    Task<Order?> GetOrderAsync(int id, string? userId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Order>> GetUserOrdersAsync(string userId, CancellationToken cancellationToken = default);
    Task CompletePaymentAsync(int orderId, CancellationToken cancellationToken = default);
    // Stores gateway/provider metadata and writes status history.
    Task CompletePaymentWithMetadataAsync(
        int orderId,
        string? paymentReferenceId,
        string? paymentProvider,
        CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Order>> GetAllOrdersAsync(CancellationToken cancellationToken = default);
    Task UpdateOrderStatusAsync(int orderId, Domain.Enums.OrderStatus status, CancellationToken cancellationToken = default);
    Task UpdateOrderStatusWithHistoryAsync(
        int orderId,
        Domain.Enums.OrderStatus status,
        string? changedByUserId,
        string? note,
        CancellationToken cancellationToken = default);
}
