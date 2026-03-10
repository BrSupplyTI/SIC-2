namespace SIC.Web.Models.Profile;

public sealed class UserProfileVm
{
    public int UsuarioId { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Telefone { get; set; }
    public int? AreaId { get; set; }
    public string? AreaNome { get; set; }
    public string? Foto { get; set; }
    public IReadOnlyList<UserPermissionVm> Permissoes { get; set; } = [];
}
