namespace SIC.Api.Contracts.Produtos;

public sealed class ProductStockEstablishmentDto
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
