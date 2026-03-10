namespace SIC.Web.Models.Auth;

public sealed class EstablishmentListRequestVm
{
    public int UsuarioId { get; set; }
    public bool IsAdmin { get; set; }
    public int? CurrentEstabelecimentoId { get; set; }
}
