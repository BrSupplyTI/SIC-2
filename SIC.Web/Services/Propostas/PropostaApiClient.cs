using System.Net.Http.Json;
using SIC.Web.Models.Propostas;

namespace SIC.Web.Services.Propostas;

public sealed class PropostaApiClient(HttpClient httpClient)
{
    public async Task<IReadOnlyList<PropostaItemVm>> GetListAsync(
        string? filtroCodigo,
        string? filtroNome,
        string? filtroEstabelecimento,
        string? filtroStatus,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var query = new List<string>();

            if (!string.IsNullOrWhiteSpace(filtroCodigo))
                query.Add($"filtroCodigo={Uri.EscapeDataString(filtroCodigo)}");

            if (!string.IsNullOrWhiteSpace(filtroNome))
                query.Add($"filtroNome={Uri.EscapeDataString(filtroNome)}");

            if (!string.IsNullOrWhiteSpace(filtroEstabelecimento))
                query.Add($"filtroEstabelecimento={Uri.EscapeDataString(filtroEstabelecimento)}");

            if (!string.IsNullOrWhiteSpace(filtroStatus))
                query.Add($"filtroStatus={Uri.EscapeDataString(filtroStatus)}");

            var qs = query.Count > 0 ? "?" + string.Join("&", query) : string.Empty;

            var data = await httpClient.GetFromJsonAsync<List<PropostaItemVm>>(
                $"api/propostas{qs}", cancellationToken);

            return data ?? [];
        }
        catch
        {
            return [];
        }
    }

    public async Task<IReadOnlyList<SegmentoVm>> GetSegmentosAsync(
        CancellationToken cancellationToken = default)
    {
        try
        {
            var data = await httpClient.GetFromJsonAsync<List<SegmentoVm>>(
                "api/propostas/segmentos", cancellationToken);
            return data ?? [];
        }
        catch
        {
            return [];
        }
    }

    public async Task<SalvarPropostaResultVm?> SalvarPropostaAsync(
        SalvarPropostaRequestVm request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await httpClient.PostAsJsonAsync("api/propostas", request, cancellationToken);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<SalvarPropostaResultVm>(cancellationToken);
        }
        catch
        {
            return null;
        }
    }

    public async Task<PropostaDetalheVm?> GetByIdAsync(
        int propostaId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            return await httpClient.GetFromJsonAsync<PropostaDetalheVm>(
                $"api/propostas/{propostaId}", cancellationToken);
        }
        catch
        {
            return null;
        }
    }

    public async Task<CodificacaoViewModel?> GetCodificacaoAsync(
        int propostaId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            return await httpClient.GetFromJsonAsync<CodificacaoViewModel>(
                $"api/propostas/{propostaId}/codificacao", cancellationToken);
        }
        catch
        {
            return null;
        }
    }

    public async Task<List<ItemBuscaResultVm>> BuscarItensBrSupplyAsync(
        int estabelecimentoId,
        string filtro,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var qs = $"?estabelecimentoId={estabelecimentoId}&filtro={Uri.EscapeDataString(filtro)}";
            var data = await httpClient.GetFromJsonAsync<List<ItemBuscaResultVm>>(
                $"api/propostas/buscar-itens{qs}", cancellationToken);
            return data ?? [];
        }
        catch
        {
            return [];
        }
    }

    public async Task<bool> AdicionarItemPropostaAsync(
        AdicionarItemRequestVm request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await httpClient.PostAsJsonAsync("api/propostas/adicionar-item", request, cancellationToken);
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> ExcluirItemPropostaAsync(
        int propostaId,
        int propostaItemId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await httpClient.DeleteAsync(
                $"api/propostas/{propostaId}/itens/{propostaItemId}", cancellationToken);
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }

    public async Task<(bool Success, int Inserted)> ImportarItensAsync(
        ImportarItensRequestVm request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await httpClient.PostAsJsonAsync("api/propostas/importar-itens", request, cancellationToken);
            if (!response.IsSuccessStatusCode) return (false, 0);

            var result = await response.Content.ReadFromJsonAsync<ImportarItensResultVm>(cancellationToken);
            return (result?.Success ?? false, result?.Inserted ?? 0);
        }
        catch
        {
            return (false, 0);
        }
    }

    private sealed class ImportarItensResultVm
    {
        public bool Success { get; set; }
        public int Inserted { get; set; }
    }

    public async Task<CodificarItemResultVm?> CodificarItemAsync(
        int propostaItemId,
        int estabelecimentoId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await httpClient.PostAsync(
                $"api/propostas/{propostaItemId}/codificar-item?estabelecimentoId={estabelecimentoId}",
                null, cancellationToken);
            if (!response.IsSuccessStatusCode) return null;
            return await response.Content.ReadFromJsonAsync<CodificarItemResultVm>(cancellationToken);
        }
        catch
        {
            return null;
        }
    }

    public async Task<bool> MarcarSegundoPlanoAsync(
        int propostaId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await httpClient.PostAsync(
                $"api/propostas/{propostaId}/codificar-segundo-plano",
                null, cancellationToken);
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> ExcluirPropostaAsync(
        int propostaId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await httpClient.DeleteAsync(
                $"api/propostas/{propostaId}", cancellationToken);
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> VincularItemManualAsync(
        int propostaItemId,
        int itemId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await httpClient.PostAsync(
                $"api/propostas/{propostaItemId}/vincular-item-manual?itemId={itemId}",
                null, cancellationToken);
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }
}
