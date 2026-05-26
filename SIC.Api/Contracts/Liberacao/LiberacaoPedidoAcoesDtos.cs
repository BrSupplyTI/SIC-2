namespace SIC.Api.Contracts.Liberacao;

/// <summary>Resultado genérico de uma operação de escrita.</summary>
public sealed class LiberacaoPedidoAcaoResultadoDto
{
    public bool Sucesso { get; set; }
    public string Mensagem { get; set; } = string.Empty;

    /// <summary>Para GerarPedidoRupturas: ID do novo pedido gerado.</summary>
    public int? NovoCotacaoId { get; set; }
}

// ---------- Requests ----------

public sealed class AlterarObservacaoRequest
{
    public int CotacaoID { get; set; }
    public int UsuarioID { get; set; }
    public string ObsAntiga { get; set; } = string.Empty;
    public string ObsNova { get; set; } = string.Empty;
    public string Motivo { get; set; } = string.Empty;
}

public sealed class AlterarOrdemCompraRequest
{
    public int CotacaoID { get; set; }
    public int UsuarioID { get; set; }
    public string OrdemAntiga { get; set; } = string.Empty;
    public string OrdemNova { get; set; } = string.Empty;
    public string Motivo { get; set; } = string.Empty;
}

public sealed class AlterarCanalVendaRequest
{
    public int CotacaoID { get; set; }
    public int UsuarioID { get; set; }
    public string NmCanalAntigo { get; set; } = string.Empty;
    public int CanalVendaIDNovo { get; set; }
    public string Motivo { get; set; } = string.Empty;
}

public sealed class AlterarCategoriaRequest
{
    public int CotacaoID { get; set; }
    public int UsuarioID { get; set; }
    public string NmCategoriaAntiga { get; set; } = string.Empty;
    public int CategoriaIDNova { get; set; }
    public string Motivo { get; set; } = string.Empty;
}

public sealed class AlterarCondPagtoRequest
{
    public int CotacaoID { get; set; }
    public int UsuarioID { get; set; }
    public string NmCondPagtoAntiga { get; set; } = string.Empty;
    public int CondPagtoIDNova { get; set; }
    public string Motivo { get; set; } = string.Empty;
}

public sealed class CobrarFreteRequest
{
    public int CotacaoID { get; set; }
    public int UsuarioID { get; set; }
    public decimal VlrFrete { get; set; }
    public int FlagFreteServico { get; set; }
}

public sealed class CancelarPedidoRequest
{
    public int CotacaoID { get; set; }
    public int UsuarioID { get; set; }
    public string Motivo { get; set; } = string.Empty;
}

public sealed class DesbloquearAlocacoesRequest
{
    public int CotacaoID { get; set; }
    public int UsuarioID { get; set; }
    public string Motivo { get; set; } = string.Empty;
}

public sealed class GerarPedidoRupturasRequest
{
    public int CotacaoID { get; set; }
    public int UsuarioID { get; set; }
    public int ClienteID { get; set; }
    public int ClienteUsuarioID { get; set; }
    public string Motivo { get; set; } = string.Empty;
}

public sealed class LiberarMarketplaceRequest
{
    public int CotacaoID { get; set; }
    public int UsuarioID { get; set; }
}
