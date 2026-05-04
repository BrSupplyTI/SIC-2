using SIC.Web.Models.Produtos;

namespace SIC.Web.Models.Propostas;

public sealed class CadastroPropostaViewModel
{
    public int? PropostaID { get; set; }
    public int? EstabelecimentoID { get; set; }
    public string? NomeProposta { get; set; }

    public IReadOnlyList<CatalogEstablishmentVm> Estabelecimentos { get; set; } = [];
    public IReadOnlyList<SegmentoVm> Segmentos { get; set; } = [];
    public List<QualSegDetalheVm> QualSegCadastrados { get; set; } = [];

    public static IReadOnlyList<QualidadeOption> Qualidades { get; } =
    [
        new() { Value = "B", Desc = "Básico" },
        new() { Value = "I", Desc = "Intermediário" },
        new() { Value = "P", Desc = "Premium" },
    ];
}

public sealed class QualidadeOption
{
    public string Value { get; set; } = string.Empty;
    public string Desc { get; set; } = string.Empty;
}
