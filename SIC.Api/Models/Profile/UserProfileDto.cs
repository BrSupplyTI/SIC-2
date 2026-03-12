namespace SIC.Api.Models.Profile;

public sealed class UserProfileDto
{
    public int UsuarioId { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Telefone { get; set; }
    public string? Ramal { get; set; }
    public int? Matricula { get; set; }
    public string? Cargo { get; set; }
    public string? Setor { get; set; }
    public int? AreaId { get; set; }
    public string? AreaNome { get; set; }
    public string? Foto { get; set; }
    public IReadOnlyList<UserPermissionDto> Permissoes { get; set; } = [];
}
