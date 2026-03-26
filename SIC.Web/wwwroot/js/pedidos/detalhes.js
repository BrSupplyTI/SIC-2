document.addEventListener('DOMContentLoaded', function () {

    const cfg = window.pedidoConfig;

    // --- Funções utilitárias compartilhadas ---

    function toggleState(element, show) {
        if (element) element.classList.toggle('d-none', !show);
    }

    function fmtBrl(value) {
        if (value === null || value === undefined) return '—';
        return 'R$ ' + Number(value).toLocaleString('pt-BR', { minimumFractionDigits: 2, maximumFractionDigits: 2 });
    }

    function fmtDec(value) {
        if (value === null || value === undefined) return '—';
        return Number(value).toLocaleString('pt-BR', { minimumFractionDigits: 2, maximumFractionDigits: 2 });
    }

    function fmtPer(value) {
        if (value === null || value === undefined) return '—';
        return Number(value).toLocaleString('pt-BR', { minimumFractionDigits: 2, maximumFractionDigits: 2 }) + '%';
    }

    function fmtDate(value) {
        if (!value) return '—';
        return new Date(value).toLocaleDateString('pt-BR');
    }

    function fmtDiasUteis(value) {
        return value === 1 ? '1 dia útil' : value + ' dias úteis';
    }

    function fmtSimNao(value) {
        return value
            ? '<span class="badge bg-danger">SIM</span>'
            : '<span class="badge bg-success">NÃO</span>';
    }

    function fmtPair(pct, val) {
        return fmtPer(pct) + '<br>' + fmtBrl(val);
    }

    function fmtPairVal(val1, val2) {
        return fmtBrl(val1) + '<br>' + fmtBrl(val2);
    }

    const dtLanguagePtBr = {
        emptyTable: 'Nenhum registro encontrado',
        info: 'Mostrando _START_ a _END_ de _TOTAL_ registros',
        infoEmpty: 'Nenhum registro',
        infoFiltered: '(filtrado de _MAX_ registros)',
        lengthMenu: 'Exibir _MENU_ registros',
        search: 'Buscar:',
        zeroRecords: 'Nenhum registro encontrado',
        paginate: {
            first: 'Primeiro',
            last: 'Último',
            next: 'Próximo',
            previous: 'Anterior'
        }
    };

    // --- Integração SAP (lazy load) ---
    const sapCollapse = document.getElementById('collapseSAP');

    if (sapCollapse) {
        const loader = document.getElementById('sapIntegrationLoader');
        const error = document.getElementById('sapIntegrationError');
        const empty = document.getElementById('sapIntegrationEmpty');
        const tableWrapper = document.getElementById('divTblIntegracaoSAP');
        const tableBody = document.querySelector('#tblIntegracaoSAP tbody');
        const endpoint = cfg.endpoints.integracaoSap;
        let loaded = false;
        let loading = false;

        function createCell(value, className) {
            const cell = document.createElement('td');
            cell.textContent = value || '';
            if (className) cell.className = className;
            return cell;
        }

        async function loadSapIntegration() {
            if (loaded || loading) return;

            loading = true;
            toggleState(loader, true);
            toggleState(error, false);
            toggleState(empty, false);
            toggleState(tableWrapper, false);

            try {
                const response = await fetch(endpoint, {
                    method: 'GET',
                    headers: { 'X-Requested-With': 'XMLHttpRequest' }
                });

                if (!response.ok) throw new Error('Falha ao carregar a integração SAP.');

                const items = await response.json();
                tableBody.innerHTML = '';

                if (!items || items.length === 0) {
                    toggleState(empty, true);
                    loaded = true;
                    return;
                }

                items.forEach(function (item) {
                    const row = document.createElement('tr');
                    row.appendChild(createCell(item.nrPedCli, 'text-center hide-sm'));
                    row.appendChild(createCell(item.ordemVenda, 'text-center'));
                    row.appendChild(createCell(item.remessaSAP, 'text-center hide-lg'));
                    row.appendChild(createCell(item.msgRetorno));
                    row.appendChild(createCell(item.dtHrEnvioSAP, 'text-center hide-lg'));
                    row.appendChild(createCell(item.tipoOVSAP, 'text-center hide-md'));
                    row.appendChild(createCell(item.faturaSAP, 'text-center hide-lg'));
                    row.appendChild(createCell(item.nrNF, 'text-center'));
                    tableBody.appendChild(row);
                });

                toggleState(tableWrapper, true);
                loaded = true;
            }
            catch {
                toggleState(error, true);
            }
            finally {
                toggleState(loader, false);
                loading = false;
            }
        }

        sapCollapse.addEventListener('shown.bs.collapse', loadSapIntegration);
    }

    // --- Análise de Crédito (lazy load) ---
    const creditoCollapse = document.getElementById('collapseCredito');

    if (creditoCollapse) {
        const creditoLoader = document.getElementById('creditoLoader');
        const creditoError = document.getElementById('creditoError');
        const creditoEmpty = document.getElementById('creditoEmpty');
        const creditoContent = document.getElementById('divCreditoContent');
        const creditoEndpoint = cfg.endpoints.analiseCredito;
        let creditoLoaded = false;

        function statusBadge(flagAprovado, label) {
            if (flagAprovado === 0) return '<span class="badge-status badge-status-warning"><i class="fa-solid fa-clock me-1"></i>' + label + '</span>';
            if (flagAprovado === 1) return '<span class="badge-status badge-status-success"><i class="fa-solid fa-check me-1"></i>' + label + '</span>';
            return '<span class="badge-status badge-status-danger"><i class="fa-solid fa-times me-1"></i>' + label + '</span>';
        }

        async function loadAnaliseCredito() {
            if (creditoLoaded) return;

            toggleState(creditoLoader, true);
            toggleState(creditoError, false);
            toggleState(creditoEmpty, false);
            toggleState(creditoContent, false);

            try {
                const response = await fetch(creditoEndpoint, {
                    method: 'GET',
                    headers: { 'X-Requested-With': 'XMLHttpRequest' }
                });

                if (!response.ok) {
                    if (response.status === 404) {
                        toggleState(creditoEmpty, true);
                        creditoLoaded = true;
                        return;
                    }
                    throw new Error();
                }

                const data = await response.json();

                if (!data) {
                    toggleState(creditoEmpty, true);
                    creditoLoaded = true;
                    return;
                }

                creditoContent.innerHTML =
                    '<div class="cad-row">' +
                    '<div class="cad-label text-muted">Status</div>' +
                    '<div class="cad-info">' + statusBadge(data.flagAprovado, data.statusAprovacao || '') + '</div>' +
                    '</div>' +
                    '<div class="cad-row">' +
                    '<div class="cad-label text-muted">Motivo do Bloqueio</div>' +
                    '<div class="cad-info">' + (data.motivoBloqueio || '—') + '</div>' +
                    '</div>' +
                    '<div class="cad-row">' +
                    '<div class="cad-label text-muted">Data / Hora do Bloqueio</div>' +
                    '<div class="cad-info">' + (data.dataHoraBloqueio || '—') + '</div>' +
                    '</div>' +
                    '<div class="cad-row">' +
                    '<div class="cad-label text-muted">Aprovador</div>' +
                    '<div class="cad-info">' + (data.nmUsuario || '—') + '</div>' +
                    '</div>' +
                    '<div class="cad-row">' +
                    '<div class="cad-label text-muted">Data / Hora da Aprovação</div>' +
                    '<div class="cad-info">' + (data.dataHoraAprovacao || '—') + '</div>' +
                    '</div>' +
                    '<div class="cad-row">' +
                    '<div class="cad-label text-muted">Motivo da Aprovação</div>' +
                    '<div class="cad-info">' + (data.motivoAprovacao || '—') + '</div>' +
                    '</div>';

                toggleState(creditoContent, true);
                creditoLoaded = true;
            }
            catch {
                toggleState(creditoError, true);
            }
            finally {
                toggleState(creditoLoader, false);
            }
        }

        creditoCollapse.addEventListener('shown.bs.collapse', loadAnaliseCredito);
    }

    // --- Itens Br Supply (lazy load) ---
    const itensBrCollapse = document.getElementById('collapseItensBR');

    if (itensBrCollapse) {
        const itensBrLoader = document.getElementById('itensBrLoader');
        const itensBrError = document.getElementById('itensBrError');
        const itensBrEmpty = document.getElementById('itensBrEmpty');
        const divTblItensBr = document.getElementById('divTblItensBrSupply');
        const itensBrTbody = document.querySelector('#tblItensBrSupply tbody');
        const itensBrEndpoint = cfg.endpoints.itensBrSupply;
        let itensBrLoaded = false;
        let dtInstanceItens = null;

        async function loadItensBrSupply() {
            if (itensBrLoaded) return;

            toggleState(itensBrLoader, true);
            toggleState(itensBrError, false);
            toggleState(itensBrEmpty, false);
            toggleState(divTblItensBr, false);

            try {
                const response = await fetch(itensBrEndpoint, {
                    method: 'GET',
                    headers: { 'X-Requested-With': 'XMLHttpRequest' }
                });

                if (!response.ok) throw new Error();

                const items = await response.json();

                if (!items || items.length === 0) {
                    toggleState(itensBrEmpty, true);
                    itensBrLoaded = true;
                    return;
                }

                itensBrTbody.innerHTML = '';
                items.forEach(function (item) {
                    const row = document.createElement('tr');
                    var vlrOriginalHtml = '';
                    if (item.vlrOriginal > 0) {
                        vlrOriginalHtml = '<small class="text-muted text-decoration-line-through">' + fmtBrl(item.vlrOriginal) + '</small><br />';
                    } else {
                        vlrOriginalHtml = '';
                    }

                    row.innerHTML =
                        '<td><img class="icone-produto" src="' + (item.foto || '') + '" /></td>' +
                        '<td class="text-center">' + (item.cdItem || '') + '</td>' +
                        '<td>' + (item.nmItem || '') + '</td>' +
                        '<td class="text-center fw-semibold">' + item.qtItem + '</td>' +
                        '<td class="text-end text-nowrap" data-order="' + item.vlrFinal + '">' + vlrOriginalHtml + fmtBrl(item.vlrFinal) + '</td>' +
                        '<td class="text-end text-nowrap fw-semibold" data-order="' + item.vlrTotal + '">' + fmtBrl(item.vlrTotal) + '</td>' +
                        '<td class="text-center">' + (item.ordemCliente || '') + '</td>' +
                        '<td class="text-center" data-order="' + item.situacaoItem + '">' + (item.situacaoItem || '') + '<br /><small class="text-muted">' + (item.dtAlocacao || '') + '</small></td>' +
                        '<td class="text-end" data-order="' + item.margemCalculada + '">' + fmtPer(item.margemCalculada) + '</td>' +
                        '<td class="text-center">' + (item.versao || '') + '</td>';
                    itensBrTbody.appendChild(row);
                });

                toggleState(divTblItensBr, true);

                dtInstanceItens = new DataTable('#tblItensBrSupply', {
                    pageLength: 10,
                    lengthMenu: [[-1, 10, 25, 50, 100, -1], ["Todos", 10, 25, 50, 100]],
                    order: [[1, 'asc']],
                    searching: true,
                    ordering: true,
                    info: true,
                    responsive: { details: false },
                    scrollX: true,
                    layout: {
                        topStart: 'search',
                        topEnd: 'pageLength',
                        bottomStart: 'info',
                        bottomEnd: 'paging'
                    },
                    language: dtLanguagePtBr,
                    columnDefs: [
                        { className: 'text-end text-nowrap', targets: [4, 5, 8] },
                        { className: 'text-center', targets: [3, 7, 9] },
                        { className: 'text-nowrap', targets: [0, 1] }
                    ]
                });

                itensBrLoaded = true;
            }
            catch {
                toggleState(itensBrError, true);
            }
            finally {
                toggleState(itensBrLoader, false);
            }
        }
        itensBrCollapse.addEventListener('shown.bs.collapse', function () {
            loadItensBrSupply();
            if (dtInstanceItens) {
                dtInstanceItens.columns.adjust();
            }
        });
    }

    // --- Itens Marketplace (lazy load) ---
    const itensMarketplaceCollapse = document.getElementById('collapseItensMktplc');

    if (itensMarketplaceCollapse) {
        const itensMarketplaceLoader = document.getElementById('itensMarketplaceLoader');
        const itensMarketplaceError = document.getElementById('itensMarketplaceError');
        const itensMarketplaceEmpty = document.getElementById('itensMarketplaceEmpty');
        const divTblItensMarketplace = document.getElementById('divTblItensMarketplace');
        const itensMarketplaceTbody = document.querySelector('#tblItensMarketplace tbody');
        const itensMarketplaceEndpoint = cfg.endpoints.itensMarketplace;
        let itensMarketplaceLoaded = false;
        let dtInstanceItens = null;

        async function loadItensMarketplace() {
            if (itensMarketplaceLoaded) return;

            toggleState(itensMarketplaceLoader, true);
            toggleState(itensMarketplaceError, false);
            toggleState(itensMarketplaceEmpty, false);
            toggleState(divTblItensMarketplace, false);

            try {
                const response = await fetch(itensMarketplaceEndpoint, {
                    method: 'GET',
                    headers: { 'X-Requested-With': 'XMLHttpRequest' }
                });

                if (!response.ok) throw new Error();

                const items = await response.json();

                if (!items || items.length === 0) {
                    toggleState(itensMarketplaceEmpty, true);
                    itensMarketplaceLoaded = true;
                    return;
                }

                itensMarketplaceTbody.innerHTML = '';
                items.forEach(function (item) {
                    const row = document.createElement('tr');
                    var vlrOriginalHtml = '';
                    if (item.vlrOriginal > 0) {
                        vlrOriginalHtml = '<small class="text-muted text-decoration-line-through">' + fmtBrl(item.vlrOriginal) + '</small><br />';
                    } else {
                        vlrOriginalHtml = '';
                    }

                    row.innerHTML =
                        '<td><img class="icone-produto" src="' + (item.foto || '') + '" /></td>' +
                        '<td class="text-center">' + (item.cdItem || '') + '</td>' +
                        '<td>' + (item.nmItem || '') + '</td>' +
                        '<td>' + (item.nmFornecedor || '') + '</td>' +
                        '<td class="text-center fw-semibold">' + item.qtItem + '</td>' +
                        '<td class="text-end text-nowrap" data-order="' + item.vlrFinal + '">' + vlrOriginalHtml + fmtBrl(item.vlrFinal) + '</td>' +
                        '<td class="text-end text-nowrap fw-semibold" data-order="' + item.vlrTotal + '">' + fmtBrl(item.vlrTotal) + '</td>' +
                        '<td class="text-center">' + (item.ordemCliente || '') + '</td>';
                    itensMarketplaceTbody.appendChild(row);
                });

                toggleState(divTblItensMarketplace, true);

                dtInstanceItens = new DataTable('#tblItensMarketplace', {
                    pageLength: 10,
                    lengthMenu: [[-1, 10, 25, 50, 100, -1], ["Todos", 10, 25, 50, 100]],
                    order: [[1, 'asc']],
                    searching: true,
                    ordering: true,
                    info: true,
                    responsive: { details: false },
                    scrollX: true,
                    layout: {
                        topStart: 'search',
                        topEnd: 'pageLength',
                        bottomStart: 'info',
                        bottomEnd: 'paging'
                    },
                    language: dtLanguagePtBr,
                    columnDefs: [
                        { className: 'text-end text-nowrap', targets: [5, 6] },
                        { className: 'text-center', targets: [4, 7] },
                        { className: 'text-nowrap', targets: [0, 1] }
                    ]
                });

                itensMarketplaceLoaded = true;
            }
            catch {
                toggleState(itensMarketplaceError, true);
            }
            finally {
                toggleState(itensMarketplaceLoader, false);
            }
        }
        itensMarketplaceCollapse.addEventListener('shown.bs.collapse', function () {
            loadItensMarketplace();
            if (dtInstanceItens) {
                dtInstanceItens.columns.adjust();
            }
        });
    }

    // --- Itens em Ruptura (lazy load) ---
    const itensBrRupturaCollapse = document.getElementById('collapseItensRuptura');

    if (itensBrRupturaCollapse) {
        const itensBrRupturaLoader = document.getElementById('itensBrRupturaLoader');
        const itensBrRupturaError = document.getElementById('itensBrRupturaError');
        const itensBrRupturaEmpty = document.getElementById('itensBrRupturaEmpty');
        const divTblItensBrRuptura = document.getElementById('divTblItensBrRuptura');
        const itensBrRupturaTbody = document.querySelector('#tblItensBrRuptura tbody');
        const itensBrRupturaEndpoint = cfg.endpoints.itensBrRuptura;
        let itensBrRupturaLoaded = false;
        let dtInstanceItensRuptura = null;

        async function loadItensBrRuptura() {
            if (itensBrRupturaLoaded) return;

            toggleState(itensBrRupturaLoader, true);
            toggleState(itensBrRupturaError, false);
            toggleState(itensBrRupturaEmpty, false);
            toggleState(divTblItensBrRuptura, false);

            try {
                const response = await fetch(itensBrRupturaEndpoint, {
                    method: 'GET',
                    headers: { 'X-Requested-With': 'XMLHttpRequest' }
                });

                if (!response.ok) throw new Error();

                const items = await response.json();

                if (!items || items.length === 0) {
                    toggleState(itensBrRupturaEmpty, true);
                    itensBrRupturaLoaded = true;
                    return;
                }

                itensBrRupturaTbody.innerHTML = '';
                items.forEach(function (item) {
                    const row = document.createElement('tr');

                    row.innerHTML =
                        '<td><img class="icone-produto" src="' + (item.foto || '') + '" /></td>' +
                        '<td class="text-center">' + (item.cdItem || '') + '</td>' +
                        '<td>' + (item.nmItem || '') + '</td>' +
                        '<td class="text-center fw-semibold">' + item.qtItem + '</td>' +
                        '<td class="text-end text-nowrap" data-order="' + item.vlrTotal + '">' + fmtBrl(item.vlrTotal) + '</td>' +
                        '<td>' + (item.mensagemRuptura || '') + '</td>' +
                        '<td class="text-center fw-semibold">' + item.qtDisponivel + '</td>' +
                        '<td class="text-center"><span class="fw-semibold">' + item.qtItemPrevEntrega + '</span><br />' + (item.dtPrevEntrega || '') + '</td>';

                    itensBrRupturaTbody.appendChild(row);
                });

                toggleState(divTblItensBrRuptura, true);

                dtInstanceItensRuptura = new DataTable('#tblItensBrRuptura', {
                    pageLength: 10,
                    lengthMenu: [[-1, 10, 25, 50, 100, -1], ["Todos", 10, 25, 50, 100]],
                    order: [[1, 'asc']],
                    searching: true,
                    ordering: true,
                    info: true,
                    responsive: { details: false },
                    scrollX: true,
                    layout: {
                        topStart: 'search',
                        topEnd: 'pageLength',
                        bottomStart: 'info',
                        bottomEnd: 'paging'
                    },
                    language: dtLanguagePtBr,
                    columnDefs: [
                        { className: 'text-end text-nowrap', targets: [4] },
                        { className: 'text-center', targets: [3, 6, 7] },
                        { className: 'text-nowrap', targets: [0, 1] }
                    ]
                });

                itensBrRupturaLoaded = true;
            }
            catch {
                toggleState(itensBrRupturaError, true);
            }
            finally {
                toggleState(itensBrRupturaLoader, false);
            }
        }
        itensBrRupturaCollapse.addEventListener('shown.bs.collapse', function () {
            loadItensBrRuptura();
            if (dtInstanceItensRuptura) {
                dtInstanceItensRuptura.columns.adjust();
            }
        });
    }

    // --- Logs de Aprovação (lazy load) ---
    const aprovacaoCollapse = document.getElementById('collapseAprovacao');

    if (aprovacaoCollapse) {
        const aprovacaoLoader = document.getElementById('logsAprovacaoLoader');
        const aprovacaoError = document.getElementById('logsAprovacaoError');
        const aprovacaoEmpty = document.getElementById('logsAprovacaoEmpty');
        const divTblAprovacao = document.getElementById('divTblLogsAprovacao');
        const aprovacaoTbody = document.querySelector('#tblLogsAprovacao tbody');
        const aprovacaoEndpoint = cfg.endpoints.logsAprovacao;
        let aprovacaoLoaded = false;

        function badgeStatus(statusId, label) {
            const map = {
                1: 'badge-status-secondary',
                2: 'badge-status-warning text-dark',
                3: 'badge-status-success',
                4: 'badge-status-danger',
                5: 'badge-status-dark'
            };
            const cls = map[statusId] || 'badge-status-secondary';
            return '<span class="badge-status ' + cls + '">' + label + '</span>';
        }

        async function loadLogsAprovacao() {
            if (aprovacaoLoaded) return;

            toggleState(aprovacaoLoader, true);
            toggleState(aprovacaoError, false);
            toggleState(aprovacaoEmpty, false);
            toggleState(divTblAprovacao, false);

            try {
                const response = await fetch(aprovacaoEndpoint, {
                    method: 'GET',
                    headers: { 'X-Requested-With': 'XMLHttpRequest' }
                });

                if (!response.ok) throw new Error();

                const items = await response.json();

                if (!items || items.length === 0) {
                    toggleState(aprovacaoEmpty, true);
                    aprovacaoLoaded = true;
                    return;
                }

                aprovacaoTbody.innerHTML = '';
                items.forEach(function (item) {
                    const row = document.createElement('tr');
                    row.innerHTML =
                        '<td class="text-center">' + item.nrSequencia + '</td>' +
                        '<td>' + (item.nmUsuario || '') + '</td>' +
                        '<td>' + (item.tipoAlcada || '') + '</td>' +
                        '<td class="text-center">' + badgeStatus(item.statusAlcadaID, item.statusAlcada || '') + '</td>' +
                        '<td class="text-center">' + (item.dtAprovacao || '—') + '</td>';
                    aprovacaoTbody.appendChild(row);
                });

                toggleState(divTblAprovacao, true);
                aprovacaoLoaded = true;
            }
            catch {
                toggleState(aprovacaoError, true);
            }
            finally {
                toggleState(aprovacaoLoader, false);
            }
        }

        aprovacaoCollapse.addEventListener('shown.bs.collapse', loadLogsAprovacao);
    }

    // --- Notas Fiscais (lazy load — cards) ---
    const nfCollapse = document.getElementById('collapseNf');

    if (nfCollapse) {
        const nfLoader = document.getElementById('notasFiscaisLoader');
        const nfError = document.getElementById('notasFiscaisError');
        const nfEmpty = document.getElementById('notasFiscaisEmpty');
        const nfCards = document.getElementById('divNotasFiscaisCards');
        const nfEndpoint = cfg.endpoints.notasFiscais;
        let nfLoaded = false;

        function buildNfCard(item) {
            const statusNF = Number(item.statusNF);
            let statusBadge, borderCls;
            if (statusNF === 0) {
                statusBadge = '<span class="badge-status badge-status-danger"><i class="fa-solid fa-ban me-1"></i>Cancelada</span>';
                borderCls = 'border border-danger';
            } else if (statusNF === 9) {
                statusBadge = '<span class="badge-status badge-status-warning"><i class="fa-solid fa-triangle-exclamation me-1"></i>Ocorrência</span>';
                borderCls = 'border border-warning';
            } else {
                statusBadge = '<span class="badge-status badge-status-success"><i class="fa-solid fa-check me-1"></i>Válida</span>';
                borderCls = 'border';
            }

            let cancelHtml = '';
            if ((statusNF === 0 || statusNF === 9) && item.motivoCancelamento) {
                cancelHtml = '<div class="alert alert-danger py-1 px-2 mb-2 small mb-3">' +
                    '<i class="fa-solid fa-triangle-exclamation me-1"></i>' + item.motivoCancelamento + '</div>';
            }

            let atestoHtml = '';
            if (item.tipoAtestoID === 1) {
                atestoHtml = '<div class="row g-2">' +
                    '<div class="col-12 me-4">' +
                    '<small class="text-muted d-block">Confirmação de Recebimento (Atesto)</small>' +
                    '<span class="small"><i class="fa-solid fa-check me-1 text-success"></i>' + item.dsAtestoRecebimento + '</span>' +
                    '</div>' +
                    '</div>';
            } else if (item.tipoAtestoID === 2) {
                atestoHtml = '<div class="row g-2">' +
                    '<div class="col-12 me-4">' +
                    '<small class="text-muted d-block">Confirmação de Recebimento (Atesto)</small>' +
                    '<span class="small"><i class="fa-solid fa-ban me-1 text-warning"></i>' + item.dsAtestoRecebimento + '</span>' +
                    '</div>' +
                    '</div>';
            } else if (item.tipoAtestoID === 3) {
                atestoHtml = '<div class="row g-2">' +
                    '<div class="col-12 me-4">' +
                    '<small class="text-muted d-block">Confirmação de Recebimento (Atesto)</small>' +
                    '<span class="small"><i class="fa-solid fa-xmark me-1 text-danger"></i>' + item.dsAtestoRecebimento + '</span>' +
                    '</div>' +
                    '</div>';
            }

            const dtEmissaoFmt = item.dtEmissao ? new Date(item.dtEmissao).toISOString().slice(0, 10) : '';
            const pdfUrl = 'https://www.supplymanager.com.br/danfe/' + dtEmissaoFmt + '/' + item.chave + '-nfe.pdf';

            return '<div class="col-12">' +
                '<div class="' + borderCls + ' rounded-3 bg-body-tertiary overflow-hidden h-100 d-flex flex-column">' +
                '<div class="d-flex justify-content-between align-items-center p-3 border-bottom">' +
                '<div class="row d-flex align-items-center gap-2">' +
                '<div class="col-auto me-4 pe-4 border-end">' +
                '<small class="text-muted d-block mb-1"><i class="fa-duotone fa-file-invoice text-br me-1"></i>Nota Fiscal / Série</small>' +
                '<span class="fw-bold fs-5">' + (item.nrNotaFiscal || '—') + ' - ' + (item.serie || '—') + '</span>' +
                '</div>' +
                '<div class="col me-4">' +
                '<small class="text-muted d-block"><i class="fa-solid fa-calendar me-1"></i>Emissão</small>' +
                '<span class="fw-semibold">' + fmtDate(item.dtEmissao) + '</span>' +
                '</div>' +
                '<div class="col-auto me-4 hide-sm">' +
                '<small class="text-muted d-block mb-1"><i class="fa-solid fa-key me-1"></i>Chave Danfe</small>' +
                '<span class="font-monospace text-break user-select-all">' + item.chave + '</span>' +
                '</div>' +
                '</div>' +
                statusBadge +
                '</div>' +
                '<div class="p-3 flex-grow-1">' +
                cancelHtml +
                '<div class="row g-2 mb-3">' +
                '<div class="col-auto me-4">' +
                '<small class="text-muted d-block">Operação</small>' +
                '<span>' + (item.operacao || '—') + '</span>' +
                '</div>' +
                '<div class="col me-4">' +
                '<small class="text-muted d-block">Volumes</small>' +
                '<span class="fw-semibold">' + (item.qtdeVolumes ?? '—') + '</span>' +
                '</div>' +
                '<div class="col me-4">' +
                '<small class="text-muted d-block">Peso (kg)</small>' +
                '<span>' + fmtDec(item.pesoBruto) + '</span>' +
                '</div>' +
                '<div class="col-auto me-4">' +
                '<small class="text-muted d-block">Cub. (m³)</small>' +
                '<span>' + (item.cubagemNF || '—') + '</span>' +
                '</div>' +
                '</div>' +
                '<div class="row g-2 mb-3">' +
                '<div class="col me-4 hide-sm">' +
                '<small class="text-muted d-block">CNPJ Emitente</small>' +
                '<span class="small">' + (item.emitCNPJ || '—') + '</span>' +
                '</div>' +
                '<div class="col me-4">' +
                '<small class="text-muted d-block">Versão</small>' +
                '<span>' + (item.versao || '—') + '</span>' +
                '</div>' +
                '<div class="col me-4 justify-content-end text-end border-start ps-3">' +
                '<small class="text-muted d-block">Valor Total</small>' +
                '<span class="fw-bold text-br fs-6">' + fmtBrl(item.vlrTotalNF) + '</span>' +
                '</div>' +
                '</div>' +
                atestoHtml +
                '</div>' +
                '<div class="d-flex justify-content-end align-items-center px-3 py-2 border-top">' +
                '<a href="' + pdfUrl + '" target="_blank" class="btn btn-sm btn-outline-primary me-2"><i class="fa-solid fa-file-invoice me-2"></i> Visualizar PDF </a>' +
                '<a href="#" class="btn btn-sm btn-outline-info me-2" data-chave="' + item.chave + '" id="btnDownloadXML"><i class="fa-solid fa-file-xml me-2"></i> Download do XML </a>' +
                '<a href="#" class="btn btn-sm btn-outline-secondary me-2 btnVisualizarVolumes" data-pedcli="' + item.versao + '-' + cfg.numeroPedido + '" id="btnVisualizarVolumes"><i class="fa-solid fa-box-open me-2"></i> Volumes </a>' +
                '</div>' +
                '</div>' +
                '</div>';
        }

        async function loadNotasFiscais() {
            if (nfLoaded) return;

            toggleState(nfLoader, true);
            toggleState(nfError, false);
            toggleState(nfEmpty, false);
            toggleState(nfCards, false);

            try {
                const response = await fetch(nfEndpoint, {
                    method: 'GET',
                    headers: { 'X-Requested-With': 'XMLHttpRequest' }
                });

                if (!response.ok) throw new Error();

                const items = await response.json();

                if (!items || items.length === 0) {
                    toggleState(nfEmpty, true);
                    nfLoaded = true;
                    return;
                }

                nfCards.innerHTML = items.map(buildNfCard).join('');
                toggleState(nfCards, true);
                nfLoaded = true;
            }
            catch {
                toggleState(nfError, true);
            }
            finally {
                toggleState(nfLoader, false);
            }
        }

        nfCollapse.addEventListener('shown.bs.collapse', loadNotasFiscais);

        nfCards.addEventListener('click', function (e) {
            const btn = e.target.closest('#btnDownloadXML');
            if (!btn) return;
            e.preventDefault();
            const chave = btn.dataset.chave;
            if (chave) {
                window.location.href = cfg.endpoints.downloadXml.replace('__CHAVE__', chave);
            }
        });

        // --- Modal Volumes Coleta ---
        const volumesModalEl = document.getElementById('modalVolumesColeta');
        const volumesModal = new bootstrap.Modal(volumesModalEl);
        const volumesLoader = document.getElementById('volumesColetaLoader');
        const volumesError = document.getElementById('volumesColetaError');
        const volumesEmpty = document.getElementById('volumesColetaEmpty');
        const divTblVolumes = document.getElementById('divTblVolumesColeta');
        const volumesTbody = document.querySelector('#tblVolumesColeta tbody');
        const volumesTitle = document.getElementById('modalVolumesColetaLabel');
        let dtInstanceVolumes = null;

        nfCards.addEventListener('click', async function (e) {
            const btn = e.target.closest('.btnVisualizarVolumes');
            if (!btn) return;
            e.preventDefault();

            const pedCli = btn.dataset.pedcli;
            if (!pedCli) return;

            const nfLabel = btn.closest('.col-12')?.querySelector('.fw-bold.fs-5')?.textContent || '';
            volumesTitle.innerHTML = '<i class="fa-duotone fa-box-open me-2 text-br"></i>Volumes Coletados — Nota Fiscal ' + nfLabel;

            volumesModal.show();

            if (dtInstanceVolumes) {
                dtInstanceVolumes.destroy();
                dtInstanceVolumes = null;
            }
            volumesTbody.innerHTML = '';

            toggleState(volumesLoader, true);
            toggleState(volumesError, false);
            toggleState(volumesEmpty, false);
            toggleState(divTblVolumes, false);

            try {
                const endpoint = cfg.endpoints.volumesColeta + '?pedCli=' + encodeURIComponent(pedCli);
                const response = await fetch(endpoint, {
                    method: 'GET',
                    headers: { 'X-Requested-With': 'XMLHttpRequest' }
                });

                if (!response.ok) throw new Error();

                const items = await response.json();

                if (!items || items.length === 0) {
                    toggleState(volumesEmpty, true);
                    return;
                }

                items.forEach(function (item) {
                    const row = document.createElement('tr');
                    row.innerHTML =
                        '<td>' + (item.cdItem || '') + '</td>' +
                        '<td>' + (item.nmItem || '') + '</td>' +
                        '<td class="text-center">' + item.qtSolicitada + ' / ' + item.qtColetada + '</td>' +
                        '<td class="text-center">' + (item.dataColeta || '') + '</td>' +
                        '<td>' + (item.nmOperador || '') + '</td>' +
                        '<td class="text-center">' + item.numVol + '</td>' +
                        '<td class="text-center">' + (item.volume || '') + '<br /><small class="text-muted">' + (item.enderecoAtual || '') + '</small></td>' +
                        '<td class="text-center">' + (item.dtLeituraRomaneio || '') + '</td>' +
                        '<td>' + (item.obsCarga || '') + '</td>';
                    volumesTbody.appendChild(row);
                });

                toggleState(divTblVolumes, true);

                dtInstanceVolumes = new DataTable('#tblVolumesColeta', {
                    pageLength: 10,
                    lengthMenu: [[10, 25, 50, 100, -1], [10, 25, 50, 100, "Todos"]],
                    order: [[0, 'asc']],
                    searching: true,
                    ordering: true,
                    info: true,
                    responsive: { details: false },
                    scrollX: true,
                    layout: {
                        topStart: 'search',
                        topEnd: 'pageLength',
                        bottomStart: 'info',
                        bottomEnd: 'paging'
                    },
                    language: dtLanguagePtBr,
                    columnDefs: [
                        { className: 'text-center', targets: [2, 3, 5, 6, 7] }
                    ]
                });
            }
            catch {
                toggleState(volumesError, true);
            }
            finally {
                toggleState(volumesLoader, false);
            }
        });

        volumesModalEl.addEventListener('shown.bs.modal', function () {
            if (dtInstanceVolumes) {
                dtInstanceVolumes.columns.adjust();
            }
        });
    }

    // --- Romaneios (lazy load — cards) ---
    const romaneioCollapse = document.getElementById('collapseRomaneio');

    if (romaneioCollapse) {
        const romaneioLoader = document.getElementById('romaneiosLoader');
        const romaneioError = document.getElementById('romaneiosError');
        const romaneioEmpty = document.getElementById('romaneiosEmpty');
        const romaneioCards = document.getElementById('divRomaneiosCards');
        const romaneioEndpoint = cfg.endpoints.romaneios;
        let romaneioLoaded = false;

        function buildRomaneioCard(item) {
            let entregaHtml = '';
            if (item.dtEntrega) {
                entregaHtml =
                    '<div class="col-auto me-4">' +
                    '<small class="text-muted d-block"><i class="fa-solid fa-flag-checkered me-1"></i>Entrega</small>' +
                    '<span class="fw-semibold text-success">' + item.dtEntrega + '</span>' +
                    '</div>';
            }

            let recebedorHtml = '';
            if (item.nmRecebedor) {
                if (item.nmRecebedor === 'EXTRAVIO TOTAL' || item.nmRecebedor === 'Retorno para Reembarque') {
                    recebedorHtml =
                        '<div class="col me-4">' +
                        '<small class="text-muted d-block">Recebedor</small>' +
                        '<span class="text-danger fw-semibold"><i class="fa-solid fa-exclamation-triangle me-2"></i>' + item.nmRecebedor + '</span>' +
                        '</div>';
                } else {
                    recebedorHtml =
                        '<div class="col me-4">' +
                        '<small class="text-muted d-block">Recebedor</small>' +
                        '<span>' + item.nmRecebedor + '</span>' +
                        '</div>';
                }
            }

            let hubHtml = '';
            if (item.nmHub) {
                hubHtml =
                    '<div class="col-auto me-4">' +
                    '<small class="text-muted d-block">Hub</small>' +
                    '<span>' + item.nmHub + '</span>' +
                    '</div>';
            }

            let comprovanteHtml = '';
            if (item.flagTemComprovante === 1) {
                comprovanteHtml = '<div class="d-flex justify-content-end align-items-center px-3 py-2 border-top">' +
                    '<a href="https://www.supplymanager.com.br/comprovantes/' + item.nmArquivoComprovante + '" target="_blank" class="btn btn-sm btn-outline-primary me-2"><i class="fa-solid fa-stamp me-2"></i> Comprovante de Entrega </a>' +
                    '</div>';
            }

            let statusBadge, borderCls;
            if (item.situacaoRomaneio === 'Em Embarque') {
                statusBadge = '<span class="badge-status badge-status-info"><i class="fa-solid fa-forklift me-1"></i>Em Embarque</span>';
                borderCls = 'border border-info';
            } else if (item.situacaoRomaneio === 'Em Rota') {
                statusBadge = '<span class="badge-status badge-status-primary"><i class="fa-solid fa-truck-fast me-1"></i>Em Rota</span>';
                borderCls = 'border border-primary';
            } else if (item.situacaoRomaneio === 'Com Ocorrência') {
                statusBadge = '<span class="badge-status badge-status-warning"><i class="fa-solid fa-triangle-exclamation me-1"></i>Com Ocorrência</span>';
                borderCls = 'border border-warning';
            } else {
                statusBadge = '<span class="badge-status badge-status-success"><i class="fa-solid fa-check me-1"></i>Entregue</span>';
                statusBadge = '<span class="badge-status badge-status-success"><i class="fa-solid fa-check me-1"></i>Entregue</span>';
                borderCls = 'border';
            }

            return '<div class="col-12">' +
                '<div class="' + borderCls + ' border rounded-3 bg-body-tertiary overflow-hidden h-100 d-flex flex-column">' +
                '<div class="d-flex justify-content-between align-items-center p-3 border-bottom">' +
                '<div class="row d-flex align-items-center gap-2">' +
                '<div class="col-auto me-4 pe-4 border-end">' +
                '<small class="text-muted d-block mb-1"><i class="fa-duotone fa-forklift text-br me-1"></i>Romaneio</small>' +
                '<span class="fw-bold fs-5">' + (item.romaneioID || '—') + '</span>' +
                '</div>' +
                '<div class="col-auto me-4 pe-4">' +
                '<small class="text-muted d-block"><i class="fa-solid fa-file-invoice me-1"></i>NF / Série</small>' +
                '<span class="fw-semibold">' + (item.nrNotaFiscal || '—') + ' - ' + (item.serie || '—') + '</span>' +
                '</div>' +
                '<div class="col-auto me-4 ps-4 border-start">' +
                '<small class="text-muted d-block">Tipo</small>' +
                '<span class="">' + (item.nmTipoRomaneio || '—') + '</span>' +
                '</div>' +
                hubHtml +
                '</div>' +
                statusBadge +
                '</div>' +
                '<div class="p-3 flex-grow-1">' +
                '<div class="row g-2 mb-3">' +
                '<div class="col-auto me-4">' +
                '<small class="text-muted d-block">Transportadora</small>' +
                '<span class="fw-semibold">' + (item.transportadora || '—') + '</span>' +
                '</div>' +
                '<div class="col-auto me-4">' +
                '<small class="text-muted d-block">Estabelecimento</small>' +
                '<span>' + (item.nmCurto || '') + '</span>' +
                '</div>' +
                '</div>' +
                '<div class="row g-2">' +
                '<div class="col-auto me-4">' +
                '<small class="text-muted d-block"><i class="fa-solid fa-calendar-arrow-up me-1"></i>Despacho</small>' +
                '<span>' + (item.dtPortaria || '—') + '</span>' +
                '</div>' +
                entregaHtml +
                recebedorHtml +
                '</div>' +
                '</div>' +
                comprovanteHtml +
                '</div>' +
                '</div>';
        }

        async function loadRomaneios() {
            if (romaneioLoaded) return;

            toggleState(romaneioLoader, true);
            toggleState(romaneioError, false);
            toggleState(romaneioEmpty, false);
            toggleState(romaneioCards, false);

            try {
                const response = await fetch(romaneioEndpoint, {
                    method: 'GET',
                    headers: { 'X-Requested-With': 'XMLHttpRequest' }
                });

                if (!response.ok) throw new Error();

                const items = await response.json();

                if (!items || items.length === 0) {
                    toggleState(romaneioEmpty, true);
                    romaneioLoaded = true;
                    return;
                }

                romaneioCards.innerHTML = items.map(buildRomaneioCard).join('');
                toggleState(romaneioCards, true);
                romaneioLoaded = true;
            }
            catch {
                toggleState(romaneioError, true);
            }
            finally {
                toggleState(romaneioLoader, false);
            }
        }

        romaneioCollapse.addEventListener('shown.bs.collapse', loadRomaneios);
    }

    // --- Logs de Tracking (lazy load) ---
    const trackingCollapse = document.getElementById('collapseTracking');

    if (trackingCollapse) {
        const trackingLoader = document.getElementById('logsTrackingLoader');
        const trackingError = document.getElementById('logsTrackingError');
        const trackingEmpty = document.getElementById('logsTrackingEmpty');
        const divTblTracking = document.getElementById('divTblLogsTracking');
        const trackingTbody = document.querySelector('#tblLogsTracking tbody');
        const trackingEndpoint = cfg.endpoints.logsTracking;
        let trackingLoaded = false;
        let dtInstanceTracking = null;

        async function loadLogsTracking() {
            if (trackingLoaded) return;

            toggleState(trackingLoader, true);
            toggleState(trackingError, false);
            toggleState(trackingEmpty, false);
            toggleState(divTblTracking, false);

            try {
                const response = await fetch(trackingEndpoint, {
                    method: 'GET',
                    headers: { 'X-Requested-With': 'XMLHttpRequest' }
                });

                if (!response.ok) throw new Error();

                const items = await response.json();

                if (!items || items.length === 0) {
                    toggleState(trackingEmpty, true);
                    trackingLoaded = true;
                    return;
                }

                trackingTbody.innerHTML = '';
                items.forEach(function (item) {
                    const row = document.createElement('tr');
                    row.innerHTML =
                        '<td class="text-center text-nowrap">' + (item.dtEvento || '—') + '</td>' +
                        '<td><span class="fw-semibold">' + (item.evento || '') + '</span><br /><span class="text-muted">' + (item.detalhes || '') + '</span></td>' +
                        '<td>' + (item.usuario || '') + '</td>';
                    trackingTbody.appendChild(row);
                });

                toggleState(divTblTracking, true);

                dtInstanceTracking = new DataTable('#tblLogsTracking', {
                    pageLength: 10,
                    lengthMenu: [[10, 25, 50, 100, -1], [10, 25, 50, 100, "Todos"]],
                    order: [[0, 'desc']],
                    searching: true,
                    ordering: true,
                    info: true,
                    responsive: { details: false },
                    scrollX: true,
                    layout: {
                        topStart: 'search',
                        topEnd: 'pageLength',
                        bottomStart: 'info',
                        bottomEnd: 'paging'
                    },
                    language: dtLanguagePtBr,
                    columnDefs: [
                        { className: 'text-center text-nowrap', targets: [0] }
                    ]
                });

                trackingLoaded = true;
            }
            catch {
                toggleState(trackingError, true);
            }
            finally {
                toggleState(trackingLoader, false);
            }
        }

        trackingCollapse.addEventListener('shown.bs.collapse', function () {
            loadLogsTracking();
            if (dtInstanceTracking) {
                dtInstanceTracking.columns.adjust();
            }
        });
    }

    // --- Chamados (lazy load — cards) ---
    const chamadosCollapse = document.getElementById('collapseChamados');

    if (chamadosCollapse) {
        const chamadosLoader = document.getElementById('chamadosLoader');
        const chamadosError = document.getElementById('chamadosError');
        const chamadosEmpty = document.getElementById('chamadosEmpty');
        const chamadosCards = document.getElementById('divChamadosCards');
        const chamadosEndpoint = cfg.endpoints.chamados;
        let chamadosLoaded = false;

        function buildChamadoCard(item) {
            let statusBadge, borderCls;
            const sit = (item.situacao || '').toLowerCase();
            if (sit === 'encerrado') {
                statusBadge = '<span class="badge-status badge-status-success"><i class="fa-solid fa-check me-1"></i>' + item.situacao + '</span>';
                borderCls = 'border';
            } else if (sit === 'cancelado') {
                statusBadge = '<span class="badge-status badge-status-danger"><i class="fa-solid fa-ban me-1"></i>' + item.situacao + '</span>';
                borderCls = 'border border-danger';
            } else {
                statusBadge = '<span class="badge-status badge-status-primary"><i class="fa-solid fa-headset me-1"></i>' + item.situacao + '</span>';
                borderCls = 'border border-primary';
            }

            let atrasoHtml = '';
            if (item.atraso) {
                atrasoHtml = '<span class="badge bg-danger ms-2"><i class="fa-solid fa-clock me-1"></i>' + item.atraso + '</span>';
            }

            return '<div class="col-12">' +
                '<div class="' + borderCls + ' rounded-3 bg-body-tertiary overflow-hidden h-100 d-flex flex-column">' +
                '<div class="d-flex justify-content-between align-items-center p-3 border-bottom">' +
                '<div class="row d-flex align-items-center gap-2">' +
                '<div class="col-auto me-4 pe-4 border-end">' +
                '<small class="text-muted d-block mb-1"><i class="fa-duotone fa-user-headset text-br me-1"></i>Protocolo</small>' +
                '<span class="fw-bold fs-5">' + item.protocolo + '</span>' +
                '</div>' +
                '<div class="col-auto me-4">' +
                '<small class="text-muted d-block">Origem</small>' +
                '<span class="fw-semibold">' + (item.origem || '—') + '</span>' +
                '<span class="text-muted ms-1">(' + (item.origemValor || '') + ')</span>' +
                '</div>' +
                '<div class="col-auto me-4 hide-sm">' +
                '<small class="text-muted d-block">Prazo de Resolução</small>' +
                '<span class="fw-semibold">' + (item.prazoResolucao || '—') + '</span>' +
                atrasoHtml +
                '</div>' +
                '</div>' +
                statusBadge +
                '</div>' +
                '<div class="p-3 flex-grow-1">' +
                '<div class="row g-2 mb-3">' +
                '<div class="col-auto me-4">' +
                '<small class="text-muted d-block">Área</small>' +
                '<span class="fw-semibold">' + (item.nmArea || '—') + '</span>' +
                '</div>' +
                '<div class="col-auto me-4">' +
                '<small class="text-muted d-block">Nível</small>' +
                '<span>' + (item.nmNivel || '—') + '</span>' +
                '</div>' +
                '<div class="col-auto me-4">' +
                '<small class="text-muted d-block">Problema</small>' +
                '<span>' + (item.nmProblema || '—') + '</span>' +
                '</div>' +
                '</div>' +
                '<div class="row g-2">' +
                '<div class="col-auto me-4">' +
                '<small class="text-muted d-block">Solicitante</small>' +
                '<span>' + (item.nmSolicitante || '—') + '</span>' +
                '</div>' +
                '<div class="col-auto me-4 hide-sm">' +
                '<small class="text-muted d-block">E-mail</small>' +
                '<span class="text-lowercase">' + (item.emailSolicitante || '—') + '</span>' +
                '</div>' +
                '<div class="col-auto me-4">' +
                '<small class="text-muted d-block"><i class="fa-solid fa-calendar-arrow-up me-1"></i>Abertura</small>' +
                '<span>' + (item.dtHrAbertura || '—') + '</span>' +
                '</div>' +
                '<div class="col-auto me-4">' +
                '<small class="text-muted d-block"><i class="fa-solid fa-calendar-check me-1"></i>Encerramento</small>' +
                '<span>' + (item.dtHrEncerramento || '—') + '</span>' +
                '</div>' +
                '</div>' +
                '</div>' +
                '<div class="d-flex justify-content-end align-items-center px-3 py-2 border-top">' +
                '<a href="https://intranet.brsupply.com.br/Intranet/html/chamado.php?id=' + item.protocolo + '" target="_blank" class="btn btn-sm btn-outline-primary me-2"><i class="fa-solid fa-search me-2"></i> Ver Chamado </a>' +
                '</div>' +
                '</div>' +
                '</div>';
        }

        async function loadChamados() {
            if (chamadosLoaded) return;

            toggleState(chamadosLoader, true);
            toggleState(chamadosError, false);
            toggleState(chamadosEmpty, false);
            toggleState(chamadosCards, false);

            try {
                const response = await fetch(chamadosEndpoint, {
                    method: 'GET',
                    headers: { 'X-Requested-With': 'XMLHttpRequest' }
                });

                if (!response.ok) throw new Error();

                const items = await response.json();

                if (!items || items.length === 0) {
                    toggleState(chamadosEmpty, true);
                    chamadosLoaded = true;
                    return;
                }

                chamadosCards.innerHTML = items.map(buildChamadoCard).join('');
                toggleState(chamadosCards, true);
                chamadosLoaded = true;
            }
            catch {
                toggleState(chamadosError, true);
            }
            finally {
                toggleState(chamadosLoader, false);
            }
        }

        chamadosCollapse.addEventListener('shown.bs.collapse', loadChamados);
    }

    // --- Modal Impostos ---
    const btnImpostos = document.getElementById('BtnVerImpostos');
    if (btnImpostos) {
        const impostosEndpoint = cfg.endpoints.impostos;
        const modalEl = document.getElementById('modalImpostos');
        const modal = new bootstrap.Modal(modalEl);
        const impostosLoader = document.getElementById('impostosLoader');
        const impostosError = document.getElementById('impostosError');
        const impostosEmpty = document.getElementById('impostosEmpty');
        const divTblImpostos = document.getElementById('divTblImpostos');
        let impostosLoaded = false;
        let dtInstance = null;

        btnImpostos.addEventListener('click', async function (e) {
            e.preventDefault();
            modal.show();

            if (impostosLoaded) return;

            toggleState(impostosLoader, true);
            toggleState(impostosError, false);
            toggleState(impostosEmpty, false);
            toggleState(divTblImpostos, false);

            try {
                const response = await fetch(impostosEndpoint, {
                    method: 'GET',
                    headers: { 'X-Requested-With': 'XMLHttpRequest' }
                });

                if (!response.ok) throw new Error();

                const items = await response.json();

                if (!items || items.length === 0) {
                    toggleState(impostosEmpty, true);
                    impostosLoaded = true;
                    return;
                }

                const rows = items.map(function (i) {
                    return [
                        i.itemDocumentoSAP || '',
                        i.cdItem || '',
                        fmtPer(i.margemCalculada),
                        fmtPair(i.percentualICMS, i.valorICMS),
                        fmtPair(i.percentualFCP, i.valorFundoCombPobreza),
                        fmtPair(i.percentualIPI, i.valorIPI),
                        fmtPair(i.percentualCOFINS, i.valorCOFINS),
                        fmtPair(i.percentualPIS, i.valorPIS),
                        fmtBrl(i.valorICMSPartilhaOrigem),
                        fmtBrl(i.valorICMSPartilhaDestino),
                        fmtBrl(i.valorST),
                        fmtDec(i.valorFCPST),
                        fmtPairVal(i.rol, i.lb),
                        fmtPairVal(i.vlrUnitario, i.vlrTotalNF)
                    ];
                });

                toggleState(divTblImpostos, true);

                dtInstance = new DataTable('#tblImpostos', {
                    data: rows,
                    pageLength: 10,
                    lengthMenu: [[10, 25, 50, 100, -1], [10, 25, 50, 100, "Todos"]],
                    order: [[0, 'asc']],
                    searching: true,
                    ordering: true,
                    info: true,
                    responsive: { details: false },
                    scrollX: true,
                    layout: {
                        topStart: 'search',
                        topEnd: 'pageLength',
                        bottomStart: 'info',
                        bottomEnd: 'paging'
                    },
                    language: dtLanguagePtBr,
                    columnDefs: [
                        { className: 'text-end text-nowrap', targets: [2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13] },
                        { className: 'text-nowrap', targets: [0, 1] }
                    ]
                });

                impostosLoaded = true;
            }
            catch {
                toggleState(impostosError, true);
            }
            finally {
                toggleState(impostosLoader, false);
            }
        });

        modalEl.addEventListener('shown.bs.modal', function () {
            if (dtInstance) {
                dtInstance.columns.adjust();
            }
        });
    }

    // --- Modal Histórico Frete ---
    const btnHistoricoFrete = document.getElementById('BtnHistoricoFrete');
    if (btnHistoricoFrete) {
        const freteEndpoint = cfg.endpoints.historicoFrete;
        const freteModalEl = document.getElementById('modalHistoricoFrete');
        const freteModal = new bootstrap.Modal(freteModalEl);
        const freteLoader = document.getElementById('historicoFreteLoader');
        const freteError = document.getElementById('historicoFreteError');
        const freteEmpty = document.getElementById('historicoFreteEmpty');
        const divTblFrete = document.getElementById('divTblHistoricoFrete');
        const freteTbody = document.querySelector('#tblHistoricoFrete tbody');
        let freteLoaded = false;

        btnHistoricoFrete.addEventListener('click', async function (e) {
            e.preventDefault();
            freteModal.show();

            if (freteLoaded) return;

            toggleState(freteLoader, true);
            toggleState(freteError, false);
            toggleState(freteEmpty, false);
            toggleState(divTblFrete, false);

            try {
                const response = await fetch(freteEndpoint, {
                    method: 'GET',
                    headers: { 'X-Requested-With': 'XMLHttpRequest' }
                });

                if (!response.ok) throw new Error();

                const items = await response.json();

                if (!items || items.length === 0) {
                    toggleState(freteEmpty, true);
                    freteLoaded = true;
                    return;
                }

                freteTbody.innerHTML = '';
                items.forEach(function (item) {
                    const row = document.createElement('tr');
                    var vItensRestritos = 0;
                    var vtaxaExtra = 0;
                    if (item.qtItensRestritos > 0) {
                        vItensRestritos = '<span class="badge bg-danger fs-6">' + item.qtItensRestritos + '</span>';
                    } else {
                        vItensRestritos = '<span class="">0</span>';
                    }
                    if (item.taxaExtra > 0) {
                        vtaxaExtra = '<span class="text-danger fw-bold">' + fmtBrl(item.taxaExtra) + '</span>';
                    } else {
                        vtaxaExtra = '<span class="">' + fmtBrl(item.taxaExtra) + '</span>';
                    }

                    row.innerHTML =
                        '<td class="text-center hide-lg">' + item.transportadoraID + '</td>' +
                        '<td>' + (item.nomeTransportadora || '') + '</td>' +
                        '<td class="text-center hide-sm"">' + fmtDiasUteis(item.prazoLogistico) + '</td>' +
                        '<td class="text-center">' + fmtDiasUteis(item.prazoComercial) + '</td>' +
                        '<td class="text-end">' + fmtBrl(item.valorFrete) + '</td>' +
                        '<td class="text-end">' + vtaxaExtra + '</td>' +
                        '<td class="text-center">' + vItensRestritos + '</td>' +
                        '<td class="text-center hide-md">' + fmtSimNao(item.clienteRestrito) + '</td>' +
                        '<td class="text-center hide-sm">' + fmtSimNao(item.clienteFixo) + '</td>' +
                        '<td class="text-center hide-md">' + fmtSimNao(item.obrigatoriaCanalVenda) + '</td>';
                    freteTbody.appendChild(row);
                });

                toggleState(divTblFrete, true);
                freteLoaded = true;
            }
            catch {
                toggleState(freteError, true);
            }
            finally {
                toggleState(freteLoader, false);
            }
        });
    }

    // --- Modal Calculo Frete ---
    const btnCalculoFrete = document.getElementById('BtnCalculoFrete');
    if (btnCalculoFrete) {
        const calcFreteEndpoint = cfg.endpoints.calculoFrete;
        const calcFreteModalEl = document.getElementById('modalCalculoFrete');
        const calcFreteModal = new bootstrap.Modal(calcFreteModalEl);
        const calcFreteLoader = document.getElementById('calculoFreteLoader');
        const calcFreteError = document.getElementById('calculoFreteError');
        const calcFreteEmpty = document.getElementById('calculoFreteEmpty');
        const divTblCalcFrete = document.getElementById('divTblCalculoFrete');
        const calcFreteTbody = document.querySelector('#tblCalculoFrete tbody');
        let calcFreteLoaded = false;

        btnCalculoFrete.addEventListener('click', async function (e) {
            e.preventDefault();
            calcFreteModal.show();

            if (calcFreteLoaded) return;

            toggleState(calcFreteLoader, true);
            toggleState(calcFreteError, false);
            toggleState(calcFreteEmpty, false);
            toggleState(divTblCalcFrete, false);

            try {
                const response = await fetch(calcFreteEndpoint, {
                    method: 'GET',
                    headers: { 'X-Requested-With': 'XMLHttpRequest' }
                });

                if (!response.ok) throw new Error();

                const items = await response.json();

                if (!items || items.length === 0) {
                    toggleState(calcFreteEmpty, true);
                    calcFreteLoaded = true;
                    return;
                }

                calcFreteTbody.innerHTML = '';
                items.forEach(function (item) {
                    const row = document.createElement('tr');
                    var vItensRestritos = 0;
                    var vtaxaExtra = 0;
                    if (item.qtItensRestritos > 0) {
                        vItensRestritos = '<span class="badge bg-danger fs-6">' + item.qtItensRestritos + '</span>';
                    } else {
                        vItensRestritos = '<span class="">0</span>';
                    }
                    if (item.taxaExtra > 0) {
                        vtaxaExtra = '<span class="text-danger fw-bold">' + fmtBrl(item.taxaExtra) + '</span>';
                    } else {
                        vtaxaExtra = '<span class="">' + fmtBrl(item.taxaExtra) + '</span>';
                    }

                    row.innerHTML =
                        '<td class="text-center hide-lg">' + item.transportadoraID + '</td>' +
                        '<td>' + (item.nomeTransportadora || '') + '</td>' +
                        '<td class="text-center hide-sm"">' + fmtDiasUteis(item.prazoLogistico) + '</td>' +
                        '<td class="text-center">' + fmtDiasUteis(item.prazoComercial) + '</td>' +
                        '<td class="text-end">' + fmtBrl(item.valorFrete) + '</td>' +
                        '<td class="text-end">' + vtaxaExtra + '</td>' +
                        '<td class="text-center">' + vItensRestritos + '</td>' +
                        '<td class="text-center hide-md">' + fmtSimNao(item.clienteRestrito) + '</td>' +
                        '<td class="text-center hide-sm">' + fmtSimNao(item.clienteFixo) + '</td>' +
                        '<td class="text-center hide-md">' + fmtSimNao(item.obrigatoriaCanalVenda) + '</td>';
                    calcFreteTbody.appendChild(row);
                });

                toggleState(divTblCalcFrete, true);
                calcFreteLoaded = true;
            }
            catch {
                toggleState(calcFreteError, true);
            }
            finally {
                toggleState(calcFreteLoader, false);
            }
        });
    }

    // --- Registros de Log (lazy load com filtro por Origem) ---
    const registrosLogsCollapse = document.getElementById('collapseRegistrosLogs');

    if (registrosLogsCollapse) {
        const registrosLogsLoader = document.getElementById('logsRegistrosLogsLoader');
        const registrosLogsError = document.getElementById('logsRegistrosLogsError');
        const registrosLogsEmpty = document.getElementById('logsRegistrosLogsEmpty');
        const divTblRegistrosLogs = document.getElementById('divTblRegistrosLogs');
        const registrosLogsTbody = document.querySelector('#tblRegistrosLogs tbody');
        const filtroOrigem = document.getElementById('filtroOrigemLogs');
        const registrosLogsEndpoint = cfg.endpoints.registrosLogs;
        let registrosLogsLoaded = false;
        let allLogsData = [];
        let dtInstanceLogs = null;

        function renderFilteredLogs(origem) {
            const filtered = allLogsData.filter(function (item) { return item.origem === origem; });

            if (dtInstanceLogs) {
                dtInstanceLogs.destroy();
                dtInstanceLogs = null;
            }

            registrosLogsTbody.innerHTML = '';

            if (filtered.length === 0) {
                toggleState(divTblRegistrosLogs, false);
                toggleState(registrosLogsEmpty, true);
                return;
            }

            toggleState(registrosLogsEmpty, false);

            filtered.forEach(function (item) {
                const row = document.createElement('tr');
                row.innerHTML =
                    '<td class="text-center text-nowrap">' + (item.dataHora || '—') + '</td>' +
                    '<td><span class="fw-semibold">' + (item.acao || '') + '</span><br /><span class="text-muted">' + (item.descricao || '') + '</span></td>' +
                    '<td class="text-start text-nowrap">' + (item.nmUsuario || '') + '</td>';
                registrosLogsTbody.appendChild(row);
            });

            toggleState(divTblRegistrosLogs, true);

            dtInstanceLogs = new DataTable('#tblRegistrosLogs', {
                pageLength: 25,
                lengthMenu: [[25, 50, 100, -1], [25, 50, 100, "Todos"]],
                searching: true,
                ordering: false,
                info: true,
                responsive: { details: false },
                scrollX: true,
                layout: {
                    topStart: 'search',
                    topEnd: 'pageLength',
                    bottomStart: 'info',
                    bottomEnd: 'paging'
                },
                language: dtLanguagePtBr,
                columnDefs: [
                    { className: 'text-center text-nowrap', targets: [0] }
                ]
            });
        }

        async function loadRegistrosLogs() {
            if (registrosLogsLoaded) return;

            toggleState(registrosLogsLoader, true);
            toggleState(registrosLogsError, false);
            toggleState(registrosLogsEmpty, false);
            toggleState(divTblRegistrosLogs, false);
            toggleState(filtroOrigem, false);

            try {
                const response = await fetch(registrosLogsEndpoint, {
                    method: 'GET',
                    headers: { 'X-Requested-With': 'XMLHttpRequest' }
                });

                if (!response.ok) throw new Error();

                allLogsData = await response.json();

                if (!allLogsData || allLogsData.length === 0) {
                    toggleState(registrosLogsEmpty, true);
                    registrosLogsLoaded = true;
                    return;
                }

                toggleState(filtroOrigem, true);
                updateFilterCounts();
                const activeBtn = filtroOrigem.querySelector('.log-filter-btn.active:not(.d-none)');
                const targetBtn = activeBtn || filtroOrigem.querySelector('.log-filter-btn:not(.d-none)');
                if (targetBtn) {
                    renderFilteredLogs(targetBtn.dataset.origem);
                } else {
                    toggleState(registrosLogsEmpty, true);
                }
                registrosLogsLoaded = true;
            }
            catch {
                toggleState(registrosLogsError, true);
            }
            finally {
                toggleState(registrosLogsLoader, false);
            }
        }

        function updateFilterCounts() {
            filtroOrigem.querySelectorAll('.log-filter-btn').forEach(function (btn) {
                const origem = btn.dataset.origem;
                const count = allLogsData.filter(function (item) { return item.origem === origem; }).length;
                const badge = btn.querySelector('[data-count-origem]');
                if (badge) badge.textContent = count;
                btn.classList.toggle('d-none', count === 0);
            });

            const activeBtn = filtroOrigem.querySelector('.log-filter-btn.active:not(.d-none)');
            if (!activeBtn) {
                const firstVisible = filtroOrigem.querySelector('.log-filter-btn:not(.d-none)');
                if (firstVisible) firstVisible.classList.add('active');
            }
        }

        filtroOrigem.addEventListener('click', function (e) {
            const btn = e.target.closest('.log-filter-btn');
            if (!btn || btn.classList.contains('active')) return;

            filtroOrigem.querySelectorAll('.log-filter-btn').forEach(function (b) { b.classList.remove('active'); });
            btn.classList.add('active');
            renderFilteredLogs(btn.dataset.origem);
        });

        registrosLogsCollapse.addEventListener('shown.bs.collapse', function () {
            loadRegistrosLogs();
            if (dtInstanceLogs) {
                dtInstanceLogs.columns.adjust();
            }
        });
    }

    // --- Validação do Pedido (auto-load após carregamento da página) ---
    setTimeout(async function () {
        const container = document.getElementById('msgValidacaoPedido');
        if (!container) return;

        try {
            const response = await fetch(cfg.endpoints.validacoes, {
                method: 'GET',
                headers: { 'X-Requested-With': 'XMLHttpRequest' }
            });

            if (!response.ok) return;

            const items = await response.json();
            if (!items || items.length === 0) return;

            container.innerHTML = items.map(function (item) {
                return '<div class="alert alert-danger alert-dismissible fade show" role="alert">' +
                    '<i class="fa-solid fa-triangle-exclamation fa-lg me-2"></i>' +
                    '<span class="fw-semibold">' + (item.erro || '') + '</span><br />' +
                    '<span class="fs-7">' + (item.correcao || '') + '</span>' +
                    '<button type="button" class="btn-close" data-bs-dismiss="alert" aria-label="Close"></button>' +
                    '</div>';
            }).join('');
        }
        catch { }
    }, 0);

});
