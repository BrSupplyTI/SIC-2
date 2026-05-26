namespace SIC.Api.Contracts.Liberacao;

/// <summary>
/// DTO de cabeçalho de pedido para a tela de detalhes.
/// Datas já formatadas para exibição (dd/MM/yyyy ou dd/MM/yyyy HH:mm).
/// </summary>
public sealed class LiberacaoPedidoDetalheDto
{
    public int CotacaoID { get; set; }
    public int EstabelecimentoID { get; set; }
    public string DescTipoOVSAP { get; set; } = string.Empty;
    public string TipoOVSAP { get; set; } = string.Empty;
    public string DataHoraPedido { get; set; } = string.Empty;
    public string Estabelecimento { get; set; } = string.Empty;

    public string CodERPCliente { get; set; } = string.Empty;
    public string RazaoSocialCliente { get; set; } = string.Empty;
    public string TipoDocumentoCliente { get; set; } = string.Empty;
    public string NmCliente { get; set; } = string.Empty;
    public string CPFCNPJCliente { get; set; } = string.Empty;
    public string InscrEstCliente { get; set; } = string.Empty;
    public int FlagFreteServico { get; set; }
    public string UFCliente { get; set; } = string.Empty;
    public string NmUFCliente { get; set; } = string.Empty;
    public string TelefoneCliente { get; set; } = string.Empty;
    public string LogoCliente { get; set; } = string.Empty;
    public string LogoClienteDark { get; set; } = string.Empty;
    public int ClienteID { get; set; }
    public int ClienteLocalEntregaID { get; set; }

    public string CompStatusCotacao { get; set; } = string.Empty;
    public string OrdemCompra { get; set; } = string.Empty;
    public string ObsCotacao { get; set; } = string.Empty;
    public string ObsAprovacao { get; set; } = string.Empty;
    public string ObsNota { get; set; } = string.Empty;
    public int CanalVendaID { get; set; }
    public string NmCanalVenda { get; set; } = string.Empty;
    public string NmCarteira { get; set; } = string.Empty;
    public int StatusCotacao { get; set; }
    public int ClienteUsuarioID { get; set; }
    public string NmUsuario { get; set; } = string.Empty;
    public string EmailUsuario { get; set; } = string.Empty;
    public string NmCondPagto { get; set; } = string.Empty;
    public int CondPagtoID { get; set; }
    public string Situacao { get; set; } = string.Empty;
    public int StatusID { get; set; }
    public decimal VlrFrete { get; set; }
    public decimal VlrFreteServico { get; set; }

    public int ClienteEnderecoID { get; set; }
    public string RazaoSocialEndereco { get; set; } = string.Empty;
    public string TipoDocumentoEndereco { get; set; } = string.Empty;
    public string CodERPEndereco { get; set; } = string.Empty;
    public string CPFCNPJEndereco { get; set; } = string.Empty;
    public string RuaEndereco { get; set; } = string.Empty;
    public string NumeroEndereco { get; set; } = string.Empty;
    public string ComplementoEndereco { get; set; } = string.Empty;
    public string BairroEndereco { get; set; } = string.Empty;
    public string CidadeEndereco { get; set; } = string.Empty;
    public string IBGEEndereco { get; set; } = string.Empty;
    public string UFEndereco { get; set; } = string.Empty;
    public string CEPEndereco { get; set; } = string.Empty;
    public string FoneEndereco { get; set; } = string.Empty;

    public int FlagEnderecoDirerente { get; set; }
    public string TipoEnderecoEntrega { get; set; } = string.Empty;
    public string RuaEntrega { get; set; } = string.Empty;
    public string NumeroEntrega { get; set; } = string.Empty;
    public string ComplementoEntrega { get; set; } = string.Empty;
    public string BairroEntrega { get; set; } = string.Empty;
    public string CidadeEntrega { get; set; } = string.Empty;
    public string IBGEEntrega { get; set; } = string.Empty;
    public string UFEntrega { get; set; } = string.Empty;
    public string CEPEntrega { get; set; } = string.Empty;
    public string CdControle { get; set; } = string.Empty;
    public string NmLocalEntrega { get; set; } = string.Empty;
    public string ObsLocalEntrega { get; set; } = string.Empty;
    public int FlagBloqCredito { get; set; }
    public int SituacaoLocal { get; set; }

    public int CategoriaID { get; set; }
    public string NmCategoria { get; set; } = string.Empty;
    public string LiberaAutomatico { get; set; } = string.Empty;
    public string FormaPagamento { get; set; } = string.Empty;

    public string DataHoraUltimaAprovacao { get; set; } = string.Empty;
    public string DataProgLiberacao { get; set; } = string.Empty;
    public string DataProgEmbarque { get; set; } = string.Empty;
    public string DataProgEntrega { get; set; } = string.Empty;
    public string DataSLACliente { get; set; } = string.Empty;
    public string DataPedido { get; set; } = string.Empty; // apenas a data (sem hora) para exibição na Programação
    public int DiasSLA { get; set; }
    public string ObsCalcFrete { get; set; } = string.Empty;

    public decimal Peso { get; set; }
    public int QtItens { get; set; }
    public int QtItensBRSupply { get; set; }
    public int QtItensMarketplace { get; set; }
    public int QtItensAlocados { get; set; }
    public int QtItensNaoAlocados { get; set; }
    public int QtItensBloqueados { get; set; }
    public decimal VlrTotalBRSupply { get; set; }
    public decimal VlrTotalMarketplace { get; set; }
    public decimal VlrTotalProdutos { get; set; }
    public decimal VlrTotalItensAlocados { get; set; }
    public decimal VlrTotalItensNaoAlocados { get; set; }

    public string StatusSLACliente { get; set; } = string.Empty;
    public int DiasAtrasoSLACliente { get; set; }

    public string NmTransportadora { get; set; } = string.Empty;
    public string ApelidoTransportadora { get; set; } = string.Empty;
    public string CNPJTransportadora { get; set; } = string.Empty;
    public int TransportadoraID { get; set; }
    public int PrazoEntregaCalc { get; set; }
    public int PrazoEntregaTransp { get; set; }
    public string FreteAgrupado { get; set; } = string.Empty;
    public int TblFreteID { get; set; }
    public int CidadeIDDestino { get; set; }
    public decimal VlrFreteCalc { get; set; }
    public decimal PercentualFrete { get; set; }

    public decimal MargemBruta { get; set; }
    public string NrContrato { get; set; } = string.Empty;
    public string LB { get; set; } = string.Empty;
    public string ROL { get; set; } = string.Empty;
    public int QtFilaSAP { get; set; }

    // Parâmetros do cliente (SIC_Parametros_ClienteEndereco)
    public decimal Taxa { get; set; }
    public decimal Minimo { get; set; }
    public decimal Bloqueio { get; set; }
    public int FlagNaoEditarPedidoComOC { get; set; }
}
