namespace SIC.Web.Models.Auth;

public sealed class ChangeEstablishmentRequestVm
{
    public int UsuarioId { get; set; }
    public bool IsAdmin { get; set; }
    public int EstabelecimentoId { get; set; }
}
