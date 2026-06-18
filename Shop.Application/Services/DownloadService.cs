using Shop.Application.Interfaces;

namespace Shop.Application.Services;

public class DownloadService : IDownloadService
{
    private readonly IDownloadTokenRepository _downloadTokenRepository;
    private readonly IOrderRepository _orderRepository;
    private readonly IProductRepository _productRepository;
    private readonly string _digitalProductsPath;

    public DownloadService(
        IDownloadTokenRepository downloadTokenRepository,
        IOrderRepository orderRepository,
        IProductRepository productRepository,
        string digitalProductsPath)
    {
        _downloadTokenRepository = downloadTokenRepository;
        _orderRepository = orderRepository;
        _productRepository = productRepository;
        _digitalProductsPath = digitalProductsPath;
    }

    public async Task<(string FilePath, string FileName)?> ValidateAndGetFileAsync(string token, string? userId, CancellationToken cancellationToken = default)
    {
        var downloadToken = await _downloadTokenRepository.GetByTokenAsync(token, cancellationToken);
        if (downloadToken == null || !downloadToken.IsValid)
            return null;

        var order = downloadToken.OrderItem.Order;
        if (!string.IsNullOrEmpty(userId) && order.UserId != userId)
            return null;

        if (order.Status != Domain.Enums.OrderStatus.Paid && order.Status != Domain.Enums.OrderStatus.Completed)
            return null;

        var product = downloadToken.OrderItem.Product;
        if (string.IsNullOrEmpty(product.DigitalFileName))
            return null;

        var filePath = Path.Combine(_digitalProductsPath, product.DigitalFileName);
        if (!File.Exists(filePath))
            return null;

        var fileName = product.DigitalFileOriginalName ?? product.DigitalFileName;
        return (filePath, fileName);
    }

    public async Task RecordDownloadAsync(string token, CancellationToken cancellationToken = default)
    {
        var downloadToken = await _downloadTokenRepository.GetByTokenAsync(token, cancellationToken);
        if (downloadToken == null) return;

        downloadToken.DownloadCount++;
        downloadToken.UpdatedAt = DateTime.UtcNow;
        await _downloadTokenRepository.UpdateAsync(downloadToken, cancellationToken);
        await _downloadTokenRepository.SaveChangesAsync(cancellationToken);
    }
}
