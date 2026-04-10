namespace SIC.Api.Contracts.Home;

public sealed class ShortcutDto
{
    public int AtalhoID { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public int FlagExterna { get; set; }
    public string Icone { get; set; } = string.Empty;
}
