namespace SIC.Web.Models.Home;

public sealed class MonitorVm
{
    public int MonitorID { get; set; }
    public string Nivel { get; set; } = string.Empty;
    public string Icone { get; set; } = string.Empty;
    public string Nome { get; set; } = string.Empty;
    public string Titulo { get; set; } = string.Empty;
    public string PromptValor { get; set; } = string.Empty;
}
