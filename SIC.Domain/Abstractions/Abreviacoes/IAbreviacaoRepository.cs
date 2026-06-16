using SIC.Domain.Entities.Abreviacoes;

namespace SIC.Domain.Abstractions.Abreviacoes;

public interface IAbreviacaoRepository
{
    Task<IReadOnlyList<AbreviacaoItem>> BuscarDadosAsync(CancellationToken cancellationToken = default);
    Task<bool> GravarAsync(string texto, string abreviacao, int usuarioId, CancellationToken cancellationToken = default);
    Task<bool> ExcluirAsync(int id, CancellationToken cancellationToken = default);
}
