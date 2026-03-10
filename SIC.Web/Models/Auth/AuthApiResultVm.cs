namespace SIC.Web.Models.Auth;

public sealed class AuthApiResultVm
{
    public bool Success { get; set; }
    public string? ErrorCode { get; set; }
    public string? Message { get; set; }
    public string? ExistingIp { get; set; }
    public SicUserVm? User { get; set; }
}
