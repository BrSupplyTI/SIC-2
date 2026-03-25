namespace SIC.Web.Models.Pedidos;

public sealed class OrderSapIntegrationItemVm
{
    public string NrPedCli { get; set; } = string.Empty;
    public string OrdemVenda { get; set; } = string.Empty;
    public string MsgRetorno { get; set; } = string.Empty;
    public string DtHrEnvioSAP { get; set; } = string.Empty;
    public string RemessaSAP { get; set; } = string.Empty;
    public string FaturaSAP { get; set; } = string.Empty;
    public string NrNF { get; set; } = string.Empty;
    public string TipoOVSAP { get; set; } = string.Empty;
}
