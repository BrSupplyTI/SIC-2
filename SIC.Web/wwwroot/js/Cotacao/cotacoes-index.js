/* ============================================
   Cotações — Index (DataTables Server-Side)
   ============================================ */
$(function () {
    // Lê filtros dos hidden inputs do form
    function getFilterParams() {
        return {
            cdExtCliente: document.getElementById('inputCdExtCliente')?.value || '',
            propostaId: document.getElementById('inputPropostaId')?.value || '',
            cnpj: document.getElementById('inputCNPJ')?.value || '',
            estabelecimentoID: document.getElementById('inputEstabelecimentoID')?.value || '',
            statusID: document.getElementById('inputStatusID')?.value || '',
            dataInicial: document.getElementById('inputDataInicial')?.value || '',
            dataFinal: document.getElementById('inputDataFinal')?.value || '',
            filtroCotacao: document.getElementById('inputFiltroCotacao')?.value || '1'
        };
    }

    var dt = $('#cotacoesTable').DataTable({
        serverSide: true,
        processing: true,
        pageLength: 25,
        responsive: true,
        deferRender: true,
        lengthChange: false,
        searching: true,
        info: false,
        paging: true,
        order: [[0, 'desc']],
        pagingType: 'full_numbers',
        ajax: {
            url: '/Cotacao/ListData',
            type: 'GET',
            data: function (d) {
                var filters = getFilterParams();
                d.cdExtCliente = filters.cdExtCliente;
                d.propostaId = filters.propostaId || undefined;
                d.cnpj = filters.cnpj;
                d.estabelecimentoID = filters.estabelecimentoID || undefined;
                d.statusID = filters.statusID || undefined;
                d.dataInicial = filters.dataInicial;
                d.dataFinal = filters.dataFinal;
                d.filtroCotacao = filters.filtroCotacao;
            },
            dataFilter: function (raw) {
                var json = JSON.parse(raw);
                var el = document.getElementById('totalRegistros');
                if (el) {
                    var total = (json.recordsTotal || 0).toLocaleString('pt-BR');
                    el.innerHTML = '<strong>' + total + '</strong> cotação(ões) encontrada(s)';
                }
                return raw;
            },
            error: function (xhr, error, thrown) {
                console.error('ListData error:', xhr.status, error, thrown);
            }
        },
        columnDefs: [
            { targets: 0, className: 'text-center' },
            { targets: 7, className: 'text-end' },
            { targets: [8, 9], className: 'text-center' },
            {
                targets: 10,
                orderable: false,
                searchable: false,
                className: 'text-center',
                render: function (data) {
                    var propostaId = data.propostaId;
                    var statusId = data.statusId;

                    var html = '<div class="d-flex gap-1 justify-content-center">';

                    // botão visualizar (sempre aparece)
                    html += '<a href="/Cotacao/Cotacao?propostaId=' + propostaId + '" class="cotacao-btn cotacao-btn-exibir" title="Exibir Cotação">'
                        + '<i class="fa-solid fa-eye"></i></a>';

                    // regra do editar (igual PHP)
                    if (statusId != 3 && statusId != 7) {
                        html += '<a href="/Cotacao/Edit?propostaId=' + propostaId + '" class="cotacao-btn cotacao-btn-abrir" title="Editar Cotação">'
                            + '<i class="fa-solid fa-pen-to-square"></i></a>';
                    }

                    html += '</div>';

                    return html;
                }
            }
        ],
        language: {
            emptyTable: 'Nenhuma cotação encontrada.',
            processing: '<i class="fa-solid fa-spinner fa-spin me-2"></i>Carregando...',
            info: 'Mostrando _START_ até _END_ de _TOTAL_ cotações',
            infoEmpty: 'Mostrando 0 até 0 de 0 cotações',
            zeroRecords: 'Nenhuma cotação corresponde ao filtro',
            paginate: {
                first: 'Primeiro',
                previous: 'Anterior',
                next: 'Próximo',
                last: 'Último'
            }
        }
    });

    // Esconde a busca nativa do DataTables
    var dtSearch = document.querySelector('#cotacoesTable_wrapper .dt-search');
    if (dtSearch) dtSearch.style.display = 'none';

    // Busca personalizada
    var inputBusca = document.getElementById('inputBuscaTabela');
    var btnBusca = document.getElementById('btnBuscaTabela');

    function aplicarBusca() {
        dt.search(inputBusca.value || '').draw();
    }

    btnBusca?.addEventListener('click', aplicarBusca);
    inputBusca?.addEventListener('keyup', function (e) {
        if (e.key === 'Enter') { e.preventDefault(); aplicarBusca(); }
    });

    // Ordem via select
    var orderMap = {
        'Proposta (Recente)': [[0, 'desc']],
        'Proposta (Antigo)':  [[0, 'asc']],
        'Cliente (A-Z)':      [[2, 'asc']],
        'Cliente (Z-A)':      [[2, 'desc']],
        'Status (A-Z)':       [[5, 'asc']],
        'Status (Z-A)':       [[5, 'desc']],
        'Abertura (Recente)': [[8, 'desc']],
        'Abertura (Antigo)':  [[8, 'asc']]
    };

    document.getElementById('selectOrderBy')?.addEventListener('change', function () {
        var order = orderMap[this.value];
        if (order) dt.order(order).draw();
    });

    // Por página
    document.getElementById('selectPageSize')?.addEventListener('change', function () {
        dt.page.len(parseInt(this.value, 10)).draw();
    });
});

// ── Filtros ──
var filterInputMap = {
    cdExtCliente:      'inputCdExtCliente',
    propostaId:        'inputPropostaId',
    cnpj:              'inputCNPJ',
    estabelecimentoID: 'inputEstabelecimentoID',
    statusID:          'inputStatusID',
    dataInicial:       'inputDataInicial',
    dataFinal:         'inputDataFinal'
};

function removeFilter(field) {
    var inputId = filterInputMap[field];
    if (!inputId) return;
    var el = document.getElementById(inputId);
    if (el) el.value = '';
    document.getElementById('formCotacoes').submit();
}

document.getElementById('btnAplicarFiltros')?.addEventListener('click', function () {
    document.getElementById('inputCdExtCliente').value = document.getElementById('filtroCdExtCliente').value;
    document.getElementById('inputPropostaId').value = document.getElementById('filtroPropostaId').value;
    document.getElementById('inputCNPJ').value = document.getElementById('filtroCNPJ').value;
    document.getElementById('inputEstabelecimentoID').value = document.getElementById('filtroEstabelecimentoID').value;
    document.getElementById('inputStatusID').value = document.getElementById('filtroStatusID').value;
    document.getElementById('inputDataInicial').value = document.getElementById('filtroDataInicial').value;
    document.getElementById('inputDataFinal').value = document.getElementById('filtroDataFinal').value;
    document.getElementById('inputFiltroCotacao').value = document.getElementById('filtroComCotacao').checked ? '1' : '0';
    document.getElementById('formCotacoes').submit();
});

document.getElementById('btnLimparFiltros')?.addEventListener('click', function () {
    var clearUrl = document.getElementById('formCotacoes')?.getAttribute('action') || window.location.pathname;
    window.location.href = clearUrl;
});
