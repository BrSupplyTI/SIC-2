namespace SIC.Domain.Abstractions;

using SIC.Domain.Entities;

public interface ILiberacaoPedidoRepository
{
    Task<IReadOnlyList<LiberacaoPedidoItem>> ListarAsync(
        int estabelecimentoId,
        int usuarioId,
        string? filtroPalavra1 = null,
        string? filtroPalavra2 = null,
        string? filtroPalavra3 = null,
        int filtroOrdemCompra = 0,
        int filtroRuptura = 0,
        int filtroFrete = 0,
        int filtroMargemNegativa = 0,
        decimal filtroValorAbaixo = 0,
        decimal filtroValorAcima = 0,
        string? filtroIntegracaoSAP = null,
        string? filtroContemItem = null,
        int filtroAtrasados = 0,
        int filtroFretePagar = 0,
        CancellationToken cancellationToken = default);
}
