using Microsoft.Extensions.Caching.Memory;
using SIC.Domain.Abstractions;

namespace SIC.Api.Services;

public sealed class PermissaoService(
    IPermissaoRepository repository,
    IMemoryCache cache) : IPermissaoService
{
    private static readonly TimeSpan CacheTtl = TimeSpan.FromMinutes(5);

    public async Task<bool> TemPermissaoAsync(int usuarioId, int permissaoId, bool flagAdmin, CancellationToken cancellationToken = default)
    {
        if (flagAdmin) return true;
        if (usuarioId <= 0 || permissaoId <= 0) return false;

        var key = $"perm:{usuarioId}:{permissaoId}";
        if (cache.TryGetValue<bool>(key, out var cached))
            return cached;

        var tem = await repository.TemPermissaoAsync(usuarioId, permissaoId, cancellationToken);
        cache.Set(key, tem, CacheTtl);
        return tem;
    }
}
