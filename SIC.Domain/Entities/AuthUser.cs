namespace SIC.Domain.Entities;

public sealed class AuthUser
{
    public int UsuarioId { get; set; }
    public string Login { get; set; } = string.Empty;
    public string Nome { get; set; } = string.Empty;
    public string? Email { get; set; }
    public bool FlagAdmin { get; set; }
    public int? EstabelecimentoId { get; set; }
    public string? NmEstabelecimento { get; set; }
    public string? ApelidoEstabelecimento { get; set; }
    public string? Foto { get; set; }
}
