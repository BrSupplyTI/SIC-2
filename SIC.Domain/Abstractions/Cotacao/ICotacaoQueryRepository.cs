using SIC.Domain.Entities.Cotacao;

namespace SIC.Domain.Abstractions.Cotacao;

/// <summary>
/// Operações de leitura da Cotação no banco de dados.
/// </summary>
public interface ICotacaoQueryRepository
{
    Task<IReadOnlyList<CotacaoCatalogoItem>> BuscarCatalogoAsync(
        string descricao,
        int clienteId,
        int tblPrecoId,
        int estabelecimentoId,
        int propostaId,
        CancellationToken cancellationToken = default);
}
