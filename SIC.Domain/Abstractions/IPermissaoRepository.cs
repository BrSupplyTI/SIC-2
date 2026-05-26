namespace SIC.Domain.Abstractions;

/// <summary>
/// Verifica permissões do usuário consultando BrWeb..Intranet_PermissoesUsuario.
/// Administradores (FlagAdmin) devem ser tratados no serviço consumidor como tendo todas as permissões.
/// </summary>
public interface IPermissaoRepository
{
    /// <summary>
    /// Retorna true se o usuário possui a permissão informada.
    /// Esta chamada NÃO considera flag de administrador — apenas consulta a tabela.
    /// </summary>
    Task<bool> TemPermissaoAsync(int usuarioId, int permissaoId, CancellationToken cancellationToken = default);
}
