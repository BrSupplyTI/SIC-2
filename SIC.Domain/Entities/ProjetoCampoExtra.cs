namespace SIC.Domain.Entities;

/// <summary>
/// Campo personalizado (nome/valor) definido pelo usuário em um projeto.
/// Cada projeto pode ter até 4 campos (Ordem 1..4).
/// </summary>
public sealed class ProjetoCampoExtra
{
    public int ProjetoCampoExtraID { get; set; }
    public int ProjetoID { get; set; }
    public byte Ordem { get; set; }
    public string NmCampo { get; set; } = string.Empty;
    public string? VlCampo { get; set; }
}
