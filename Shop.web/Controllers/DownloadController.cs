using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shop.Application.Services;

namespace Shop.web.Controllers;

[Authorize]
public class DownloadController : Controller
{
    private readonly IDownloadService _downloadService;

    public DownloadController(IDownloadService downloadService)
    {
        _downloadService = downloadService;
    }

    [HttpGet]
    public async Task<IActionResult> File(string token, CancellationToken cancellationToken)
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        var fileInfo = await _downloadService.ValidateAndGetFileAsync(token, userId, cancellationToken);

        if (fileInfo == null)
            return NotFound();

        await _downloadService.RecordDownloadAsync(token, cancellationToken);

        return PhysicalFile(fileInfo.Value.FilePath, "application/octet-stream", fileInfo.Value.FileName);
    }
}
