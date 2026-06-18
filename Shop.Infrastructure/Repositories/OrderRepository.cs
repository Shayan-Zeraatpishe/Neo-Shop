using Microsoft.EntityFrameworkCore;
using Shop.Application.Interfaces;
using Shop.Application.DTOs;
using Shop.Domain.Entities;
using Shop.Infrastructure.Data;
using Shop.Domain.Enums;

namespace Shop.Infrastructure.Repositories;

public class OrderRepository : Repository<Order>, IOrderRepository
{
    public OrderRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<Order?> GetByOrderNumberAsync(string orderNumber, CancellationToken cancellationToken = default)
        => await DbSet
            .Include(o => o.Items)
            .ThenInclude(i => i.Product)
            .Include(o => o.Items)
            .ThenInclude(i => i.DownloadToken)
            .FirstOrDefaultAsync(o => o.OrderNumber == orderNumber, cancellationToken);

    public async Task<Order?> GetWithItemsAsync(int id, CancellationToken cancellationToken = default)
        => await DbSet
            .Include(o => o.Items)
            .ThenInclude(i => i.Product)
            .Include(o => o.Items)
            .ThenInclude(i => i.DownloadToken)
            .FirstOrDefaultAsync(o => o.Id == id, cancellationToken);

    public async Task<IReadOnlyList<Order>> GetByUserIdAsync(string userId, CancellationToken cancellationToken = default)
        => await DbSet
            .AsNoTracking()
            .Where(o => o.UserId == userId)
            .Include(o => o.Items)
            .ThenInclude(i => i.Product)
            .Include(o => o.Items)
            .ThenInclude(i => i.DownloadToken)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync(cancellationToken);

    public override async Task<IReadOnlyList<Order>> GetAllAsync(CancellationToken cancellationToken = default)
        => await DbSet
            .AsNoTracking()
            .Include(o => o.Items)
            .ThenInclude(i => i.Product)
            .Include(o => o.Items)
            .ThenInclude(i => i.DownloadToken)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync(cancellationToken);

    public async Task<PagedResult<AdminOrderListItemDto>> GetAdminOrdersAsync(
        AdminOrderFilterDto filter,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        pageNumber = pageNumber < 1 ? 1 : pageNumber;
        pageSize = pageSize < 1 ? 20 : pageSize;

        var query = DbSet.AsNoTracking().AsQueryable();

        if (filter.OrderId.HasValue)
            query = query.Where(o => o.Id == filter.OrderId.Value);

        if (!string.IsNullOrWhiteSpace(filter.Customer))
        {
            var term = filter.Customer.Trim();
            query = query.Where(o =>
                o.FullName.Contains(term) ||
                o.Email.Contains(term) ||
                o.PhoneNumber.Contains(term) ||
                o.OrderNumber.Contains(term));
        }

        if (filter.Status.HasValue)
            query = query.Where(o => o.Status == filter.Status.Value);

        if (filter.PaymentStatus.HasValue)
        {
            if (filter.PaymentStatus.Value == PaymentStatus.Paid)
                query = query.Where(o => o.PaidAt != null);
            else
                query = query.Where(o => o.PaidAt == null);
        }

        if (filter.FromDateUtc.HasValue)
            query = query.Where(o => o.CreatedAt >= filter.FromDateUtc.Value);

        if (filter.ToDateUtc.HasValue)
            query = query.Where(o => o.CreatedAt < filter.ToDateUtc.Value.AddDays(1));

        if (filter.MinTotalAmount.HasValue)
            query = query.Where(o => o.TotalAmount >= filter.MinTotalAmount.Value);

        if (filter.MaxTotalAmount.HasValue)
            query = query.Where(o => o.TotalAmount <= filter.MaxTotalAmount.Value);

        var totalCount = await query.CountAsync(cancellationToken);

        query = ApplyAdminOrderSort(query, filter);

        var skip = (pageNumber - 1) * pageSize;
        var items = await query
            .Skip(skip)
            .Take(pageSize)
            .Select(o => new AdminOrderListItemDto
            {
                Id = o.Id,
                OrderNumber = o.OrderNumber,
                FullName = o.FullName,
                Email = o.Email,
                PhoneNumber = o.PhoneNumber,
                Status = o.Status,
                PaymentStatus = o.PaidAt != null ? PaymentStatus.Paid : PaymentStatus.Unpaid,
                CreatedAtUtc = o.CreatedAt,
                PaidAtUtc = o.PaidAt,
                TotalAmount = o.TotalAmount
            })
            .ToListAsync(cancellationToken);

        return new PagedResult<AdminOrderListItemDto>
        {
            Items = items,
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize
        };
    }

    public async Task<AdminOrderDetailsDto?> GetAdminOrderDetailsAsync(
        int orderId,
        CancellationToken cancellationToken = default)
    {
        var order = await DbSet
            .AsNoTracking()
            .Include(o => o.Items)
            .Include(o => o.StatusHistory)
            .FirstOrDefaultAsync(o => o.Id == orderId, cancellationToken);

        if (order == null) return null;

        return new AdminOrderDetailsDto
        {
            Id = order.Id,
            OrderNumber = order.OrderNumber,
            FullName = order.FullName,
            Email = order.Email,
            PhoneNumber = order.PhoneNumber,
            ShippingAddress = order.ShippingAddress,
            BillingAddress = order.BillingAddress,
            AddressFallback = order.Address,
            Notes = order.Notes,
            SubTotal = order.SubTotal,
            DiscountAmount = order.DiscountAmount,
            TotalAmount = order.TotalAmount,
            Status = order.Status,
            CreatedAtUtc = order.CreatedAt,
            PaidAtUtc = order.PaidAt,
            PaymentStatus = order.PaidAt != null ? PaymentStatus.Paid : PaymentStatus.Unpaid,
            PaymentProvider = order.PaymentProvider,
            PaymentReferenceId = order.PaymentReferenceId,
            Items = order.Items
                .Select(i => new AdminOrderItemDto
                {
                    ProductName = i.ProductName,
                    Quantity = i.Quantity,
                    UnitPrice = i.UnitPrice,
                    TotalPrice = i.TotalPrice
                })
                .ToList(),
            StatusHistory = order.StatusHistory
                .OrderByDescending(h => h.CreatedAt)
                .Select(h => new OrderStatusHistoryItemDto
                {
                    OldStatus = h.OldStatus,
                    NewStatus = h.NewStatus,
                    ChangedAtUtc = h.CreatedAt,
                    ChangedByUserId = h.ChangedByUserId,
                    Note = h.Note
                })
                .ToList()
        };
    }

    private static IQueryable<Order> ApplyAdminOrderSort(IQueryable<Order> query, AdminOrderFilterDto filter)
    {
        var desc = filter.SortDescending;
        return filter.SortBy switch
        {
            "OrderId" => desc ? query.OrderByDescending(o => o.Id) : query.OrderBy(o => o.Id),
            "OrderNumber" => desc ? query.OrderByDescending(o => o.OrderNumber) : query.OrderBy(o => o.OrderNumber),
            "TotalAmount" => desc ? query.OrderByDescending(o => o.TotalAmount) : query.OrderBy(o => o.TotalAmount),
            "Status" => desc ? query.OrderByDescending(o => o.Status) : query.OrderBy(o => o.Status),
            "PaidAt" => desc ? query.OrderByDescending(o => o.PaidAt) : query.OrderBy(o => o.PaidAt),
            _ => desc ? query.OrderByDescending(o => o.CreatedAt) : query.OrderBy(o => o.CreatedAt)
        };
    }
}
