namespace SIC.Api.Models.Profile;

public sealed class UpdateUserPhotoRequest
{
    public int UsuarioId { get; set; }
    public string Foto { get; set; } = string.Empty;
}
