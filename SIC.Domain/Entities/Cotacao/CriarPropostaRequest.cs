namespace SIC.Domain.Entities.Cotacao;

public sealed class CriarPropostaRequest
{
    public string? Nome { get; set; }
    public int TipoID { get; set; }
    public string? TipoNome { get; set; }
    public int EstabelecimentoID { get; set; }
    public int ClienteId { get; set; }
    public int? ClienteEnderecoID { get; set; }
    public int? ClienteLocalEntregaID { get; set; }
    public string? ObsLocalEntrega { get; set; }
    public int? TabelaPrecoID { get; set; }
    public bool FlagPrecoConformeTabela { get; set; }
    public string? UfOrigem { get; set; }
    public string? UfDestino { get; set; }
    public int? CodigoIBGE { get; set; }
    public decimal? MargemPadrao { get; set; }
    public DateTime? DataValidade { get; set; }
    public int? CondPagtoId { get; set; }
    public int? FormaPagamentoSAP { get; set; }
    public string? TipoOVSAP { get; set; }
    public string? OrdemCompra { get; set; }
    public string? NrContrato { get; set; }
    public int? TipoMotivoIDSAP { get; set; }
    public string? NrChamado { get; set; }
    public int? PedidoOriginalID { get; set; }
    public string? ContatoNome { get; set; }
    public string? ContatoEmail { get; set; }
    public string? Obs { get; set; }
    public int UsuarioId { get; set; }
    public decimal ValorVendaTotal { get; set; }
    public decimal Frete { get; set; }
    public decimal VlrPedidoMinimo { get; set; }
}
