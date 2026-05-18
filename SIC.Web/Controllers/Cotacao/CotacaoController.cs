using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using SIC.Web.Models.Cotacao;
using SIC.Web.Services.Cotacao;

namespace SIC.Web.Controllers.Cotacao;

[Authorize]
[Route("Cotacao")]
public sealed class CotacaoController(
    CotacaoQueryService queryService,
    CotacaoAddService addService,
    CotacaoApiClient apiClient,
    CotacaoEmailService emailService) : Controller
{
    [HttpGet("")]
    [HttpGet("Index")]
    public async Task<IActionResult> Index(
        string? cdExtCliente,
        int? propostaId,
        string? cnpj,
        int? estabelecimentoID,
        int? statusID,
        string? dataInicial,
        string? dataFinal,
        int filtroCotacao = 1,
        CancellationToken cancellationToken = default)
    {
        var filtroDataInicial = DateTime.TryParse(dataInicial, out var di) ? di : DateTime.Today.AddMonths(-1);
        var filtroDataFinal = DateTime.TryParse(dataFinal, out var df) ? df : DateTime.Today;

        var filtroAplicado = !string.IsNullOrWhiteSpace(cdExtCliente)
            || propostaId.HasValue
            || !string.IsNullOrWhiteSpace(cnpj)
            || estabelecimentoID.HasValue
            || statusID.HasValue
            || !string.IsNullOrWhiteSpace(dataInicial)
            || !string.IsNullOrWhiteSpace(dataFinal);

        var filtro = new CotacaoListFilterViewModel
        {
            UsuarioID = GetUsuarioId(),
            FiltroCotacao = filtroCotacao,
            CdExtCliente = cdExtCliente,
            PropostaId = propostaId,
            CNPJ = cnpj,
            EstabelecimentoID = estabelecimentoID,
            StatusID = statusID,
            DataInicial = dataInicial,
            DataFinal = dataFinal,
        };

        var estabelecimentosTask = queryService.GetEstabelecimentoOptionsAsync(cancellationToken);
        var statusOptionsTask = queryService.GetStatusOptionsAsync(cancellationToken);

        await Task.WhenAll(estabelecimentosTask, statusOptionsTask);

        var vm = new CotacaoListPageViewModel
        {
            Filtro = filtro,
            FiltroDataInicial = filtroDataInicial,
            FiltroDataFinal = filtroDataFinal,
            FiltroAplicado = filtroAplicado,
            EstabelecimentoOptions = estabelecimentosTask.Result,
            StatusOptions = statusOptionsTask.Result,
        };

        return View(vm);
    }

    /// <summary>
    /// Endpoint AJAX para DataTables server-side processing.
    /// </summary>
    [HttpGet("ListData")]
    public async Task<IActionResult> ListData(
        int draw,
        int start,
        int length,
        string? cdExtCliente,
        int? propostaId,
        string? cnpj,
        int? estabelecimentoID,
        int? statusID,
        string? dataInicial,
        string? dataFinal,
        int filtroCotacao = 1,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var filtroDataInicial = DateTime.TryParse(dataInicial, out var di) ? di : DateTime.Today.AddMonths(-1);
            var filtroDataFinal = DateTime.TryParse(dataFinal, out var df) ? df : DateTime.Today;

            var filtro = new CotacaoListFilterViewModel
            {
                UsuarioID = GetUsuarioId(),
                FiltroCotacao = filtroCotacao,
                CdExtCliente = cdExtCliente,
                PropostaId = propostaId,
                CNPJ = cnpj,
                EstabelecimentoID = estabelecimentoID,
                StatusID = statusID,
                DataInicial = dataInicial,
                DataFinal = dataFinal,
            };

            var searchValue = Request.Query["search[value]"].ToString();
            var orderColStr = Request.Query["order[0][column]"].ToString();
            var orderDir = Request.Query["order[0][dir]"].ToString();

            var allItems = await queryService.GetListAsync(filtro, filtroDataInicial, filtroDataFinal, cancellationToken);

            IEnumerable<CotacaoListItemViewModel> filtered = allItems;
            if (!string.IsNullOrWhiteSpace(searchValue))
            {
                var term = searchValue.Trim();
                filtered = allItems.Where(r =>
                    r.ClienteNome.Contains(term, StringComparison.OrdinalIgnoreCase)
                    || r.ClienteCNPJ.Contains(term, StringComparison.OrdinalIgnoreCase)
                    || r.PropostaId.ToString().Contains(term, StringComparison.OrdinalIgnoreCase)
                    || r.CdExtCliente.Contains(term, StringComparison.OrdinalIgnoreCase)
                    || r.NmEstabelecimento.Contains(term, StringComparison.OrdinalIgnoreCase)
                    || r.StatusName.Contains(term, StringComparison.OrdinalIgnoreCase)
                    || r.Nome.Contains(term, StringComparison.OrdinalIgnoreCase));
            }

            var orderedItems = ApplyOrder(filtered, orderColStr, orderDir).ToList();

            var recordsTotal = allItems.Count;
            var recordsFiltered = orderedItems.Count;
            var pageData = orderedItems.Skip(start).Take(length);

            var data = pageData.Select(r => new object[]
            {
                r.PropostaId,
                r.Nome,
                r.CdExtCliente,
                r.ClienteNome,
                r.ClienteCNPJ,
                r.NmEstabelecimento,
                r.StatusName,
                r.TotalVenda.ToString("N2", new System.Globalization.CultureInfo("pt-BR")),
                r.QtdItens,
                r.DataAbertura,
                new { propostaId = r.PropostaId, statusId = r.StatusID }
            }).ToList();

            return Json(new { draw, recordsTotal, recordsFiltered, data });
        }
        catch (Exception ex)
        {
            return Json(new { draw, recordsTotal = 0, recordsFiltered = 0, data = Array.Empty<object>(), error = ex.Message });
        }
    }

    private static IEnumerable<CotacaoListItemViewModel> ApplyOrder(
        IEnumerable<CotacaoListItemViewModel> items, string colIndex, string dir)
    {
        var asc = !string.Equals(dir, "desc", StringComparison.OrdinalIgnoreCase);
        return colIndex switch
        {
            "0" => asc ? items.OrderBy(r => r.PropostaId) : items.OrderByDescending(r => r.PropostaId),
            "1" => asc ? items.OrderBy(r => r.Nome) : items.OrderByDescending(r => r.Nome),
            "3" => asc ? items.OrderBy(r => r.ClienteNome) : items.OrderByDescending(r => r.ClienteNome),
            "6" => asc ? items.OrderBy(r => r.StatusName) : items.OrderByDescending(r => r.StatusName),
            "9" => asc ? items.OrderBy(r => r.DataAberturaSQL) : items.OrderByDescending(r => r.DataAberturaSQL),
            _ => items.OrderByDescending(r => r.PropostaId),
        };
    }

    [HttpGet("Edit")]
    public async Task<IActionResult> Edit(int propostaId, CancellationToken cancellationToken)
    {
        if (propostaId <= 0)
            return RedirectToAction(nameof(Index));

        var dados = await addService.GetPropostaParaEditAsync(propostaId, cancellationToken);
        if (dados is null)
        {
            TempData["SwalIcon"]  = "warning";
            TempData["SwalTitle"] = "Não encontrado";
            TempData["SwalText"]  = $"Proposta #{propostaId} não encontrada.";
            return RedirectToAction(nameof(Index));
        }

        var vm = new CotacaoAddViewModel
        {
            PropostaId          = dados.PropostaId,
            StatusID            = dados.StatusID,
            StatusNome          = dados.StatusNome,
            NomeCotacao         = dados.Nome,
            TipoNome            = dados.TipoCotacao,
            Estabelecimento     = dados.EstabelecimentoID.ToString(),
            Cliente             = dados.ClienteId.ToString(),
            Endereco            = dados.ClienteEnderecoID?.ToString(),
            LocalEntrega        = dados.ClienteLocalEntregaID?.ToString(),
            ObsLocalEntrega     = dados.ObsLocalEntrega,
            TabelaPrecoId       = dados.TabelaPrecoID,
            TabelaPreco         = dados.TabelaPrecoNome,
            PrecoItens          = dados.FlagPrecoConformeTabela,
            UfOrigem            = dados.UfOrigem,
            UfDestino           = dados.UfDestino,
            CidadeDestino       = dados.CodigoIBGE?.ToString(),
            MargemPadrao        = dados.MargemPadrao?.ToString("N2", new System.Globalization.CultureInfo("pt-BR")),
            Validade            = dados.DataValidade?.ToString("yyyy-MM-dd"),
            CondPagtoId         = dados.CondPagtoId,
            FormaPagtoId        = dados.FormaPagamentoSAP,
            TipoOrdem           = dados.TipoOVSAP,
            OrdemCompra         = dados.OrdemCompra,
            NrContrato          = dados.NrContrato,
            TipoMotivoIDSAP     = dados.TipoMotivoIDSAP,
            NomeContatoExterno  = dados.ContatoNome,
            EmailContatoExterno = dados.ContatoEmail,
            Observacoes         = dados.Obs,
        };

        // Pré-popula o select2 de cliente com o texto já salvo
        vm.ClienteOptions =
        [
            new SelectListItem { Value = dados.ClienteId.ToString(), Text = dados.ClienteNome }
        ];

        // Carrega todos os lookups estáticos (Tipos, Estabelecimentos, CondPagto, etc.)
        await CarregarLookupsAsync(vm, cancellationToken);

        // Acerta o campo Tipo (ID) buscando pelo nome salvo no banco
        var tipoMatch = vm.Tipos.FirstOrDefault(t => t.Text == dados.TipoCotacao);
        if (tipoMatch is not null)
            vm.Tipo = tipoMatch.Value;

        // Carrega Tipos de Ordem para o tipo salvo
        if (!string.IsNullOrWhiteSpace(vm.Tipo) && int.TryParse(vm.Tipo, out var tipoId))
        {
            vm.TipoOrdemOptions = await addService.GetTiposOrdemAsync(tipoId, GetUsuarioId(), cancellationToken);
        }

        // Carrega Endereços do cliente e pré-seleciona
        if (dados.ClienteId > 0)
        {
            var enderecos = await addService.GetEnderecosByClienteAsync(dados.ClienteId, cancellationToken);
            vm.EnderecoOptions = enderecos.Select(e => new SelectListItem
            {
                Value    = e.ClienteEnderecoId.ToString(),
                Text     = e.Text,
                Selected = e.ClienteEnderecoId.ToString() == vm.Endereco
            }).ToList();
        }

        // Carrega Locais de Entrega do endereço e pré-seleciona
        if (dados.ClienteEnderecoID.HasValue)
        {
            var locais = await addService.GetLocaisEntregaByEnderecoAsync(dados.ClienteEnderecoID.Value, cancellationToken);
            vm.LocalEntregaOptions = locais.Select(l => new SelectListItem
            {
                Value    = l.ClienteLocalEntregaId.ToString(),
                Text     = l.Text,
                Selected = l.ClienteLocalEntregaId.ToString() == vm.LocalEntrega
            }).ToList();

            // Pré-seleciona ObsLocalEntrega a partir do local de entrega salvo
            var localSelecionado = locais.FirstOrDefault(l => l.ClienteLocalEntregaId.ToString() == vm.LocalEntrega);
            if (localSelecionado?.ObsLocalEntrega is not null)
            {
                vm.ObsLocalEntregaOptions =
                [
                    new SelectListItem { Value = localSelecionado.ObsLocalEntrega, Text = localSelecionado.ObsLocalEntrega, Selected = true }
                ];
                vm.ObsLocalEntrega = localSelecionado.ObsLocalEntrega;
            }
        }

        // Carrega Cidades da UF destino e pré-seleciona
        if (!string.IsNullOrWhiteSpace(dados.UfDestino))
        {
            var cidades = await addService.GetCidadesByUfAsync(dados.UfDestino, cancellationToken);
            vm.CidadeDestinoOptions = cidades.Select(c => new SelectListItem
            {
                Value    = c.Value,
                Text     = c.Text,
                Selected = c.Value == vm.CidadeDestino
            }).ToList();
        }

        // Carrega Contratos do cliente (para Comodato)
        if (dados.ClienteId > 0 && !string.IsNullOrWhiteSpace(dados.NrContrato))
        {
            var contratos = await addService.GetContratosAsync(dados.ClienteId, cancellationToken);
            // TipoOrdemOptions já foi carregado, apenas garantimos que NrContrato está no select
            // Será visível via JS se o tipo for Comodato
            _ = contratos; // disponíveis via AJAX normal caso o usuário altere o tipo
        }

        return View(vm);
    }

    [HttpPost("Edit")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(CotacaoAddViewModel vm, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(vm.Tipo))
            ModelState.AddModelError(nameof(vm.Tipo), "O campo Tipo é obrigatório.");
        if (string.IsNullOrWhiteSpace(vm.NomeCotacao))
            ModelState.AddModelError(nameof(vm.NomeCotacao), "O campo Nome da Cotação é obrigatório.");
        if (string.IsNullOrWhiteSpace(vm.Estabelecimento))
            ModelState.AddModelError(nameof(vm.Estabelecimento), "O campo Estabelecimento é obrigatório.");
        if (string.IsNullOrWhiteSpace(vm.Cliente))
            ModelState.AddModelError(nameof(vm.Cliente), "O campo Cliente é obrigatório.");
        if (string.IsNullOrWhiteSpace(vm.Endereco))
            ModelState.AddModelError(nameof(vm.Endereco), "O campo Endereço é obrigatório.");

        if (!ModelState.IsValid)
        {
            await CarregarLookupsAsync(vm, cancellationToken);
            return View(vm);
        }

        try
        {
            var tipoNome = vm.TipoNome ?? string.Empty;

            int? tipoMotivoIdSap = null;
            if (tipoNome is "Pedido - Remessa Reposição" or "Pedido - Bonificação")
                tipoMotivoIdSap = vm.MotivoBonificacaoId;

            string? nrContrato = tipoNome == "Comodato" ? vm.NrContrato : null;

            static decimal ParseDecimal(string? value) =>
                decimal.TryParse(value?.Replace(".", "").Replace(",", "."),
                    System.Globalization.NumberStyles.Any,
                    System.Globalization.CultureInfo.InvariantCulture, out var d) ? d : 0m;

            int? clienteEnderecoId      = int.TryParse(vm.Endereco, out var ceid) ? ceid : null;
            int? clienteLocalEntregaId  = int.TryParse(vm.LocalEntrega, out var clid) ? clid : null;
            int? codigoIBGE             = int.TryParse(vm.CidadeDestino, out var ibge) ? ibge : null;
            int estabelecimentoId       = int.TryParse(vm.Estabelecimento, out var eid) ? eid : 0;
            int clienteId               = int.TryParse(vm.Cliente, out var cid) ? cid : 0;

            var request = new CriarPropostaRequest
            {
                Nome                  = vm.NomeCotacao,
                TipoID                = 2,
                TipoNome              = tipoNome,
                EstabelecimentoID     = estabelecimentoId,
                ClienteId             = clienteId,
                ClienteEnderecoID     = clienteEnderecoId,
                ClienteLocalEntregaID = clienteLocalEntregaId,
                ObsLocalEntrega       = string.IsNullOrWhiteSpace(vm.ObsLocalEntrega) ? null : vm.ObsLocalEntrega,
                TabelaPrecoID         = vm.TabelaPrecoId,
                FlagPrecoConformeTabela = vm.PrecoItens,
                UfOrigem              = vm.UfOrigem,
                UfDestino             = vm.UfDestino,
                CodigoIBGE            = codigoIBGE,
                MargemPadrao          = ParseDecimal(vm.MargemPadrao),
                DataValidade          = DateTime.TryParse(vm.Validade, out var dv) ? dv : null,
                CondPagtoId           = vm.CondPagtoId,
                FormaPagamentoSAP     = vm.FormaPagtoId,
                TipoOVSAP             = vm.TipoOrdem,
                OrdemCompra           = string.IsNullOrWhiteSpace(vm.OrdemCompra) ? null : vm.OrdemCompra,
                NrContrato            = nrContrato,
                TipoMotivoIDSAP       = tipoMotivoIdSap,
                ContatoNome           = string.IsNullOrWhiteSpace(vm.NomeContatoExterno) ? null : vm.NomeContatoExterno,
                ContatoEmail          = string.IsNullOrWhiteSpace(vm.EmailContatoExterno) ? null : vm.EmailContatoExterno,
                Obs                   = string.IsNullOrWhiteSpace(vm.Observacoes) ? null : vm.Observacoes,
                UsuarioId             = GetUsuarioId(),
                ValorVendaTotal       = 0m,
                Frete                 = 0m,
                VlrPedidoMinimo       = 0m,
            };

            await addService.AtualizarPropostaAsync(vm.PropostaId, request, cancellationToken);

            TempData["SwalIcon"]  = "success";
            TempData["SwalTitle"] = "Salvo!";
            TempData["SwalText"]  = "Cotação atualizada com sucesso.";

            return RedirectToAction(nameof(Cotacao), new { propostaId = vm.PropostaId });
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, $"Erro ao atualizar cotação: {ex.Message}");
            await CarregarLookupsAsync(vm, cancellationToken);
            return View(vm);
        }
    }

    [HttpGet("Cotacao")]
    public async Task<IActionResult> Cotacao(int propostaId, CancellationToken cancellationToken = default)
    {
        if (propostaId <= 0)
        {
            TempData["SwalIcon"] = "error";
            TempData["SwalTitle"] = "Erro";
            TempData["SwalText"] = "ID da proposta inválido.";
            return RedirectToAction(nameof(Index));
        }

        var cotacao = await queryService.GetByPropostaIdAsync(propostaId, cancellationToken);

        if (cotacao == null)
        {
            TempData["SwalIcon"] = "warning";
            TempData["SwalTitle"] = "Não encontrado";
            TempData["SwalText"] = $"Proposta #{propostaId} não encontrada.";
            return RedirectToAction(nameof(Index));
        }

        cotacao.CondicoesPagamento = await queryService.GetCondicoesPagamentoAsync(
            cotacao.EstabelecimentoID, cotacao.ValorVendaTotal, cancellationToken);

        var usuarioId   = GetUsuarioId();
        var isAdmin     = User.FindFirst("sic_admin")?.Value == "1";
        var isBackOffice = User.FindFirst("sic_backoffice")?.Value == "1";
        cotacao.PodeAprovar = isAdmin || isBackOffice || usuarioId == cotacao.AtendenteAprovadorID;

        return View(cotacao);
    }

    [HttpGet("Add")]
    public async Task<IActionResult> Add(CancellationToken cancellationToken)
    {
        var vm = new CotacaoAddViewModel();
        await CarregarLookupsAsync(vm, cancellationToken);
        return View(vm);
    }

    [HttpPost("Add")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Add(CotacaoAddViewModel vm, CancellationToken cancellationToken)
    {
        // Validação manual — sem [Required] no ViewModel para evitar bloqueio por campos dinâmicos
        if (string.IsNullOrWhiteSpace(vm.Tipo))
            ModelState.AddModelError(nameof(vm.Tipo), "O campo Tipo é obrigatório.");
        if (string.IsNullOrWhiteSpace(vm.NomeCotacao))
            ModelState.AddModelError(nameof(vm.NomeCotacao), "O campo Nome da Cotação é obrigatório.");
        if (string.IsNullOrWhiteSpace(vm.Estabelecimento))
            ModelState.AddModelError(nameof(vm.Estabelecimento), "O campo Estabelecimento é obrigatório.");
        if (string.IsNullOrWhiteSpace(vm.Cliente))
            ModelState.AddModelError(nameof(vm.Cliente), "O campo Cliente é obrigatório.");
        if (string.IsNullOrWhiteSpace(vm.Endereco))
            ModelState.AddModelError(nameof(vm.Endereco), "O campo Endereço é obrigatório.");

        if (!ModelState.IsValid)
            return View(vm);

        try
        {
            // TipoID sempre fixo = 2
            const int tipoId = 2;
            var tipoNome = vm.TipoNome ?? string.Empty;

            // Regra TipoMotivoIDSAP: apenas para remessa reposição/bonificação
            int? tipoMotivoIdSap = null;
            if (tipoNome is "Pedido - Remessa Reposição" or "Pedido - Bonificação")
                tipoMotivoIdSap = vm.MotivoBonificacaoId;

            // Regra NrContrato: apenas para Comodato
            string? nrContrato = tipoNome == "Comodato" ? vm.NrContrato : null;

            // Parseia campos numéricos no formato BR (vírgula como decimal)
            static decimal ParseDecimal(string? value) =>
                decimal.TryParse(value?.Replace(".", "").Replace(",", "."),
                    System.Globalization.NumberStyles.Any,
                    System.Globalization.CultureInfo.InvariantCulture, out var d) ? d : 0m;

            int? clienteEnderecoId = int.TryParse(vm.Endereco, out var ceid) ? ceid : null;
            int? clienteLocalEntregaId = int.TryParse(vm.LocalEntrega, out var clid) ? clid : null;
            int? codigoIBGE = int.TryParse(vm.CidadeDestino, out var ibge) ? ibge : null;
            int estabelecimentoId = int.TryParse(vm.Estabelecimento, out var eid) ? eid : 0;
            int clienteId = int.TryParse(vm.Cliente, out var cid) ? cid : 0;

            // Calcula frete antes do insert
            var (frete, vlrPedidoMinimo) = clienteEnderecoId.HasValue
                ? await addService.BuscarFreteInicialAsync(clienteEnderecoId.Value, clienteId, vm.UfDestino, cancellationToken)
                : (0m, 0m);

            var request = new CriarPropostaRequest
            {
                Nome = vm.NomeCotacao,
                TipoID = tipoId,
                TipoNome = tipoNome,
                EstabelecimentoID = estabelecimentoId,
                ClienteId = clienteId,
                ClienteEnderecoID = clienteEnderecoId,
                ClienteLocalEntregaID = clienteLocalEntregaId,
                ObsLocalEntrega = string.IsNullOrWhiteSpace(vm.ObsLocalEntrega) ? null : vm.ObsLocalEntrega,
                TabelaPrecoID = vm.TabelaPrecoId,
                FlagPrecoConformeTabela = vm.PrecoItens,
                UfOrigem = vm.UfOrigem,
                UfDestino = vm.UfDestino,
                CodigoIBGE = codigoIBGE,
                MargemPadrao = ParseDecimal(vm.MargemPadrao),
                DataValidade = DateTime.TryParse(vm.Validade, out var dv) ? dv : null,
                CondPagtoId = vm.CondPagtoId,
                FormaPagamentoSAP = vm.FormaPagtoId,
                TipoOVSAP = vm.TipoOrdem,
                OrdemCompra = string.IsNullOrWhiteSpace(vm.OrdemCompra) ? null : vm.OrdemCompra,
                NrContrato = nrContrato,
                TipoMotivoIDSAP = tipoMotivoIdSap,
                ContatoNome = string.IsNullOrWhiteSpace(vm.NomeContatoExterno) ? null : vm.NomeContatoExterno,
                ContatoEmail = string.IsNullOrWhiteSpace(vm.EmailContatoExterno) ? null : vm.EmailContatoExterno,
                Obs = string.IsNullOrWhiteSpace(vm.Observacoes) ? null : vm.Observacoes,
                UsuarioId = GetUsuarioId(),
                ValorVendaTotal = 0m,
                Frete = frete,
                VlrPedidoMinimo = vlrPedidoMinimo,
            };

            var propostaId = await addService.CriarPropostaAsync(request, cancellationToken);

            return RedirectToAction(nameof(Cotacao), new { propostaId });
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, $"Erro ao criar cotação: {ex.Message}");
            await CarregarLookupsAsync(vm, cancellationToken);
            return View(vm);
        }
    }

    [HttpGet("SearchClientes")]
    public async Task<IActionResult> SearchClientes(string term, int estabelecimentoId, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(term) || term.Length < 2)
            return Json(new { results = Array.Empty<object>() });

        var items = await addService.SearchClientesAsync(term, estabelecimentoId, cancellationToken);
        return Json(new { results = items.Select(i => new { id = i.Id, text = i.Text }) });
    }

    [HttpGet("GetEnderecos")]
    public async Task<IActionResult> GetEnderecos(int clienteId, CancellationToken cancellationToken)
    {
        var items = await addService.GetEnderecosByClienteAsync(clienteId, cancellationToken);
        return Json(items.Select(i => new { id = i.ClienteEnderecoId, text = i.Text }));
    }

    [HttpGet("GetLocaisEntrega")]
    public async Task<IActionResult> GetLocaisEntrega(int clienteEnderecoId, CancellationToken cancellationToken)
    {
        var items = await addService.GetLocaisEntregaByEnderecoAsync(clienteEnderecoId, cancellationToken);
        return Json(items.Select(i => new
        {
            id                  = i.ClienteLocalEntregaId,
            text                = i.Text,
            logradouro          = i.Logradouro,
            cdUF                = i.CdUF,
            cidade              = i.Cidade,
            flagEnderecoDiferente = i.FlagEnderecoDiferente,
            cdControle          = i.CdControle,
            obsLocalEntrega     = i.ObsLocalEntrega,
            tipoOVSAP           = i.TipoOVSAP,
            condPagtoId         = i.CondPagtoId
        }));
    }

    [HttpGet("GetTabelaPreco")]
    public async Task<IActionResult> GetTabelaPreco(int clienteId, CancellationToken cancellationToken)
    {
        var item = await addService.GetTabelaPrecoByClienteAsync(clienteId, cancellationToken);
        if (item is null)
            return Json(new { found = false });

        return Json(new { found = true, id = item.TblPrecoId, text = item.NmTblPreco });
    }

    [HttpGet("GetFormaPagtoCliente")]
    public async Task<IActionResult> GetFormaPagtoCliente(int clienteId, CancellationToken cancellationToken)
    {
        var formaPagtoId = await addService.GetFormaPagamentoByClienteAsync(clienteId, cancellationToken);
        if (formaPagtoId is null)
            return Json(new { found = false });

        return Json(new { found = true, id = formaPagtoId.Value });
    }

    [HttpGet("GetCidadesByUf")]
    public async Task<IActionResult> GetCidadesByUf(string cdUf, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(cdUf))
            return Json(Array.Empty<object>());

        var items = await addService.GetCidadesByUfAsync(cdUf, cancellationToken);
        return Json(items.Select(i => new { id = i.Value, text = i.Text }));
    }

    [HttpGet("GetContratos")]
    public async Task<IActionResult> GetContratos(int clienteId, CancellationToken cancellationToken)
    {
        var items = await addService.GetContratosAsync(clienteId, cancellationToken);
        return Json(items.Select(i => new { id = i.NrContrato, text = i.Text }));
    }

    [HttpGet("GetTiposOrdem")]
    public async Task<IActionResult> GetTiposOrdem(int cotacaoTipoId, CancellationToken cancellationToken)
    {
        var items = await addService.GetTiposOrdemAsync(cotacaoTipoId, GetUsuarioId(), cancellationToken);
        return Json(items.Select(i => new { id = i.Value, text = i.Text }));
    }

    [HttpGet("GetTipoOVSAPByEndereco")]
    public async Task<IActionResult> GetTipoOVSAPByEndereco(int clienteEnderecoId, CancellationToken cancellationToken)
    {
        var tipoOV = await addService.GetTipoOVSAPByEnderecoAsync(clienteEnderecoId, cancellationToken);
        if (tipoOV is null)
            return Json(new { found = false });

        return Json(new { found = true, value = tipoOV });
    }

    [HttpPost("CalcularFrete")]
    public async Task<IActionResult> CalcularFrete(int propostaId, CancellationToken cancellationToken)
    {
        try
        {
            var opcoes = await queryService.CalcularFretePropostaAsync(propostaId, cancellationToken);
            return Json(opcoes);
        }
        catch (Exception ex)
        {
            return Json(new { error = ex.Message });
        }
    }

    [HttpPost("SalvarFrete")]
    public async Task<IActionResult> SalvarFrete(
        int propostaId,
        int transportadoraId,
        decimal valorFrete,
        int prazoTotal,
        CancellationToken cancellationToken)
    {
        try
        {
            await queryService.SalvarFretePropostaAsync(propostaId, transportadoraId, valorFrete, prazoTotal, cancellationToken);
            return Json(new { success = true });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, error = ex.Message });
        }
    }

    [HttpGet("{propostaId:int}/buscar-catalogo")]
    public async Task<IActionResult> BuscarCatalogo(
        int propostaId,
        [FromQuery] string descricao,
        [FromQuery] int clienteId,
        [FromQuery] int tblPrecoId,
        [FromQuery] int estabelecimentoId,
        CancellationToken cancellationToken)
    {
        var items = await apiClient.BuscarCatalogoAsync(
            propostaId,
            descricao,
            clienteId,
            tblPrecoId,
            estabelecimentoId,
            cancellationToken);

        return Json(items, new System.Text.Json.JsonSerializerOptions
        {
            PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase
        });
    }

    [HttpPost("{propostaId:int}/itens/adicionar")]
    public async Task<IActionResult> AdicionarItem(
        int propostaId,
        [FromBody] CotacaoAdicionarItemRequest request,
        CancellationToken cancellationToken)
    {
        var result = await apiClient.AdicionarItemAsync(
            propostaId,
            request.CodItemBR,
            request.DescrItemBR,
            request.TipoCusto,
            request.PrecoItem,
            request.VlrCustoAquisicao,
            request.VlrCustoMedio,
            request.Quantidade,
            request.VlrPrecoMinimo,
            request.VlrTabelaPreco,
            cancellationToken);

        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPost("{propostaId:int}/itens/{propostaItemId:int}/calcular-margem")]
    public async Task<IActionResult> CalcularMargemItem(
        int propostaId,
        int propostaItemId,
        [FromBody] CotacaoCalcularMargemRequest request,
        CancellationToken cancellationToken)
    {
        var result = await apiClient.CalcularMargemItemAsync(
            propostaId,
            propostaItemId,
            request.Type,
            request.ViaTela,
            cancellationToken);

        return result.Success ? Ok(result) : BadRequest(result);
    }

    public sealed record CotacaoAtualizarItemRequest(decimal PrecoUnitario, int Quantidade);

    [HttpPost("{propostaId:int}/itens/{propostaItemId:int}/atualizar")]
    public async Task<IActionResult> AtualizarItem(
        int propostaId,
        int propostaItemId,
        [FromBody] CotacaoAtualizarItemRequest request,
        CancellationToken cancellationToken)
    {
        var result = await apiClient.AtualizarItemAsync(
            propostaId,
            propostaItemId,
            request.PrecoUnitario,
            request.Quantidade,
            cancellationToken);

        return result.Success ? Ok(result) : BadRequest(result);
    }

    public sealed record CotacaoAtualizarCustoItemRequest(string TipoCusto);

    [HttpPost("{propostaId:int}/itens/{propostaItemId:int}/atualizar-custo")]
    public async Task<IActionResult> AtualizarCustoItem(
        int propostaId,
        int propostaItemId,
        [FromBody] CotacaoAtualizarCustoItemRequest request,
        CancellationToken cancellationToken)
    {
        var result = await apiClient.AtualizarCustoItemAsync(
            propostaId,
            propostaItemId,
            request.TipoCusto,
            cancellationToken);

        return result.Success ? Ok(result) : BadRequest(result);
    }

    public sealed record CotacaoGerarItensRequest(string TipoGeracao);

    [HttpPost("{propostaId:int}/gerar-itens")]
    public async Task<IActionResult> GerarItens(
        int propostaId,
        [FromBody] CotacaoGerarItensRequest request,
        CancellationToken cancellationToken)
    {
        var result = await apiClient.GerarItensAsync(
            propostaId,
            request.TipoGeracao,
            GetUsuarioId(),
            cancellationToken);

        return result.Success ? Ok(result) : BadRequest(result);
    }

    public sealed record CotacaoRemoverItensRequest(List<CotacaoRemoverItemInfo> Itens, string Motivo);
    public sealed record CotacaoRemoverItemInfo(int PropostaItemId, string CdItem);

    [HttpPost("{propostaId:int}/itens/remover")]
    public async Task<IActionResult> RemoverItens(
        int propostaId,
        [FromBody] CotacaoRemoverItensRequest request,
        CancellationToken cancellationToken)
    {
        var result = await apiClient.RemoverItensAsync(
            propostaId,
            request.Itens.Select(i => (i.PropostaItemId, i.CdItem)),
            request.Motivo,
            GetUsuarioId(),
            cancellationToken);

        return result.Success ? Ok(result) : BadRequest(result);
    }

    public sealed record CotacaoSalvarCondPagtoRequest(int CondPagtoId);

    [HttpPost("{propostaId:int}/salvar-cond-pagto")]
    public async Task<IActionResult> SalvarCondPagto(
        int propostaId,
        [FromBody] CotacaoSalvarCondPagtoRequest request,
        CancellationToken cancellationToken)
    {
        var result = await apiClient.SalvarCondPagtoAsync(
            propostaId, request.CondPagtoId, cancellationToken);

        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPost("{propostaId:int}/recalcular-margem-bruta")]
    public async Task<IActionResult> RecalcularMargemBruta(
        int propostaId,
        CancellationToken cancellationToken)
    {
        var result = await apiClient.RecalcularMargemBrutaPropostaAsync(propostaId, cancellationToken);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpGet("{propostaId:int}/itens/validar-importacao")]
    public async Task<IActionResult> ValidarItensImportacao(
        int propostaId,
        CancellationToken cancellationToken)
    {
        var result = await queryService.ValidarItensImportacaoAsync(propostaId, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{propostaId:int}/itens/{propostaItemId:int}/impostos")]
    public async Task<IActionResult> GetImpostosItem(
        int propostaId,
        int propostaItemId,
        CancellationToken cancellationToken)
    {
        var result = await queryService.GetImpostosItemAsync(propostaItemId, cancellationToken);
        if (result is null) return NotFound();
        return Ok(result);
    }

    [HttpPost("{propostaId:int}/gerar-pdf")]
    public async Task<IActionResult> GerarPdf(
        int propostaId,
        [FromQuery] bool comFoto,
        [FromQuery] bool comImpostos,
        [FromServices] CotacaoPdfService pdfService,
        CancellationToken cancellationToken)
    {
        var model = await queryService.GetByPropostaIdAsync(propostaId, cancellationToken);
        if (model is null) return NotFound();

        var executivo = await queryService.GetExecutivoVendasAsync(model.ClienteID, cancellationToken);

        byte[] bytes;
        try
        {
            bytes = await pdfService.GerarAsync(model, executivo, comFoto, comImpostos);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Erro ao gerar PDF: {ex.GetType().Name} - {ex.Message}\n\n{ex.StackTrace}");
        }

        var nomeArquivo = $"Proposta_{model.CdProposta}.pdf";
        return File(bytes, "application/pdf", nomeArquivo);
    }

    [HttpPost("{propostaId:int}/gerar-excel")]
    public async Task<IActionResult> GerarExcel(
        int propostaId,
        [FromServices] CotacaoExcelService excelService,
        CancellationToken cancellationToken)
    {
        var model = await queryService.GetByPropostaIdAsync(propostaId, cancellationToken);
        if (model is null) return NotFound();

        try
        {
            var (fileBytes, fileName) = await excelService.GerarExcelAsync(model, cancellationToken);
            return File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Erro ao gerar Excel: {ex.GetType().Name} - {ex.Message}\n\n{ex.StackTrace}");
        }
    }

    [HttpPost("{propostaId:int}/finalizar")]
    public async Task<IActionResult> Finalizar(
        int propostaId,
        [FromBody] CotacaoFinalizarRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var statusId = await queryService.FinalizarAsync(
                propostaId,
                request.DataValidade,
                request.UsuarioID,
                cancellationToken);

            if (statusId is null)
                return BadRequest(new { success = false, error = "Não foi possível finalizar a proposta." });

            return Ok(new { success = true, statusId });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { success = false, error = ex.Message });
        }
    }

    public sealed record CotacaoAprovarRequest(int AprovadorID);

    [HttpPost("{propostaId:int}/aprovar")]
    public async Task<IActionResult> AprovarCotacao(
        int propostaId,
        [FromBody] CotacaoAprovarRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            await queryService.AprovarAsync(propostaId, request.AprovadorID, cancellationToken);
            return Ok(new { success = true });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { success = false, error = ex.Message });
        }
    }

    public sealed record CotacaoReprovarRequest(int AprovadorID, string Justificativa);

    [HttpPost("{propostaId:int}/reprovar")]
    public async Task<IActionResult> ReprovarCotacao(
        int propostaId,
        [FromBody] CotacaoReprovarRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            await queryService.ReprovarAsync(propostaId, request.AprovadorID, request.Justificativa, cancellationToken);
            return Ok(new { success = true });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { success = false, error = ex.Message });
        }
    }

    [HttpPost("{propostaId:int}/autorizar-faturamento")]
    public async Task<IActionResult> AutorizarFaturamento(
        int propostaId,
        CancellationToken cancellationToken)
    {
        try
        {
            var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "0.0.0.0";
            var pedidoId = await queryService.AutorizarFaturamentoAsync(propostaId, ip, cancellationToken);
            return Ok(new { success = true, pedidoId });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { success = false, error = ex.Message });
        }
    }

    private int GetUsuarioId()
    {
        var claim = User.FindFirst("sic_usuarioid")?.Value;
        return int.TryParse(claim, out var id) ? id : 0;
    }

    private async Task CarregarLookupsAsync(CotacaoAddViewModel vm, CancellationToken cancellationToken)
    {
        var tiposTask = addService.GetTiposAsync(GetUsuarioId(), cancellationToken);
        var estabelecimentosTask = addService.GetEstabelecimentosAsync(cancellationToken);
        var condPagtoTask = addService.GetCondicoesPagamentoAsync(cancellationToken);
        var formasPagtoTask = addService.GetFormasPagamentoAsync(cancellationToken);
        var ufsTask = addService.GetUfsAsync(cancellationToken);
        var motivosTask = addService.GetMotivosBonificacaoAsync(GetUsuarioId(), cancellationToken);

        await Task.WhenAll(tiposTask, estabelecimentosTask, condPagtoTask, formasPagtoTask, ufsTask, motivosTask);

        var estabelecimentos = estabelecimentosTask.Result;
        var ufs = ufsTask.Result;
        var ufMap = ufs.ToDictionary(u => u.UfId, u => u.CdUf);

        vm.Tipos = tiposTask.Result;
        vm.Estabelecimentos = estabelecimentos.Select(e => new SelectListItem
        {
            Value = e.EstabelecimentoId.ToString(),
            Text = e.Nome
        }).ToList();
        vm.CondicoesPagamento = condPagtoTask.Result;
        vm.FormasPagamento = formasPagtoTask.Result;
        vm.MotivosBonificacao = motivosTask.Result;

        vm.UfDestinoOptions = ufs.Select(u => new SelectListItem
        {
            Value = u.CdUf,
            Text = u.CdUf
        }).OrderBy(u => u.Text).ToList();

        vm.EstabelecimentoUfMap = estabelecimentos.ToDictionary(
            e => e.EstabelecimentoId.ToString(),
            e => ufMap.GetValueOrDefault(e.UfId, string.Empty));
    }

    [HttpGet("{propostaId:int}/enviar")]
    public async Task<IActionResult> Enviar(int propostaId, CancellationToken cancellationToken)
    {
        if (propostaId <= 0)
            return RedirectToAction(nameof(Index));

        var dados = await queryService.GetEnviarEmailDadosAsync(propostaId, cancellationToken);
        if (dados is null)
        {
            TempData["SwalIcon"]  = "warning";
            TempData["SwalTitle"] = "Não encontrado";
            TempData["SwalText"]  = $"Proposta #{propostaId} não encontrada.";
            return RedirectToAction(nameof(Index));
        }

        // Pré-preenche o e-mail do destinatário com o contato da proposta
        dados.EmailDestinatario = dados.ContatoEmail;

        // Pré-preenche a saudação com base no nome do contato
        var nomeContato = dados.ContatoNome?.Trim().Split(' ').FirstOrDefault() ?? "";
        dados.Saudacao = string.IsNullOrWhiteSpace(nomeContato)
            ? "Prezado(a),"
            : $"Prezado(a) {nomeContato},";

        dados.HistoricoEnvios = await queryService.GetHistoricoEnviosAsync(propostaId, cancellationToken);

        return View("EnviarEmailCotacao", dados);
    }

    [HttpPost("{propostaId:int}/enviar")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Enviar(
        int propostaId,
        EnviarEmailCotacaoViewModel form,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            // Re-popula os dados de exibição que não vêm no POST body
            var dados = await queryService.GetEnviarEmailDadosAsync(propostaId, cancellationToken);
            if (dados is not null)
            {
                form.CdProposta          = dados.CdProposta;
                form.EstabelecimentoNome = dados.EstabelecimentoNome;
                form.ClienteId           = dados.ClienteId;
                form.ClienteNome         = dados.ClienteNome;
                form.ClienteCidadeEstado = dados.ClienteCidadeEstado;
                form.ContatoNome         = dados.ContatoNome;
                form.ContatoEmail        = dados.ContatoEmail;
                form.ConsultorNome       = dados.ConsultorNome;
                form.ExecutivoNome       = dados.ExecutivoNome;
                form.TotalVenda          = dados.TotalVenda;
                form.Frete               = dados.Frete;
            }
            form.HistoricoEnvios = await queryService.GetHistoricoEnviosAsync(propostaId, cancellationToken);
            return View("EnviarEmailCotacao", form);
        }

        try
        {
            await emailService.EnviarAsync(form, GetUsuarioId(), cancellationToken);

            TempData["SwalIcon"]  = "success";
            TempData["SwalTitle"] = "E-mail enviado!";
            TempData["SwalText"]  = $"Cotação enviada para {form.EmailDestinatario} com sucesso.";
            return RedirectToAction(nameof(Cotacao), new { propostaId });
        }
        catch (Exception ex)
        {
            var dados = await queryService.GetEnviarEmailDadosAsync(propostaId, cancellationToken);
            if (dados is not null)
            {
                form.CdProposta          = dados.CdProposta;
                form.EstabelecimentoNome = dados.EstabelecimentoNome;
                form.ClienteId           = dados.ClienteId;
                form.ClienteNome         = dados.ClienteNome;
                form.ClienteCidadeEstado = dados.ClienteCidadeEstado;
                form.ContatoNome         = dados.ContatoNome;
                form.ContatoEmail        = dados.ContatoEmail;
                form.ConsultorNome       = dados.ConsultorNome;
                form.ExecutivoNome       = dados.ExecutivoNome;
                form.TotalVenda          = dados.TotalVenda;
                form.Frete               = dados.Frete;
            }
            form.HistoricoEnvios = await queryService.GetHistoricoEnviosAsync(propostaId, cancellationToken);
            ModelState.AddModelError(string.Empty, $"Erro ao enviar e-mail: {ex.Message}");
            return View("EnviarEmailCotacao", form);
        }
    }
}
