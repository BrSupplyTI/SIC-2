namespace SIC.Domain.Entities.PrePedidosPDF;

/// <summary>
/// Entidade de detalhes do pré-pedido (findByID).
/// </summary>
public sealed class PrePedidoPDFDetalhe
{
    public int PDFPrePedidoPDFID { get; set; }
    public string Arquivo { get; set; } = string.Empty;
    public string ArquivoFormat { get; set; } = string.Empty;
    public string OrdemCompraDataHoraFormat { get; set; } = string.Empty;
    public int CadastroUsuarioID { get; set; }
    public string CadastroNmUsuario { get; set; } = string.Empty;
    public int StatusPrePedidoPDFID { get; set; }
    public string StatusDescricao { get; set; } = string.Empty;
    public int CotacaoID { get; set; }
    public string OrdemCompra { get; set; } = string.Empty;
    public string CNPJ { get; set; } = string.Empty;
    public int ClienteLocalEntregaID { get; set; }
    public int ClienteEnderecoID { get; set; }
    public string Cliente { get; set; } = string.Empty;
    public string Estabelecimento { get; set; } = string.Empty;
    public int EstabelecimentoID { get; set; }
    public string Endereco { get; set; } = string.Empty;
    public string NmLocalEntrega { get; set; } = string.Empty;
    public string CondPagto { get; set; } = string.Empty;
    public string CanalVenda { get; set; } = string.Empty;
    public string TipoOVSAP { get; set; } = string.Empty;
    public string TabelaPreco { get; set; } = string.Empty;
    public string CdExtCliente { get; set; } = string.Empty;
    public int ClienteID { get; set; }
    public int TblPrecoID { get; set; }
    public string LogoCliente { get; set; } = string.Empty;
    public string NmCliente { get; set; } = string.Empty;
    public decimal VlrMinimoBloqueioPedido { get; set; }
    public string ConteudoArquivoJson { get; set; } = string.Empty;
    public string ObsNota { get; set; } = string.Empty;
    public string ObsComprador { get; set; } = string.Empty;
    public int? ClienteCategoriaPedidoID { get; set; }
    public string NmCategoriaPedido { get; set; } = string.Empty;

    public IReadOnlyList<PrePedidoPDFItem> Itens { get; set; } = [];
    public IReadOnlyList<PrePedidoPDFLog> Logs { get; set; } = [];
    public IReadOnlyList<PrePedidoPDFEndereco> Enderecos { get; set; } = [];
    public IReadOnlyList<PrePedidoPDFLocalEntrega> LocaisEntrega { get; set; } = [];
    public IReadOnlyList<PrePedidoPDFCnpj> Cnpjs { get; set; } = [];
    public int QtdLogsErro { get; set; }
}
