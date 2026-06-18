namespace Shop.Application.Services;

public interface IDownloadService
{
    Task<(string FilePath, string FileName)?> ValidateAndGetFileAsync(string token, string? userId, CancellationToken cancellationToken = default);
    Task RecordDownloadAsync(string token, CancellationToken cancellationToken = default);
}
