using System.Net.Http.Json;
using SIC.Web.Models.Admin;

namespace SIC.Web.Services.Admin;

public sealed class AdminApiClient(HttpClient httpClient)
{
    // ── Mensagens Importantes ──────────────────────────────────────

    public async Task<List<AdminNoticeVm>> GetAllNoticesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            return await httpClient.GetFromJsonAsync<List<AdminNoticeVm>>("api/admin/mensagens", cancellationToken) ?? [];
        }
        catch
        {
            return [];
        }
    }

    public async Task<bool> ExpireNoticeAsync(int avisoId, CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await httpClient.PostAsync($"api/admin/mensagens/{avisoId}/expirar", null, cancellationToken);
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> DeleteNoticeAsync(int avisoId, CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await httpClient.DeleteAsync($"api/admin/mensagens/{avisoId}", cancellationToken);
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> CreateNoticeAsync(CreateNoticeVm model, CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await httpClient.PostAsJsonAsync("api/admin/mensagens", model, cancellationToken);
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }

    public async Task<List<IntranetAreaVm>> GetAreasAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            return await httpClient.GetFromJsonAsync<List<IntranetAreaVm>>("api/admin/mensagens/areas", cancellationToken) ?? [];
        }
        catch
        {
            return [];
        }
    }

    public async Task<List<AdminUserVm>> GetActiveUsersAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            return await httpClient.GetFromJsonAsync<List<AdminUserVm>>("api/admin/mensagens/usuarios", cancellationToken) ?? [];
        }
        catch
        {
            return [];
        }
    }
}
