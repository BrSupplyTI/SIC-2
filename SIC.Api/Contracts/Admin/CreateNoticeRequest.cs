namespace SIC.Api.Contracts.Admin;

public sealed class CreateNoticeRequest
{
    public string Titulo { get; set; } = string.Empty;
    public string Descricao { get; set; } = string.Empty;
    public int Prioridade { get; set; }
    public DateTime DataHoraExpiracao { get; set; }
    public int? IntranetAreaID { get; set; }
    public int? UsuarioID { get; set; }
    public int UsuarioResponsavelID { get; set; }
}
