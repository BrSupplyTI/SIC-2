namespace SIC.Api.Contracts.Pedidos;

public sealed class FreightCalculationItemDto
{
    public int TransportadoraID { get; set; }
    public string NomeTransportadora { get; set; } = string.Empty;
    public int PrazoLogistico { get; set; }
    public int PrazoComercial { get; set; }
    public decimal TaxaExtra { get; set; }
    public int QtItensRestritos { get; set; }
    public bool ClienteRestrito { get; set; }
    public bool ClienteFixo { get; set; }
    public bool ObrigatoriaCanalVenda { get; set; }
    public decimal ValorFrete { get; set; }
}
