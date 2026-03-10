namespace SIC.Api.Models.Auth;

public sealed class EstablishmentListRequest
{
    public int UsuarioId { get; set; }
    public bool IsAdmin { get; set; }
    public int? CurrentEstabelecimentoId { get; set; }
}
