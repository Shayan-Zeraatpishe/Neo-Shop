using Shop.Application.DTOs;
using Shop.Domain.Entities;

namespace Shop.Application.Interfaces;

public interface IOrderRepository : IRepository<Order>
{
    Task<Order?> GetByOrderNumberAsync(string orderNumber, CancellationToken cancellationToken = default);
    Task<Order?> GetWithItemsAsync(int id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Order>> GetByUserIdAsync(string userId, CancellationToken cancellationToken = default);

    Task<PagedResult<AdminOrderListItemDto>> GetAdminOrdersAsync(
        AdminOrderFilterDto filter,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default);

    Task<AdminOrderDetailsDto?> GetAdminOrderDetailsAsync(int orderId, CancellationToken cancellationToken = default);
}
