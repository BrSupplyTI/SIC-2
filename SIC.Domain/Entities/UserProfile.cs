namespace SIC.Domain.Entities;

public sealed class UserProfile
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
    public int? DiaAniversario { get; set; }
    public int? MesAniversario { get; set; }
    public bool FlagAdmin { get; set; }
    public bool FlagBackOffice { get; set; }
    public bool FlagAlteraEstabelecimento { get; set; }
}
