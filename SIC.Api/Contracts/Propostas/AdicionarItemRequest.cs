namespace SIC.Api.Contracts.Propostas;

public sealed class AdicionarItemRequest
{
    public int PropostaID { get; set; }
    public int ItemID { get; set; }
    public int QtdAnual { get; set; }
    public decimal MargemPadrao { get; set; }
    public string ItemFiltro { get; set; } = string.Empty;
}
