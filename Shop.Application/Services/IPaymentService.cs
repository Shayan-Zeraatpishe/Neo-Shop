using Shop.Application.DTOs;

namespace Shop.Application.Services;

public interface IPaymentService
{
    Task<PaymentInitResult> InitiatePaymentAsync(PaymentRequestDto request, CancellationToken cancellationToken = default);
    Task<PaymentVerifyResult> VerifyPaymentAsync(string authority, decimal amount, CancellationToken cancellationToken = default);
}
