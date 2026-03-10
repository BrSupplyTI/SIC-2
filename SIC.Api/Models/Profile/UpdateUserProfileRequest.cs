namespace SIC.Api.Models.Profile;

public sealed class UpdateUserProfileRequest
{
    public int UsuarioId { get; set; }
    public int? AreaId { get; set; }
    public string? Telefone { get; set; }
}
