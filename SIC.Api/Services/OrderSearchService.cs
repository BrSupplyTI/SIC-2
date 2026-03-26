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
            FlagIntegradoSAP = data.FlagIntegradoSAP,
            QtNotasFiscais = data.QtNotasFiscais,
            QtRomaneios = data.QtRomaneios,
            QtChamados = data.QtChamados,
            QtAnaliseCredito = data.QtAnaliseCredito,
            QtAprovacoes = data.QtAprovacoes
        };
    }

    public async Task<IReadOnlyList<OrderSapIntegrationItemDto>> GetOrderSapIntegrationAsync(int pedido, CancellationToken cancellationToken = default)
    {
        if (pedido <= 0)
        {
            return [];
        }

        var items = await repository.GetOrderSapIntegrationAsync(pedido, cancellationToken);
        return items.Select(item => new OrderSapIntegrationItemDto
        {
            NrPedCli = item.NrPedCli,
            OrdemVenda = item.OrdemVenda,
            MsgRetorno = item.MsgRetorno,
            DtHrEnvioSAP = item.DtHrEnvioSAP,
            RemessaSAP = item.RemessaSAP,
            FaturaSAP = item.FaturaSAP,
            NrNF = item.NrNF,
            TipoOVSAP = item.TipoOVSAP
        }).ToList();
    }

    public async Task<IReadOnlyList<OrderTaxItemDto>> GetOrderTaxesAsync(int pedido, CancellationToken cancellationToken = default)
    {
        if (pedido <= 0)
        {
            return [];
        }

        var items = await repository.GetOrderTaxesAsync(pedido, cancellationToken);
        return items.Select(item => new OrderTaxItemDto
        {
            MVA = item.MVA,
            VlrTotalNF = item.VlrTotalNF,
            ItemDocumentoSAP = item.ItemDocumentoSAP,
            CdItem = item.CdItem,
            MKUP = item.MKUP,
            VlrUnitario = item.VlrUnitario,
            VlrCustoAquisicao = item.VlrCustoAquisicao,
            MargemEnviada = item.MargemEnviada,
            PercentualICMS = item.PercentualICMS,
            PercentualFCP = item.PercentualFCP,
            PercentualIPI = item.PercentualIPI,
            PercentualCOFINS = item.PercentualCOFINS,
            PercentualPIS = item.PercentualPIS,
            ValorICMS = item.ValorICMS,
            ValorIPI = item.ValorIPI,
            ValorST = item.ValorST,
            ValorISS = item.ValorISS,
            ValorISSRetido = item.ValorISSRetido,
            ValorCOFINS = item.ValorCOFINS,
            ValorPIS = item.ValorPIS,
            ValorFCPST = item.ValorFCPST,
            ValorICMSPartilhaOrigem = item.ValorICMSPartilhaOrigem,
            ValorICMSPartilhaDestino = item.ValorICMSPartilhaDestino,
            ValorFundoCombPobreza = item.ValorFundoCombPobreza,
            ValorPISRetido = item.ValorPISRetido,
            ValorCOFINSRetido = item.ValorCOFINSRetido,
            ValorCSLRetido = item.ValorCSLRetido,
            ValorIRRetido = item.ValorIRRetido,
            MargemCalculada = item.MargemCalculada,
            LB = item.LB,
            ROL = item.ROL
        }).ToList();
    }

    public async Task<IReadOnlyList<FreightCalculationItemDto>> GetFreightCalculationHistoryAsync(int pedido, CancellationToken cancellationToken = default)
    {
        if (pedido <= 0)
        {
            return [];
        }

        var items = await repository.GetFreightCalculationHistoryAsync(pedido, cancellationToken);
        return items.Select(item => new FreightCalculationItemDto
        {
            TransportadoraID = item.TransportadoraID,
            NomeTransportadora = item.NomeTransportadora,
            PrazoLogistico = item.PrazoLogistico,
            PrazoComercial = item.PrazoComercial,
            TaxaExtra = item.TaxaExtra,
            QtItensRestritos = item.QtItensRestritos,
            ClienteRestrito = item.FlagClienteRestrito != 0,
            ClienteFixo = item.FlagClienteFixo != 0,
            ObrigatoriaCanalVenda = item.FlagObrigatoriaCanalVenda != 0,
            ValorFrete = item.ValorFrete
        }).ToList();
    }

    public async Task<IReadOnlyList<FreightCalculationItemDto>> GetFreightCalculationAsync(int pedido, CancellationToken cancellationToken = default)
    {
        if (pedido <= 0)
        {
            return [];
        }

        var items = await repository.GetFreightCalculationAsync(pedido, cancellationToken);
        return items.Select(item => new FreightCalculationItemDto
        {
            TransportadoraID = item.TransportadoraID,
            NomeTransportadora = item.NomeTransportadora,
            PrazoLogistico = item.PrazoLogistico,
            PrazoComercial = item.PrazoComercial,
            TaxaExtra = item.TaxaExtra,
            QtItensRestritos = item.QtItensRestritos,
            ClienteRestrito = item.FlagClienteRestrito != 0,
            ClienteFixo = item.FlagClienteFixo != 0,
            ObrigatoriaCanalVenda = item.FlagObrigatoriaCanalVenda != 0,
            ValorFrete = item.ValorFrete
        }).ToList();
    }

    public async Task<IReadOnlyList<OrderBrSupplyItemDto>> GetOrderBrSupplyItemsAsync(int pedido, CancellationToken cancellationToken = default)
    {
        if (pedido <= 0)
        {
            return [];
        }

        var items = await repository.GetOrderBrSupplyItemsAsync(pedido, cancellationToken);
        return items.Select(item => new OrderBrSupplyItemDto
        {
            ClienteID = item.ClienteID,
            ItemID = item.ItemID,
            CdItem = item.CdItem,
            NmItem = item.NmItem,
            QtItem = item.QtItem,
            VlrFinal = item.VlrFinal,
            VlrTotal = item.VlrTotal,
            VlrOriginal = item.VlrOriginal,
            OrdemCliente = item.OrdemCliente,
            SituacaoItem = item.SituacaoItem,
            DtAlocacao = item.DtAlocacao?.ToString("dd/MM/yyyy HH:mm"),
            MargemCalculada = item.MargemCalculada,
            Versao = item.Versao
        }).ToList();
    }

    public async Task<IReadOnlyList<OrderBrSupplyItemDto>> GetOrderMarketplaceItemsAsync(int pedido, CancellationToken cancellationToken = default)
    {
        if (pedido <= 0)
        {
            return [];
        }

        var items = await repository.GetOrderMarketplaceItemsAsync(pedido, cancellationToken);
        return items.Select(item => new OrderBrSupplyItemDto
        {
            ClienteID = item.ClienteID,
            ItemID = item.ItemID,
            CdItem = item.CdItem,
            NmItem = item.NmItem,
            QtItem = item.QtItem,
            VlrFinal = item.VlrFinal,
            VlrTotal = item.VlrTotal,
            VlrOriginal = item.VlrOriginal,
            OrdemCliente = item.OrdemCliente,
            PathFoto = item.PathFoto,
            NmFornecedor = item.NmFornecedor
        }).ToList();
    }

    public async Task<IReadOnlyList<OrderBrSupplyItemDto>> GetOrderBrSupplyItemsRupturaAsync(int pedido, CancellationToken cancellationToken = default)
    {
        if (pedido <= 0)
        {
            return [];
        }

        var items = await repository.GetOrderBrSupplyItemsRupturaAsync(pedido, cancellationToken);
        return items.Select(item => new OrderBrSupplyItemDto
        {
            ClienteID = item.ClienteID,
            ItemID = item.ItemID,
            CdItem = item.CdItem,
            NmItem = item.NmItem,
            QtItem = item.QtItem,
            VlrFinal = item.VlrFinal,
            VlrTotal = item.VlrTotal,
            VlrOriginal = item.VlrOriginal,
            OrdemCliente = item.OrdemCliente,
            MensagemRuptura = item.MensagemRuptura,
            DtPrevEntrega = item.DtPrevEntrega?.ToString("dd/MM/yyyy"),
            QtDisponivel = item.QtDisponivel,
            QtItemPrevEntrega = item.QtItemPrevEntrega
        }).ToList();
    }

    public async Task<IReadOnlyList<OrderApprovalItemDto>> GetOrderApprovalItemsAsync(int pedido, CancellationToken cancellationToken = default)
    {
        if (pedido <= 0)
        {
            return [];
        }

        var items = await repository.GetOrderApprovalItemsAsync(pedido, cancellationToken);
        return items.Select(item => new OrderApprovalItemDto
        {
            NrSequencia = item.NrSequencia ?? 0,
            NmUsuario = item.NmUsuario,
            TipoAlcada = item.TipoAlcada,
            StatusAlcada = item.StatusAlcada,
            StatusAlcadaID = item.StatusAlcadaID ?? 0,
            DtAprovacao = item.DtAprovacao?.ToString("dd/MM/yyyy HH:mm")
        }).ToList();
    }

    public async Task<IReadOnlyList<OrderInvoiceItemDto>> GetOrderInvoiceItemsAsync(int pedido, CancellationToken cancellationToken = default)
    {
        if (pedido <= 0)
        {
            return [];
        }

        var items = await repository.GetOrderInvoiceItemsAsync(pedido, cancellationToken);
        return items.Select(item => new OrderInvoiceItemDto
        {
            NotaFiscalID = item.NotaFiscalID,
            NrNotaFiscal = item.NrNotaFiscal,
            Serie = item.Serie,
            Chave = item.Chave,
            Operacao = item.Operacao,
            EmitCNPJ = item.EmitCNPJ,
            DtEmissao = item.DtEmissao,
            Versao = item.Versao,
            QtdeVolumes = item.QtdeVolumes ?? 0,
            PesoBruto = item.PesoBruto ?? 0,
            VlrTotalNF = item.VlrTotalNF,
            StatusNF = item.StatusNF,
            MotivoCancelamento = item.MotivoCancelamento,
            DsStatusCancelamento = item.DsStatusCancelamento,
            CubagemNF = item.CubagemNF ?? "0",
            TipoAtestoID = item.TipoAtestoID ?? 0,
            DsAtestoRecebimento = item.DsAtestoRecebimento
        }).ToList();
    }

    public async Task<IReadOnlyList<OrderRomaneioItemDto>> GetOrderRomaneiosAsync(int pedido, CancellationToken cancellationToken = default)
    {
        if (pedido <= 0)
        {
            return [];
        }

        var items = await repository.GetOrderRomaneiosAsync(pedido, cancellationToken);
        return items.Select(item => new OrderRomaneioItemDto
        {
            RomaneioID = item.RomaneioID,
            NrNotaFiscal = item.NrNotaFiscal,
            Serie = item.Serie,
            NmTipoRomaneio = item.NmTipoRomaneio,
            CdEstabelecimento = item.CdEstabelecimento,
            NmCurto = item.NmCurto,
            Transportadora = item.Transportadora,
            DtPortaria = item.DtPortaria?.ToString("dd/MM/yyyy HH:mm"),
            NmRecebedor = item.NmRecebedor,
            DtEntrega = item.DtEntrega?.ToString("dd/MM/yyyy"),
            NmHub = item.NmHub,
            FlagTemComprovante = item.FlagTemComprovante,
            NmArquivoComprovante = item.NmArquivoComprovante,
            SituacaoRomaneio = item.SituacaoRomaneio
        }).ToList();
    }

    public async Task<IReadOnlyList<OrderTrackingItemDto>> GetOrderTrackingAsync(int pedido, CancellationToken cancellationToken = default)
    {
        if (pedido <= 0)
        {
            return [];
        }

        var items = await repository.GetOrderTrackingAsync(pedido, cancellationToken);
        return items.Select(item => new OrderTrackingItemDto
        {
            DtEvento = item.DtEvento?.ToString("dd/MM/yyyy HH:mm"),
            Evento = item.Evento,
            Detalhes = item.Detalhes,
            Usuario = item.Usuario
        }).ToList();
    }

    public async Task<IReadOnlyList<OrderVolumeColetaItemDto>> GetVolumesColetaAsync(string pedCli, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(pedCli))
        {
            return [];
        }

        var items = await repository.GetVolumesColetaAsync(pedCli, cancellationToken);
        return items.Select(item => new OrderVolumeColetaItemDto
        {
            CdItem = item.CdItem,
            NmItem = item.NmItem,
            QtSolicitada = item.QtSolicitada,
            QtColetada = item.QtColetada,
            Volume = item.Volume,
            NumVol = item.NumVol,
            DataColeta = item.DataColeta,
            NmOperador = item.NmOperador,
            EnderecoAtual = item.EnderecoAtual,
            ObsCarga = item.ObsCarga,
            DtLeituraRomaneio = item.DtLeituraRomaneio
        }).ToList();
    }

    public async Task<IReadOnlyList<OrderTicketItemDto>> GetOrderTicketsAsync(int pedido, CancellationToken cancellationToken = default)
    {
        if (pedido <= 0)
        {
            return [];
        }

        var items = await repository.GetOrderTicketsAsync(pedido, cancellationToken);
        return items.Select(item => new OrderTicketItemDto
        {
            Protocolo = item.Protocolo,
            Origem = item.Origem,
            OrigemValor = item.OrigemValor,
            NmSolicitante = item.NmSolicitante,
            EmailSolicitante = item.EmailSolicitante,
            NmArea = item.NmArea,
            NmNivel = item.NmNivel,
            NmProblema = item.NmProblema,
            Situacao = item.Situacao,
            Atraso = item.Atraso,
            DtHrAbertura = item.DtHrAbertura?.ToString("dd/MM/yyyy HH:mm"),
            DtHrEncerramento = item.DtHrEncerramento?.ToString("dd/MM/yyyy HH:mm"),
            PrazoResolucao = item.PrazoResolucao?.ToString("dd/MM/yyyy HH:mm")
        }).ToList();
    }

    public async Task<OrderCreditAnalysisDto?> GetOrderCreditAnalysisAsync(int pedido, CancellationToken cancellationToken = default)
    {
        if (pedido <= 0)
        {
            return null;
        }

        var data = await repository.GetOrderCreditAnalysisAsync(pedido, cancellationToken);
        if (data is null)
        {
            return null;
        }

        return new OrderCreditAnalysisDto
        {
            MotivoBloqueio = data.MotivoBloqueio,
            FlagAprovado = data.FlagAprovado,
            StatusAprovacao = data.StatusAprovacao,
            DataHoraBloqueio = data.DataHoraBloqueio?.ToString("dd/MM/yyyy HH:mm"),
            NmUsuario = data.NmUsuario,
            DataHoraAprovacao = data.DataHoraAprovacao?.ToString("dd/MM/yyyy HH:mm"),
            MotivoAprovacao = data.MotivoAprovacao
        };
    }

    public async Task<IReadOnlyList<OrderValidationItemDto>> GetOrderValidationsAsync(int pedido, CancellationToken cancellationToken = default)
    {
        if (pedido <= 0)
        {
            return [];
        }

        var items = await repository.GetOrderValidationsAsync(pedido, cancellationToken);

        return items.Select(item => new OrderValidationItemDto
        {
            Erro = item.Erro,
            Correcao = item.Correcao
        }).ToList();
    }

    public async Task<IReadOnlyList<OrderLogItemDto>> GetOrderLogsAsync(int pedido, CancellationToken cancellationToken = default)
    {
        if (pedido <= 0)
        {
            return [];
        }

        var items = await repository.GetOrderLogsAsync(pedido, cancellationToken);

        return items
            .OrderByDescending(i => i.DataHora)
            .Select(item => new OrderLogItemDto
            {
                Origem = item.Origem,
                DataHora = item.DataHora?.ToString("dd/MM/yyyy HH:mm"),
                Acao = item.Acao,
                Descricao = item.Descricao,
                NmUsuario = item.NmUsuario
            }).ToList();
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

    public Task<string?> GetInvoiceXmlAsync(string chaveDanfe, CancellationToken cancellationToken = default)
        => repository.GetInvoiceXmlAsync(chaveDanfe, cancellationToken);
}
