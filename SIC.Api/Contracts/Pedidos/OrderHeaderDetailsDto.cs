namespace SIC.Api.Contracts.Pedidos;

public sealed class OrderHeaderDetailsDto
{
    public int Pedido { get; set; }
    public string? CompStatusCotacao { get; set; }
    public string? StatusAuxiliar { get; set; } = string.Empty;
    public string? DataPedido { get; set; }
    public string Estabelecimento { get; set; } = string.Empty;
    public string OrdemCompra { get; set; } = string.Empty;
    public string CanalVenda { get; set; } = string.Empty;
    public string Carteira { get; set; } = string.Empty;
    public string Situacao { get; set; } = string.Empty;
    public string Setor { get; set; } = string.Empty;
    public int StatusID { get; set; }
    public string Categoria { get; set; } = string.Empty;
    public string LabelInfoCategoria { get; set; } = string.Empty;
    public string InfoCategoria { get; set; } = string.Empty;
    public string InfoCarrinho { get; set; } = string.Empty;
    public string LabelInfoCarrinho { get; set; } = string.Empty;
    public string NomeCliente { get; set; } = string.Empty;
    public string CodigoCliente { get; set; } = string.Empty;
    public string CNPJCliente { get; set; } = string.Empty;
    public string RazaoSocialEndereco { get; set; } = string.Empty;
    public string CpfCnpj { get; set; } = string.Empty;
    public string RuaEndereco { get; set; } = string.Empty;
    public string NumeroEndereco { get; set; } = string.Empty;
    public string ComplementoEndereco { get; set; } = string.Empty;
    public string BairroEndereco { get; set; } = string.Empty;
    public int ClienteID { get; set; }
    public string LogoCliente { get; set; } = string.Empty;
    public string LogoClienteDark { get; set; } = string.Empty;
    public string FlagTipoDocumento { get; set; } = string.Empty;
    public string TelefoneCliente { get; set; } = string.Empty;
    public string InscrEstCliente { get; set; } = string.Empty;
    public string MotivoOVSAP { get; set; } = string.Empty;
    public string DescTipoOVSAP { get; set; } = string.Empty;
    public string TipoOVSAP { get; set; } = string.Empty;
    public int? CotacaoIdOriginal { get; set; }
    public int? CotacaoIDSubstituta { get; set; }
    public string NrContrato { get; set; } = string.Empty;
    public decimal MargemBruta { get; set; } = 0;
    public decimal LB { get; set; } = 0;
    public decimal ROL { get; set; } = 0;
    public int ClienteEnderecoID { get; set; } = 0;
    public string CodClienteEndereco { get; set; } = string.Empty;
    public string FlagTipoDocumentoEndereco { get; set; } = string.Empty;
    public string CidadeEndereco { get; set; } = string.Empty;
    public string UFEndereco { get; set; } = string.Empty;
    public string CidadeIBGEEndereco { get; set; } = string.Empty;
    public string CepEndereco { get; set; } = string.Empty;
    public int FlagEnderecoDirerente { get; set; } = 0;
    public string NmLocalEntrega { get; set; } = string.Empty;
    public string CdControle { get; set; } = string.Empty;
    public int ClienteLocalEntregaID { get; set; } = 0;
    public string RuaLocal { get; set; } = string.Empty;
    public string NumeroLocal { get; set; } = string.Empty;
    public string ComplementoLocal { get; set; } = string.Empty;
    public string BairroLocal { get; set; } = string.Empty;
    public string CidadeLocal { get; set; } = string.Empty;
    public string UFLocal { get; set; } = string.Empty;
    public string CidadeIBGELocal { get; set; } = string.Empty;
    public string CEPLocal { get; set; } = string.Empty;
    public string FormaPagto { get; set; } = string.Empty;
    public string CondPagto { get; set; } = string.Empty;
    public string HashPagamento { get; set; } = string.Empty;
    public string NmSolicitante { get; set; } = string.Empty;
    public string EmailSolicitante { get; set; } = string.Empty;
    public int? TransportadoraID { get; set; }
    public string NmTransportadora { get; set; } = string.Empty;
    public string CNPJTransportadora { get; set; } = string.Empty;
    public decimal? VlrFreteCalc { get; set; }
    public int? PrazoEntregaCalc { get; set; }
    public int? PrazoEntregaTransp { get; set; }
    public string? DtProgLiberacao { get; set; }
    public string? DtProgEmbarque { get; set; }
    public string? DtProgEntrega { get; set; }
    public string? DtPlanejadaOperacao { get; set; }
    public string? DtSLACliente { get; set; }
    public string? DtProgEmbFollow { get; set; }
    public string FreteAgrupado { get; set; } = string.Empty;
    public string ObsCalcFrete { get; set; } = string.Empty;
    public string? DtPrevEntFollow { get; set; }
    public string? DtPrevisaoEntrega { get; set; }
    public string StatusSLA { get; set; } = string.Empty;
    public string ObsCotacao { get; set; } = string.Empty;
    public string ObsAprovacao { get; set; } = string.Empty;
    public string ObsNota { get; set; } = string.Empty;
    public string ObsLocalEntrega { get; set; } = string.Empty;
    public int QtItensBRSupply { get; set; } = 0;
    public int QtItensTerceiros { get; set; } = 0;
    public int QtItensRuptura { get; set; } = 0;
    public decimal ValorItensBRSupply { get; set; } = 0;
    public decimal ValorItensTerceiros { get; set; } = 0;
    public decimal VlrFrete { get; set; } = 0;
    public decimal VlrTaxaServico { get; set; } = 0;
    public int FlagIntegradoSAP { get; set; } = 0;
}
