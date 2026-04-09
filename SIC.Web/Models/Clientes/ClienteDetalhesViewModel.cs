namespace SIC.Web.Models.Clientes;

public sealed class ClienteDetalhesViewModel
{
    public int ClienteID { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string CodigoSAP { get; set; } = string.Empty;
    public string RazaoSocial { get; set; } = string.Empty;
    public string TipoDocumento { get; set; } = string.Empty;
    public string CPFCNPJ { get; set; } = string.Empty;
    public string InscrEstadual { get; set; } = string.Empty;
    public string? LogoCliente { get; set; }
    public string? LogoUrl { get; set; }
    public int CarteiraID { get; set; }
    public string NmCarteira { get; set; } = string.Empty;
    public string NmEstabelecimento { get; set; } = string.Empty;
    public int EstabelecimentoID { get; set; }
    public string CdEstabelecimento { get; set; } = string.Empty;
    public string Estado { get; set; } = string.Empty;
    public string Situacao { get; set; } = string.Empty;

    // Frete
    public decimal VlrPedidoMinimo { get; set; }
    public decimal VlrTaxaEntrega { get; set; }

    // Configurações
    public int FlagIntegracaoAutomaticaSAP { get; set; }
    public int FlagUtilizaLiberacaoAutomatica { get; set; }
    public int FlagProgramacaoAutomatica { get; set; }
    public int FlagUtilizaJanelaCorte { get; set; }
    public int FlagFreteAgrupCNPJ { get; set; }
    public int FlagValidacaoFiscal { get; set; }
    public int FlagValidaImpostosTrocaItem { get; set; }
    public int FlagNaoLiberarPedidoSemOC { get; set; }
    public int FlagNaoEditarPedidoComOC { get; set; }
    public int FlagPoliticaEntrega { get; set; }
    public int FlagMultiCD { get; set; }
    public int FlagMultiCDEnderecos { get; set; }
    public int FlagMultiCDPedidos { get; set; }
    public int FlagTrocaItemAutomatica { get; set; }
    public int FlagNaoValidaTrocaItem { get; set; }
    public int FlagNaoValidaNCMTrocaItem { get; set; }
    public int FlagAutoConcat { get; set; }
    public int FlagOrdemCompra { get; set; }
    public int FlagTipoConcat { get; set; }
    public int FlagConcatPedidoRuptura { get; set; }
    public int FlagAutoIsentaFrete { get; set; }
    public int FlagPrioConcatPerfilSolicitante { get; set; }
    public int FlagConcatItemFornecedor { get; set; }
    public int FlagConcatIsolarCategorias { get; set; }

    // Subcadastros
    public int QtUsuarios { get; set; }
    public int QtEnderecos { get; set; }
    public int QtLocaisEntrega { get; set; }
    public string? ClienteMae { get; set; }

    // Crédito
    public int PerfilCreditoID { get; set; }
    public string NmPerfilCredito { get; set; } = string.Empty;
    public string? DtAnaliseCredito { get; set; }
    public string? DtVencAnaliseCredito { get; set; }
    public decimal VlrLimiteCredito { get; set; }
    public string TipoControle { get; set; } = string.Empty;
    public int DiasAtrasoPermitido { get; set; }
    public int MesesDuracaoAnalise { get; set; }
    public string ResponsavelAnaliseCredito { get; set; } = string.Empty;
    public int UsuarioIDAnaliseCredito { get; set; }
    public string EmailResponsavelAnaliseCredito { get; set; } = string.Empty;
    public string FotoResponsavelAnaliseCredito { get; set; } = string.Empty;
    public string StatusCredito { get; set; } = string.Empty;
    public int FlagStatusCredito { get; set; }
    public int DiasRestantes { get; set; }    
    public string NmCanalDistribuicaoSAP { get; set; } = string.Empty;
    public string TipoDocumentoSAP { get; set; } = string.Empty;
    public string DsTipoDocumentoSAP { get; set; } = string.Empty;
    public string DsFormaPagamentoSAP { get; set; } = string.Empty;
    public string CodFormaPagamentoSAP { get; set; } = string.Empty;
    public string NmTblPreco { get; set; } = string.Empty;
    public int TblPrecoID { get; set; } 
    public string TelefoneCliente { get; set; } = string.Empty;
    public string SegmentoCliente { get; set; } = string.Empty;
    public string NmCanalVenda { get; set; } = string.Empty;
    public string NmClientePerfil { get; set; } = string.Empty;
    public string NmCondPagto { get; set; } = string.Empty;
    public int CanalVendaID { get; set; }
    public string Cnae { get; set; } = string.Empty;
    public string CodCnaeSetor { get; set; } = string.Empty;
    public string DsCnaeSetor { get; set; } = string.Empty;
    public string CdNatJuridica { get; set; } = string.Empty;
    public string DsNatJuridica { get; set; } = string.Empty;
    // Consultores
    public IReadOnlyList<ConsultorVm> Consultores { get; set; } = [];
}
