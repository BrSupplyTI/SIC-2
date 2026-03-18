namespace SIC.Domain.Entities;

public sealed class PurchaseOrderSearchResult
{
    public int Total { get; set; }
    public IReadOnlyList<PurchaseOrderOrderItem> Orders { get; set; } = [];
}
