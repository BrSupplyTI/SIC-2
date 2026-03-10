namespace SIC.Api.Models.Auth;

public sealed class ChangeEstablishmentRequest
{
    public int UsuarioId { get; set; }
    public bool IsAdmin { get; set; }
    public int EstabelecimentoId { get; set; }
}
