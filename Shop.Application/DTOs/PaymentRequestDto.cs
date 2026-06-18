namespace Shop.Application.DTOs;

public class PaymentRequestDto
{
    public int OrderId { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Description { get; set; } = string.Empty;
    public string CallbackUrl { get; set; } = string.Empty;
}

public class PaymentInitResult
{
    public bool Success { get; set; }
    public string? Authority { get; set; }
    public string? GatewayUrl { get; set; }
    public string? ErrorMessage { get; set; }
}

public class PaymentVerifyResult
{
    public bool Success { get; set; }
    public string? ReferenceId { get; set; }
    public string? ErrorMessage { get; set; }
}
