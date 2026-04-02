namespace SIC.Domain.Entities;

public sealed class ProductStockEstablishment
{
    public string NmEstabelecimento { get; set; } = string.Empty;
    public string NmCurto { get; set; } = string.Empty;
    public int EstabelecimentoID { get; set; }
    public string CdEstabelecimento { get; set; } = string.Empty;

    // Estoque SAP
    public int QtdContabilSAP { get; set; }
    public int QtEstoqueVirtualSP { get; set; }
    public int QtdRemessaSAP { get; set; }
    public int QtdProcessamentoSAP { get; set; }
    public int QtdDisponivelSAP { get; set; }

    // Alocações
    public int QtAlocadaSemOVSAP { get; set; }
    public int QtAlocadaComOVSAP { get; set; }
    public int QtAlocadaSIC { get; set; }
    public int QtNaoDebitaEstoqueSIC { get; set; }
    public int QtDisponivelSIC { get; set; }

    // Estoque consolidado
    public int QtdEstoqueSAP { get; set; }
    public int QtEstoque { get; set; }
    public int QtReservadaSIC { get; set; }

    // Estoque WMS
    public int QtEstoqueWMS { get; set; } 
    public int QtProcessamentoWMS { get; set; }

    // Custos
    public decimal? VlrCustoAquisicao { get; set; }
    public decimal? VlrCustoMedio { get; set; }

    // Follow compras
    public string? FollowComprasNegociacao { get; set; }
    public DateTime? DtFollowComprasNegociacao { get; set; }
    public string? DsFollowCompras { get; set; }
    public DateTime? DtFollowCompras { get; set; }

    // Classificação
    public string Curva { get; set; } = "-";
    public string Criticidade { get; set; } = string.Empty;
    public int FlagOutlet { get; set; }
    public int FlagSobDemanda { get; set; }
    public int FlagOcultoEstoqueZero { get; set; }

    // Lead time
    public int MinLeadTime { get; set; }
    public int MaxLeadTime { get; set; }

    // Fornecedor
    public string? DetalhesCustoAquisicao { get; set; }

    // Responsáveis
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
