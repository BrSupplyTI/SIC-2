namespace SIC.Domain.Entities.Cotacao;

/// <summary>
/// Item do histórico de envios de cotação (GetHistoricoEnviosAsync).
/// </summary>
public sealed class CotacaoEnvioHistoricoItem
{
    public int PropostaCotacaoEnvioID { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string DtEnvio { get; set; } = string.Empty;
    public string NmUsuario { get; set; } = string.Empty;
    public string DtVisualizacao { get; set; } = string.Empty;
    public string FlagVisualizaEstoque { get; set; } = string.Empty;
    public string FlagPodeNegociar { get; set; } = string.Empty;
    public string FlagPodeTrocarTransportadora { get; set; } = string.Empty;
    public string FlagPodeTrocarCondPagto { get; set; } = string.Empty;
    public int FlagAtivo { get; set; }
}
