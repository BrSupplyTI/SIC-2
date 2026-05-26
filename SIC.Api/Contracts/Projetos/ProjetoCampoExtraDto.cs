namespace SIC.Api.Contracts.Projetos;

public sealed class ProjetoCampoExtraDto
{
    public byte Ordem { get; set; }
    public string NmCampo { get; set; } = string.Empty;
    public string? VlCampo { get; set; }
}
