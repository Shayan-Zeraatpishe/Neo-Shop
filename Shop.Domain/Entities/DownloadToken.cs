using Shop.Domain.Common;

namespace Shop.Domain.Entities;

public class DownloadToken : BaseEntity
{
    public int OrderItemId { get; set; }
    public string Token { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public int DownloadCount { get; set; }
    public int MaxDownloads { get; set; } = 5;
    public bool IsRevoked { get; set; }

    public OrderItem OrderItem { get; set; } = null!;

    public bool IsValid => !IsRevoked && DateTime.UtcNow <= ExpiresAt && DownloadCount < MaxDownloads;
}
