using System.Collections.Concurrent;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Shop.Application.DTOs;
using Shop.Application.Services;

namespace Shop.Infrastructure.Services;

/// <summary>
/// ZarinPal payment gateway with sandbox fallback when merchant ID is not configured.
/// </summary>
public class ZarinPalPaymentService : IPaymentService
{
    private static readonly ConcurrentDictionary<string, decimal> PendingPayments = new();

    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;
    private readonly ILogger<ZarinPalPaymentService> _logger;

    public ZarinPalPaymentService(
        IHttpClientFactory httpClientFactory,
        IConfiguration configuration,
        ILogger<ZarinPalPaymentService> logger)
    {
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<PaymentInitResult> InitiatePaymentAsync(PaymentRequestDto request, CancellationToken cancellationToken = default)
    {
        var merchantId = _configuration["Payment:ZarinPal:MerchantId"];
        var useSandbox = _configuration.GetValue("Payment:ZarinPal:UseSandbox", true);

        if (string.IsNullOrWhiteSpace(merchantId))
        {
            var authority = Guid.NewGuid().ToString("N");
            PendingPayments[authority] = request.Amount;
            return new PaymentInitResult
            {
                Success = true,
                Authority = authority,
                GatewayUrl = $"/Payment/Gateway?authority={authority}&orderId={request.OrderId}"
            };
        }

        var baseUrl = useSandbox
            ? "https://sandbox.zarinpal.com/pg/v4/payment"
            : "https://api.zarinpal.com/pg/v4/payment";

        try
        {
            var client = _httpClientFactory.CreateClient();
            var payload = new
            {
                merchant_id = merchantId,
                amount = (int)(request.Amount / 10),
                callback_url = request.CallbackUrl,
                description = request.Description,
                metadata = new { order_id = request.OrderId.ToString() }
            };

            var response = await client.PostAsJsonAsync($"{baseUrl}/request.json", payload, cancellationToken);
            var json = await response.Content.ReadFromJsonAsync<JsonElement>(cancellationToken: cancellationToken);

            if (json.TryGetProperty("data", out var data) &&
                data.TryGetProperty("authority", out var authorityEl) &&
                data.TryGetProperty("code", out var codeEl) &&
                codeEl.GetInt32() == 100)
            {
                var authority = authorityEl.GetString()!;
                PendingPayments[authority] = request.Amount;
                var gatewayHost = useSandbox ? "https://sandbox.zarinpal.com" : "https://www.zarinpal.com";
                return new PaymentInitResult
                {
                    Success = true,
                    Authority = authority,
                    GatewayUrl = $"{gatewayHost}/pg/StartPay/{authority}"
                };
            }

            var message = json.TryGetProperty("errors", out var errors)
                ? errors.ToString()
                : "خطا در اتصال به درگاه پرداخت";
            return new PaymentInitResult { Success = false, ErrorMessage = message };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "ZarinPal payment initiation failed");
            return new PaymentInitResult { Success = false, ErrorMessage = "خطا در اتصال به درگاه پرداخت" };
        }
    }

    public async Task<PaymentVerifyResult> VerifyPaymentAsync(string authority, decimal amount, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(authority))
            return new PaymentVerifyResult { Success = false, ErrorMessage = "کد پرداخت نامعتبر است" };

        var merchantId = _configuration["Payment:ZarinPal:MerchantId"];
        var useSandbox = _configuration.GetValue("Payment:ZarinPal:UseSandbox", true);

        if (string.IsNullOrWhiteSpace(merchantId))
        {
            if (PendingPayments.TryRemove(authority, out var expectedAmount) && expectedAmount == amount)
            {
                return new PaymentVerifyResult
                {
                    Success = true,
                    ReferenceId = $"SIM-{DateTime.UtcNow:yyyyMMddHHmmss}"
                };
            }
            return new PaymentVerifyResult { Success = false, ErrorMessage = "پرداخت تایید نشد" };
        }

        var baseUrl = useSandbox
            ? "https://sandbox.zarinpal.com/pg/v4/payment"
            : "https://api.zarinpal.com/pg/v4/payment";

        try
        {
            var client = _httpClientFactory.CreateClient();
            var payload = new
            {
                merchant_id = merchantId,
                amount = (int)(amount / 10),
                authority
            };

            var response = await client.PostAsJsonAsync($"{baseUrl}/verify.json", payload, cancellationToken);
            var json = await response.Content.ReadFromJsonAsync<JsonElement>(cancellationToken: cancellationToken);

            if (json.TryGetProperty("data", out var data) &&
                data.TryGetProperty("code", out var codeEl))
            {
                var code = codeEl.GetInt32();
                if (code is 100 or 101)
                {
                    PendingPayments.TryRemove(authority, out _);
                    var refId = data.TryGetProperty("ref_id", out var refEl) ? refEl.ToString() : authority;
                    return new PaymentVerifyResult { Success = true, ReferenceId = refId };
                }
            }

            return new PaymentVerifyResult { Success = false, ErrorMessage = "تایید پرداخت ناموفق بود" };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "ZarinPal payment verification failed");
            return new PaymentVerifyResult { Success = false, ErrorMessage = "خطا در تایید پرداخت" };
        }
    }
}
