namespace SIC.Web.Models.Auth;

public sealed class OperationResultVm
{
    public bool Success { get; set; }
    public string? ErrorCode { get; set; }
    public string Message { get; set; } = string.Empty;
    public string? ResetToken { get; set; }
    public DateTimeOffset? ExpiresAt { get; set; }
}
