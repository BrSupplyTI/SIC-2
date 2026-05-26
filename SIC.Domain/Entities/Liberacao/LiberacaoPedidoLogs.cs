namespace SIC.Domain.Entities.Liberacao;

/// <summary>Entrada em BrSupply.dbo.BR_CotLog (log principal do pedido).</summary>
public sealed class LiberacaoPedidoCotLog
{
    public DateTime DtOperacao { get; set; }
    public int UsuarioID { get; set; }
    public string NmUsuario { get; set; } = string.Empty;
    /// <summary>A=Aprovação/Alteração, R=Alteração de dados, E=Exclusão.</summary>
    public string TipoOperacao { get; set; } = string.Empty;
    public string Modificacao { get; set; } = string.Empty;
}

/// <summary>Entrada em Integracao_Clientes.dbo.BR_BackOfficeLog (tentativas/ações do backoffice).</summary>
public sealed class LiberacaoPedidoBackOfficeLog
{
    public DateTime DataHora { get; set; }
    public int UsuarioID { get; set; }
    public string NmUsuario { get; set; } = string.Empty;
    public string DsAcao { get; set; } = string.Empty;
    public string Motivo { get; set; } = string.Empty;
}

/// <summary>Entrada em Integracao_Clientes.dbo.BR_CotLogDetalhado (operações item a item).</summary>
public sealed class LiberacaoPedidoCotLogDetalhado
{
    public DateTime DataHora { get; set; }
    public int UsuarioID { get; set; }
    public string NmUsuario { get; set; } = string.Empty;
    public int CotacaoItemID { get; set; }
    /// <summary>A=Alterar, E=Excluir, T=Trocar.</summary>
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
