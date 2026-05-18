namespace SIC.Web.Models.Cotacao;

/// <summary>
/// Representa uma linha do histórico de envios da cotação.
/// Fonte: BRWeb..Proposta_CotacaoEnvio JOIN BrSupply..BR_Usuario
/// </summary>
public sealed class CotacaoEnvioHistoricoItemViewModel
{
    public int PropostaCotacaoEnvioID { get; set; }

    /// <summary>E.Nome — nome do destinatário.</summary>
    public string Nome { get; set; } = string.Empty;

    /// <summary>E.Email — e-mail do destinatário.</summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>Data/hora do envio formatada (dd/MM/yyyy HH:mm).</summary>
    public string DtEnvio { get; set; } = string.Empty;

    /// <summary>U.NmUsuario — usuário que realizou o envio.</summary>
    public string NmUsuario { get; set; } = string.Empty;

    /// <summary>Data/hora da visualização formatada (dd/MM/yyyy HH:mm).</summary>
    public string DtVisualizacao { get; set; } = string.Empty;

    /// <summary>"S" ou "N".</summary>
    public string FlagVisualizaEstoque { get; set; } = string.Empty;

    /// <summary>"S" ou "N".</summary>
    public string FlagPodeNegociar { get; set; } = string.Empty;

    /// <summary>"S" ou "N".</summary>
    public string FlagPodeTrocarTransportadora { get; set; } = string.Empty;

    /// <summary>"S" ou "N".</summary>
    public string FlagPodeTrocarCondPagto { get; set; } = string.Empty;

    public int FlagAtivo { get; set; }
}
