namespace SIC.Web.Models.Produtos;

public sealed class ProdutoDetalhesViewModel
{
    public int ItemID { get; set; }
    public string CdItem { get; set; } = string.Empty;
    public string NmItem { get; set; } = string.Empty;
    public int SegmentoID { get; set; }
    public string NmSegmento { get; set; } = string.Empty;
    public int FamiliaID { get; set; }
    public string NmFamilia { get; set; } = string.Empty;
    public int SubFamiliaID { get; set; }
    public string NmSubFamilia { get; set; } = string.Empty;
    public string NmMarca { get; set; } = string.Empty;
    public string DescricaoLonga { get; set; } = string.Empty;
    public string TituloDsInformacaoTecnica { get; set; } = string.Empty;
    public string InformacaoTecnica { get; set; } = string.Empty;
    public int QtMultiplicador { get; set; }
    public int QtMultiplicadorLiberado { get; set; }
    public decimal NrPeso { get; set; }
    public string Mensagem { get; set; } = string.Empty;
    public string? DtMensagem { get; set; }
    public string? DtCadastro { get; set; }
    public int FlagMarcaPropria { get; set; }
    public string IconeSegmento { get; set; } = string.Empty;
    public int FlagAtivoSegmento { get; set; }
    public string? Tags { get; set; }
    public string? NumCA { get; set; }
    public string? ValidadeCA { get; set; }
    public int FlagLancamento { get; set; }
    public int FlagSustentavel { get; set; }
    public string CdUnidade { get; set; } = string.Empty;
    public int QtdEmbalagem { get; set; }
    public string NmEmbalagem { get; set; } = string.Empty;
    public string UnidadeMedida { get; set; } = string.Empty;
    public int QtdeCaixaMaster { get; set; }
    public string? CodigoBarras { get; set; }
    public string? CodDUN { get; set; }
    public int FlagFaltaNoFabricante { get; set; }
    public int FlagAtivo { get; set; }
    public int FlagCatalogo { get; set; }
    public string CdClassificacaoFiscal { get; set; } = string.Empty;
    public string? Modelo { get; set; }
    public string? Normas { get; set; }
    public string? Referencia { get; set; }
    public string? FSC { get; set; }
    public string? ABNT { get; set; }
    public string? Anatel { get; set; }
    public string? Anvisa { get; set; }
    public string? Inmetro { get; set; }
    public int FlagDualSourcing { get; set; }
    public string? Origem { get; set; }
    public int FlagOutlet { get; set; }
    public string FotoPrincipal { get; set; } = string.Empty;
    public IReadOnlyList<string> FotosSecundarias { get; set; } = [];
    public IReadOnlyList<ProdutoPropriedadeVm> Propriedades { get; set; } = [];
    public IReadOnlyList<ProdutoEstoqueEstabelecimentoVm> Estoques { get; set; } = [];
}

public sealed class ProdutoPropriedadeVm
{
    public string Tipo { get; set; } = string.Empty;
    public string Nome { get; set; } = string.Empty;
    public string Valor { get; set; } = string.Empty;
}

public sealed class ProdutoEstoqueEstabelecimentoVm
{
    public string NmEstabelecimento { get; set; } = string.Empty;
    public string NmCurto { get; set; } = string.Empty;
    public int EstabelecimentoID { get; set; }
    public string CdEstabelecimento { get; set; } = string.Empty;

    public int QtdContabilSAP { get; set; }
    public int QtEstoqueVirtualSP { get; set; }
    public int QtdRemessaSAP { get; set; }
    public int QtdProcessamentoSAP { get; set; }
    public int QtdDisponivelSAP { get; set; }

    public int QtAlocadaSemOVSAP { get; set; }
    public int QtAlocadaComOVSAP { get; set; }
    public int QtAlocadaSIC { get; set; }
    public int QtNaoDebitaEstoqueSIC { get; set; }
    public int QtDisponivelSIC { get; set; }

    public int QtdEstoqueSAP { get; set; }
    public int QtEstoque { get; set; }
    public int QtReservadaSIC { get; set; }
    public int QtEstoqueWMS { get; set; }
    public int QtProcessamentoWMS { get; set; }

    public decimal? VlrCustoAquisicao { get; set; }
    public decimal? VlrCustoMedio { get; set; }

    public string? FollowComprasNegociacao { get; set; }
    public string? DtFollowComprasNegociacao { get; set; }
    public string? DsFollowCompras { get; set; }
    public string? DtFollowCompras { get; set; }

    public string Curva { get; set; } = "-";
    public string Criticidade { get; set; } = string.Empty;
    public int FlagOutlet { get; set; }
    public int FlagSobDemanda { get; set; }
    public int FlagOcultoEstoqueZero { get; set; }

    public int MinLeadTime { get; set; }
    public int MaxLeadTime { get; set; }

    public string? DetalhesCustoAquisicao { get; set; }

    public string? NmComprador { get; set; }
    public string? EmailComprador { get; set; }
    public string? FotoComprador { get; set; }
    public string? NmGestor { get; set; }
    public string? EmailGestor { get; set; }
    public string? FotoGestor { get; set; }
    public string? NmCompradorInternacional { get; set; }
    public string? EmailCompradorInternacional { get; set; }
    public string? FotoCompradorInternacional { get; set; }
}

public sealed class ProdutoAlocacaoEstoqueVm
{
    public int Pedido { get; set; }
    public string DtPedido { get; set; } = string.Empty;
    public string? DtProgLiberacao { get; set; }
    public string NmCliente { get; set; } = string.Empty;
    public string DsStatusCotacao { get; set; } = string.Empty;
    public string CdEstabelecimento { get; set; } = string.Empty;
    public int QtSolicitada { get; set; }
    public int QtRupturas { get; set; }
    public string NmCanalVenda { get; set; } = string.Empty;
    public string OrdemVendaSAP { get; set; } = string.Empty;
}

public sealed class ProdutoOrdemCompraVm
{
    public int Quantidade { get; set; }
    public string? DtPrevisao { get; set; }
    public string OrdemCompra { get; set; } = string.Empty;
    public string XPed { get; set; } = string.Empty;
    public string NmEstabelecimento { get; set; } = string.Empty;
    public string CdEstabelecimento { get; set; } = string.Empty;
    public string RazaoSocial { get; set; } = string.Empty;
}
