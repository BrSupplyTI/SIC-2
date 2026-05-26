namespace SIC.Api.Services;

public interface IPermissaoService
{
    /// <summary>
    /// Verifica se o usuário possui a permissão informada.
    /// Quando <paramref name="flagAdmin"/> for true, retorna sempre true (admin tem todas as permissões).
    /// </summary>
    Task<bool> TemPermissaoAsync(int usuarioId, int permissaoId, bool flagAdmin, CancellationToken cancellationToken = default);
}
