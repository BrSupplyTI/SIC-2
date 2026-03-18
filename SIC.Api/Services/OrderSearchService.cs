using SIC.Api.Contracts.Pedidos;
using SIC.Domain.Abstractions;
using System.Reflection.PortableExecutable;

namespace SIC.Api.Services;

public sealed class OrderSearchService(IOrderSearchRepository repository) : IOrderSearchService
{
    public async Task<OrderSearchResultDto> SearchByOrderNumberAsync(string? numeroPedido, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(numeroPedido) || !int.TryParse(numeroPedido, out var numero))
        {
            return new OrderSearchResultDto
            {
                Success = false,
                ErrorCode = "INVALID_INPUT",
                Message = "ERRO: Digite um número de pedido válido !"
            };
        }

        var found = await repository.ExistsOrderByNumberAsync(numero, cancellationToken);
        return found
            ? new OrderSearchResultDto { Success = true, Message = "Pedido encontrado.", RedirectUrl = $"/Pedidos/Detalhes/{numero}" }
            : new OrderSearchResultDto { Success = false, ErrorCode = "NOT_FOUND", Message = "ERRO: O número de pedido digitado não existe !" };
    }

    public async Task<OrderHeaderDetailsDto?> GetOrderHeaderDetailsAsync(int pedido, CancellationToken cancellationToken = default)
    {
        if (pedido <= 0)
        {
            return null;
        }

        var data = await repository.GetOrderHeaderDetailsAsync(pedido, cancellationToken);
        if (data is null)
        {
            return null;
        }

        return new OrderHeaderDetailsDto
        {
            Pedido = data.Pedido,
            CompStatusCotacao = data.CompStatusCotacao,
            StatusAuxiliar = data.StatusAuxiliar,
            DataPedido = data.DataPedido?.ToString("dd/MM/yyyy HH:mm"),
            Estabelecimento = data.Estabelecimento,
            OrdemCompra = data.OrdemCompra,
            CanalVenda = data.CanalVenda,
            Carteira = data.Carteira,
            Situacao = data.Situacao,
            Setor = data.Setor,
            StatusID = data.StatusID,
            Categoria = data.Categoria,
            LabelInfoCategoria = data.LabelInfoCategoria,
            InfoCategoria = data.InfoCategoria,
            InfoCarrinho = data.InfoCarrinho,
            LabelInfoCarrinho = data.LabelInfoCarrinho,
            ClienteID = data.ClienteID,
            NomeCliente = data.NomeCliente,
            CodigoCliente = data.CodigoCliente,
            CNPJCliente = data.CNPJCliente,
            RazaoSocialEndereco = data.RazaoSocialEndereco,
            CpfCnpj = data.CpfCnpj,
            RuaEndereco = data.RuaEndereco,
            NumeroEndereco = data.NumeroEndereco,
            ComplementoEndereco = data.ComplementoEndereco,
            BairroEndereco = data.BairroEndereco,
            LogoCliente = data.LogoCliente,
            LogoClienteDark = data.LogoClienteDark,
            FlagTipoDocumento = data.FlagTipoDocumento,
            TelefoneCliente = data.TelefoneCliente,
            InscrEstCliente = data.InscrEstCliente,
            MotivoOVSAP = data.MotivoOVSAP,
            DescTipoOVSAP = data.DescTipoOVSAP,
            TipoOVSAP = data.TipoOVSAP,
            CotacaoIdOriginal = data.CotacaoIdOriginal,
            CotacaoIDSubstituta = data.CotacaoIDSubstituta,
            NrContrato = data.NrContrato,
            MargemBruta = data.MargemBruta,
            LB = data.LB,
            ROL = data.ROL,
            ClienteEnderecoID = data.ClienteEnderecoID,
            CodClienteEndereco = data.CodClienteEndereco,
            FlagTipoDocumentoEndereco = data.FlagTipoDocumentoEndereco,
            CidadeEndereco = data.CidadeEndereco,
            UFEndereco = data.UFEndereco,
            CidadeIBGEEndereco = data.CidadeIBGEEndereco,
            CepEndereco = data.CepEndereco,
            FlagEnderecoDirerente = data.FlagEnderecoDirerente,
            NmLocalEntrega = data.NmLocalEntrega,
            CdControle = data.CdControle,
            ClienteLocalEntregaID = data.ClienteLocalEntregaID,
            RuaLocal = data.RuaLocal,
            NumeroLocal = data.NumeroLocal,
            ComplementoLocal = data.ComplementoLocal,
            BairroLocal = data.BairroLocal,
            CidadeLocal = data.CidadeLocal,
            UFLocal = data.UFLocal,
            CidadeIBGELocal = data.CidadeIBGELocal,
            CEPLocal = data.CEPLocal,
            FormaPagto = data.FormaPagto,
            CondPagto = data.CondPagto,
            HashPagamento = data.HashPagamento,
            NmSolicitante = data.NmSolicitante,
            EmailSolicitante = data.EmailSolicitante,
            TransportadoraID = data.TransportadoraID,
            NmTransportadora = data.NmTransportadora,
            CNPJTransportadora = data.CNPJTransportadora,
            VlrFreteCalc = data.VlrFreteCalc,
            PrazoEntregaCalc = data.PrazoEntregaCalc,
            PrazoEntregaTransp = data.PrazoEntregaTransp,
            DtProgLiberacao = data.DtProgLiberacao?.ToString("dd/MM/yyyy"),
            DtProgEmbarque = data.DtProgEmbarque?.ToString("dd/MM/yyyy"),
            DtProgEntrega = data.DtProgEntrega?.ToString("dd/MM/yyyy"),
            DtPlanejadaOperacao = data.DtPlanejadaOperacao?.ToString("dd/MM/yyyy"),
            DtSLACliente = data.DtSLACliente?.ToString("dd/MM/yyyy"),
            DtProgEmbFollow = data.DtProgEmbFollow?.ToString("dd/MM/yyyy"),
            FreteAgrupado = data.FreteAgrupado,
            ObsCalcFrete = data.ObsCalcFrete,
            DtPrevEntFollow = data.DtPrevEntFollow?.ToString("dd/MM/yyyy"),
            DtPrevisaoEntrega = data.DtPrevisaoEntrega?.ToString("dd/MM/yyyy"),
            StatusSLA = data.StatusSLA,
            ObsCotacao = data.ObsCotacao,
            ObsAprovacao = data.ObsAprovacao,
            ObsNota = data.ObsNota,
            ObsLocalEntrega = data.ObsLocalEntrega,
            QtItensBRSupply = data.QtItensBRSupply,
            QtItensTerceiros = data.QtItensTerceiros,
            QtItensRuptura = data.QtItensRuptura,
            ValorItensBRSupply = data.ValorItensBRSupply,
            ValorItensTerceiros = data.ValorItensTerceiros,
            VlrFrete = data.VlrFrete,
            VlrTaxaServico = data.VlrTaxaServico,
            FlagIntegradoSAP = data.FlagIntegradoSAP
        };
    }

    public async Task<OrderSearchResultDto> SearchByPurchaseOrderAsync(string? ordemCompra, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(ordemCompra))
        {
            return new OrderSearchResultDto
            {
                Success = false,
                ErrorCode = "INVALID_INPUT",
                Message = "ERRO: Digite uma ordem de compra válida !"
            };
        }

        var search = await repository.SearchByPurchaseOrderAsync(ordemCompra.Trim(), cancellationToken);
        if (search.Total <= 0)
        {
            return new OrderSearchResultDto
            {
                Success = false,
                ErrorCode = "NOT_FOUND",
                Message = "ERRO: O número de OC digitado não existe !"
            };
        }

        if (search.Total > 100)
        {
            return new OrderSearchResultDto
            {
                Success = false,
                ErrorCode = "TOO_MANY_RESULTS",
                Message = "ERRO: Mais de 100 pedidos encontrados com esta OC! Utilize outro parâmetro de busca"
            };
        }

        if (search.Total == 1)
        {
            var pedido = search.Orders[0].PedidoId;
            return new OrderSearchResultDto
            {
                Success = true,
                Message = "Pedido encontrado.",
                TotalPedidos = 1,
                RedirectUrl = $"/Pedidos/Detalhes/{pedido}"
            };
        }

        return new OrderSearchResultDto
        {
            Success = true,
            Message = "Pedidos encontrados para esta ordem de compra.",
            TotalPedidos = search.Total,
            ShowModal = true,
            Pedidos = search.Orders.Select(x => new PurchaseOrderItemDto
            {
                PedidoId = x.PedidoId,
                ClienteNome = x.ClienteNome,
                DataPedido = x.DataPedido?.ToString("dd/MM/yyyy"),
                Situacao = x.Situacao,
                OrdemCompra = x.OrdemCompra,
                ValorTotalProdutos = x.ValorTotalProdutos,
                EstabelecimentoNome = x.EstabelecimentoNome,
                PedidoDetalheUrl = $"/Pedidos/Detalhes/{x.PedidoId}"
            }).ToList()
        };
    }

    public async Task<OrderSearchResultDto> SearchByInvoiceAsync(string? notaFiscal, int? serie, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(notaFiscal) || !serie.HasValue)
        {
            return new OrderSearchResultDto
            {
                Success = false,
                ErrorCode = "INVALID_INPUT",
                Message = "ERRO: Digite número e série da nota fiscal válidos !"
            };
        }

        var pedidoId = await repository.GetOrderIdByInvoiceAsync(notaFiscal.Trim(), serie.Value, cancellationToken);
        return pedidoId.HasValue
            ? new OrderSearchResultDto
            {
                Success = true,
                Message = "Pedido encontrado.",
                RedirectUrl = $"/Pedidos/Detalhes/{pedidoId.Value}"
            }
            : new OrderSearchResultDto
            {
                Success = false,
                ErrorCode = "NOT_FOUND",
                Message = "ERRO: A nota fiscal informada não existe !"
            };
    }
}
