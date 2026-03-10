namespace SIC.Api.Models.Auth;

public sealed class UpdateUserPhotoRequest
{
    public int UsuarioId { get; set; }
    public string Foto { get; set; } = string.Empty;
}
