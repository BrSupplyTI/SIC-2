using System.Net.Http.Json;
using SIC.Web.Models.Cotacao;

namespace SIC.Web.Services.Cotacao;

/// <summary>
/// Operações de leitura (queries) da Cotação via API.
/// </summary>
public sealed partial class CotacaoApiClient
{
    public async Task<IReadOnlyList<CotacaoCatalogoItemViewModel>> BuscarCatalogoAsync(
        int propostaId,
        string descricao,
        int clienteId,
        int tblPrecoId,
        int estabelecimentoId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var qs = $"?descricao={Uri.EscapeDataString(descricao)}&clienteId={clienteId}&tblPrecoId={tblPrecoId}&estabelecimentoId={estabelecimentoId}";
            var data = await httpClient.GetFromJsonAsync<List<CotacaoCatalogoItemViewModel>>(
                $"api/cotacao/{propostaId}/buscar-catalogo{qs}", cancellationToken);

            return data ?? [];
        }
        catch
        {
            return [];
        }
    }
}
