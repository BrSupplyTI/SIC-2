namespace SIC.Api.Contracts.Produtos;

public sealed class ProductDetailDto
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
    public int FlagMarcaPropria { get; set; }
    public string IconeSegmento { get; set; } = string.Empty;
    public int FlagAtivoSegmento { get; set; }
    public string? DtMensagem { get; set; }
    public string? DtCadastro { get; set; }
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
    public IReadOnlyList<ProductPropertyDto> Propriedades { get; set; } = [];
    public bool HasFichaTecnica { get; set; }
    public bool HasFichaSeguranca { get; set; }
}
