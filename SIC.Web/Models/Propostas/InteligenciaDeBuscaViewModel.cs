using SIC.Web.Models.Produtos;

namespace SIC.Web.Models.Propostas;

public sealed class InteligenciaDeBuscaViewModel
{
    public IReadOnlyList<CatalogEstablishmentVm> Estabelecimentos { get; set; } = [];
}
