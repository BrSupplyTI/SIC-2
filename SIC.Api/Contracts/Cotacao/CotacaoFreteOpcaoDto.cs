namespace SIC.Api.Contracts.Cotacao;

public sealed class CotacaoFreteOpcaoDto
{
    public int TransportadoraID { get; set; }
    public string Nome { get; set; } = string.Empty;
    public int TempoLogistico { get; set; }
    public int TempoComercial { get; set; }
    public decimal TaxaExtra { get; set; }
    public decimal ValorFrete { get; set; }
    public int QtItensRestritos { get; set; }
    public bool FlagObrigatoriaCanalVenda { get; set; }
    public bool FlagClienteRestrito { get; set; }
    public bool FlagClienteFixo { get; set; }
}
