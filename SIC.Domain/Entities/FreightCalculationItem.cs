namespace SIC.Domain.Entities;

public sealed class FreightCalculationItem
{
    public int TransportadoraID { get; set; }
    public string NomeTransportadora { get; set; } = string.Empty;
    public int PrazoLogistico { get; set; }
    public int PrazoComercial { get; set; }
    public decimal TaxaExtra { get; set; }
    public int QtItensRestritos { get; set; }
    public int FlagClienteRestrito { get; set; }
    public int FlagClienteFixo { get; set; }
    public int FlagObrigatoriaCanalVenda { get; set; }
    public decimal ValorFrete { get; set; }
}
