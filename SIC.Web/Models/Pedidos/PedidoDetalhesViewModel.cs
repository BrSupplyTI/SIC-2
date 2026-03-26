namespace SIC.Web.Models.Pedidos;

public sealed class PedidoDetalhesViewModel
{
    public int PedidoId { get; set; }

    public HeaderSection Header { get; set; } = new();
    public ClienteSection Cliente { get; set; } = new();
    public FaturamentoSection Faturamento { get; set; } = new();
    public FreteSection Frete { get; set; } = new();
    public ObservacaoSection Observacao { get; set; } = new();
    public TotalSection Total { get; set; } = new();

    //public IReadOnlyList<ItemSection> Itens { get; set; } = [];
    public IReadOnlyList<LogAprovacaoSection> LogsAprovacao { get; set; } = [];
    public IReadOnlyList<NotaFiscalRelacionadaSection> NotasFiscaisRelacionadas { get; set; } = [];
    public IReadOnlyList<TrackingSection> Trackings { get; set; } = [];

    public sealed class HeaderSection
    {
        public string NumeroPedido { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string? StatusAuxiliar { get; set; } = string.Empty;
        public int? StatusID { get; set; } = 0;
        public string Setor { get; set; } = string.Empty;
        public string DataCriacao { get; set; } = string.Empty;
        public string Origem { get; set; } = string.Empty;
        public string OrdemCompra { get; set; } = string.Empty;
        public string CanalVenda { get; set; } = string.Empty;
        public string Carteira { get; set; } = string.Empty;
        public string Categoria { get; set; } = string.Empty;
        public string LabelInfoCategoria { get; set; } = string.Empty;
        public string InfoCategoria { get; set; } = string.Empty;
        public string InfoCarrinho { get; set; } = string.Empty;
        public string LabelInfoCarrinho { get; set; } = string.Empty;
        public string Estabelecimento { get; set; } = string.Empty;
        public string MotivoOVSAP { get; set; } = string.Empty;
        public string DescTipoOVSAP { get; set; } = string.Empty;
        public string TipoOVSAP { get; set; } = string.Empty;
        public int? CotacaoIdOriginal { get; set; }
        public int? CotacaoIDSubstituta { get; set; }
        public string NrContrato { get; set; } = string.Empty;
        public decimal MargemBruta { get; set; } = 0;
        public decimal LB { get; set; } = 0;
        public decimal ROL { get; set; } = 0;
        public string NmSolicitante { get; set; } = string.Empty;
        public string EmailSolicitante { get; set; } = string.Empty;
        public int FlagIntegradoSAP { get; set; } = 0;
        public int QtNotasFiscais { get; set; } = 0;
        public int QtRomaneios { get; set; } = 0;
        public int QtChamados { get; set; } = 0;
        public int QtAnaliseCredito { get; set; } = 0;
        public int QtAprovacoes { get; set; } = 0;
    }

    public sealed class ClienteSection
    {
        public int ClienteID { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string CNPJCliente { get; set; } = string.Empty;        
        public string CodigoExterno { get; set; } = string.Empty;        
        public string LogoCliente { get; set; } = string.Empty;
        public string LogoClienteDark { get; set; } = string.Empty;
        public string FlagTipoDocumento { get; set; } = string.Empty;
        public string TelefoneCliente { get; set; } = string.Empty;
        public string InscrEstCliente { get; set; } = string.Empty;
    }

    public sealed class FaturamentoSection
    {
        public int ClienteEnderecoID { get; set; } = 0;
        public string CodClienteEndereco { get; set; } = string.Empty;
        public string FlagTipoDocumentoEndereco { get; set; } = string.Empty;
        public string RazaoSocialEndereco { get; set; } = string.Empty;
        public string CpfCnpj { get; set; } = string.Empty;
        public string RuaEndereco { get; set; } = string.Empty;
        public string NumeroEndereco { get; set; } = string.Empty;
        public string ComplementoEndereco { get; set; } = string.Empty;
        public string BairroEndereco { get; set; } = string.Empty;
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
    }

    public sealed class FreteSection
    {
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
    }
    /*
    public sealed class ItemSection
    {
        public string Codigo { get; set; } = string.Empty;
        public string Descricao { get; set; } = string.Empty;
        public int Quantidade { get; set; }
        public string ValorUnitario { get; set; } = string.Empty;
    }
    */
    public sealed class LogAprovacaoSection
    {
        public string DataHora { get; set; } = string.Empty;
        public string Usuario { get; set; } = string.Empty;
        public string Acao { get; set; } = string.Empty;
    }

    public sealed class NotaFiscalRelacionadaSection
    {
        public string Numero { get; set; } = string.Empty;
        public string Serie { get; set; } = string.Empty;
        public string Emissao { get; set; } = string.Empty;
    }

    public sealed class TrackingSection
    {
        public string DataHora { get; set; } = string.Empty;
        public string Evento { get; set; } = string.Empty;
        public string Local { get; set; } = string.Empty;
    }

    public sealed class ObservacaoSection
    {
        public string ObsCotacao { get; set; } = string.Empty;
        public string ObsAprovacao { get; set; } = string.Empty;
        public string ObsNota { get; set; } = string.Empty;
        public string ObsLocalEntrega { get; set; } = string.Empty;
    }
    public sealed class TotalSection
    {
        public int QtItensBRSupply { get; set; } = 0;
        public int QtItensTerceiros { get; set; } = 0;
        public int QtItensRuptura { get; set; } = 0;
        public decimal ValorItensBRSupply { get; set; } = 0;
        public decimal ValorItensTerceiros { get; set; } = 0;
        public decimal VlrFrete { get; set; } = 0;
        public decimal VlrTaxaServico { get; set; } = 0;
    }
}
