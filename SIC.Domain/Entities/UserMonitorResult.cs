namespace SIC.Domain.Entities;

public sealed class UserMonitorResult
{
    public int UsuarioMonitorID { get; set; }
    public int MonitorID { get; set; }
    public string Nivel { get; set; } = string.Empty;
    public string Icone { get; set; } = string.Empty;
    public string Nome { get; set; } = string.Empty;
    public string Titulo { get; set; } = string.Empty;
    public string Resultado { get; set; } = string.Empty;
}
