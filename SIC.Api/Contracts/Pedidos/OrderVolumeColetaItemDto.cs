namespace SIC.Api.Contracts.Pedidos;

public sealed class OrderVolumeColetaItemDto
{
    public string CdItem { get; set; } = string.Empty;
    public string NmItem { get; set; } = string.Empty;
    public int QtSolicitada { get; set; }
    public int QtColetada { get; set; }
    public string Volume { get; set; } = string.Empty;
    public int NumVol { get; set; }
    public string DataColeta { get; set; } = string.Empty;
    public string NmOperador { get; set; } = string.Empty;
    public string EnderecoAtual { get; set; } = string.Empty;
    public string ObsCarga { get; set; } = string.Empty;
    public string DtLeituraRomaneio { get; set; } = string.Empty;
}
