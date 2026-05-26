namespace SIC.Web.Models.Liberacao;

public sealed class LiberacaoPedidoCotLogViewModel
{
    public DateTime DtOperacao { get; set; }
    public int UsuarioID { get; set; }
    public string NmUsuario { get; set; } = string.Empty;
    public string TipoOperacao { get; set; } = string.Empty;
    public string Modificacao { get; set; } = string.Empty;
}

public sealed class LiberacaoPedidoBackOfficeLogViewModel
{
    public DateTime DataHora { get; set; }
    public int UsuarioID { get; set; }
    public string NmUsuario { get; set; } = string.Empty;
    public string DsAcao { get; set; } = string.Empty;
    public string Motivo { get; set; } = string.Empty;
}

public sealed class LiberacaoPedidoCotLogDetalhadoViewModel
{
    public DateTime DataHora { get; set; }
    public int UsuarioID { get; set; }
    public string NmUsuario { get; set; } = string.Empty;
    public int CotacaoItemID { get; set; }
    public string Operacao { get; set; } = string.Empty;
    public int? OldItemID { get; set; }
    public string OldCdItem { get; set; } = string.Empty;
    public string OldNmItem { get; set; } = string.Empty;
    public decimal? OldQtItem { get; set; }
    public decimal? OldVlrFinal { get; set; }
    public int? NewItemID { get; set; }
    public string NewCdItem { get; set; } = string.Empty;
    public string NewNmItem { get; set; } = string.Empty;
    public decimal? NewQtItem { get; set; }
    public decimal? NewVlrFinal { get; set; }
    public string Motivo { get; set; } = string.Empty;
}
