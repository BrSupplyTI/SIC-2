/* ============================================
   Cotação — Visualização (Análise da Proposta)
   ============================================ */
(() => {
    'use strict';

    // Botão Voltar
    var btnVoltar = document.getElementById('btnVoltarCotacao');
    if (btnVoltar) {
        btnVoltar.addEventListener('click', function (e) {
            e.preventDefault();
            if (window.history.length > 1) {
                window.history.back();
            } else {
                window.location.href = this.getAttribute('href') || '/Cotacao';
            }
        });
    }

    // ══════════════════════════════════════════════════════════════
    // VALIDAÇÃO DO BOTÃO FINALIZAR COTAÇÃO
    // ══════════════════════════════════════════════════════════════

    function validarBotaoFinalizarCotacao() {
        const btnFinalizarModal = document.querySelector('[data-bs-target="#modalFinalizarCotacao"]');
        const btnLiberar = document.getElementById('btnLiberar');
        
        if (!btnFinalizarModal) return;

        const todosItens = (window.cotacaoConfig || {}).itens ?? [];
        const possuiItensComErro = todosItens.some(item => item.totalComImposto === 0);

        if (possuiItensComErro) {
            // Desabilitar botão que abre o modal
            btnFinalizarModal.disabled = true;
            btnFinalizarModal.classList.add('disabled');
            btnFinalizarModal.title = 'Calcule a margem de todos os itens antes de finalizar a cotação';
            
            // Desabilitar botão Liberar dentro do modal
            if (btnLiberar) {
                btnLiberar.disabled = true;
                btnLiberar.classList.add('disabled');
                btnLiberar.title = 'Calcule a margem de todos os itens antes de finalizar';
            }
        } else {
            // Habilitar botão que abre o modal
            btnFinalizarModal.disabled = false;
            btnFinalizarModal.classList.remove('disabled');
            btnFinalizarModal.title = 'Finalizar Cotação';
            
            // Habilitar botão Liberar dentro do modal
            if (btnLiberar) {
                btnLiberar.disabled = false;
                btnLiberar.classList.remove('disabled');
                btnLiberar.title = 'Liberar';
            }
        }
    }

    // Validar estado inicial ao carregar a página
    validarBotaoFinalizarCotacao();

    // Validar também quando o modal for aberto
    const modalFinalizarCotacao = document.getElementById('modalFinalizarCotacao');
    if (modalFinalizarCotacao) {
        modalFinalizarCotacao.addEventListener('show.bs.modal', function () {
            validarBotaoFinalizarCotacao();
        });
    }

    // ══════════════════════════════════════════════════════════════
    // CONDIÇÃO DE PAGAMENTO — Tom Select pesquisável + lápis + ESC
    // ══════════════════════════════════════════════════════════════

    const selectCondPagto = document.getElementById('selectCondPagto');

    function initCondPagtoSelect(select) {
        if (!select || typeof TomSelect === 'undefined') return null;

        var instance = new TomSelect(select, {
            create: false,
            allowEmptyOption: true,
            maxOptions: null,
            placeholder: select.dataset.placeholder || 'Digite para pesquisar...',
            searchField: ['text'],
            sortField: [{ field: 'text', direction: 'asc' }],
            onFocus: function () {
                this._clearOnNextType = true;
            },
            onType: function () {
                if (this._clearOnNextType) {
                    this._clearOnNextType = false;
                    this.clear(true);
                }
            },
            onBlur: function () {
                this._clearOnNextType = false;
                // Se não houve mudança pendente, restaura o valor original
                if (select.dataset.pendingChange !== '1') {
                    var lastValue = select.dataset.lastValue || '';
                    this.setValue(lastValue, true);
                }
                select.dataset.pendingChange = '0';
            },
            onChange: function () {
                select.dataset.pendingChange = '1';
            }
        });

        return instance;
    }

    initCondPagtoSelect(selectCondPagto);

    async function updateCondPagto() {
        if (!selectCondPagto) return;

        const value    = parseInt(selectCondPagto.value, 10);
        const lastValue = parseInt(selectCondPagto.dataset.lastValue || '0', 10);
        if (!value || value === lastValue) return;

        const url = window.cotacaoConfig?.urls?.salvarCondPagto;
        if (!url) return;

        try {
            const resp = await fetch(url, {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ condPagtoId: value })
            });
            const data = await resp.json();
            if (data.success) {
                selectCondPagto.dataset.lastValue = String(value);
                window.location.reload();
            } else {
                Swal.fire({ icon: 'error', title: 'Erro', text: data.message || 'Erro ao salvar condição de pagamento.' });
                // Restaura o valor anterior no Tom Select
                if (selectCondPagto.tomselect) {
                    selectCondPagto.tomselect.setValue(String(lastValue), true);
                }
            }
        } catch {
            Swal.fire({ icon: 'error', title: 'Erro', text: 'Falha na comunicação com o servidor.' });
        }
    }

    selectCondPagto?.addEventListener('change', updateCondPagto);

    // ── Botão lápis: abre o wrapper e foca o Tom Select ──
    document.querySelectorAll('.js-mapping-edit-toggle').forEach(function (btn) {
        btn.addEventListener('click', function () {
            var wrapperId = btn.dataset.target;
            var displayId = btn.dataset.display;
            var wrapper   = document.getElementById(wrapperId);
            var display   = document.getElementById(displayId);
            if (!wrapper) return;

            wrapper.classList.remove('d-none');
            if (display) display.classList.add('d-none');
            btn.classList.add('d-none');

            var sel = wrapper.querySelector('select');
            if (sel && sel.tomselect) {
                sel.tomselect.focus();
            } else if (sel) {
                sel.focus();
            }
        });
    });

    // ── ESC: fecha sem salvar, volta ao modo exibição ──
    if (selectCondPagto) {
        selectCondPagto.addEventListener('keydown', function (e) {
            if (e.key !== 'Escape') return;
            var wrapper = selectCondPagto.closest('[id$="Wrapper"]');
            if (!wrapper) return;
            var displayId  = wrapper.id.replace('Wrapper', 'Display');
            var display    = document.getElementById(displayId);
            var toggleBtn  = document.querySelector('.js-mapping-edit-toggle[data-target="' + wrapper.id + '"]');

            // Restaura valor original no Tom Select
            if (selectCondPagto.tomselect) {
                selectCondPagto.tomselect.setValue(selectCondPagto.dataset.lastValue || '', true);
            }
            selectCondPagto.dataset.pendingChange = '0';

            wrapper.classList.add('d-none');
            if (display) display.classList.remove('d-none');
            if (toggleBtn) toggleBtn.classList.remove('d-none');
        });
    }

    // ══════════════════════════════════════════════════════════════
    // GERAR ITENS (Consumo / Último Pedido)
    // ══════════════════════════════════════════════════════════════

    document.querySelectorAll('.btn-gerar').forEach(btn => {
        btn.addEventListener('click', async function (e) {
            e.preventDefault();

            const tipo = btn.dataset.value;

            // ── Se for "Outro Pedido", abrir o modal em vez de gerar itens ─
            if (tipo === 'P') {
                const modal = bootstrap.Modal.getOrCreateInstance(
                    document.getElementById('modalGerarCotacao')
                );
                document.getElementById('inputTipoGeracao').value = 'P';
                document.getElementById('msgTipoGeracao').textContent = 'Digite o número do Pedido que deseja replicar os itens.';
                document.getElementById('inputCotacaoID').value = '';
                document.getElementById('inputCotacaoID').placeholder = 'Ex: 4259647';
                modal.show();
                return;
            }

            // ── Para C e U, usar o fluxo original de geração de itens ─
            const url = window.cotacaoConfig?.urls?.gerarItens;
            if (!url || !tipo) return;

            const label = tipo === 'C' ? 'Com base no Consumo' : 'Último Pedido';
            const confirmar = await Swal.fire({
                title: 'Gerar Itens',
                text: `Deseja gerar itens "${label}"? Os itens existentes serão substituídos.`,
                icon: 'question',
                showCancelButton: true,
                confirmButtonText: 'Sim, gerar',
                cancelButtonText: 'Cancelar'
            });

            if (!confirmar.isConfirmed) return;

            Swal.fire({ title: 'Gerando itens...', allowOutsideClick: false, didOpen: () => Swal.showLoading() });

            try {
                const resp = await fetch(url, {
                    method: 'POST',
                    headers: { 'Content-Type': 'application/json', 'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]')?.value ?? '' },
                    body: JSON.stringify({ tipoGeracao: tipo })
                });

                const data = await resp.json();

                if (data.success) {
                    await Swal.fire({ icon: 'success', title: 'Itens gerados!', text: data.message || 'Os itens foram gerados com sucesso.', timer: 1500, showConfirmButton: false });
                    window.location.reload();
                } else {
                    Swal.fire({ icon: 'error', title: 'Erro', text: data.message || 'Não foi possível gerar os itens.' });
                }
            } catch {
                Swal.fire({ icon: 'error', title: 'Erro', text: 'Falha na comunicação com o servidor.' });
            }
        });
    });

    // Checkbox "Selecionar todos" na grid
    var checkAll = document.getElementById('checkAllItens');
    if (checkAll) {
        checkAll.addEventListener('change', function () {
            var checks = document.querySelectorAll('.check-item-cotacao');
            checks.forEach(function (c) { c.checked = checkAll.checked; });
        });
    }

    // ══════════════════════════════════════════════════════════════
    // CALCULAR MARGEM POR ITEM
    // ══════════════════════════════════════════════════════════════

    const btnCalcularMargem = document.getElementById('btnCalcularMargem');
    if (btnCalcularMargem) {
        btnCalcularMargem.addEventListener('click', async function () {
            const propostaId = window.cotacaoConfig?.propostaId;
            if (!propostaId) return;

            // propostaItemId = 0 → NULL na procedure → calcula todos os itens e atualiza o cabeçalho
            const url = window.cotacaoConfig?.urls?.calcularMargem ?? '';
            if (!url) return;

            const iconeOriginal = btnCalcularMargem.innerHTML;
            btnCalcularMargem.disabled = true;
            btnCalcularMargem.innerHTML = '<i class="fa-solid fa-spinner fa-spin me-1"></i>Calculando...';

            try {
                const resp = await fetch(url, {
                    method: 'POST',
                    headers: { 'Content-Type': 'application/json' },
                    body: JSON.stringify({ type: 'preco', viaTela: 'NAO' })
                });
                const data = await resp.json();
                if (!resp.ok || !data.success) {
                    mostrarAlerta('Atenção', data.message || 'Erro ao calcular margem.', 'warning');
                }
                window.location.reload();
            } catch {
                mostrarAlerta('Erro', 'Falha na comunicação com o servidor.', 'error');
            } finally {
                btnCalcularMargem.disabled = false;
                btnCalcularMargem.innerHTML = iconeOriginal;
            }
        });
    }

    // ══════════════════════════════════════════════════════════════
    // MODAL DE OPÇÕES DE FRETE
    // ══════════════════════════════════════════════════════════════

    const cardPrincipal = document.querySelector('[data-proposta-id]');
    const propostaId = cardPrincipal ? cardPrincipal.dataset.propostaId : null;
    const btnCalcularFrete = document.getElementById('btnCalcularFrete');
    const btnSalvarFrete = document.getElementById('btnSalvarFrete');
    const tabelaFreteBody = document.getElementById('tabelaFreteBody');

    // Calcular Frete - AJAX call para o backend
    if (btnCalcularFrete) {
        btnCalcularFrete.addEventListener('click', function () {
            if (!propostaId) {
                mostrarAlerta('Erro', 'ID da proposta não encontrado.', 'error');
                return;
            }

            const btn = this;
            const textoOriginal = btn.innerHTML;
            btn.disabled = true;
            btn.innerHTML = '<i class="fa-solid fa-spinner fa-spin me-2"></i>Calculando...';

            const urlCalcularFrete = window.cotacaoConfig?.urls?.calcularFrete || `/Cotacao/CalcularFrete?propostaId=${propostaId}`;
            fetch(urlCalcularFrete, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'X-Requested-With': 'XMLHttpRequest'
                }
            })
                .then(async response => {
                    if (response.status === 401) {
                        window.location.reload();
                        return null;
                    }
                    const text = await response.text();
                    try {
                        return JSON.parse(text);
                    } catch {
                        throw new Error(`Resposta inválida do servidor (HTTP ${response.status})`);
                    }
                })
                .then(data => {
                    if (!data) return;
                    if (data.error) {
                        mostrarAlerta('Erro', data.error, 'error');
                    } else {
                        renderizarTabelaFrete(data);
                    }
                })
                .catch(error => {
                    console.error('Erro ao calcular frete:', error);
                    mostrarAlerta('Erro', error.message || 'Erro ao calcular frete. Tente novamente.', 'error');
                })
                .finally(() => {
                    btn.disabled = false;
                    btn.innerHTML = textoOriginal;
                });
        });
    }

    // Renderizar a tabela de fretes
    function renderizarTabelaFrete(dados) {
        if (!tabelaFreteBody) return;

        if (!dados || dados.length === 0) {
            tabelaFreteBody.innerHTML = `
                <tr>
                    <td colspan="7" class="text-center py-4">
                        <p class="text-muted mb-0">Nenhum histórico de cálculo de fretes registrado até o momento.</p>
                    </td>
                </tr>
            `;
            if (btnSalvarFrete) btnSalvarFrete.disabled = true;
            return;
        }

        let html = '';
        dados.forEach(item => {
            let regrasAdicionais = '';

            if (item.flagObrigatoriaCanalVenda) {
                regrasAdicionais += 'Preferência no canal de venda<br>';
            }
            if (item.flagClienteRestrito) {
                regrasAdicionais += 'Bloqueado para o cliente<br>';
            }
            if (item.flagClienteFixo) {
                regrasAdicionais += 'Fixa no Cadastro do Cliente<br>';
            }

            const corRestrito = item.qtItensRestritos > 0 ? 'text-danger fw-semibold' : '';
            const iconRestrito = item.qtItensRestritos > 0 ? '<i class="fa-solid fa-exclamation-triangle me-1"></i>' : '';

            html += `
                <tr class="frete-row" 
                    data-transportadora-id="${item.transportadoraID}" 
                    data-valor="${item.valorFrete}" 
                    data-logistico="${item.tempoLogistico}" 
                    data-comercial="${item.tempoComercial}">
                    <td class="text-start ${corRestrito}">${item.nome}</td>
                    <td class="text-center ${corRestrito}">${item.tempoLogistico} Dia(s)</td>
                    <td class="text-center ${corRestrito}">${item.tempoComercial} Dia(s)</td>
                    <td class="text-end ${corRestrito}">R$ ${formatarValor(item.taxaExtra)}</td>
                    <td class="text-end fw-semibold ${corRestrito}">R$ ${formatarValor(item.valorFrete)}</td>
                    <td class="text-center ${corRestrito}">${iconRestrito}${item.qtItensRestritos}</td>
                    <td class="text-start ${corRestrito}" style="font-size: 0.8rem;">${regrasAdicionais || '-'}</td>
                </tr>
            `;
        });

        tabelaFreteBody.innerHTML = html;

        // Adicionar evento de clique nas linhas para seleção
        document.querySelectorAll('.frete-row').forEach(row => {
            row.addEventListener('click', function () {
                // Remover seleção anterior
                document.querySelectorAll('.frete-row').forEach(r => r.classList.remove('table-active'));

                // Adicionar seleção na linha clicada
                this.classList.add('table-active');

                // Habilitar botão Salvar
                if (btnSalvarFrete) btnSalvarFrete.disabled = false;
            });
        });
    }

    // Salvar Frete Selecionado
    if (btnSalvarFrete) {
        btnSalvarFrete.addEventListener('click', function () {
            const linhaSelecionada = document.querySelector('.frete-row.table-active');

            if (!linhaSelecionada) {
                mostrarAlerta('Atenção', 'Selecione uma transportadora antes de salvar.', 'warning');
                return;
            }

            const transportadoraId = parseInt(linhaSelecionada.dataset.transportadoraId);
            const valorFrete       = parseFloat(linhaSelecionada.dataset.valor);
            const prazoTotal       = parseInt(linhaSelecionada.dataset.comercial);

            const btn = this;
            const textoOriginal = btn.innerHTML;
            btn.disabled = true;
            btn.innerHTML = '<i class="fa-solid fa-spinner fa-spin me-2"></i>Salvando...';

            const urlSalvarFrete = window.cotacaoConfig?.urls?.salvarFrete || `/Cotacao/SalvarFrete`;

            const payload = {
                transportadoraId: transportadoraId,
                valorFrete: valorFrete,
                prazoTotal: prazoTotal
            };

            fetch(urlSalvarFrete, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'X-Requested-With': 'XMLHttpRequest'
                },
                body: JSON.stringify(payload)
            })
                .then(async response => {
                    if (response.status === 401) { window.location.reload(); return null; }
                    const text = await response.text();
                    try { return JSON.parse(text); }
                    catch { throw new Error(`Resposta inválida do servidor (HTTP ${response.status})`); }
                })
                .then(data => {
                    if (!data) return;
                    if (data.success) {
                        const modal = bootstrap.Modal.getInstance(document.getElementById('modalOpcoesFrete'));
                        if (modal) modal.hide();
                        mostrarAlertaAsync('Sucesso', 'Frete salvo com sucesso!', 'success')
                            .then(() => window.location.reload());
                    } else {
                        mostrarAlerta('Erro', data.error || 'Erro ao salvar frete.', 'error');
                    }
                })
                .catch(error => {
                    console.error('Erro ao salvar frete:', error);
                    mostrarAlerta('Erro', error.message || 'Erro ao salvar frete. Tente novamente.', 'error');
                })
                .finally(() => {
                    btn.disabled = false;
                    btn.innerHTML = textoOriginal;
                });
        });
    }

    // Função auxiliar para formatar valores monetários
    function formatarValor(valor) {
        return parseFloat(valor).toFixed(2).replace('.', ',').replace(/\B(?=(\d{3})+(?!\d))/g, '.');
    }

    // Escapa HTML para evitar XSS ao inserir strings no innerHTML
    function escHtml(str) {
        return String(str ?? '')
            .replace(/&/g, '&amp;')
            .replace(/</g, '&lt;')
            .replace(/>/g, '&gt;')
            .replace(/"/g, '&quot;');
    }

    // Função auxiliar para mostrar alertas (compatível com SweetAlert2)
    function mostrarAlerta(titulo, texto, icone) {
        if (typeof Swal !== 'undefined') {
            Swal.fire({ title: titulo, text: texto, icon: icone, confirmButtonText: 'OK' });
        } else {
            alert(`${titulo}: ${texto}`);
        }
    }

    function mostrarAlertaAsync(titulo, texto, icone) {
        if (typeof Swal !== 'undefined') {
            return Swal.fire({ title: titulo, text: texto, icon: icone, confirmButtonText: 'OK' });
        }
        alert(`${titulo}: ${texto}`);
        return Promise.resolve();
    }

    // ══════════════════════════════════════════════════════════════
    // MODAL: ADICIONAR ITEM — Pesquisa de Catálogo (com paginação)
    // ══════════════════════════════════════════════════════════════

    const cfg                   = window.cotacaoConfig || {};
    const formSearchProds       = document.getElementById('formSearchProds');
    const inputSearchProds      = document.getElementById('inputSearchProds');
    const tbodyItens            = document.getElementById('tbodyItens');
    const tfootAdicionarItens   = document.getElementById('tfootAdicionarItens');
    const addItensMarcados      = document.getElementById('addItensMarcados');
    const catalogoPaginationEl  = document.getElementById('catalogoPagination');
    const tbodyQuantidadeItens  = document.getElementById('tbodyQuantidadeItens');
    const btnGravarQuantidade   = document.getElementById('btnGravarQuantidade');
    const btnGravarQtdTexto     = document.getElementById('btnGravarQuantidadeTexto');

    const CATALOGO_POR_PAGINA = 10;
    let catalogoItens       = [];
    let catalogoPagina      = 1;
    let catalogoSelecionados = {};
    let itensSelecionados   = [];

    // ── Salvar seleções da página atual no mapa ───────────────────
    function catalogoSalvarSelecoes() {
        if (!tbodyItens) return;
        tbodyItens.querySelectorAll('.check-catalogo').forEach(cb => {
            const key = `${cb.dataset.cdItem}_${cb.dataset.itemId}`;
            if (cb.checked) {
                catalogoSelecionados[key] = {
                    itemID:        cb.dataset.itemId,
                    cdItem:        cb.dataset.cdItem,
                    nmItem:        cb.dataset.nmItem,
                    vlrTabela:     cb.dataset.vlrTabela,
                    vlrPrecoMinimo: cb.dataset.vlrPrecoMinimo,
                    vlrAquisicao:  cb.dataset.vlrCustoAquisicao,
                    vlrCustoMedio: cb.dataset.vlrCustoMedio
                };
            } else {
                delete catalogoSelecionados[key];
            }
        });
    }

    // ── Renderizar uma página do catálogo ─────────────────────────
    function catalogoRenderPagina(pagina) {
        if (!tbodyItens) return;
        catalogoSalvarSelecoes();
        catalogoPagina = pagina;

        const total       = catalogoItens.length;
        const totalPaginas = Math.ceil(total / CATALOGO_POR_PAGINA);
        if (catalogoPagina > totalPaginas) catalogoPagina = totalPaginas;
        if (catalogoPagina < 1)            catalogoPagina = 1;

        const inicio      = (catalogoPagina - 1) * CATALOGO_POR_PAGINA;
        const fim         = Math.min(inicio + CATALOGO_POR_PAGINA, total);
        const paginaItens = catalogoItens.slice(inicio, fim);

        let html = '';
        paginaItens.forEach(item => {
            const key     = `${escHtml(item.cdItem)}_${item.itemID}`;
            const checked = catalogoSelecionados[key] ? ' checked' : '';
            html += `
                <tr style="cursor:pointer" data-item-id="${item.itemID}">
                    <td class="text-center">
                        <input type="checkbox" class="form-check-input check-catalogo"${checked}
                            data-item-id="${item.itemID}"
                            data-cd-item="${escHtml(item.cdItem)}"
                            data-nm-item="${escHtml(item.nmItem)}"
                            data-vlr-tabela="${item.vlrTabela}"
                            data-vlr-preco-minimo="${item.vlrPrecoMinimo ?? 0}"
                            data-vlr-aquisicao="${item.vlrCustoAquisicao}"
                            data-vlr-custo-medio="${item.vlrCustoMedio}" />
                    </td>
                    <td class="fw-semibold text-primary">${escHtml(item.cdItem)}</td>
                    <td>${escHtml(item.nmItem)}</td>
                    <td class="text-end">R$ ${formatarValor(item.vlrTabela)}</td>
                    <td class="text-end">R$ ${formatarValor(item.vlrCustoAquisicao)}</td>
                    <td class="text-end">${item.qtdDisponivel != null ? item.qtdDisponivel : '-'}</td>
                </tr>`;
        });

        tbodyItens.innerHTML = html;

        // Click na linha → abre modal de quantidade só com aquele item
        // Click no checkbox → apenas marca/desmarca (sem abrir modal)
        tbodyItens.querySelectorAll('tr[data-item-id]').forEach(tr => {
            tr.addEventListener('click', function (e) {
                if (e.target.closest('.check-catalogo')) return; // checkbox: só marca
                const id   = this.dataset.itemId;
                const item = catalogoItens.find(i => String(i.itemID) === String(id));
                if (!item) return;
                abrirModalQuantidade([{
                    itemID:        item.itemID,
                    cdItem:        item.cdItem,
                    nmItem:        item.nmItem,
                    vlrTabela:     item.vlrTabela,
                    vlrPrecoMinimo: item.vlrPrecoMinimo ?? 0,
                    vlrAquisicao:  item.vlrCustoAquisicao,
                    vlrCustoMedio: item.vlrCustoMedio,
                    quantidade:    1
                }]);
            });
        });

        catalogoRenderPaginacao(totalPaginas, total);
    }

    // ── Abrir modal de quantidade com lista de itens ──────────────
    function abrirModalQuantidade(itens) {
        itensSelecionados = itens;
        renderizarModalQuantidade(itensSelecionados);
        bootstrap.Modal.getOrCreate(
            document.getElementById('modalQuantidade')
        ).show();
    }

    // ── Renderizar controles de paginação ─────────────────────────
    function catalogoRenderPaginacao(totalPaginas, totalItens) {
        if (!catalogoPaginationEl) return;

        if (totalPaginas <= 1) {
            catalogoPaginationEl.classList.add('d-none');
            return;
        }

        catalogoPaginationEl.classList.remove('d-none');
        catalogoPaginationEl.innerHTML = '';

        const btnPrev = document.createElement('button');
        btnPrev.type = 'button';
        btnPrev.className = 'page-btn';
        btnPrev.innerHTML = '<i class="fa-solid fa-chevron-left"></i>';
        btnPrev.disabled = catalogoPagina <= 1;
        btnPrev.addEventListener('click', () => catalogoRenderPagina(catalogoPagina - 1));
        catalogoPaginationEl.appendChild(btnPrev);

        const maxBotoes = 5;
        const metade    = Math.floor(maxBotoes / 2);
        let pInicio     = Math.max(1, catalogoPagina - metade);
        let pFim        = Math.min(totalPaginas, pInicio + maxBotoes - 1);
        if (pFim - pInicio < maxBotoes - 1) pInicio = Math.max(1, pFim - maxBotoes + 1);

        for (let p = pInicio; p <= pFim; p++) {
            const btn = document.createElement('button');
            btn.type = 'button';
            btn.className = 'page-btn' + (p === catalogoPagina ? ' active' : '');
            btn.textContent = p;
            btn.dataset.page = p;
            btn.addEventListener('click', function () { catalogoRenderPagina(Number(this.dataset.page)); });
            catalogoPaginationEl.appendChild(btn);
        }

        const btnNext = document.createElement('button');
        btnNext.type = 'button';
        btnNext.className = 'page-btn';
        btnNext.innerHTML = '<i class="fa-solid fa-chevron-right"></i>';
        btnNext.disabled = catalogoPagina >= totalPaginas;
        btnNext.addEventListener('click', () => catalogoRenderPagina(catalogoPagina + 1));
        catalogoPaginationEl.appendChild(btnNext);

        const info = document.createElement('span');
        info.className = 'page-info';
        info.textContent = `${totalItens} produto(s)`;
        catalogoPaginationEl.appendChild(info);
    }

    // ── Pesquisar produtos ────────────────────────────────────────
    if (formSearchProds) {
        formSearchProds.addEventListener('submit', async function (e) {
            e.preventDefault();

            const descricao = inputSearchProds ? inputSearchProds.value.trim() : '';
            if (!descricao) return;

            const btn = this.querySelector('button[type="submit"]');
            const textoOriginal = btn ? btn.innerHTML : '';
            if (btn) {
                btn.disabled = true;
                btn.innerHTML = '<i class="fa-solid fa-spinner fa-spin me-1"></i>Pesquisando...';
            }

            if (tbodyItens) tbodyItens.innerHTML = '<tr><td colspan="6" class="text-center py-3"><i class="fa-solid fa-spinner fa-spin me-1"></i>Pesquisando...</td></tr>';
            if (tfootAdicionarItens) tfootAdicionarItens.classList.add('d-none');
            if (catalogoPaginationEl) catalogoPaginationEl.classList.add('d-none');

            catalogoItens        = [];
            catalogoSelecionados = {};
            catalogoPagina       = 1;

            try {
                const url = new URL(cfg.urls.buscarCatalogo, window.location.origin);
                url.searchParams.set('descricao',         descricao);
                url.searchParams.set('clienteId',         cfg.clienteId);
                url.searchParams.set('tblPrecoId',        cfg.tabelaPrecoId);
                url.searchParams.set('estabelecimentoId', cfg.estabelecimentoId);

                const data = await fetch(url.toString()).then(r => r.json());

                if (!data || data.length === 0) {
                    if (tbodyItens) tbodyItens.innerHTML = `
                        <tr>
                            <td colspan="6" class="text-center py-4">
                                <i class="fa-duotone fa-inbox fa-2x mb-2 text-muted"></i>
                                <div class="text-muted">Nenhum produto encontrado para a descrição informada.</div>
                            </td>
                        </tr>`;
                    return;
                }

                catalogoItens = data;
                catalogoRenderPagina(1);
                if (tfootAdicionarItens) tfootAdicionarItens.classList.remove('d-none');
            } catch {
                mostrarAlerta('Erro', 'Erro ao pesquisar produtos. Tente novamente.', 'error');
            } finally {
                if (btn) { btn.disabled = false; btn.innerHTML = textoOriginal; }
            }
        });
    }

    // ── Coletar todos os selecionados (todas as páginas) ──────────
    function catalogoColetarSelecionados() {
        catalogoSalvarSelecoes();
        return Object.values(catalogoSelecionados).map(s => ({
            itemID:        parseInt(s.itemID),
            cdItem:        s.cdItem,
            nmItem:        s.nmItem,
            vlrTabela:     parseFloat(s.vlrTabela),
            vlrPrecoMinimo: parseFloat(s.vlrPrecoMinimo) || 0,
            vlrAquisicao:  parseFloat(s.vlrAquisicao),
            vlrCustoMedio: parseFloat(s.vlrCustoMedio) || 0,
            quantidade:    1
        }));
    }

    // ── Adicionar Itens Marcados → abrir modal de quantidade ──────
    if (addItensMarcados) {
        addItensMarcados.addEventListener('click', function () {
            const selecionados = catalogoColetarSelecionados();

            if (selecionados.length === 0) {
                mostrarAlerta('Atenção', 'Selecione ao menos um produto.', 'warning');
                return;
            }

            abrirModalQuantidade(selecionados);
        });
    }

    // ── Renderizar modal de quantidade (idêntico ao PrePedido) ───
    function atualizarModalQuantidade() {
        if (!tbodyQuantidadeItens) return;

        tbodyQuantidadeItens.innerHTML = '';

        itensSelecionados.forEach((item, idx) => {
            const tr = document.createElement('tr');
            tr.innerHTML =
                `<td class="small fw-medium text-nowrap">
                    <i class="fa-solid fa-barcode me-1" style="color: var(--sic-muted);"></i>${escHtml(item.cdItem || '—')}
                </td>
                <td class="small">${escHtml(item.nmItem || '—')}</td>
                <td class="text-end small text-nowrap fw-semibold">R$ ${formatarValor(item.vlrTabela)}</td>
                <td class="text-center">
                    <input type="number"
                        class="form-control form-control-sm text-center js-cotacao-qtd-input"
                        min="1" value="${item.quantidade || 1}"
                        data-idx="${idx}"
                        style="width:90px;margin:0 auto;font-weight:700;" />
                </td>`;
            tbodyQuantidadeItens.appendChild(tr);
        });

        const firstInput = tbodyQuantidadeItens.querySelector('.js-cotacao-qtd-input');
        if (firstInput) firstInput.focus();

        if (btnGravarQtdTexto) btnGravarQtdTexto.textContent = 'Adicionar';
    }

    // ── Iniciar fluxo de quantidade (click na linha ou checkbox) ─
    function abrirModalQuantidade(itens) {
        if (!itens || itens.length === 0) return;
        itensSelecionados = itens;
        atualizarModalQuantidade();
        bootstrap.Modal.getOrCreateInstance(
            document.getElementById('modalQuantidade')
        ).show();
    }

    // ── Gravar → fecha modal primeiro, depois posta todos os itens
    if (btnGravarQuantidade) {
        btnGravarQuantidade.addEventListener('click', async function () {
            // Coletar quantidades de todos os inputs do modal
            document.querySelectorAll('#tbodyQuantidadeItens .js-cotacao-qtd-input').forEach(input => {
                const idx = parseInt(input.dataset.idx);
                itensSelecionados[idx].quantidade = Math.max(1, parseInt(input.value) || 1);
            });

            // Fecha modal ANTES de postar (igual ao PrePedido)
            const modalQtd = bootstrap.Modal.getOrCreateInstance(
                document.getElementById('modalQuantidade')
            );
            modalQtd.hide();

            let erros = 0;
            for (const item of itensSelecionados) {
                try {
                    const resp = await fetch(cfg.urls.adicionarItem, {
                        method: 'POST',
                        headers: { 'Content-Type': 'application/json' },
                        body: JSON.stringify({
                            codItemBR:         item.cdItem,
                            descrItemBR:       item.nmItem,
                            tipoCusto:         'A',
                            precoItem:         item.vlrTabela,
                            vlrCustoAquisicao: parseFloat(item.vlrAquisicao) || 0,
                            vlrCustoMedio:     parseFloat(item.vlrCustoMedio) || 0,
                            quantidade:        item.quantidade,
                            vlrPrecoMinimo:    parseFloat(item.vlrPrecoMinimo) || 0,
                            vlrTabelaPreco:    parseFloat(item.vlrTabela) || 0
                        })
                    });
                    if (!resp.ok) erros++;
                } catch {
                    erros++;
                }
            }

            if (erros === 0) {
                await mostrarAlertaAsync('Sucesso', 'Itens adicionados com sucesso!', 'success');
                window.location.reload();
            } else {
                mostrarAlerta('Atenção', `${erros} item(s) não pud${erros === 1 ? 'e' : 'eram'} ser adicionado(s).`, 'warning');
                window.location.reload();
            }
        });
    }

    // ── Enter em input de quantidade aciona o botão Adicionar ────
    const modalQuantidadeEl = document.getElementById('modalQuantidade');
    if (modalQuantidadeEl && btnGravarQuantidade) {
        modalQuantidadeEl.addEventListener('keydown', function (e) {
            if (e.key === 'Enter' && e.target.classList.contains('js-cotacao-qtd-input')) {
                e.preventDefault();
                btnGravarQuantidade.click();
            }
        });
    }

    // ══════════════════════════════════════════════════════════════
    // HELPERS — Total c/ Imp. e highlight de linha
    // ══════════════════════════════════════════════════════════════

    // Retorna a célula "Total c/ Imp." da linha (penúltima td antes de Ações)
    function getTotalCell(tr) {
        if (!tr) return null;
        // A célula de total é a que tem a classe fw-bold text-success
        return tr.querySelector('td.fw-bold.text-success');
    }

    const ICONE_CALCULAR = `<span class="total-calcular-hint text-warning" title="Recalcule a margem para atualizar o total"><i class="fa-solid fa-calculator fa-sm me-1"></i><small>Calcular</small></span>`;

    // Limpa o total e exibe o ícone indicativo na célula
    function limparTotal(tr) {
        const td = getTotalCell(tr);
        if (!td) return;
        td.innerHTML = ICONE_CALCULAR;
    }

    // Aplica o ícone ou limpa conforme o conteúdo atual da célula
    function atualizarIconeTotal(tr) {
        if (!tr) return;
        const td = getTotalCell(tr);
        if (!td) return;
        const texto = td.textContent.trim();
        if (texto === '' || texto === '0,00') {
            td.innerHTML = ICONE_CALCULAR;
        }
    }

    // Aplicar ícone inicial: itensRenderPagina já trata cells zeradas via JS

    // ── Formatação automática do campo Preço Unitário ─────────────
    // Mantém apenas dígitos e formata como valor decimal (ex: 1.234,56)
    function formatarPrecoInput(input) {
        let digits = input.value.replace(/\D/g, '');
        if (!digits) { input.value = ''; return; }
        // Remove zeros à esquerda
        digits = digits.replace(/^0+/, '') || '0';
        // Garante ao menos 3 dígitos (para ter centavos)
        digits = digits.padStart(3, '0');
        const intPart  = digits.slice(0, -2).replace(/^0+/, '') || '0';
        const decPart  = digits.slice(-2);
        // Formata milhar no intPart
        const intFormatado = intPart.replace(/\B(?=(\d{3})+(?!\d))/g, '.');
        input.value = `${intFormatado},${decPart}`;
    }

    // ══════════════════════════════════════════════════════════════
    // EDIÇÃO INLINE — Preço Un. e Quantidade
    // ══════════════════════════════════════════════════════════════

    async function salvarItemInline(propostaItemId, precoUnitario, quantidade, inputEl) {
        const pid = window.cotacaoConfig?.propostaId;
        if (!pid) return;

        const iconEl = document.createElement('i');
        iconEl.className = 'fa-solid fa-spinner fa-spin ms-1';
        inputEl.after(iconEl);
        inputEl.disabled = true;

        try {
            const urlAtualizar = window.cotacaoConfig?.urls?.atualizarItem 
                ? window.cotacaoConfig.urls.atualizarItem.replace('{itemId}', propostaItemId)
                : `/Cotacao/${pid}/itens/${propostaItemId}/atualizar`;
            const resp = await fetch(urlAtualizar, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'X-Requested-With': 'XMLHttpRequest'
                },
                body: JSON.stringify({ precoUnitario, quantidade })
            });

            if (resp.status === 401) { window.location.reload(); return; }

            const text = await resp.text();
            let data;
            try { data = JSON.parse(text); } catch { throw new Error(`Resposta inválida (HTTP ${resp.status})`); }

            if (!resp.ok || !data.success) {
                mostrarAlerta('Erro', data.message || 'Erro ao salvar item.', 'error');
                return;
            }

            window.location.reload();
        } catch (err) {
            console.error('Erro ao salvar item:', err);
            mostrarAlerta('Erro', err.message || 'Erro ao salvar item. Tente novamente.', 'error');
        } finally {
            iconEl.remove();
            inputEl.disabled = false;
        }
    }

    // Preço Unitário — formatação automática ao digitar + limpa Total
    document.addEventListener('input', function (e) {
        const input = e.target;
        if (!input.classList.contains('js-preco-unitario')) return;
        formatarPrecoInput(input);
        const tr = input.closest('tr');
        limparTotal(tr);
    });

    // Preço Unitário — blur com validação de preço mínimo
    document.addEventListener('blur', async function (e) {
        const input = e.target;
        if (!input.classList.contains('js-preco-unitario')) return;

        const propostaItemId = input.dataset.propostaItemId;
        const precoMinimo    = parseFloat(input.dataset.precoMinimo) || 0;
        const rawValue       = input.value.replace(/\./g, '').replace(',', '.');
        const precoUnitario  = parseFloat(rawValue);

        if (isNaN(precoUnitario) || precoUnitario < 0) {
            mostrarAlerta('Atenção', 'Preço inválido.', 'warning');
            input.focus();
            return;
        }

        if (precoUnitario < precoMinimo) {
            mostrarAlerta(
                'Valor abaixo do mínimo',
                `O preço mínimo para este item é R$ ${precoMinimo.toFixed(2).replace('.', ',')}. O valor foi revertido.`,
                'warning'
            );
            // Reverter para o valor mínimo formatado
            input.value = precoMinimo.toFixed(2).replace('.', ',');
            return;
        }

        // Busca a quantidade da mesma linha
        const tr = input.closest('tr');
        const qtdInput = tr ? tr.querySelector('.js-quantidade-item') : null;
        const quantidade = qtdInput ? (parseInt(qtdInput.value) || 1) : 1;

        await salvarItemInline(propostaItemId, precoUnitario, quantidade, input);
    }, true);

    // Quantidade — change com debounce
    let qtdDebounceTimer = null;
    document.addEventListener('change', function (e) {
        const input = e.target;
        if (!input.classList.contains('js-quantidade-item')) return;

        // Limpa total imediatamente ao alterar quantidade
        limparTotal(input.closest('tr'));

        clearTimeout(qtdDebounceTimer);
        qtdDebounceTimer = setTimeout(async function () {

            const propostaItemId = input.dataset.propostaItemId;
            const quantidade     = Math.max(1, parseInt(input.value) || 1);
            input.value = quantidade;

            // Busca o preço unitário da mesma linha
            const tr = input.closest('tr');
            const precoInput = tr ? tr.querySelector('.js-preco-unitario') : null;
            const rawValue   = precoInput ? precoInput.value.replace(/\./g, '').replace(',', '.') : '0';
            const precoUnitario = parseFloat(rawValue) || 0;

            await salvarItemInline(propostaItemId, precoUnitario, quantidade, input);
        }, 400);
    });
    // ══════════════════════════════════════════════════════════════
    // TIPO DE CUSTO — Global e por item
    // ══════════════════════════════════════════════════════════════

    const urlAtualizarCusto = window.cotacaoConfig?.urls?.atualizarCusto;

    // ── Aplica visualmente o tipo de custo numa linha ─────────────
    function aplicarTipoCustoNaLinha(tr, tipo) {
        const select = tr.querySelector('.js-tipo-custo-item');
        const input  = tr.querySelector('.js-custo-display');
        if (!select || !input) return;

        select.value = tipo;

        const vlr = tipo === 'M'
            ? parseFloat(input.dataset.vlrMed  || '0')
            : parseFloat(input.dataset.vlrAqs  || '0');

        input.value = vlr.toLocaleString('pt-BR', { minimumFractionDigits: 2, maximumFractionDigits: 2 });
    }

    // ── Persiste o tipo de custo via AJAX ─────────────────────────
    async function persistirTipoCusto(propostaItemId, tipo, triggerEl) {
        if (!urlAtualizarCusto) return;
        const url = urlAtualizarCusto.replace('{itemId}', propostaItemId);

        if (triggerEl) triggerEl.disabled = true;
        try {
            const resp = await fetch(url, {
                method: 'POST',
                headers: { 'Content-Type': 'application/json', 'X-Requested-With': 'XMLHttpRequest' },
                body: JSON.stringify({ tipoCusto: tipo })
            });
            if (resp.status === 401) { window.location.reload(); return; }
            const data = await resp.json().catch(() => ({}));
            if (!resp.ok || !data.success) {
                mostrarAlerta('Erro', data.message || 'Erro ao atualizar custo.', 'error');
            }
        } catch (err) {
            mostrarAlerta('Erro', err.message || 'Erro ao atualizar custo.', 'error');
        } finally {
            if (triggerEl) triggerEl.disabled = false;
        }
    }

    // ── Alteração de custo por item (select individual) ───────────
    document.addEventListener('change', async function (e) {
        const select = e.target;
        if (!select.classList.contains('js-tipo-custo-item')) return;

        const tipo           = select.value;
        const propostaItemId = select.dataset.propostaItemId;
        const tr             = select.closest('tr');

        aplicarTipoCustoNaLinha(tr, tipo);
        await persistirTipoCusto(propostaItemId, tipo, select);
    });

    // ── Alteração de custo global (botões do dropdown) ────────────
    async function alterarCustoGlobal(tipo) {
        const linhas = [...document.querySelectorAll('tr[data-cot-item-id]')];
        if (!linhas.length) return;

        linhas.forEach(tr => aplicarTipoCustoNaLinha(tr, tipo));

        let erros = 0;
        for (const tr of linhas) {
            const id = tr.dataset.cotItemId;
            if (!id) continue;
            const url = urlAtualizarCusto?.replace('{itemId}', id);
            if (!url) continue;
            try {
                const resp = await fetch(url, {
                    method: 'POST',
                    headers: { 'Content-Type': 'application/json', 'X-Requested-With': 'XMLHttpRequest' },
                    body: JSON.stringify({ tipoCusto: tipo })
                });
                if (resp.status === 401) { window.location.reload(); return; }
                const data = await resp.json().catch(() => ({}));
                if (!resp.ok || !data.success) erros++;
            } catch {
                erros++;
            }
        }

        if (erros > 0) {
            mostrarAlerta('Atenção', `${erros} item(ns) não puderam ser atualizados.`, 'warning');
        }
    }

    document.getElementById('btnAlterarCustoAqs')?.addEventListener('click', function (e) {
        e.preventDefault();
        alterarCustoGlobal('A');
    });

    document.getElementById('btnAlterarCustoMed')?.addEventListener('click', function (e) {
        e.preventDefault();
        alterarCustoGlobal('M');
    });

    // ══════════════════════════════════════════════════════════════
    // GRID ITENS DA COTAÇÃO — Paginação client-side
    // ══════════════════════════════════════════════════════════════

    const podeEditar       = (window.cotacaoConfig?.statusId === 1 || window.cotacaoConfig?.statusId === 14);
    let   itensPorPagina    = 25;
    let   itensPaginaAtual  = 1;
    let   filtroItensTexto  = '';
    const tbodyCotacaoItens = document.getElementById('tbodyCotacaoItens');
    const itensPaginationEl = document.getElementById('itensPagination');
    const inputFiltroItens  = document.getElementById('inputFiltroItens');

    // Itens com totalComImposto === 0 precisam recalcular → ficam no topo
    const todosItensCot = ((window.cotacaoConfig || {}).itens ?? []).slice().sort((a, b) => {
        const aNeed = a.totalComImposto === 0 ? 0 : 1;
        const bNeed = b.totalComImposto === 0 ? 0 : 1;
        return aNeed - bNeed;
    });

    function cotFmtBR(val, dec = 2) {
        return Number(val).toLocaleString('pt-BR', { minimumFractionDigits: dec, maximumFractionDigits: dec });
    }

    function itensRenderPagina(pagina) {
        if (!tbodyCotacaoItens) return;

        const filtro = filtroItensTexto.toLowerCase();
        const itensFiltrados = filtro
            ? todosItensCot.filter(i =>
                (i.codigoProduto  || '').toLowerCase().includes(filtro) ||
                (i.descricaoProduto || '').toLowerCase().includes(filtro))
            : todosItensCot;

        const total        = itensFiltrados.length;
        const totalPaginas = Math.max(1, Math.ceil(total / itensPorPagina));
        itensPaginaAtual   = Math.min(Math.max(pagina, 1), totalPaginas);

        if (total === 0) {
            tbodyCotacaoItens.innerHTML = `
                <tr><td colspan="15" class="text-center py-4">
                    <i class="fa-duotone fa-inbox fa-2x mb-2 text-muted"></i>
                    <div class="text-muted">Nenhum item encontrado para esta cotação.</div>
                </td></tr>`;
            if (itensPaginationEl) itensPaginationEl.classList.add('d-none');
            return;
        }

        const inicio      = (itensPaginaAtual - 1) * itensPorPagina;
        const paginaItens = itensFiltrados.slice(inicio, Math.min(inicio + itensPorPagina, total));

        const ICONE_CALC = `<span class="total-calcular-hint text-warning" title="Recalcule a margem para atualizar o total"><i class="fa-solid fa-calculator fa-sm me-1"></i><small>Calcular</small></span>`;

        let html = '';
        paginaItens.forEach(item => {
            const needsCalc   = item.totalComImposto === 0;
            const custoVal    = item.tipoCusto === 'M' ? item.vlrCustoMedio : item.vlrCustoAquisicao;
            const totalHtml   = needsCalc ? ICONE_CALC : cotFmtBR(item.totalComImposto);
            const margemBadge = needsCalc
                ? `<span class="badge bg-warning-subtle text-warning">-</span>`
                : `<span class="badge bg-success-subtle text-success">${cotFmtBR(item.margemPercentual, 1)}%</span>`;

            html += `
            <tr data-cot-item-id="${item.propostaItemID}">
                <td class="text-center"><input type="checkbox" class="form-check-input check-item-cotacao" data-proposta-item-id="${item.propostaItemID}" /></td>
                <td class="fw-semibold"><a href="${cfg.intranetUrl}/intranet/html/produtos.php?codigo=${encodeURIComponent(item.codigoProduto)}" target="_blank" class="text-primary text-decoration-none" title="Ver produto na Intranet">${escHtml(item.codigoProduto)}</a></td>
                <td><div>${escHtml(item.descricaoProduto)}</div></td>
                <td class="text-center">${cotFmtBR(item.estoqueDisponivel, 0)}</td>
                <td class="text-end">R$ ${cotFmtBR(item.precoMinimo)}</td>
                <td class="text-end">R$ ${cotFmtBR(item.precoTabelaPreco)}</td>
                <td class="text-center">${margemBadge}</td>
                <td>
                    <div class="d-flex align-items-center gap-1">
                        <select class="form-select form-select-sm flex-shrink-0 js-tipo-custo-item" style="width:95px;" data-proposta-item-id="${item.propostaItemID}"${podeEditar ? '' : ' disabled'}>
                            <option value="A"${item.tipoCusto !== 'M' ? ' selected' : ''}>Aquisição</option>
                            <option value="M"${item.tipoCusto === 'M' ? ' selected' : ''}>Médio</option>
                        </select>
                        <input type="text" class="form-control form-control-sm text-end js-custo-display" style="width:90px;"
                            value="${cotFmtBR(custoVal)}"
                            data-vlr-aqs="${item.vlrCustoAquisicao}"
                            data-vlr-med="${item.vlrCustoMedio}"
                            disabled />
                    </div>
                </td>
                <td class="text-end">
                    <input type="text" class="form-control form-control-sm text-end fw-semibold js-preco-unitario" style="width:100px;"
                        value="${cotFmtBR(item.precoUnitario)}"
                        data-proposta-item-id="${item.propostaItemID}"
                        data-preco-minimo="${item.precoMinimo}"${podeEditar ? '' : ' disabled'} />
                </td>
                <td class="text-center">
                    <input type="number" class="form-control form-control-sm text-center js-quantidade-item"
                        value="${item.quantidade}" min="1" style="width:80px;"
                        data-proposta-item-id="${item.propostaItemID}"${podeEditar ? '' : ' disabled'} />
                </td>
                <td class="text-center">${cotFmtBR(item.icms, 1)}%</td>
                <td class="text-end">R$ ${cotFmtBR(item.ipi)}</td>
                <td class="text-end">R$ ${cotFmtBR(item.st)}</td>
                <td class="text-end fw-bold text-success">${needsCalc ? totalHtml : 'R$ ' + cotFmtBR(item.totalComImposto)}</td>
                <td class="text-center">
                    <div class="d-flex gap-1 justify-content-center">
                        <a href="#" class="btn btn-sm btn-outline-info btn-ver-impostos"
                            title="Ver impostos"
                            data-proposta-item-id="${item.propostaItemID}"><i class="fa-solid fa-scale-balanced"></i></a>
                        ${podeEditar ? `<a href="#" class="btn btn-sm btn-outline-danger btn-remover-item"
                            title="Remover item"
                            data-proposta-item-id="${item.propostaItemID}"
                            data-cd-item="${escHtml(item.codigoProduto)}">
                            <i class="fa-solid fa-trash-can"></i>
                        </a>` : ''}
                    </div>
                </td>
            </tr>`;
        });

        tbodyCotacaoItens.innerHTML = html;
        itensRenderPaginacao(totalPaginas, total);

        const chkAll = document.getElementById('checkAllItens');
        if (chkAll) chkAll.checked = false;
    }

    function itensRenderPaginacao(totalPaginas, totalItens) {
        if (!itensPaginationEl) return;

        itensPaginationEl.classList.remove('d-none');
        itensPaginationEl.innerHTML = '';

        // ── Navegação (só se houver mais de 1 página) ─────────────
        if (totalPaginas > 1) {
            const btnPrev = document.createElement('button');
            btnPrev.type = 'button';
            btnPrev.className = 'page-btn';
            btnPrev.innerHTML = '<i class="fa-solid fa-chevron-left"></i>';
            btnPrev.disabled = itensPaginaAtual <= 1;
            btnPrev.addEventListener('click', () => itensRenderPagina(itensPaginaAtual - 1));
            itensPaginationEl.appendChild(btnPrev);

            const maxBotoes = 5;
            const metade    = Math.floor(maxBotoes / 2);
            let pInicio     = Math.max(1, itensPaginaAtual - metade);
            let pFim        = Math.min(totalPaginas, pInicio + maxBotoes - 1);
            if (pFim - pInicio < maxBotoes - 1) pInicio = Math.max(1, pFim - maxBotoes + 1);

            for (let p = pInicio; p <= pFim; p++) {
                const btn = document.createElement('button');
                btn.type = 'button';
                btn.className = 'page-btn' + (p === itensPaginaAtual ? ' active' : '');
                btn.textContent = p;
                btn.dataset.page = p;
                btn.addEventListener('click', function () { itensRenderPagina(Number(this.dataset.page)); });
                itensPaginationEl.appendChild(btn);
            }

            const btnNext = document.createElement('button');
            btnNext.type = 'button';
            btnNext.className = 'page-btn';
            btnNext.innerHTML = '<i class="fa-solid fa-chevron-right"></i>';
            btnNext.disabled = itensPaginaAtual >= totalPaginas;
            btnNext.addEventListener('click', () => itensRenderPagina(itensPaginaAtual + 1));
            itensPaginationEl.appendChild(btnNext);
        }

        // ── Info: exibindo X–Y de Z ────────────────────────────────
        const inicio   = (itensPaginaAtual - 1) * itensPorPagina + 1;
        const fim      = Math.min(itensPaginaAtual * itensPorPagina, totalItens);
        const info     = document.createElement('span');
        info.className = 'page-info';
        info.textContent = totalItens <= itensPorPagina
            ? `${totalItens} item(ns)`
            : `${inicio}–${fim} de ${totalItens}`;
        itensPaginationEl.appendChild(info);

        // ── Separador ─────────────────────────────────────────────
        const sep = document.createElement('span');
        sep.className = 'page-size-sep';
        itensPaginationEl.appendChild(sep);

        // ── Seletor de itens por página ───────────────────────────
        const label = document.createElement('span');
        label.className = 'page-info';
        label.textContent = 'Itens por página:';
        itensPaginationEl.appendChild(label);

        const sel = document.createElement('select');
        sel.className = 'page-size-select';
        sel.title = 'Itens por página';
        [10, 25, 50, 100].forEach(n => {
            const opt = document.createElement('option');
            opt.value = n;
            opt.textContent = n;
            if (n === itensPorPagina) opt.selected = true;
            sel.appendChild(opt);
        });
        sel.addEventListener('change', function () {
            itensPorPagina = Number(this.value);
            itensRenderPagina(1);
        });
        itensPaginationEl.appendChild(sel);
    }

    // Renderizar primeira página ao carregar
    itensRenderPagina(1);

    // Métricas: ocultar totais enquanto houver itens sem cálculo
    (function atualizarMetricasTotais() {
        const semCalculo = todosItensCot.some(i => i.totalComImposto === 0);
        const elSemImp   = document.getElementById('metricTotalSemImposto');
        const elComImp   = document.getElementById('metricTotalComImposto');
        if (!elSemImp || !elComImp) return;
        if (semCalculo) {
            const icone = `<span class="total-calcular-hint text-warning" title="Calcule a margem de todos os itens para obter o total"><i class="fa-solid fa-calculator fa-sm me-1"></i><small>Calcular</small></span>`;
            elSemImp.innerHTML = icone;
            elComImp.innerHTML = icone;
        }
    })();

    // Filtro de itens por código/descrição
    if (inputFiltroItens) {
        let filtroDebounce = null;
        inputFiltroItens.addEventListener('input', function () {
            clearTimeout(filtroDebounce);
            filtroDebounce = setTimeout(() => {
                filtroItensTexto = this.value.trim();
                itensRenderPagina(1);
            }, 250);
        });
    }

    // ══════════════════════════════════════════════════════════════
    // MODAL DE MOTIVO — Exclusão de item(ns)
    // ══════════════════════════════════════════════════════════════

    const modalMotivoEl       = document.getElementById('modalMotivoExclusao');
    const selectMotivo        = document.getElementById('selectMotivoExclusao');
    const btnConfirmarExclusao = document.getElementById('btnConfirmarExclusao');
    const urlRemoverItens     = window.cotacaoConfig?.urls?.removerItens;
    const propostaIdRemocao   = window.cotacaoConfig?.propostaId;

    // Itens pendentes de exclusão (preenchido ao abrir o modal)
    let itensPendentesExclusao = [];

    function abrirModalExclusao(itens) {
        if (!itens.length || !modalMotivoEl) return;
        itensPendentesExclusao = itens;
        if (selectMotivo) selectMotivo.value = '';
        bootstrap.Modal.getOrCreateInstance(modalMotivoEl).show();
    }

    // Botão "Remover Selecionados" (rodapé)
    const btnRemoverSelecionados = document.getElementById('btnRemoverSelecionados');
    if (btnRemoverSelecionados) {
        btnRemoverSelecionados.addEventListener('click', function () {
            const checks = [...document.querySelectorAll('.check-item-cotacao:checked')];
            if (!checks.length) {
                mostrarAlerta('Atenção', 'Selecione ao menos um item para remover.', 'warning');
                return;
            }
            const itens = checks.map(cb => ({
                propostaItemId: parseInt(cb.dataset.propostaItemId),
                cdItem:         cb.dataset.propostaItemId
            }));
            // Enriquecer com cdItem da lista completa
            itens.forEach(it => {
                const found = todosItensCot.find(i => i.propostaItemID === it.propostaItemId);
                if (found) it.cdItem = found.codigoProduto;
            });
            abrirModalExclusao(itens);
        });
    }

    // Botão de remover individual (event delegation na tabela)
    document.addEventListener('click', function (e) {
        const btn = e.target.closest('.btn-remover-item');
        if (!btn) return;
        e.preventDefault();
        abrirModalExclusao([{
            propostaItemId: parseInt(btn.dataset.propostaItemId),
            cdItem:         btn.dataset.cdItem
        }]);
    });

    // ══════════════════════════════════════════════════════════════
    // IMPOSTOS DO ITEM
    // ══════════════════════════════════════════════════════════════

    const modalImpostosEl    = document.getElementById('modalImpostosItem');
    const impostosLoading    = document.getElementById('modalImpostosLoading');
    const impostosConteudo   = document.getElementById('modalImpostosConteudo');
    const tbodyImpostos      = document.getElementById('tbodyImpostos');
    const modalImpostos      = modalImpostosEl ? new bootstrap.Modal(modalImpostosEl) : null;

    document.addEventListener('click', async function (e) {
        const btn = e.target.closest('.btn-ver-impostos');
        if (!btn || !modalImpostos) return;
        e.preventDefault();

        const itemId = btn.dataset.propostaItemId;
        const propostaId = window.cotacaoConfig?.propostaId;

        if (!itemId || !propostaId) return;

        const url = `/Cotacao/${propostaId}/itens/${itemId}/impostos`;

        impostosLoading?.classList.remove('d-none');
        impostosConteudo?.classList.add('d-none');
        if (tbodyImpostos) tbodyImpostos.innerHTML = '';
        modalImpostos.show();

        try {
            const resp = await fetch(url);
            if (!resp.ok) throw new Error('Erro ao carregar impostos.');
            const d = await resp.json();

            if (tbodyImpostos) {
                tbodyImpostos.innerHTML = `
                    <tr>
                        <td class="text-center">${escHtml(d.codItemBR)}</td>
                        <td class="text-center">${escHtml(d.mb)}</td>
                        <td class="text-center">${escHtml(d.vlrLiqUnit)}</td>
                        <td class="text-center">${escHtml(d.percICMS)}</td>
                        <td class="text-center">${escHtml(d.vlrICMS)}</td>
                        <td class="text-center">${escHtml(d.percIPI)}</td>
                        <td class="text-center">${escHtml(d.vlrIPI)}</td>
                        <td class="text-center">${escHtml(d.vlrFCP)}</td>
                        <td class="text-center">${escHtml(d.percPIS)}</td>
                        <td class="text-center">${escHtml(d.vlrPIS)}</td>
                        <td class="text-center">${escHtml(d.percCOFINS)}</td>
                        <td class="text-center">${escHtml(d.vlrCOFINS)}</td>
                        <td class="text-center">${escHtml(d.mva)}</td>
                        <td class="text-center">${escHtml(d.st)}</td>
                        <td class="text-center">${escHtml(d.vlrFCPST)}</td>
                        <td class="text-center">${escHtml(d.vlrICMSPartOrigem)}</td>
                        <td class="text-center">${escHtml(d.vlrICMSPartDestino)}</td>
                    </tr>`;
            }

            impostosLoading?.classList.add('d-none');
            impostosConteudo?.classList.remove('d-none');
        } catch {
            impostosLoading?.classList.add('d-none');
            if (tbodyImpostos) {
                tbodyImpostos.innerHTML = '<tr><td colspan="17" class="text-center text-danger">Erro ao carregar impostos.</td></tr>';
            }
            impostosConteudo?.classList.remove('d-none');
        }
    });

    // Confirmar exclusão
    if (btnConfirmarExclusao) {
        btnConfirmarExclusao.addEventListener('click', async function () {
            const motivo = selectMotivo ? selectMotivo.value.trim() : '';
            if (!motivo) {
                selectMotivo?.classList.add('is-invalid');
                mostrarAlerta('Atenção', 'Selecione um motivo antes de remover.', 'warning');
                return;
            }
            selectMotivo?.classList.remove('is-invalid');

            if (!urlRemoverItens || !itensPendentesExclusao.length) return;

            const btn = this;
            const textoOriginal = btn.innerHTML;
            btn.disabled = true;
            btn.innerHTML = '<i class="fa-solid fa-spinner fa-spin me-1"></i>Removendo...';

            try {
                const resp = await fetch(urlRemoverItens, {
                    method: 'POST',
                    headers: {
                        'Content-Type': 'application/json',
                        'X-Requested-With': 'XMLHttpRequest'
                    },
                    body: JSON.stringify({
                        itens:  itensPendentesExclusao,
                        motivo: motivo
                    })
                });

                if (resp.status === 401) { window.location.reload(); return; }

                const data = await resp.json().catch(() => ({}));

                if (resp.ok && data.success) {
                    bootstrap.Modal.getOrCreateInstance(modalMotivoEl).hide();
                    await mostrarAlertaAsync('Removido!', data.message || 'Item(ns) removido(s) com sucesso.', 'success');

                    // Recalcula margem de todos os itens restantes após a exclusão
                    const pid = window.cotacaoConfig?.propostaId;
                    if (pid) {
                        const urlRecalcularMargem = window.cotacaoConfig?.urls?.calcularMargem || `/Cotacao/${pid}/itens/0/calcular-margem`;
                        await fetch(urlRecalcularMargem, {
                            method: 'POST',
                            headers: { 'Content-Type': 'application/json' },
                            body: JSON.stringify({ type: 'preco', viaTela: 'NAO' })
                        }).catch(() => {});
                    }

                    window.location.reload();
                } else {
                    mostrarAlerta('Erro', data.message || 'Não foi possível remover o(s) item(ns).', 'error');
                }
            } catch {
                mostrarAlerta('Erro', 'Falha na comunicação com o servidor.', 'error');
            } finally {
                btn.disabled = false;
                btn.innerHTML = textoOriginal;
            }
        });
    }

    // Limpar validação ao trocar o select
    if (selectMotivo) {
        selectMotivo.addEventListener('change', function () {
            this.classList.remove('is-invalid');
        });
    }

    // ══════════════════════════════════════════════════════════════
    // IMPORTAR ITENS VIA EXCEL
    // ══════════════════════════════════════════════════════════════

    const inputImportarExcel    = document.getElementById('inputImportarExcel');
    const btnConfirmarImportacao = document.getElementById('btnConfirmarImportacao');
    const importarPreviewArea   = document.getElementById('importarPreviewArea');
    const importarPreviewTexto  = document.getElementById('importarPreviewTexto');
    const modalImportarItens    = document.getElementById('modalImportarItens');

    let dadosImportacaoExcel = [];

    if (inputImportarExcel) {
        inputImportarExcel.addEventListener('change', function (event) {
            dadosImportacaoExcel = [];
            btnConfirmarImportacao?.classList.add('d-none');
            importarPreviewArea?.classList.add('d-none');

            const file = event.target.files[0];
            if (!file) return;

            const reader = new FileReader();
            reader.onload = function (e) {
                try {
                    const data = new Uint8Array(e.target.result);
                    const workbook = XLSX.read(data, { type: 'array' });
                    const worksheet = workbook.Sheets[workbook.SheetNames[0]];
                    const linhas = XLSX.utils.sheet_to_json(worksheet, { raw: false, defval: '' });

                    dadosImportacaoExcel = linhas.filter(l => String(l.CdItem ?? '').trim() !== '');

                    if (dadosImportacaoExcel.length === 0) {
                        mostrarAlerta('Atenção', 'Nenhum item válido encontrado. Verifique se o arquivo possui a coluna "CdItem".', 'warning');
                        return;
                    }

                    if (importarPreviewTexto) importarPreviewTexto.textContent = `${dadosImportacaoExcel.length} item(ns) encontrado(s) no arquivo.`;
                    importarPreviewArea?.classList.remove('d-none');
                    btnConfirmarImportacao?.classList.remove('d-none');
                } catch {
                    mostrarAlerta('Erro', 'Não foi possível ler o arquivo Excel. Verifique o formato.', 'error');
                }
            };
            reader.readAsArrayBuffer(file);
        });
    }

    if (btnConfirmarImportacao) {
        btnConfirmarImportacao.addEventListener('click', async function () {
            const urlValidar   = window.cotacaoConfig?.urls?.validarItensImportacao;
            const urlAdicionar = window.cotacaoConfig?.urls?.adicionarItem;
            if (!urlValidar || !urlAdicionar || dadosImportacaoExcel.length === 0) return;

            // Fechar modal e exibir loading
            if (modalImportarItens) bootstrap.Modal.getInstance(modalImportarItens)?.hide();

            Swal.fire({ title: 'Importando itens...', html: '<span class="contador-import">Aguarde...</span>', allowOutsideClick: false, didOpen: () => Swal.showLoading() });

            try {
                // 1. Buscar itens da proposta para cruzar com o Excel
                const respValidar = await fetch(urlValidar, {
                    headers: { 'X-Requested-With': 'XMLHttpRequest' }
                });
                if (respValidar.status === 401) {
                    window.location.href = '/Account/Login';
                    return;
                }
                if (!respValidar.ok) throw new Error('Erro ao carregar itens da proposta.');
                const contentType = respValidar.headers.get('content-type') ?? '';
                if (!contentType.includes('application/json')) throw new Error('Resposta inesperada do servidor. Tente novamente.');
                const listaProposta = await respValidar.json();

                const erros = [];
                let contador = 0;
                const total  = dadosImportacaoExcel.length;

                // 2. Processar cada linha do Excel
                for (const linha of dadosImportacaoExcel) {
                    const cdItem = String(linha.CdItem ?? '').trim();
                    const itemProposta = listaProposta.find(i => String(i.cdItem ?? '').trim() === cdItem);

                    contador++;
                    const spanContador = document.querySelector('.contador-import');
                    if (spanContador) spanContador.textContent = `${contador} de ${total}`;

                    if (!itemProposta) {
                        erros.push({ cdItem, erro: 'Item não encontrado na proposta.' });
                        continue;
                    }

                    const quantidade  = parseInt(linha.Quantidade) || 1;
                    const vlrTabela   = String(linha.VlrTabela ?? '').trim();
                    const precoItem   = vlrTabela !== ''
                        ? parseFloat(vlrTabela.replace(',', '.')) || itemProposta.vlrUnit
                        : itemProposta.vlrUnit;

                    try {
                        const respAdd = await fetch(urlAdicionar, {
                            method: 'POST',
                            headers: {
                                'Content-Type': 'application/json',
                                'X-Requested-With': 'XMLHttpRequest',
                                'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]')?.value ?? ''
                            },
                            body: JSON.stringify({
                                codItemBR:        cdItem,
                                descrItemBR:      itemProposta.nmItem,
                                tipoCusto:        'A',
                                precoItem:        precoItem,
                                vlrCustoAquisicao: itemProposta.vlrCustoAquisicao,
                                vlrCustoMedio:    itemProposta.vlrCustoMedio,
                                quantidade:       quantidade,
                                vlrPrecoMinimo:   itemProposta.vlrPrecoMinimo ?? 0,
                                vlrTabelaPreco:   itemProposta.vlrUnit ?? 0
                            })
                        });
                        const resData = await respAdd.json();
                        if (!respAdd.ok || !resData.success) {
                            erros.push({ cdItem, erro: resData.message || resData.error || 'Erro ao adicionar item.' });
                        }
                    } catch {
                        erros.push({ cdItem, erro: 'Falha na comunicação com o servidor.' });
                    }
                }

                Swal.close();

                // 3. Exibir resultado
                if (erros.length > 0) {
                    const linhasErro = erros.map(e => `<tr><td>${escHtml(e.cdItem)}</td><td class="text-danger">${escHtml(e.erro)}</td></tr>`).join('');
                    await Swal.fire({
                        title: 'Importação concluída com erros',
                        icon: 'warning',
                        width: '500px',
                        html: `<table class="table table-sm table-bordered text-start">
                                   <thead><tr><th>CdItem</th><th>Erro</th></tr></thead>
                                   <tbody>${linhasErro}</tbody>
                               </table>`,
                        confirmButtonText: 'Fechar'
                    });
                } else {
                    await Swal.fire({ icon: 'success', title: 'Importação concluída!', text: 'Todos os itens foram importados com sucesso.', timer: 2000, showConfirmButton: false });
                }

                window.location.reload();
            } catch (err) {
                Swal.close();
                mostrarAlerta('Erro', err.message || 'Ocorreu um erro durante a importação.', 'error');
            }
        });
    }

    // Limpar estado ao fechar o modal de importação
    if (modalImportarItens) {
        modalImportarItens.addEventListener('hidden.bs.modal', function () {
            dadosImportacaoExcel = [];
            if (inputImportarExcel) inputImportarExcel.value = '';
            btnConfirmarImportacao?.classList.add('d-none');
            importarPreviewArea?.classList.add('d-none');
        });
    }

})();
