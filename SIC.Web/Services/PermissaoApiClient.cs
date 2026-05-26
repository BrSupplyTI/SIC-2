using System.Net.Http.Json;

namespace SIC.Web.Services;

/// <summary>
/// Cliente HTTP genérico para checagem de permissões de usuário (BrWeb..Intranet_PermissoesUsuario).
/// Administradores (FlagAdmin) são tratados no server-side como tendo todas as permissões.
/// </summary>
public sealed class PermissaoApiClient(HttpClient httpClient)
{
    public async Task<bool> TemPermissaoAsync(int usuarioId, int permissaoId, bool flagAdmin, CancellationToken cancellationToken = default)
    {
        if (flagAdmin) return true;
        if (usuarioId <= 0 || permissaoId <= 0) return false;

        try
        {
            var result = await httpClient.GetFromJsonAsync<PermissaoResultado>(
                $"api/permissoes/{usuarioId}/{permissaoId}?flagAdmin=false",
                cancellationToken);
            return result?.TemPermissao ?? false;
        }
        catch
        {
            return false;
        }
    }

    private sealed class PermissaoResultado
    {
        public bool TemPermissao { get; set; }
    }
}
