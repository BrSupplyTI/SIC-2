namespace SIC.Api.Contracts.Pedidos;

public sealed class OrderRomaneioItemDto
{
    public int RomaneioID { get; set; }
    public string NrNotaFiscal { get; set; } = string.Empty;
    public string Serie { get; set; } = string.Empty;
    public string NmTipoRomaneio { get; set; } = string.Empty;
    public string CdEstabelecimento { get; set; } = string.Empty;
    public string NmCurto { get; set; } = string.Empty;
    public string Transportadora { get; set; } = string.Empty;
    public string? DtPortaria { get; set; }
    public string NmRecebedor { get; set; } = string.Empty;
    public string? DtEntrega { get; set; }
    public string NmHub { get; set; } = string.Empty;
    public int FlagTemComprovante { get; set; }
    public string NmArquivoComprovante { get; set; } = string.Empty;
    public string SituacaoRomaneio { get; set; } = string.Empty;
}
