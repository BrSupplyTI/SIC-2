namespace SIC.Web.Models.IntegracaoClientes.ProcessadoresPedidos;

public sealed class ProcessadoresPedidosIndexViewModel
{
    public int? ProcessadorPedidoIdSelecionado { get; set; }
    public IReadOnlyList<ProcessadorPedidoOptionVm> ProcessadoresPedido { get; init; } = [];
    public IReadOnlyList<ProcessadorPedidoConfiguracaoVm> Configuracoes { get; init; } = [];
}

public sealed class ProcessadorPedidoItemViewModel
{
    public required string Nome { get; init; }

    public required string Descricao { get; init; }

    public string Status { get; init; } = "Pendente";
}
