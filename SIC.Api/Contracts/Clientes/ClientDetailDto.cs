namespace SIC.Api.Contracts.Clientes;

public sealed class ClientDetailDto
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
    public int FlagValidacaoFiscal { get; set; }
    public int FlagValidaImpostosTrocaItem { get; set; }
    public int FlagProgramacaoAutomatica { get; set; }
    public int FlagUtilizaJanelaCorte { get; set; }
    public int FlagUtilizaLiberacaoAutomatica { get; set; }
    public int FlagLibCatTercAutomatico { get; set; }
    public int FlagNaoValidaNCMTrocaItem { get; set; }

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
    public string StatusCredito { get; set; } = string.Empty;
    public int FlagStatusCredito { get; set; }
    public int DiasRestantes { get; set; }
    public int FlagIntegracaoAutomaticaSAP { get; set; }
    public string NmCanalDistribuicaoSAP { get; set; } = string.Empty;
    public string TipoDocumentoSAP { get; set; } = string.Empty;
    public string DsTipoDocumentoSAP { get; set; } = string.Empty;
    public string DsFormaPagamentoSAP { get; set; } = string.Empty;
    public string CodFormaPagamentoSAP { get; set; } = string.Empty;
}
