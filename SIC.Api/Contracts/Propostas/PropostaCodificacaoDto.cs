namespace SIC.Api.Contracts.Propostas;

public sealed class PropostaCodificacaoDto
{
    public int PropostaID { get; set; }
    public string CdProposta { get; set; } = string.Empty;
    public int EstabelecimentoID { get; set; }
    public string NmEstabelecimento { get; set; } = string.Empty;
    public string NomeProposta { get; set; } = string.Empty;
    public int StatusID { get; set; }
    public string NmStatus { get; set; } = string.Empty;
    public int TotalItens { get; set; }
    public string PercentualConcluido { get; set; } = "0%";
    public List<QualSegCodificacaoDto> QualSeg { get; set; } = [];
    public List<PropostaCodificacaoItemDto> Itens { get; set; } = [];
}

public sealed class QualSegCodificacaoDto
{
    public string Qualidade { get; set; } = string.Empty;
    public string NmSegmento { get; set; } = string.Empty;
}

public sealed class PropostaCodificacaoItemDto
{
    public int PropostaItemID { get; set; }
    public int PropostaID { get; set; }
    public string DescricaoBreve { get; set; } = string.Empty;
    public string NumeroCA { get; set; } = string.Empty;
    public string NmMarca { get; set; } = string.Empty;
    public int? ItemID { get; set; }
    public string CdItem { get; set; } = string.Empty;
    public string NmItem { get; set; } = string.Empty;
    public string Qualidade { get; set; } = string.Empty;
    public string VlrCustoAquisicaoFormat { get; set; } = string.Empty;
    public bool FlagForaDeMix { get; set; }
    public bool FlagSemCorrespondencia { get; set; }
    public bool FlagAddManual { get; set; }
    public string CodCliente { get; set; } = string.Empty;
    public string DescricaoDetalhada { get; set; } = string.Empty;
    public string Familia { get; set; } = string.Empty;
    public string MarcaFornecedor { get; set; } = string.Empty;
    public string UnidadeMedida { get; set; } = string.Empty;
    public string TargetFormat { get; set; } = string.Empty;
    public int? QtdAnual { get; set; }
}
