/* ============================================
   Cotações — Index (DataTables AJAX)
   ============================================ */
(() => {
    if (typeof DataTable === 'undefined') return;

    // Colunas da tabela:
    // 0:Proposta 1:Nome 2:Cód.Cliente 3:Cliente 4:CNPJ
    // 5:Estabelecimento 6:Status 7:Total Venda 8:Itens 9:Abertura 10:Ações
    var orderMap = {
        'Proposta (Recente)': [[0, 'desc']],
        'Proposta (Antigo)':  [[0, 'asc']],
        'Cliente (A-Z)':      [[3, 'asc']],
        'Cliente (Z-A)':      [[3, 'desc']],
        'Status (A-Z)':       [[6, 'asc']],
        'Status (Z-A)':       [[6, 'desc']],
        'Abertura (Recente)': [[9, 'desc']],
        'Abertura (Antigo)':  [[9, 'asc']]
    };

    // Monta a URL do AJAX reaproveitando os filtros da query string atual
    var ajaxUrl = '/Cotacao/ListaJson' + window.location.search;

    var dt = new DataTable('#cotacoesTable', {
        ajax: {
            url: ajaxUrl,
            dataSrc: 'data'
        },
        columns: [
            { data: 'propostaId',       className: 'text-center' },
            { data: 'nome' },
            { data: 'cdExtCliente' },
            { data: 'clienteNome' },
            { data: 'clienteCNPJ' },
            { data: 'nmEstabelecimento' },
            { data: 'statusName' },
            { data: 'totalVenda',       className: 'text-end' },
            { data: 'qtdItens',         className: 'text-center' },
            { data: 'dataAbertura',     className: 'text-center' },
            {
                data: null,
                orderable: false,
                searchable: false,
                className: 'text-center',
                render: function (data, type, row) {
                    var podeEditar = row.statusID !== 3 && row.statusID !== 7;
                    var html = '<div class="d-flex gap-1 justify-content-center">';
                    html += '<a href="/Cotacao/Cotacao?propostaId=' + row.propostaId + '" class="cotacao-btn cotacao-btn-exibir" title="Exibir Cotação"><i class="fa-solid fa-eye"></i></a>';
                    if (podeEditar) {
                        html += '<a href="/Cotacao/Edit?propostaId=' + row.propostaId + '" class="cotacao-btn cotacao-btn-abrir" title="Editar Cotação"><i class="fa-solid fa-pen-to-square"></i></a>';
                    }
                    html += '</div>';
                    return html;
                }
            }
        ],
        pageLength: 25,
        responsive: true,
        lengthChange: false,
        searching: true,
        info: false,
        paging: true,
        order: [[0, 'desc']],
        pagingType: 'full_numbers',
        language: {
            emptyTable:  'Nenhuma cotação encontrada.',
            zeroRecords: 'Nenhuma cotação corresponde ao filtro',
            info:        'Mostrando _START_ até _END_ de _TOTAL_ cotações',
            infoEmpty:   'Mostrando 0 até 0 de 0 cotações',
            loadingRecords: 'Carregando...',
            processing:  'Processando...',
            paginate: {
                first:    'Primeiro',
                previous: 'Anterior',
                next:     'Próximo',
                last:     'Último'
            }
        }
    });

    // Atualiza o contador de registros após o AJAX retornar
    dt.on('init.dt draw.dt', function () {
        var total = dt.rows({ search: 'applied' }).count();
        var spanTotal = document.getElementById('spanTotalRegistros');
        if (spanTotal) {
            spanTotal.textContent = total.toLocaleString('pt-BR');
        }
    });

    // Esconde a busca nativa do DataTables
    var dtSearch = document.querySelector('#cotacoesTable_wrapper .dt-search');
    if (dtSearch) dtSearch.style.display = 'none';

    // Busca personalizada
    var inputBusca = document.getElementById('inputBuscaTabela');
    var btnBusca   = document.getElementById('btnBuscaTabela');

    function aplicarBusca() {
        dt.search(inputBusca.value || '').draw();
    }

    btnBusca?.addEventListener('click', aplicarBusca);
    inputBusca?.addEventListener('keyup', function (e) {
        if (e.key === 'Enter') { e.preventDefault(); aplicarBusca(); }
    });

    // Ordem via select
    document.getElementById('selectOrderBy')?.addEventListener('change', function () {
        var order = orderMap[this.value];
        if (order) dt.order(order).draw();
    });

    // Por página
    document.getElementById('selectPageSize')?.addEventListener('change', function () {
        dt.page.len(parseInt(this.value, 10)).draw();
    });
})();

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
