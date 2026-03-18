namespace SIC.Api.Contracts.Pedidos;

public sealed class OrderSearchResultDto
{
    public bool Success { get; set; }
    public string? ErrorCode { get; set; }
    public string Message { get; set; } = string.Empty;
    public int? TotalPedidos { get; set; }
    public bool ShowModal { get; set; }
    public string? RedirectUrl { get; set; }
    public IReadOnlyList<PurchaseOrderItemDto> Pedidos { get; set; } = [];
}
