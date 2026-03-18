namespace SIC.Api.Models.Profile;

public sealed class UpdateUserProfileRequest
{
    public int UsuarioId { get; set; }
    public int? AreaId { get; set; }
    public string? Telefone { get; set; }
    public string? Ramal { get; set; }
    public int? Matricula { get; set; }
    public string? Cargo { get; set; }
    public string? Setor { get; set; }
    public int? DiaAniversario { get; set; }
    public int? MesAniversario { get; set; }
}
