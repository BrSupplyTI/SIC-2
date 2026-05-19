namespace SIC.Domain.Entities.Cotacao;

/// <summary>
/// Opção de frete retornada por Fn_Calcula_Fretes_Proposta.
/// </summary>
public sealed class CotacaoFreteOpcao
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
