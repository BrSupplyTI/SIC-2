namespace SIC.Web.Models.Auth;

public sealed class UpdateUserPhotoRequestVm
{
    public int UsuarioId { get; set; }
    public string Foto { get; set; } = string.Empty;
}
