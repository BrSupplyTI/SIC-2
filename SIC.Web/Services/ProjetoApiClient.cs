using System.Net.Http.Json;
using SIC.Web.Areas.Projetos.Models;

namespace SIC.Web.Services;

public sealed class ProjetoApiClient(HttpClient httpClient)
{
    // ── Lookups ──────────────────────────────────────────────

    public async Task<IReadOnlyList<ProjetoStatusItemVm>> GetStatusListAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var data = await httpClient.GetFromJsonAsync<List<ProjetoStatusItemVm>>("api/projetos/status", cancellationToken);
            return data ?? [];
        }
        catch
        {
            return [];
        }
    }

    public async Task<IReadOnlyList<ProjetoTarefaStatusItemVm>> GetTarefaStatusListAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var data = await httpClient.GetFromJsonAsync<List<ProjetoTarefaStatusItemVm>>("api/projetos/tarefa-status", cancellationToken);
            return data ?? [];
        }
        catch
        {
            return [];
        }
    }

    public async Task<IReadOnlyList<ProjetoTarefaPrioridadeItemVm>> GetPrioridadeListAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var data = await httpClient.GetFromJsonAsync<List<ProjetoTarefaPrioridadeItemVm>>("api/projetos/tarefa-prioridades", cancellationToken);
            return data ?? [];
        }
        catch
        {
            return [];
        }
    }

    public async Task<IReadOnlyList<UsuarioBuscaItemVm>> BuscarUsuariosAsync(string texto, CancellationToken cancellationToken = default)
    {
        try
        {
            var qs = $"api/projetos/usuarios?texto={Uri.EscapeDataString(texto)}";
            var data = await httpClient.GetFromJsonAsync<List<UsuarioBuscaItemVm>>(qs, cancellationToken);
            return data ?? [];
        }
        catch
        {
            return [];
        }
    }

    public async Task<bool> VerificarParticipanteAsync(int projetoId, int usuarioId, CancellationToken cancellationToken = default)
    {
        try
        {
            var qs = $"api/projetos/{projetoId}/verificar-participante?usuarioId={usuarioId}";
            var result = await httpClient.GetFromJsonAsync<VerificarParticipanteResult>(qs, cancellationToken);
            return result?.EhParticipante ?? false;
        }
        catch
        {
            return false;
        }
    }

    // ── Lista de Projetos ────────────────────────────────────

    public async Task<ProjetoListaViewModel> GetProjetosAsync(
        string? texto, int projetoStatusId, string orderBy,
        int pageNumber, int pageSize,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var qs = $"api/projetos?PageNumber={pageNumber}&PageSize={pageSize}"
                   + $"&ProjetoStatusID={projetoStatusId}";

            if (!string.IsNullOrWhiteSpace(texto))
                qs += $"&Texto={Uri.EscapeDataString(texto)}";
            if (!string.IsNullOrWhiteSpace(orderBy))
                qs += $"&OrderBy={Uri.EscapeDataString(orderBy)}";

            var result = await httpClient.GetFromJsonAsync<ProjetoListaViewModel>(qs, cancellationToken);

            if (result is null)
                return new ProjetoListaViewModel { Texto = texto, ProjetoStatusID = projetoStatusId, OrderBy = orderBy };

            result.Texto = texto;
            result.ProjetoStatusID = projetoStatusId;
            result.OrderBy = orderBy;

            return result;
        }
        catch
        {
            return new ProjetoListaViewModel { Texto = texto, ProjetoStatusID = projetoStatusId, OrderBy = orderBy };
        }
    }

    // ── Detalhes do Projeto ──────────────────────────────────

    public async Task<ProjetoListaComTarefasViewModel> GetProjetosComTarefasAsync(
        string? texto, int projetoStatusId, string orderBy,
        int pageNumber, int pageSize,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var qs = $"api/projetos/com-tarefas?PageNumber={pageNumber}&PageSize={pageSize}"
                   + $"&ProjetoStatusID={projetoStatusId}";

            if (!string.IsNullOrWhiteSpace(texto))
                qs += $"&Texto={Uri.EscapeDataString(texto)}";
            if (!string.IsNullOrWhiteSpace(orderBy))
                qs += $"&OrderBy={Uri.EscapeDataString(orderBy)}";

            var result = await httpClient.GetFromJsonAsync<ProjetoListaComTarefasViewModel>(qs, cancellationToken);

            if (result is null)
                return new ProjetoListaComTarefasViewModel { Texto = texto, ProjetoStatusID = projetoStatusId, OrderBy = orderBy };

            result.Texto = texto;
            result.ProjetoStatusID = projetoStatusId;
            result.OrderBy = orderBy;

            // Montar hierarquia de subtarefas para cada projeto
            foreach (var projeto in result.Itens)
            {
                projeto.Tarefas = MontarHierarquia(projeto.Tarefas.ToList());
            }

            return result;
        }
        catch
        {
            return new ProjetoListaComTarefasViewModel { Texto = texto, ProjetoStatusID = projetoStatusId, OrderBy = orderBy };
        }
    }

    public async Task<ProjetoDetalhesViewModel?> GetProjetoDetalhesAsync(int projetoId, CancellationToken cancellationToken = default)
    {
        try
        {
            var vm = await httpClient.GetFromJsonAsync<ProjetoDetalhesViewModel>($"api/projetos/{projetoId}", cancellationToken);
            if (vm is null) return null;

            var tarefasFlat = await httpClient.GetFromJsonAsync<List<ProjetoTarefaItemVm>>($"api/projetos/{projetoId}/tarefas", cancellationToken);
            vm.Tarefas = MontarHierarquia(tarefasFlat ?? []);

            var participantes = await httpClient.GetFromJsonAsync<List<ProjetoParticipanteItemVm>>($"api/projetos/{projetoId}/participantes", cancellationToken);
            vm.Participantes = participantes ?? [];

            var historico = await httpClient.GetFromJsonAsync<List<ProjetoHistoricoItemVm>>($"api/projetos/{projetoId}/historico", cancellationToken);
            vm.Historico = historico ?? [];

            return vm;
        }
        catch
        {
            return null;
        }
    }

    // ── Hierarquia de Tarefas ────────────────────────────────

    private static IReadOnlyList<ProjetoTarefaItemVm> MontarHierarquia(List<ProjetoTarefaItemVm> flat)
    {
        var lookup = flat.ToLookup(t => t.ProjetoTarefaPaiID);

        foreach (var tarefa in flat)
        {
            tarefa.SubTarefas = lookup[tarefa.ProjetoTarefaID].ToList();
        }

        return flat.Where(t => t.ProjetoTarefaPaiID is null).ToList();
    }

    // ── Escrita — Projeto ────────────────────────────────────

    public async Task<int> CriarProjetoAsync(object payload, CancellationToken cancellationToken = default)
    {
        var response = await httpClient.PostAsJsonAsync("api/projetos", payload, cancellationToken);
        await EnsureSuccessOrThrowAsync(response, cancellationToken);
        var result = await response.Content.ReadFromJsonAsync<IdResult>(cancellationToken: cancellationToken);
        return result?.ProjetoID ?? 0;
    }

    public async Task<int> AtualizarProjetoAsync(object payload, CancellationToken cancellationToken = default)
    {
        var response = await httpClient.PutAsJsonAsync("api/projetos", payload, cancellationToken);
        await EnsureSuccessOrThrowAsync(response, cancellationToken);
        var result = await response.Content.ReadFromJsonAsync<IdResult>(cancellationToken: cancellationToken);
        return result?.ProjetoID ?? 0;
    }

    // ── Escrita — Tarefa ─────────────────────────────────────

    public async Task<int> CriarTarefaAsync(object payload, CancellationToken cancellationToken = default)
    {
        var response = await httpClient.PostAsJsonAsync("api/projetos/tarefas", payload, cancellationToken);
        await EnsureSuccessOrThrowAsync(response, cancellationToken);
        var result = await response.Content.ReadFromJsonAsync<TarefaIdResult>(cancellationToken: cancellationToken);
        return result?.ProjetoTarefaID ?? 0;
    }

    public async Task<int> AtualizarTarefaAsync(object payload, CancellationToken cancellationToken = default)
    {
        var response = await httpClient.PutAsJsonAsync("api/projetos/tarefas", payload, cancellationToken);
        await EnsureSuccessOrThrowAsync(response, cancellationToken);
        var result = await response.Content.ReadFromJsonAsync<TarefaIdResult>(cancellationToken: cancellationToken);
        return result?.ProjetoTarefaID ?? 0;
    }

    public async Task<bool> ExcluirTarefaAsync(int projetoTarefaId, int usuarioId, CancellationToken cancellationToken = default)
    {
        var response = await httpClient.DeleteAsync($"api/projetos/tarefas/{projetoTarefaId}?usuarioId={usuarioId}", cancellationToken);
        await EnsureSuccessOrThrowAsync(response, cancellationToken);
        return true;
    }

    // ── Escrita — Participante ───────────────────────────────

    public async Task<int> AdicionarParticipanteAsync(object payload, CancellationToken cancellationToken = default)
    {
        var response = await httpClient.PostAsJsonAsync("api/projetos/participantes", payload, cancellationToken);
        await EnsureSuccessOrThrowAsync(response, cancellationToken);
        var result = await response.Content.ReadFromJsonAsync<ParticipanteIdResult>(cancellationToken: cancellationToken);
        return result?.ProjetoParticipanteID ?? 0;
    }

    public async Task<int> AtualizarPapelParticipanteAsync(object payload, CancellationToken cancellationToken = default)
    {
        var response = await httpClient.PutAsJsonAsync("api/projetos/participantes/papel", payload, cancellationToken);
        await EnsureSuccessOrThrowAsync(response, cancellationToken);
        var result = await response.Content.ReadFromJsonAsync<ParticipanteIdResult>(cancellationToken: cancellationToken);
        return result?.ProjetoParticipanteID ?? 0;
    }

    public async Task<bool> RemoverParticipanteAsync(int projetoParticipanteId, int usuarioLogadoId, CancellationToken cancellationToken = default)
    {
        var response = await httpClient.DeleteAsync($"api/projetos/participantes/{projetoParticipanteId}?usuarioLogadoId={usuarioLogadoId}", cancellationToken);
        await EnsureSuccessOrThrowAsync(response, cancellationToken);
        return true;
    }

    // ── Response helpers ─────────────────────────────────────

    private sealed class IdResult { public int ProjetoID { get; set; } }
    private sealed class TarefaIdResult { public int ProjetoTarefaID { get; set; } }
    private sealed class ParticipanteIdResult { public int ProjetoParticipanteID { get; set; } }
    private sealed class VerificarParticipanteResult { public bool EhParticipante { get; set; } }

    private static async Task EnsureSuccessOrThrowAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        if (response.IsSuccessStatusCode) return;

        var body = await response.Content.ReadAsStringAsync(cancellationToken);

        // Tenta extrair mensagem estruturada da API ({ "mensagem": "..." } ou ProblemDetails { "title": "..." })
        string mensagem;
        try
        {
            var doc = System.Text.Json.JsonDocument.Parse(body);
            mensagem = doc.RootElement.TryGetProperty("mensagem", out var m) ? m.GetString() ?? body
                     : doc.RootElement.TryGetProperty("title", out var t) ? t.GetString() ?? body
                     : body;
        }
        catch
        {
            mensagem = string.IsNullOrWhiteSpace(body) ? $"Erro HTTP {(int)response.StatusCode}" : body;
        }

        throw new HttpRequestException(mensagem, null, response.StatusCode);
    }
}
