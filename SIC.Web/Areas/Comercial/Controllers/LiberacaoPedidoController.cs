using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SIC.Web.Models.Liberacao;
using SIC.Web.Services;

namespace SIC.Web.Areas.Comercial.Controllers;

[Area("Comercial")]
[Authorize]
public sealed class LiberacaoPedidoController(LiberacaoPedidoApiClient apiClient) : Controller
{
    public async Task<IActionResult> Index(
        string? filtroPalavra1,
        string? filtroPalavra2,
        string? filtroPalavra3,
        int filtroOrdemCompra = 0,
        int filtroRuptura = 0,
        int filtroFrete = 0,
        int filtroMargemNegativa = 0,
        decimal filtroValorAbaixo = 0,
        decimal filtroValorAcima = 0,
        string? filtroIntegracaoSAP = null,
        string? filtroContemItem = null,
        int filtroAtrasados = 0,
        int filtroFretePagar = 0,
        CancellationToken cancellationToken = default)
    {
        var estabId = int.TryParse(User.FindFirst("sic_estabelecimentoid")?.Value, out var e) ? e : 0;
        var userId = int.TryParse(User.FindFirst("sic_usuarioid")?.Value, out var u) ? u : 0;

        if (estabId <= 0 || userId <= 0)
            TempData["Erro"] = "Estabelecimento ou usuário não identificado. Verifique seu login.";

        var dados = await apiClient.ListarAsync(
            estabId, userId,
            filtroPalavra1, filtroPalavra2, filtroPalavra3,
            filtroOrdemCompra, filtroRuptura, filtroFrete,
            filtroMargemNegativa, filtroValorAbaixo, filtroValorAcima,
            filtroIntegracaoSAP, filtroContemItem,
            filtroAtrasados, filtroFretePagar,
            cancellationToken);

        var vm = new LiberacaoPedidoListViewModel
        {
            FiltroPalavra1 = filtroPalavra1,
            FiltroPalavra2 = filtroPalavra2,
            FiltroPalavra3 = filtroPalavra3,
            FiltroOrdemCompra = filtroOrdemCompra,
            FiltroRuptura = filtroRuptura,
            FiltroFrete = filtroFrete,
            FiltroMargemNegativa = filtroMargemNegativa,
            FiltroValorAbaixo = filtroValorAbaixo,
            FiltroValorAcima = filtroValorAcima,
            FiltroIntegracaoSAP = filtroIntegracaoSAP,
            FiltroContemItem = filtroContemItem,
            FiltroAtrasados = filtroAtrasados,
            FiltroFretePagar = filtroFretePagar,
            Pedidos = dados,
            TotalComOV = dados.Count(p => !string.IsNullOrWhiteSpace(p.StatusIntegradoSAP) && p.StatusIntegradoSAP == "Com OV"),
            TotalComRuptura = dados.Count(p => p.QtRuptura > 0),
            TotalAtrasados = dados.Count(p => !string.IsNullOrWhiteSpace(p.StatusSLACliente) && p.StatusSLACliente == "Atrasado"),
            TotalErroOV = dados.Count(p => !string.IsNullOrWhiteSpace(p.DescricaoErroSAP)),
            TotalSemOC = dados.Count(p => !string.IsNullOrWhiteSpace(p.MsgOrdemCompraObrigatoria) && p.MsgOrdemCompraObrigatoria == "Obrigatória"),
            FiltrosAtivos = MontarFiltrosAtivos(filtroPalavra1, filtroPalavra2, filtroPalavra3,
                filtroOrdemCompra, filtroRuptura, filtroFrete,
                filtroMargemNegativa, filtroValorAbaixo, filtroValorAcima,
                filtroIntegracaoSAP, filtroContemItem, filtroAtrasados, filtroFretePagar),
        };

        return View(vm);
    }

    public async Task<IActionResult> Detalhes(int cotacaoId, CancellationToken cancellationToken)
    {
        if (cotacaoId <= 0)
        {
            TempData["Erro"] = "Pedido inválido.";
            return RedirectToAction(nameof(Index));
        }

        var estabId = int.TryParse(User.FindFirst("sic_estabelecimentoid")?.Value, out var e) ? e : 0;
        var userId = int.TryParse(User.FindFirst("sic_usuarioid")?.Value, out var u) ? u : 0;
        var flagAdmin = string.Equals(User.FindFirst("sic_admin")?.Value, "1", StringComparison.OrdinalIgnoreCase);

        if (estabId <= 0 || userId <= 0)
            TempData["Erro"] = "Estabelecimento ou usuário não identificado. Verifique seu login.";

        var vm = await apiClient.GetDetalhesAsync(cotacaoId, cancellationToken);
        if (vm is null)
        {
            TempData["Erro"] = $"Pedido {cotacaoId} não encontrado.";
            return RedirectToAction(nameof(Index));
        }

        vm.SessaoEstabelecimentoID = estabId;
        vm.FlagAdmin = flagAdmin;

        // Se o pedido é do mesmo estabelecimento e tem itens BR Supply, executa a análise de liberação.
        if (!vm.EstabelecimentoIncompativel && vm.TemItensBRSupply && userId > 0)
        {
            vm.Analise = await apiClient.AnalisarAsync(cotacaoId, userId, cancellationToken);
        }

        // Carrega combos para modais de edição (pedidos editáveis do mesmo estabelecimento).
        if (!vm.EstabelecimentoIncompativel)
        {
            var tCanais = userId > 0
                ? apiClient.ListarCanaisVendaAsync(userId, vm.NmCanalVenda, cancellationToken)
                : Task.FromResult<IReadOnlyList<LiberacaoPedidoComboItemViewModel>>([]);
            var tCategorias = vm.ClienteID > 0
                ? apiClient.ListarCategoriasAsync(vm.ClienteID, cancellationToken)
                : Task.FromResult<IReadOnlyList<LiberacaoPedidoComboItemViewModel>>([]);
            var tCondPagto = apiClient.ListarCondicoesPagamentoAsync(vm.NmCondPagto, cancellationToken);

            await Task.WhenAll(tCanais, tCategorias, tCondPagto);

            vm.CanaisVenda = tCanais.Result;
            vm.Categorias = tCategorias.Result;
            vm.CondicoesPagamento = tCondPagto.Result;
        }

        // Permissões.
        // TODO: quando o sistema de permissões numéricas for implementado em claims, substituir estes
        //       gates por uma consulta real. Por enquanto, admins têm todas; demais usuários ganham as
        //       permissões de uso cotidiano (obs, OC, canal, categoria) e as críticas exigem admin.
        vm.PodeAlterarObsSolicitante = flagAdmin || HasPermissao(LiberacaoPedidoDetalhesViewModel.PermAlterarObsSolicitante);
        vm.PodeAlterarObsAprovador = flagAdmin || HasPermissao(LiberacaoPedidoDetalhesViewModel.PermAlterarObsAprovador);
        vm.PodeAlterarCondPagto = flagAdmin || HasPermissao(LiberacaoPedidoDetalhesViewModel.PermAlterarCondPagto);
        vm.PodeDesbloquearAlocacoes = flagAdmin || HasPermissao(LiberacaoPedidoDetalhesViewModel.PermDesbloquearAlocacoes);
        vm.PodeGerarPedidoRupturas = flagAdmin || HasPermissao(LiberacaoPedidoDetalhesViewModel.PermGerarPedidoRupturas);

        return View(vm);
    }

    /// <summary>
    /// Placeholder para gate de permissões numéricas da Intranet.
    /// TODO: Quando as permissões forem expostas em claims (ex.: "sic_permissoes" = "14,135,195,..."),
    /// trocar esta implementação pela consulta real à claim.
    /// </summary>
    private bool HasPermissao(int _) => false;

    // ======================================================================
    //  FASE 3 — Endpoints JSON para modais read-only (Fretes / Impostos)
    //  Consumidos via fetch dentro do bs-modal-fretes e bs-modal-impostos.
    // ======================================================================

    [HttpGet]
    public async Task<IActionResult> OpcoesFreteJson(int cotacaoId, CancellationToken ct)
    {
        if (cotacaoId <= 0) return BadRequest();
        var items = await apiClient.ListarOpcoesFreteAsync(cotacaoId, ct);
        return Json(items);
    }

    [HttpGet]
    public async Task<IActionResult> ImpostosJson(int cotacaoId, CancellationToken ct)
    {
        if (cotacaoId <= 0) return BadRequest();
        var items = await apiClient.ListarImpostosAsync(cotacaoId, ct);
        return Json(items);
    }

    // ---------- Logs (Fase 4) ----------

    [HttpGet]
    public async Task<IActionResult> CotLogJson(int cotacaoId, CancellationToken ct)
    {
        if (cotacaoId <= 0) return BadRequest();
        return Json(await apiClient.ListarCotLogAsync(cotacaoId, ct));
    }

    [HttpGet]
    public async Task<IActionResult> BackOfficeLogJson(int cotacaoId, CancellationToken ct)
    {
        if (cotacaoId <= 0) return BadRequest();
        return Json(await apiClient.ListarBackOfficeLogAsync(cotacaoId, ct));
    }

    [HttpGet]
    public async Task<IActionResult> CotLogDetalhadoJson(int cotacaoId, CancellationToken ct)
    {
        if (cotacaoId <= 0) return BadRequest();
        return Json(await apiClient.ListarCotLogDetalhadoAsync(cotacaoId, ct));
    }

    // ---------- Itens (Fase 5) ----------

    [HttpGet]
    public async Task<IActionResult> ItensBrSupplyJson(int cotacaoId, CancellationToken ct)
    {
        if (cotacaoId <= 0) return BadRequest();
        return Json(await apiClient.ListarItensBrSupplyAsync(cotacaoId, ct));
    }

    [HttpGet]
    public async Task<IActionResult> ItensMarketplaceJson(int cotacaoId, CancellationToken ct)
    {
        if (cotacaoId <= 0) return BadRequest();
        return Json(await apiClient.ListarItensMarketplaceAsync(cotacaoId, ct));
    }

    [HttpGet]
    public async Task<IActionResult> ItensCompativeisJson(int cotacaoItemId, CancellationToken ct)
    {
        if (cotacaoItemId <= 0) return BadRequest();
        return Json(await apiClient.BuscarCompativeisTrocaAsync(cotacaoItemId, ct));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> AlterarItem(AlterarItemInputModel input, CancellationToken ct)
    {
        var req = new
        {
            input.CotacaoID, input.CotacaoItemID, input.ItemIDOld,
            input.CdItemOld, input.NmItemOld,
            input.Quantidade, input.QuantidadeOld,
            Valor = ParseDecimalPtBr(input.Valor),
            ValorOld = ParseDecimalPtBr(input.ValorOld),
            input.OrdemItem, input.OrdemItemOld,
            input.Sequencia, input.SequenciaOld,
            input.Motivo,
            UsuarioID = ObterUsuarioId()
        };
        var r = await apiClient.AlterarItemAsync(req, ct);
        TempData[r.Sucesso ? "MensagemSucesso" : "MensagemErro"] = r.Mensagem;
        return RedirectToAction(nameof(Detalhes), new { id = input.CotacaoID });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> AlterarItemComOv(AlterarItemComOvInputModel input, CancellationToken ct)
    {
        var req = new
        {
            input.CotacaoID, input.CotacaoItemID,
            input.CdItemOld, input.NmItemOld,
            input.OrdemItem, input.OrdemItemOld,
            input.Sequencia, input.SequenciaOld,
            input.Motivo,
            UsuarioID = ObterUsuarioId()
        };
        var r = await apiClient.AlterarItemComOvAsync(req, ct);
        TempData[r.Sucesso ? "MensagemSucesso" : "MensagemErro"] = r.Mensagem;
        return RedirectToAction(nameof(Detalhes), new { id = input.CotacaoID });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> ExcluirItem(ExcluirItemInputModel input, CancellationToken ct)
    {
        var req = new
        {
            input.CotacaoID, input.CotacaoItemID, input.ItemIDOld,
            input.CdItemOld, input.NmItemOld,
            QuantidadeOld = ParseDecimalPtBr(input.QuantidadeOld),
            ValorOld = ParseDecimalPtBr(input.ValorOld),
            input.Motivo,
            UsuarioID = ObterUsuarioId(),
            EstabelecimentoID = await ObterEstabelecimentoIdAsync(input.CotacaoID, ct)
        };
        var r = await apiClient.ExcluirItemAsync(req, ct);
        TempData[r.Sucesso ? "MensagemSucesso" : "MensagemErro"] = r.Mensagem;
        return RedirectToAction(nameof(Detalhes), new { id = input.CotacaoID });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> TrocarItem(TrocarItemInputModel input, CancellationToken ct)
    {
        var req = new
        {
            input.CotacaoID, input.CotacaoItemID, input.ItemIDOld,
            input.CdItemOld, input.NmItemOld,
            input.ItemSubstitutoID, input.FlagTrocaAutomatica,
            input.Motivo,
            UsuarioID = ObterUsuarioId(),
            EstabelecimentoID = await ObterEstabelecimentoIdAsync(input.CotacaoID, ct)
        };
        var r = await apiClient.TrocarItemAsync(req, ct);
        TempData[r.Sucesso ? "MensagemSucesso" : "MensagemErro"] = r.Mensagem;
        return RedirectToAction(nameof(Detalhes), new { id = input.CotacaoID });
    }

    private async Task<int> ObterEstabelecimentoIdAsync(int cotacaoId, CancellationToken ct)
    {
        var det = await apiClient.GetDetalhesAsync(cotacaoId, ct);
        return det?.EstabelecimentoID ?? 0;
    }

    private int ObterUsuarioId()
        => int.TryParse(User.FindFirst("sic_usuarioid")?.Value, out var u) ? u : 0;

    /// <summary>
    /// Faz parse tolerante de decimal em formato pt-BR ("1.234,56") ou invariante ("1234.56").
    /// </summary>
    private static decimal ParseDecimalPtBr(string? input)
    {
        if (string.IsNullOrWhiteSpace(input)) return 0m;
        var s = input.Trim().Replace("R$", "", StringComparison.OrdinalIgnoreCase).Trim();
        var ptBr = System.Globalization.CultureInfo.GetCultureInfo("pt-BR");
        if (decimal.TryParse(s, System.Globalization.NumberStyles.Any, ptBr, out var v)) return v;
        if (decimal.TryParse(s, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out v)) return v;
        return 0m;
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> IntegrarPedido(int cotacaoId, CancellationToken cancellationToken)
    {
        if (cotacaoId <= 0)
        {
            TempData["Erro"] = "Pedido inválido.";
            return RedirectToAction(nameof(Index));
        }

        var (sucesso, mensagem) = await apiClient.IntegrarAsync([cotacaoId], cancellationToken);
        if (sucesso)
            TempData["Sucesso"] = string.IsNullOrWhiteSpace(mensagem) ? "Pedido enviado para integração com o SAP." : mensagem;
        else
            TempData["Erro"] = mensagem;

        return RedirectToAction(nameof(Detalhes), new { cotacaoId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> LiberarPedido(int cotacaoId, CancellationToken cancellationToken)
    {
        if (cotacaoId <= 0)
        {
            TempData["Erro"] = "Pedido inválido.";
            return RedirectToAction(nameof(Index));
        }

        var (sucesso, mensagem) = await apiClient.LiberarAsync([cotacaoId], cancellationToken);
        if (sucesso)
            TempData["Sucesso"] = string.IsNullOrWhiteSpace(mensagem) ? "Pedido liberado." : mensagem;
        else
            TempData["Erro"] = mensagem;

        return RedirectToAction(nameof(Detalhes), new { cotacaoId });
    }

    // ======================================================================
    //  FASE 3 — Ações dos modais (detalhes do pedido)
    // ======================================================================

    [HttpPost, ValidateAntiForgeryToken]
    public Task<IActionResult> AlterarObsNota(int CotacaoID, string ObsAntiga, string Observacao, string Motivo, CancellationToken ct)
        => ExecutarAcaoAsync(CotacaoID, ct, uid => apiClient.AlterarObsNotaAsync(new
        {
            CotacaoID,
            UsuarioID = uid,
            ObsAntiga = ObsAntiga ?? string.Empty,
            ObsNova = Observacao ?? string.Empty,
            Motivo = Motivo ?? string.Empty
        }, ct));

    [HttpPost, ValidateAntiForgeryToken]
    public Task<IActionResult> AlterarObsSolicitante(int CotacaoID, string ObsAntiga, string Observacao, string Motivo, CancellationToken ct)
        => ExecutarAcaoAsync(CotacaoID, ct, uid => apiClient.AlterarObsSolicitanteAsync(new
        {
            CotacaoID,
            UsuarioID = uid,
            ObsAntiga = ObsAntiga ?? string.Empty,
            ObsNova = Observacao ?? string.Empty,
            Motivo = Motivo ?? string.Empty
        }, ct));

    [HttpPost, ValidateAntiForgeryToken]
    public Task<IActionResult> AlterarObsAprovador(int CotacaoID, string ObsAntiga, string Observacao, string Motivo, CancellationToken ct)
        => ExecutarAcaoAsync(CotacaoID, ct, uid => apiClient.AlterarObsAprovadorAsync(new
        {
            CotacaoID,
            UsuarioID = uid,
            ObsAntiga = ObsAntiga ?? string.Empty,
            ObsNova = Observacao ?? string.Empty,
            Motivo = Motivo ?? string.Empty
        }, ct));

    [HttpPost, ValidateAntiForgeryToken]
    public Task<IActionResult> AlterarOrdemCompra(int CotacaoID, string OrdemCompraAntiga, string OrdemCompra, string Motivo, CancellationToken ct)
        => ExecutarAcaoAsync(CotacaoID, ct, uid => apiClient.AlterarOrdemCompraAsync(new
        {
            CotacaoID,
            UsuarioID = uid,
            OrdemAntiga = OrdemCompraAntiga ?? string.Empty,
            OrdemNova = OrdemCompra ?? string.Empty,
            Motivo = Motivo ?? string.Empty
        }, ct));

    [HttpPost, ValidateAntiForgeryToken]
    public Task<IActionResult> AlterarCanalVenda(int CotacaoID, string NmCanalVendaAntigo, int CanalVendaID, string Motivo, CancellationToken ct)
        => ExecutarAcaoAsync(CotacaoID, ct, uid => apiClient.AlterarCanalVendaAsync(new
        {
            CotacaoID,
            UsuarioID = uid,
            NmCanalAntigo = NmCanalVendaAntigo ?? string.Empty,
            CanalVendaIDNovo = CanalVendaID,
            Motivo = Motivo ?? string.Empty
        }, ct));

    [HttpPost, ValidateAntiForgeryToken]
    public Task<IActionResult> AlterarCategoria(int CotacaoID, string NmCategoriaAntiga, int CategoriaID, string Motivo, CancellationToken ct)
        => ExecutarAcaoAsync(CotacaoID, ct, uid => apiClient.AlterarCategoriaAsync(new
        {
            CotacaoID,
            UsuarioID = uid,
            NmCategoriaAntiga = NmCategoriaAntiga ?? string.Empty,
            CategoriaIDNova = CategoriaID,
            Motivo = Motivo ?? string.Empty
        }, ct));

    [HttpPost, ValidateAntiForgeryToken]
    public Task<IActionResult> AlterarCondPagto(int CotacaoID, string NmCondPagtoAntiga, int CondPagtoID, string Motivo, CancellationToken ct)
        => ExecutarAcaoAsync(CotacaoID, ct, uid => apiClient.AlterarCondPagtoAsync(new
        {
            CotacaoID,
            UsuarioID = uid,
            NmCondPagtoAntiga = NmCondPagtoAntiga ?? string.Empty,
            CondPagtoIDNova = CondPagtoID,
            Motivo = Motivo ?? string.Empty
        }, ct));

    [HttpPost, ValidateAntiForgeryToken]
    public Task<IActionResult> CobrarFrete(int CotacaoID, decimal VlrFrete, int FlagFreteServico, CancellationToken ct)
        => ExecutarAcaoAsync(CotacaoID, ct, uid => apiClient.CobrarFreteAsync(new
        {
            CotacaoID,
            UsuarioID = uid,
            VlrFrete,
            FlagFreteServico
        }, ct));

    [HttpPost, ValidateAntiForgeryToken]
    public Task<IActionResult> LiberarMarketplaceModal(int CotacaoID, CancellationToken ct)
        => ExecutarAcaoAsync(CotacaoID, ct, uid => apiClient.LiberarMarketplaceModalAsync(new
        {
            CotacaoID,
            UsuarioID = uid
        }, ct));

    [HttpPost, ValidateAntiForgeryToken]
    public Task<IActionResult> CancelarPedido(int CotacaoID, string Motivo, CancellationToken ct)
        => ExecutarAcaoAsync(CotacaoID, ct, uid => apiClient.CancelarPedidoAsync(new
        {
            CotacaoID,
            UsuarioID = uid,
            Motivo = Motivo ?? string.Empty
        }, ct));

    [HttpPost, ValidateAntiForgeryToken]
    public Task<IActionResult> CancelarMarketplace(int CotacaoID, string Motivo, CancellationToken ct)
        => ExecutarAcaoAsync(CotacaoID, ct, uid => apiClient.CancelarMarketplaceAsync(new
        {
            CotacaoID,
            UsuarioID = uid,
            Motivo = Motivo ?? string.Empty
        }, ct));

    [HttpPost, ValidateAntiForgeryToken]
    public Task<IActionResult> DesbloquearAlocacoes(int CotacaoID, string Motivo, CancellationToken ct)
        => ExecutarAcaoAsync(CotacaoID, ct, uid => apiClient.DesbloquearAlocacoesAsync(new
        {
            CotacaoID,
            UsuarioID = uid,
            Motivo = Motivo ?? string.Empty
        }, ct));

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> GerarPedidoRupturas(int CotacaoID, int ClienteID, int ClienteUsuarioID, string Motivo, CancellationToken ct)
    {
        if (CotacaoID <= 0)
        {
            TempData["Erro"] = "Pedido inválido.";
            return RedirectToAction(nameof(Index));
        }

        var uid = int.TryParse(User.FindFirst("sic_usuarioid")?.Value, out var u) ? u : 0;

        var resultado = await apiClient.GerarPedidoRupturasAsync(new
        {
            CotacaoID,
            UsuarioID = uid,
            ClienteID,
            ClienteUsuarioID,
            Motivo = Motivo ?? string.Empty
        }, ct);

        if (resultado.Sucesso)
        {
            TempData["Sucesso"] = resultado.Mensagem;
            // Se um novo pedido foi gerado, redireciona para ele; caso contrário volta para o atual.
            var destinoId = resultado.NovoCotacaoId is > 0 ? resultado.NovoCotacaoId.Value : CotacaoID;
            return RedirectToAction(nameof(Detalhes), new { cotacaoId = destinoId });
        }

        TempData["Erro"] = resultado.Mensagem;
        return RedirectToAction(nameof(Detalhes), new { cotacaoId = CotacaoID });
    }

    /// <summary>
    /// Executa uma ação do modal de detalhes: valida pedido, obtém UsuarioID da sessão,
    /// chama a função informada e redireciona de volta para a tela de Detalhes com TempData.
    /// </summary>
    private async Task<IActionResult> ExecutarAcaoAsync(int cotacaoId, CancellationToken ct, Func<int, Task<LiberacaoPedidoAcaoResultadoViewModel>> acao)
    {
        if (cotacaoId <= 0)
        {
            TempData["Erro"] = "Pedido inválido.";
            return RedirectToAction(nameof(Index));
        }

        var uid = int.TryParse(User.FindFirst("sic_usuarioid")?.Value, out var u) ? u : 0;
        if (uid <= 0)
        {
            TempData["Erro"] = "Usuário não identificado.";
            return RedirectToAction(nameof(Detalhes), new { cotacaoId });
        }

        var resultado = await acao(uid);
        if (resultado.Sucesso)
            TempData["Sucesso"] = resultado.Mensagem;
        else
            TempData["Erro"] = resultado.Mensagem;

        return RedirectToAction(nameof(Detalhes), new { cotacaoId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Liberar(int[] cotacaoIds, CancellationToken cancellationToken)
    {
        if (cotacaoIds is not { Length: > 0 })
        {
            TempData["Erro"] = "Nenhum pedido selecionado para liberação.";
            return RedirectToAction(nameof(Index));
        }

        var (sucesso, mensagem) = await apiClient.LiberarAsync(cotacaoIds, cancellationToken);

        if (sucesso)
            TempData["Sucesso"] = mensagem;
        else
            TempData["Erro"] = mensagem;

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Integrar(int[] cotacaoIds, CancellationToken cancellationToken)
    {
        if (cotacaoIds is not { Length: > 0 })
        {
            TempData["Erro"] = "Nenhum pedido selecionado para integração.";
            return RedirectToAction(nameof(Index));
        }

        var (sucesso, mensagem) = await apiClient.IntegrarAsync(cotacaoIds, cancellationToken);

        if (sucesso)
            TempData["Sucesso"] = mensagem;
        else
            TempData["Erro"] = mensagem;

        return RedirectToAction(nameof(Index));
    }

    private static List<FiltroAtivo> MontarFiltrosAtivos(
        string? filtroPalavra1, string? filtroPalavra2, string? filtroPalavra3,
        int filtroOrdemCompra, int filtroRuptura, int filtroFrete,
        int filtroMargemNegativa, decimal filtroValorAbaixo, decimal filtroValorAcima,
        string? filtroIntegracaoSAP, string? filtroContemItem,
        int filtroAtrasados, int filtroFretePagar)
    {
        var filtros = new List<FiltroAtivo>();

        if (!string.IsNullOrWhiteSpace(filtroPalavra1)) filtros.Add(new($"Expressão 1: {filtroPalavra1}", "filtroPalavra1"));
        if (!string.IsNullOrWhiteSpace(filtroPalavra2)) filtros.Add(new($"Expressão 2: {filtroPalavra2}", "filtroPalavra2"));
        if (!string.IsNullOrWhiteSpace(filtroPalavra3)) filtros.Add(new($"Expressão 3: {filtroPalavra3}", "filtroPalavra3"));
        if (filtroOrdemCompra != 0) filtros.Add(new(filtroOrdemCompra == 1 ? "OC: Preenchida" : "OC: Vazia", "filtroOrdemCompra"));
        if (filtroRuptura != 0) filtros.Add(new(filtroRuptura == 1 ? "Ruptura: Com" : "Ruptura: Sem", "filtroRuptura"));
        if (filtroFrete != 0) filtros.Add(new(filtroFrete == 1 ? "Frete: Com valor" : "Frete: Sem valor", "filtroFrete"));
        if (filtroMargemNegativa != 0) filtros.Add(new("Margem: Apenas negativa", "filtroMargemNegativa"));
        if (filtroValorAbaixo != 0) filtros.Add(new($"Valor abaixo de {filtroValorAbaixo:N2}", "filtroValorAbaixo"));
        if (filtroValorAcima != 0) filtros.Add(new($"Valor acima de {filtroValorAcima:N2}", "filtroValorAcima"));
        if (!string.IsNullOrWhiteSpace(filtroIntegracaoSAP)) filtros.Add(new($"Integração: {filtroIntegracaoSAP}", "filtroIntegracaoSAP"));
        if (!string.IsNullOrWhiteSpace(filtroContemItem)) filtros.Add(new($"Item: {filtroContemItem}", "filtroContemItem"));
        if (filtroAtrasados != 0) filtros.Add(new("SLA: Atrasado", "filtroAtrasados"));
        if (filtroFretePagar != 0) filtros.Add(new(filtroFretePagar == 1 ? "Frete pagar: Agrupados" : "Frete pagar: Acima 6%", "filtroFretePagar"));

        return filtros;
    }
}
