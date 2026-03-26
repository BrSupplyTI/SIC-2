using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SIC.Web.Models.Pedidos;
using SIC.Web.Services;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace SIC.Web.Controllers;

[Authorize]
[Route("Pedidos")]
public sealed class PedidosController(PedidoApiClient apiClient) : Controller
{
    [HttpGet("Busca")]
    public IActionResult Busca()
        => View(new PedidoBuscaViewModel());

    [HttpGet("Detalhes/{pedido:int}")]
    public async Task<IActionResult> Detalhes(int pedido, CancellationToken cancellationToken)
    {
        var headerData = await apiClient.GetOrderHeaderDetailsAsync(pedido, cancellationToken);

        var vm = new PedidoDetalhesViewModel
        {
            PedidoId = pedido,
            Header = new PedidoDetalhesViewModel.HeaderSection
            {
                NumeroPedido = headerData?.Pedido.ToString() ?? pedido.ToString(),
                Status = headerData?.Situacao ?? "Indefinido",
                StatusAuxiliar = headerData?.StatusAuxiliar ?? string.Empty,
                StatusID = headerData?.StatusID ?? 0,
                Setor = headerData?.Setor ?? "ERRO",
                DataCriacao = headerData?.DataPedido ?? DateTime.Now.AddDays(-2).ToString("dd/MM/yyyy HH:mm"),
                Origem = headerData?.CompStatusCotacao ?? "SIC",
                OrdemCompra = headerData?.OrdemCompra ?? string.Empty,
                CanalVenda = headerData?.CanalVenda ?? string.Empty,
                Carteira = headerData?.Carteira ?? string.Empty,
                Categoria = headerData?.Categoria ?? string.Empty,
                LabelInfoCategoria = headerData?.LabelInfoCategoria ?? string.Empty,
                InfoCategoria = headerData?.InfoCategoria ?? string.Empty,
                InfoCarrinho = headerData?.InfoCarrinho ?? string.Empty,
                LabelInfoCarrinho = headerData?.LabelInfoCarrinho ?? string.Empty,
                Estabelecimento = headerData?.Estabelecimento ?? string.Empty,
                MotivoOVSAP = headerData?.MotivoOVSAP ?? string.Empty,
                DescTipoOVSAP = headerData?.DescTipoOVSAP ?? string.Empty,
                TipoOVSAP = headerData?.TipoOVSAP ?? string.Empty,
                CotacaoIdOriginal = headerData?.CotacaoIdOriginal,
                CotacaoIDSubstituta = headerData?.CotacaoIDSubstituta,
                NrContrato = headerData?.NrContrato ?? string.Empty,
                MargemBruta = headerData?.MargemBruta ?? 0,
                LB = headerData?.LB ?? 0,
                ROL = headerData?.ROL ?? 0,
                NmSolicitante = headerData?.NmSolicitante ?? string.Empty,
                EmailSolicitante = headerData?.EmailSolicitante ?? string.Empty,
                FlagIntegradoSAP = headerData?.FlagIntegradoSAP ?? 0,
                QtNotasFiscais = headerData?.QtNotasFiscais ?? 0,
                QtRomaneios = headerData?.QtRomaneios ?? 0,
                QtChamados = headerData?.QtChamados ?? 0,
                QtAnaliseCredito = headerData?.QtAnaliseCredito ?? 0,
                QtAprovacoes = headerData?.QtAprovacoes ?? 0
            },
            Cliente = new PedidoDetalhesViewModel.ClienteSection
            {
                ClienteID = headerData?.ClienteID ?? 0,
                Nome = headerData?.NomeCliente ?? "",
                CNPJCliente = headerData?.CpfCnpj ?? "00.000.000/0001-00",
                CodigoExterno = headerData?.CodigoCliente ?? "",
                LogoCliente = headerData?.LogoCliente ?? string.Empty,
                LogoClienteDark = headerData?.LogoClienteDark ?? string.Empty,
                FlagTipoDocumento = headerData?.FlagTipoDocumento ?? string.Empty,
                TelefoneCliente = headerData?.TelefoneCliente ?? string.Empty,
                InscrEstCliente = headerData?.InscrEstCliente ?? string.Empty
            },
            Faturamento = new PedidoDetalhesViewModel.FaturamentoSection
            {
                ClienteEnderecoID = headerData?.ClienteEnderecoID ?? 0,
                CodClienteEndereco = headerData?.CodClienteEndereco ?? string.Empty,
                FlagTipoDocumentoEndereco = headerData?.FlagTipoDocumento ?? string.Empty,
                RazaoSocialEndereco = headerData?.RazaoSocialEndereco ?? string.Empty,
                CpfCnpj = headerData?.CpfCnpj ?? "00.000.000/0001-00",
                RuaEndereco = headerData?.RuaEndereco ?? string.Empty,
                NumeroEndereco = headerData?.NumeroEndereco ?? string.Empty,
                ComplementoEndereco = headerData?.ComplementoEndereco ?? string.Empty,
                BairroEndereco = headerData?.BairroEndereco ?? string.Empty,
                CidadeEndereco = headerData?.CidadeEndereco ?? string.Empty,
                UFEndereco = headerData?.UFEndereco ?? string.Empty,
                CidadeIBGEEndereco = headerData?.CidadeIBGEEndereco ?? string.Empty,
                CepEndereco = headerData?.CepEndereco ?? string.Empty,
                FlagEnderecoDirerente = headerData?.FlagEnderecoDirerente ?? 0,
                NmLocalEntrega = headerData?.NmLocalEntrega ?? string.Empty,
                CdControle = headerData?.CdControle ?? string.Empty,
                ClienteLocalEntregaID = headerData?.ClienteLocalEntregaID ?? 0,
                RuaLocal = headerData?.RuaLocal ?? string.Empty,
                NumeroLocal = headerData?.NumeroLocal ?? string.Empty,
                ComplementoLocal = headerData?.ComplementoLocal ?? string.Empty,
                BairroLocal = headerData?.BairroLocal ?? string.Empty,
                CidadeLocal = headerData?.CidadeLocal ?? string.Empty,
                UFLocal = headerData?.UFLocal ?? string.Empty,
                CidadeIBGELocal = headerData?.CidadeIBGELocal ?? string.Empty,
                CEPLocal = headerData?.CEPLocal ?? string.Empty,
                FormaPagto = headerData?.FormaPagto ?? string.Empty,
                CondPagto = headerData?.CondPagto ?? string.Empty,
                HashPagamento = headerData?.HashPagamento ?? string.Empty
            },
            Frete = new PedidoDetalhesViewModel.FreteSection
            {
                TransportadoraID = headerData?.TransportadoraID ?? 0,
                NmTransportadora = headerData?.NmTransportadora ?? "Sem transportadora definida",
                CNPJTransportadora = headerData?.CNPJTransportadora ?? string.Empty,
                VlrFreteCalc = headerData?.VlrFreteCalc ?? 0,
                PrazoEntregaCalc = headerData?.PrazoEntregaCalc ?? 0,
                PrazoEntregaTransp = headerData?.PrazoEntregaTransp ?? 0,
                DtProgLiberacao = headerData?.DtProgLiberacao ?? string.Empty,
                DtProgEmbarque = headerData?.DtProgEmbarque ?? string.Empty,
                DtProgEntrega = headerData?.DtProgEntrega ?? string.Empty,
                DtPlanejadaOperacao = headerData?.DtPlanejadaOperacao ?? string.Empty,
                DtSLACliente = headerData?.DtSLACliente ?? string.Empty,
                DtProgEmbFollow = headerData?.DtProgEmbFollow ?? string.Empty,
                FreteAgrupado = headerData?.FreteAgrupado ?? string.Empty,
                ObsCalcFrete = headerData?.ObsCalcFrete ?? string.Empty,
                DtPrevEntFollow = headerData?.DtPrevEntFollow ?? string.Empty,
                DtPrevisaoEntrega = headerData?.DtPrevisaoEntrega ?? string.Empty,
                StatusSLA = headerData?.StatusSLA ?? string.Empty
            },
            Observacao = new PedidoDetalhesViewModel.ObservacaoSection
            {
                ObsCotacao = headerData?.ObsCotacao ?? string.Empty,
                ObsAprovacao = headerData?.ObsAprovacao ?? string.Empty,
                ObsNota = headerData?.ObsNota ?? string.Empty,
                ObsLocalEntrega = headerData?.ObsLocalEntrega ?? string.Empty
            },
            Total = new PedidoDetalhesViewModel.TotalSection
            {
                QtItensBRSupply = headerData?.QtItensBRSupply ?? 0,
                QtItensTerceiros = headerData?.QtItensTerceiros ?? 0,
                QtItensRuptura = headerData?.QtItensRuptura ?? 0,
                ValorItensBRSupply = headerData?.ValorItensBRSupply ?? 0,
                ValorItensTerceiros = headerData?.ValorItensTerceiros ?? 0,
                VlrFrete = headerData?.VlrFrete ?? 0,
                VlrTaxaServico = headerData?.VlrTaxaServico ?? 0,
            },
           /* Itens =
            [
                new PedidoDetalhesViewModel.ItemSection { Codigo = "PROD-001", Descricao = "Produto exemplo 1", Quantidade = 2, ValorUnitario = "R$ 150,00" },
                new PedidoDetalhesViewModel.ItemSection { Codigo = "PROD-002", Descricao = "Produto exemplo 2", Quantidade = 1, ValorUnitario = "R$ 950,00" }
            ],*/
            LogsAprovacao = [],
            NotasFiscaisRelacionadas =
            [
                new PedidoDetalhesViewModel.NotaFiscalRelacionadaSection { Numero = "445566", Serie = "1", Emissao = DateTime.Now.AddDays(-1).ToString("dd/MM/yyyy") }
            ],
            Trackings =
            [
                new PedidoDetalhesViewModel.TrackingSection { DataHora = DateTime.Now.AddHours(-12).ToString("dd/MM/yyyy HH:mm"), Evento = "Separação concluída", Local = "CD São Paulo" },
                new PedidoDetalhesViewModel.TrackingSection { DataHora = DateTime.Now.AddHours(-2).ToString("dd/MM/yyyy HH:mm"), Evento = "Em transporte", Local = "São Paulo/SP" }
            ]
        };

        return View(vm);
    }

    [HttpGet("{pedido:int}/integracao-sap")]
    public async Task<IActionResult> IntegracaoSap(int pedido, CancellationToken cancellationToken)
    {
        var result = await apiClient.GetOrderSapIntegrationAsync(pedido, cancellationToken);
        return Json(result);
    }

    [HttpGet("{pedido:int}/impostos")]
    public async Task<IActionResult> Impostos(int pedido, CancellationToken cancellationToken)
    {
        var result = await apiClient.GetOrderTaxesAsync(pedido, cancellationToken);
        return Json(result);
    }

    [HttpGet("{pedido:int}/historico-frete")]
    public async Task<IActionResult> HistoricoFrete(int pedido, CancellationToken cancellationToken)
    {
        var result = await apiClient.GetFreightCalculationHistoryAsync(pedido, cancellationToken);
        return Json(result);
    }

    [HttpGet("{pedido:int}/calculo-frete")]
    public async Task<IActionResult> CalculoFrete(int pedido, CancellationToken cancellationToken)
    {
        var result = await apiClient.GetFreightCalculationAsync(pedido, cancellationToken);
        return Json(result);
    }

    [HttpGet("{pedido:int}/itens-br-supply")]
    public async Task<IActionResult> ItensBrSupply(int pedido, CancellationToken cancellationToken)
    {
        var result = await apiClient.GetOrderBrSupplyItemsAsync(pedido, cancellationToken);
        return Json(result);
    }

    [HttpGet("{pedido:int}/itens-marketplace")]
    public async Task<IActionResult> ItensMarketplace(int pedido, CancellationToken cancellationToken)
    {
        var result = await apiClient.GetOrderMarketplaceItemsAsync(pedido, cancellationToken);
        return Json(result);
    }

    [HttpGet("{pedido:int}/itens-br-supply-ruptura")]
    public async Task<IActionResult> ItensBrRuptura(int pedido, CancellationToken cancellationToken)
    {
        var result = await apiClient.GetOrderBrSupplyItemsRupturaAsync(pedido, cancellationToken);
        return Json(result);
    }

    [HttpGet("{pedido:int}/logs-aprovacao")]
    public async Task<IActionResult> LogsAprovacao(int pedido, CancellationToken cancellationToken)
    {
        var result = await apiClient.GetOrderApprovalItemsAsync(pedido, cancellationToken);
        return Json(result);
    }

    [HttpGet("{pedido:int}/notas-fiscais")]
    public async Task<IActionResult> NotasFiscais(int pedido, CancellationToken cancellationToken)
    {
        var result = await apiClient.GetOrderInvoiceItemsAsync(pedido, cancellationToken);
        return Json(result);
    }

    [HttpGet("{pedido:int}/romaneios")]
    public async Task<IActionResult> Romaneios(int pedido, CancellationToken cancellationToken)
    {
        var result = await apiClient.GetOrderRomaneiosAsync(pedido, cancellationToken);
        return Json(result);
    }

    [HttpGet("{pedido:int}/logs-tracking")]
    public async Task<IActionResult> LogsTracking(int pedido, CancellationToken cancellationToken)
    {
        var result = await apiClient.GetOrderTrackingAsync(pedido, cancellationToken);
        return Json(result);
    }

    [HttpGet("volumes-coleta")]
    public async Task<IActionResult> VolumesColeta([FromQuery] string pedCli, CancellationToken cancellationToken)
    {
        var result = await apiClient.GetVolumesColetaAsync(pedCli, cancellationToken);
        return Json(result);
    }

    public async Task<IActionResult> Chamados(int pedido, CancellationToken cancellationToken)
    {
        var result = await apiClient.GetOrderTicketsAsync(pedido, cancellationToken);
        return Json(result);
    }

    [HttpGet("{pedido:int}/analise-credito")]
    public async Task<IActionResult> AnaliseCredito(int pedido, CancellationToken cancellationToken)
    {
        var result = await apiClient.GetOrderCreditAnalysisAsync(pedido, cancellationToken);
        return result is null ? Json(null as object) : Json(result);
    }

    [HttpGet("{pedido:int}/validacoes")]
    public async Task<IActionResult> Validacoes(int pedido, CancellationToken cancellationToken)
    {
        var result = await apiClient.GetOrderValidationsAsync(pedido, cancellationToken);
        return Json(result);
    }

    [HttpGet("{pedido:int}/registros-logs")]
    public async Task<IActionResult> RegistrosLogs(int pedido, CancellationToken cancellationToken)
    {
        var result = await apiClient.GetOrderLogsAsync(pedido, cancellationToken);
        return Json(result);
    }

    [HttpGet("nf-xml/{chave}")]
    public async Task<IActionResult> DownloadInvoiceXml(string chave, CancellationToken cancellationToken)
    {
        var bytes = await apiClient.GetInvoiceXmlAsync(chave, cancellationToken);
        if (bytes is null) return NotFound();
        return File(bytes, "application/xml", $"{chave}.xml");
    }

    [HttpPost("BuscarPorPedido")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> BuscarPorPedido([FromForm] PedidoBuscaViewModel model, CancellationToken cancellationToken)
    {
        var result = await apiClient.SearchOrderByNumberAsync(model.InputPedido, cancellationToken);
        if (result?.Success == true)
        {
            if (!string.IsNullOrWhiteSpace(result.RedirectUrl))
            {
                return Redirect(result.RedirectUrl);
            }

            if (int.TryParse(model.InputPedido, out var pedidoId))
            {
                return RedirectToAction(nameof(Detalhes), new { pedido = pedidoId });
            }

            model.ErroPedido = "Não foi possível concluir a busca do pedido.";
            return View("Busca", model);
        }

        model.ErroPedido = result?.Message ?? "Não foi possível pesquisar o pedido.";
        return View("Busca", model);
    }

    [HttpPost("BuscarPorOrdemCompra")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> BuscarPorOrdemCompra([FromForm] PedidoBuscaViewModel model, CancellationToken cancellationToken)
    {
        var result = await apiClient.SearchOrderByPurchaseOrderAsync(model.InputOrdemCompra, cancellationToken);
        if (result?.Success == true)
        {
            if (!string.IsNullOrWhiteSpace(result.RedirectUrl))
            {
                return Redirect(result.RedirectUrl);
            }

            if (result.ShowModal && result.Pedidos.Count > 1)
            {
                model.ShowOcModal = true;
                model.TotalOcPedidos = result.TotalPedidos ?? result.Pedidos.Count;
                model.OcPedidos = result.Pedidos;
                return View("Busca", model);
            }

            model.ErroOrdemCompra = "Não foi possível concluir a busca da ordem de compra.";
            return View("Busca", model);
        }

        model.ErroOrdemCompra = result?.Message ?? "Não foi possível pesquisar a ordem de compra.";
        return View("Busca", model);
    }

    [HttpPost("BuscarPorNotaFiscal")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> BuscarPorNotaFiscal([FromForm] PedidoBuscaViewModel model, CancellationToken cancellationToken)
    {
        var result = await apiClient.SearchOrderByInvoiceAsync(model.InputNotaFiscal, model.InputSerieNF, cancellationToken);
        if (result?.Success == true)
        {
            if (!string.IsNullOrWhiteSpace(result.RedirectUrl))
            {
                return Redirect(result.RedirectUrl);
            }

            model.ErroNotaFiscal = "Não foi possível concluir a busca da nota fiscal.";
            return View("Busca", model);
        }

        model.ErroNotaFiscal = result?.Message ?? "Não foi possível pesquisar a nota fiscal.";
        return View("Busca", model);
    }
}
