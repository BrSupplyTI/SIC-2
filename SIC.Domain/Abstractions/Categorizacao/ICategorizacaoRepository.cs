using SIC.Domain.Entities.Categorizacao;

namespace SIC.Domain.Abstractions.Categorizacao;

public interface ICategorizacaoRepository
{
    Task<IReadOnlyList<CategorizacaoItem>> GetItensCategorizadosAsync(int? estabelecimentoId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<CategorizacaoItemSemCategoria>> GetItensSemCategoriaAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<CategorizacaoTipoLista>> GetCategoriasAsync(CancellationToken cancellationToken = default);
    Task<bool> SalvarCategoriaAsync(int itemId, int pesquisaTipoListaId, CancellationToken cancellationToken = default);
    Task<bool> RemoverCategoriaAsync(int itemId, CancellationToken cancellationToken = default);
}
