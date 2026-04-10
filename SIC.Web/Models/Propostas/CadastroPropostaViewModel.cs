using SIC.Web.Models.Produtos;

namespace SIC.Web.Models.Propostas;

public sealed class CadastroPropostaViewModel
{
    public int? EstabelecimentoID { get; set; }
    public string? NomeProposta { get; set; }
    public string? Qualidade { get; set; }
    public string? Segmento { get; set; }

    public IReadOnlyList<CatalogEstablishmentVm> Estabelecimentos { get; set; } = [];

    public IReadOnlyList<string> Qualidades { get; set; } =
    [
        "Premium",
        "Standard",
        "Econômico"
    ];

    public IReadOnlyList<string> Segmentos { get; set; } =
    [
        "Alimentício",
        "Higiene e Limpeza",
        "Cosméticos",
        "Farmacêutico",
        "Industrial"
    ];
}
