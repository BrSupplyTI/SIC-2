using SIC.Api.Contracts.Liberacao;
using SIC.Domain.Abstractions;

namespace SIC.Api.Services;

public sealed class LiberacaoPedidoAcoesService(
    ILiberacaoPedidoQueryRepository queryRepo,
    ILiberacaoPedidoCommandRepository commandRepo,
    ILiberacaoPedidoItemCommandRepository itemCommandRepo,
    ILogger<LiberacaoPedidoAcoesService> logger) : ILiberacaoPedidoAcoesService
{
    // ---------- QUERIES ----------

    public async Task<IReadOnlyList<LiberacaoPedidoComboItemDto>> ListarCanaisVendaAsync(int usuarioId, string nmCanalAtual, CancellationToken cancellationToken = default)
    {
        var items = await queryRepo.ListarCanaisVendaAsync(usuarioId, nmCanalAtual ?? string.Empty, cancellationToken);
        return items.Select(i => new LiberacaoPedidoComboItemDto { Id = i.Id, Nome = i.Nome }).ToList();
    }

    public async Task<IReadOnlyList<LiberacaoPedidoComboItemDto>> ListarCategoriasAsync(int clienteId, CancellationToken cancellationToken = default)
    {
        var items = await queryRepo.ListarCategoriasAsync(clienteId, cancellationToken);
        return items.Select(i => new LiberacaoPedidoComboItemDto { Id = i.Id, Nome = i.Nome }).ToList();
    }

    public async Task<IReadOnlyList<LiberacaoPedidoComboItemDto>> ListarCondicoesPagamentoAsync(string nmCondPagtoAtual, CancellationToken cancellationToken = default)
    {
        var items = await queryRepo.ListarCondicoesPagamentoAsync(nmCondPagtoAtual ?? string.Empty, cancellationToken);
        return items.Select(i => new LiberacaoPedidoComboItemDto { Id = i.Id, Nome = i.Nome }).ToList();
    }

    public async Task<IReadOnlyList<LiberacaoPedidoFreteOpcaoDto>> ListarOpcoesFreteAsync(int cotacaoId, CancellationToken cancellationToken = default)
    {
        var items = await queryRepo.ListarOpcoesFreteAsync(cotacaoId, cancellationToken);
        return items.Select(f => new LiberacaoPedidoFreteOpcaoDto
        {
            NomeTransportadora = f.NomeTransportadora,
            ValorFrete = f.ValorFrete,
            PrazoLogistico = f.PrazoLogistico,
            PrazoComercial = f.PrazoComercial,
            TaxaExtra = f.TaxaExtra,
            QtItensRestritos = f.QtItensRestritos,
            FlagClienteFixo = f.FlagClienteFixo,
            FlagObrigatoriaCanalVenda = f.FlagObrigatoriaCanalVenda,
            FlagClienteRestrito = f.FlagClienteRestrito
        }).ToList();
    }

    public async Task<IReadOnlyList<LiberacaoPedidoImpostoItemDto>> ListarImpostosAsync(int cotacaoId, CancellationToken cancellationToken = default)
    {
        var items = await queryRepo.ListarImpostosAsync(cotacaoId, cancellationToken);
        return items.Select(i => new LiberacaoPedidoImpostoItemDto
        {
            ItemDocumentoSAP = i.ItemDocumentoSAP,
            CdItem = i.CdItem,
            NmItemAbrev = i.NmItemAbrev,
            QtItem = i.QtItem,
            VlrUnitario = i.VlrUnitario,
            MKUP = i.MKUP,
            MargemCalculada = i.MargemCalculada,
            PercentualICMS = i.PercentualICMS,
            ValorICMS = i.ValorICMS,
            PercentualIPI = i.PercentualIPI,
            ValorIPI = i.ValorIPI,
            PercentualPIS = i.PercentualPIS,
            ValorPIS = i.ValorPIS,
            PercentualCOFINS = i.PercentualCOFINS,
            ValorCOFINS = i.ValorCOFINS,
            PercentualFCP = i.PercentualFCP,
            ValorFundoCombPobreza = i.ValorFundoCombPobreza,
            ValorST = i.ValorST,
            ValorISS = i.ValorISS,
            ValorICMSPartilhaOrigem = i.ValorICMSPartilhaOrigem,
            ValorICMSPartilhaDestino = i.ValorICMSPartilhaDestino,
            LB = i.LB,
            ROL = i.ROL
        }).ToList();
    }

    // ---------- LOGS (Fase 4) ----------

    public async Task<IReadOnlyList<LiberacaoPedidoCotLogDto>> ListarCotLogAsync(int cotacaoId, CancellationToken ct = default)
    {
        var items = await queryRepo.ListarCotLogAsync(cotacaoId, ct);
        return items.Select(x => new LiberacaoPedidoCotLogDto
        {
            DtOperacao = x.DtOperacao,
            UsuarioID = x.UsuarioID,
            NmUsuario = x.NmUsuario,
            TipoOperacao = x.TipoOperacao,
            Modificacao = x.Modificacao
        }).ToList();
    }

    public async Task<IReadOnlyList<LiberacaoPedidoBackOfficeLogDto>> ListarBackOfficeLogAsync(int cotacaoId, CancellationToken ct = default)
    {
        var items = await queryRepo.ListarBackOfficeLogAsync(cotacaoId, ct);
        return items.Select(x => new LiberacaoPedidoBackOfficeLogDto
        {
            DataHora = x.DataHora,
            UsuarioID = x.UsuarioID,
            NmUsuario = x.NmUsuario,
            DsAcao = x.DsAcao,
            Motivo = x.Motivo
        }).ToList();
    }

    public async Task<IReadOnlyList<LiberacaoPedidoCotLogDetalhadoDto>> ListarCotLogDetalhadoAsync(int cotacaoId, CancellationToken ct = default)
    {
        var items = await queryRepo.ListarCotLogDetalhadoAsync(cotacaoId, ct);
        return items.Select(x => new LiberacaoPedidoCotLogDetalhadoDto
        {
            DataHora = x.DataHora,
            UsuarioID = x.UsuarioID,
            NmUsuario = x.NmUsuario,
            CotacaoItemID = x.CotacaoItemID,
            Operacao = x.Operacao,
            OldItemID = x.OldItemID,
            OldCdItem = x.OldCdItem,
            OldNmItem = x.OldNmItem,
            OldQtItem = x.OldQtItem,
            OldVlrFinal = x.OldVlrFinal,
            NewItemID = x.NewItemID,
            NewCdItem = x.NewCdItem,
            NewNmItem = x.NewNmItem,
            NewQtItem = x.NewQtItem,
            NewVlrFinal = x.NewVlrFinal,
            Motivo = x.Motivo
        }).ToList();
    }

    // ---------- ITENS (Fase 5) ----------

    public async Task<IReadOnlyList<LiberacaoPedidoItemBrSupplyDto>> ListarItensBrSupplyAsync(int cotacaoId, CancellationToken ct = default)
    {
        var items = await queryRepo.ListarItensBrSupplyAsync(cotacaoId, ct);
        return items.Select(x => new LiberacaoPedidoItemBrSupplyDto
        {
            CotacaoItemID = x.CotacaoItemID,
            ItemID = x.ItemID,
            CdItem = x.CdItem,
            NmItem = x.NmItem,
            QtItem = x.QtItem,
            VlrUnit = x.VlrUnit,
            VlrCusto = x.VlrCusto,
            VlrTotal = x.VlrTotal,
            OrdemCliente = x.OrdemCliente,
            Ordem = x.Ordem,
            Sequencia = x.Sequencia,
            OrdemVenda = x.OrdemVenda,
            SituacaoItem = x.SituacaoItem,
            OrderBy = x.OrderBy,
            FlagAlocaPedido = x.FlagAlocaPedido,
            DsFollowCompras = x.DsFollowCompras,
            NaturezaOperacaoID = x.NaturezaOperacaoID,
            Margem = x.Margem,
            MargemCalculada = x.MargemCalculada,
            Previsao = x.Previsao,
            FlagRuptura = x.FlagRuptura,
            QtDisponivel = x.QtDisponivel
        }).ToList();
    }

    public async Task<IReadOnlyList<LiberacaoPedidoItemMarketplaceDto>> ListarItensMarketplaceAsync(int cotacaoId, CancellationToken ct = default)
    {
        var items = await queryRepo.ListarItensMarketplaceAsync(cotacaoId, ct);
        return items.Select(x => new LiberacaoPedidoItemMarketplaceDto
        {
            CotacaoItemID = x.CotacaoItemID,
            CdItem = x.CdItem,
            NmItem = x.NmItem,
            NmFornecedor = x.NmFornecedor,
            QtItem = x.QtItem,
            VlrUnit = x.VlrUnit,
            VlrTotal = x.VlrTotal
        }).ToList();
    }

    public async Task<LiberacaoPedidoTrocaCompativeisResultadoDto> BuscarCompativeisTrocaAsync(int cotacaoItemId, CancellationToken ct = default)
    {
        var r = await queryRepo.BuscarCompativeisTrocaAsync(cotacaoItemId, ct);
        return new LiberacaoPedidoTrocaCompativeisResultadoDto
        {
            FlagTrocaItemAutomatica = r.FlagTrocaItemAutomatica,
            MensagemAnalise = r.MensagemAnalise,
            Itens = r.Itens.Select(x => new LiberacaoPedidoItemCompativelDto
            {
                ItemID = x.ItemID,
                CdItem = x.CdItem,
                NmItem = x.NmItem,
                VlrCusto = x.VlrCusto,
                NCM = x.NCM,
                QtEstoqueDisponivel = x.QtEstoqueDisponivel,
                ChaveTributacao = x.ChaveTributacao,
                VlrTabelaPreco = x.VlrTabelaPreco
            }).ToList()
        };
    }

    public async Task<LiberacaoPedidoAcaoResultadoDto> AlterarItemAsync(AlterarItemRequest req, CancellationToken ct = default)
    {
        try
        {
            var erro = await itemCommandRepo.AlterarItemAsync(
                req.CotacaoID, req.CotacaoItemID, req.ItemIDOld, req.CdItemOld, req.NmItemOld,
                req.Quantidade, req.QuantidadeOld, req.Valor, req.ValorOld,
                req.OrdemItem, req.OrdemItemOld, req.Sequencia, req.SequenciaOld,
                req.Motivo, req.UsuarioID, ct);
            if (erro is not null)
                return new LiberacaoPedidoAcaoResultadoDto { Sucesso = false, Mensagem = erro };
            return new LiberacaoPedidoAcaoResultadoDto { Sucesso = true, Mensagem = "Item alterado com sucesso." };
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erro ao alterar item {CotacaoItemID}", req.CotacaoItemID);
            return new LiberacaoPedidoAcaoResultadoDto { Sucesso = false, Mensagem = "Erro ao alterar o item: " + ex.Message };
        }
    }

    public async Task<LiberacaoPedidoAcaoResultadoDto> AlterarItemComOvAsync(AlterarItemComOvRequest req, CancellationToken ct = default)
    {
        try
        {
            var erro = await itemCommandRepo.AlterarItemComOvAsync(
                req.CotacaoID, req.CotacaoItemID, req.CdItemOld, req.NmItemOld,
                req.OrdemItem, req.OrdemItemOld, req.Sequencia, req.SequenciaOld,
                req.Motivo, req.UsuarioID, ct);
            if (erro is not null)
                return new LiberacaoPedidoAcaoResultadoDto { Sucesso = false, Mensagem = erro };
            return new LiberacaoPedidoAcaoResultadoDto { Sucesso = true, Mensagem = "Item alterado com sucesso." };
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erro ao alterar item com OV {CotacaoItemID}", req.CotacaoItemID);
            return new LiberacaoPedidoAcaoResultadoDto { Sucesso = false, Mensagem = "Erro ao alterar o item: " + ex.Message };
        }
    }

    public async Task<LiberacaoPedidoAcaoResultadoDto> ExcluirItemAsync(ExcluirItemRequest req, CancellationToken ct = default)
    {
        try
        {
            var erro = await itemCommandRepo.ExcluirItemAsync(
                req.CotacaoID, req.CotacaoItemID, req.ItemIDOld, req.CdItemOld, req.NmItemOld,
                req.QuantidadeOld, req.ValorOld,
                req.Motivo, req.UsuarioID, req.EstabelecimentoID, ct);
            if (erro is not null)
                return new LiberacaoPedidoAcaoResultadoDto { Sucesso = false, Mensagem = erro };
            return new LiberacaoPedidoAcaoResultadoDto { Sucesso = true, Mensagem = "Item excluído com sucesso." };
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erro ao excluir item {CotacaoItemID}", req.CotacaoItemID);
            return new LiberacaoPedidoAcaoResultadoDto { Sucesso = false, Mensagem = "Erro ao excluir o item: " + ex.Message };
        }
    }

    public async Task<LiberacaoPedidoAcaoResultadoDto> TrocarItemAsync(TrocarItemRequest req, CancellationToken ct = default)
    {
        try
        {
            if (req.ItemSubstitutoID <= 0)
                return new LiberacaoPedidoAcaoResultadoDto { Sucesso = false, Mensagem = "Selecione um item substituto." };

            var erro = await itemCommandRepo.TrocarItemAsync(
                req.CotacaoID, req.CotacaoItemID, req.ItemIDOld, req.CdItemOld, req.NmItemOld,
                req.ItemSubstitutoID, req.FlagTrocaAutomatica,
                req.Motivo, req.UsuarioID, req.EstabelecimentoID, ct);
            if (erro is not null)
                return new LiberacaoPedidoAcaoResultadoDto { Sucesso = false, Mensagem = erro };
            return new LiberacaoPedidoAcaoResultadoDto { Sucesso = true, Mensagem = "Item substituído com sucesso." };
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erro ao trocar item {CotacaoItemID}", req.CotacaoItemID);
            return new LiberacaoPedidoAcaoResultadoDto { Sucesso = false, Mensagem = "Erro ao substituir o item: " + ex.Message };
        }
    }

    // ---------- AÇÕES ----------

    public Task<LiberacaoPedidoAcaoResultadoDto> AlterarObsNotaAsync(AlterarObservacaoRequest req, CancellationToken ct = default)
    {
        // Regra do PHP: não permitir palavra "Entrega" / "Entregar" na observação da nota.
        if (!string.IsNullOrEmpty(req.ObsNova) && req.ObsNova.Contains("Entrega", StringComparison.OrdinalIgnoreCase))
        {
            return Task.FromResult(Falha("Aparentemente foi informado um endereço de entrega na observação da nota. Isto não é permitido! Utilize o endereço do local de entrega para isto."));
        }

        return ExecutarAsync(
            () => commandRepo.AlterarObsNotaAsync(req.CotacaoID, req.UsuarioID, req.ObsAntiga, req.ObsNova, req.Motivo, ct),
            "Observação da Nota Fiscal alterada com sucesso.",
            ct);
    }

    public Task<LiberacaoPedidoAcaoResultadoDto> AlterarObsSolicitanteAsync(AlterarObservacaoRequest req, CancellationToken ct = default)
        => ExecutarAsync(
            () => commandRepo.AlterarObsSolicitanteAsync(req.CotacaoID, req.UsuarioID, req.ObsAntiga, req.ObsNova, req.Motivo, ct),
            "Observação do Solicitante alterada com sucesso.",
            ct);

    public Task<LiberacaoPedidoAcaoResultadoDto> AlterarObsAprovadorAsync(AlterarObservacaoRequest req, CancellationToken ct = default)
        => ExecutarAsync(
            () => commandRepo.AlterarObsAprovadorAsync(req.CotacaoID, req.UsuarioID, req.ObsAntiga, req.ObsNova, req.Motivo, ct),
            "Observação do Aprovador alterada com sucesso.",
            ct);

    public Task<LiberacaoPedidoAcaoResultadoDto> AlterarOrdemCompraAsync(AlterarOrdemCompraRequest req, CancellationToken ct = default)
        => ExecutarAsync(
            () => commandRepo.AlterarOrdemCompraAsync(req.CotacaoID, req.UsuarioID, req.OrdemAntiga, req.OrdemNova, req.Motivo, ct),
            "Ordem de Compra alterada com sucesso.",
            ct);

    public Task<LiberacaoPedidoAcaoResultadoDto> AlterarCanalVendaAsync(AlterarCanalVendaRequest req, CancellationToken ct = default)
        => ExecutarAsync(
            () => commandRepo.AlterarCanalVendaAsync(req.CotacaoID, req.UsuarioID, req.NmCanalAntigo, req.CanalVendaIDNovo, req.Motivo, ct),
            "Canal de Venda alterado com sucesso.",
            ct);

    public Task<LiberacaoPedidoAcaoResultadoDto> AlterarCategoriaAsync(AlterarCategoriaRequest req, CancellationToken ct = default)
        => ExecutarAsync(
            () => commandRepo.AlterarCategoriaAsync(req.CotacaoID, req.UsuarioID, req.NmCategoriaAntiga, req.CategoriaIDNova, req.Motivo, ct),
            "Categoria alterada com sucesso.",
            ct);

    public Task<LiberacaoPedidoAcaoResultadoDto> AlterarCondPagtoAsync(AlterarCondPagtoRequest req, CancellationToken ct = default)
        => ExecutarAsync(
            () => commandRepo.AlterarCondPagtoAsync(req.CotacaoID, req.UsuarioID, req.NmCondPagtoAntiga, req.CondPagtoIDNova, req.Motivo, ct),
            "Condição de Pagamento alterada com sucesso.",
            ct);

    public Task<LiberacaoPedidoAcaoResultadoDto> CobrarFreteAsync(CobrarFreteRequest req, CancellationToken ct = default)
        => ExecutarAsync(
            () => commandRepo.CobrarFreteAsync(req.CotacaoID, req.UsuarioID, req.VlrFrete, req.FlagFreteServico, ct),
            "Frete aplicado ao pedido com sucesso.",
            ct);

    public Task<LiberacaoPedidoAcaoResultadoDto> LiberarMarketplaceAsync(LiberarMarketplaceRequest req, CancellationToken ct = default)
        => ExecutarAsync(
            () => commandRepo.LiberarMarketplaceAsync(req.CotacaoID, req.UsuarioID, ct),
            "Pedido Marketplace liberado.",
            ct);

    public Task<LiberacaoPedidoAcaoResultadoDto> CancelarPedidoAsync(CancelarPedidoRequest req, CancellationToken ct = default)
        => ExecutarAsync(
            () => commandRepo.CancelarPedidoAsync(req.CotacaoID, req.UsuarioID, req.Motivo, ct),
            "Pedido cancelado com sucesso.",
            ct);

    public Task<LiberacaoPedidoAcaoResultadoDto> CancelarMarketplaceAsync(CancelarPedidoRequest req, CancellationToken ct = default)
        => ExecutarAsync(
            () => commandRepo.CancelarMarketplaceAsync(req.CotacaoID, req.UsuarioID, req.Motivo, ct),
            "Pedido Marketplace cancelado com sucesso.",
            ct);

    public Task<LiberacaoPedidoAcaoResultadoDto> DesbloquearAlocacoesAsync(DesbloquearAlocacoesRequest req, CancellationToken ct = default)
        => ExecutarAsync(
            () => commandRepo.DesbloquearAlocacoesAsync(req.CotacaoID, req.UsuarioID, req.Motivo, ct),
            "Alocações desbloqueadas com sucesso.",
            ct);

    public async Task<LiberacaoPedidoAcaoResultadoDto> GerarPedidoRupturasAsync(GerarPedidoRupturasRequest req, CancellationToken ct = default)
    {
        try
        {
            var novoId = await commandRepo.GerarPedidoRupturasAsync(
                req.CotacaoID, req.ClienteID, req.ClienteUsuarioID, req.UsuarioID, req.Motivo, ct);

            if (novoId is null)
                return Falha("Falha na criação do pedido com rupturas.");

            return new LiberacaoPedidoAcaoResultadoDto
            {
                Sucesso = true,
                Mensagem = $"Pedido {novoId} criado com sucesso!",
                NovoCotacaoId = novoId
            };
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Falha ao gerar pedido com rupturas (CotacaoID {CotacaoID})", req.CotacaoID);
            return Falha($"Erro ao gerar pedido com rupturas: {ex.Message}");
        }
    }

    // ---------- Helpers ----------

    private async Task<LiberacaoPedidoAcaoResultadoDto> ExecutarAsync(Func<Task> acao, string msgSucesso, CancellationToken ct)
    {
        try
        {
            await acao();
            return new LiberacaoPedidoAcaoResultadoDto { Sucesso = true, Mensagem = msgSucesso };
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Falha em operação de liberação de pedido");
            return Falha($"Erro ao executar operação: {ex.Message}");
        }
    }

    private static LiberacaoPedidoAcaoResultadoDto Falha(string mensagem) => new() { Sucesso = false, Mensagem = mensagem };
}
