using SIC.Api.Contracts.Cotacao;

namespace SIC.Api.Services.Cotacao;

/// <summary>
/// Operações de leitura da Cotação.
/// </summary>
public interface ICotacaoQueryService
{
    Task<IReadOnlyList<CotacaoCatalogoItemDto>> BuscarCatalogoAsync(
        string descricao,
        int clienteId,
        int tblPrecoId,
        int estabelecimentoId,
        int propostaId,
        CancellationToken cancellationToken = default);
}
