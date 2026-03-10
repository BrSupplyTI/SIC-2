namespace SIC.Api.Models.Auth;

public sealed class AuthResult
{
    public bool Success { get; set; }
    public string? ErrorCode { get; set; }
    public string? Message { get; set; }
    public string? ExistingIp { get; set; }
    public SicUserDto? User { get; set; }
}
