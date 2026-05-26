document.addEventListener('DOMContentLoaded', function () {
    const cfg = window.prePedidoConfig;
    if (!cfg) return;

    const feedback = document.getElementById('prePedidoFeedback');
    const cnpjSelect = document.getElementById('prePedidoCnpj');
    const enderecoSelect = document.getElementById('prePedidoEndereco');
    const localEntregaSelect = document.getElementById('prePedidoLocalEntrega');
    const conteudoModal = document.getElementById('conteudoArquivoModal');
    const conteudoStatus = document.getElementById('conteudoArquivoStatus');
    const conteudoPre = document.getElementById('conteudoArquivoPre');
    const trocarItemModal = document.getElementById('trocarItemModal');
    const tomSelectInstances = [];

    function showFeedback(success, message) {
        if (!feedback) return;

        feedback.innerHTML = '<div class="alert ' + (success ? 'alert-success' : 'alert-danger') + ' alert-dismissible fade show" role="alert">'
            + message
            + '<button type="button" class="btn-close" data-bs-dismiss="alert" aria-label="Fechar"></button>'
            + '</div>';
    }

    function syntaxHighlightJson(jsonStr) {
        var escaped = jsonStr.replace(/&/g, '&amp;').replace(/</g, '&lt;').replace(/>/g, '&gt;');
        return escaped.replace(
            /("(\\u[\da-fA-F]{4}|\\[^u]|[^\\"])*"(\s*:)?|\b(true|false|null)\b|-?\d+(?:\.\d*)?(?:[eE][+\-]?\d+)?)/g,
            function (match) {
                var cls = 'json-number';
                if (/^"/.test(match)) {
                    if (/:$/.test(match)) {
                        cls = 'json-key';
                    } else {
                        cls = 'json-string';
                    }
                } else if (/true|false/.test(match)) {
                    cls = 'json-boolean';
                } else if (/null/.test(match)) {
                    cls = 'json-null';
                }
                return '<span class="' + cls + '">' + match + '</span>';
            }
        );
    }

    function applyJsonHighlight(preElement, rawText) {
        try {
            var parsed = JSON.parse(rawText);
            var formatted = JSON.stringify(parsed, null, 2);
            preElement.innerHTML = syntaxHighlightJson(formatted);
        } catch {
            preElement.textContent = rawText;
        }
    }

    async function sendJson(url, method, payload) {
        const response = await fetch(url, {
            method: method,
            headers: {
                'Content-Type': 'application/json',
                'X-Requested-With': 'XMLHttpRequest'
            },
            body: JSON.stringify(payload)
        });

        return await response.json();
    }

    async function processAction(url, method, payload, reloadOnSuccess) {
        try {
            const result = await sendJson(url, method, payload);
            showFeedback(result.success, result.message || 'Operação concluída.');

            if (result.success && reloadOnSuccess) {
                window.location.reload();
            }

            return result;
        }
        catch {
            showFeedback(false, 'Não foi possível concluir a operação.');
            return { success: false };
        }
    }

    function initSearchableSelect(select) {
        if (!select || typeof TomSelect === 'undefined')
            return null;

        var instance = new TomSelect(select, {
            create: false,
            allowEmptyOption: true,
            maxOptions: null,
            placeholder: select.dataset.placeholder || 'Digite para selecionar',
            searchField: ['text'],
            sortField: [{ field: 'text', direction: 'asc' }],
            onFocus: function () {
                this._clearOnNextType = true;
            },
            onType: function () {
                // Na primeira tecla: remove o item selecionado silenciosamente,
                // deixando só o placeholder + o que o usuário está digitando
                if (this._clearOnNextType) {
                    this._clearOnNextType = false;
                    this.clear(true);
                }
            },
            onBlur: function () {
                this._clearOnNextType = false;
                // Se o usuário saiu sem confirmar nova seleção, restaura o valor original
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

        tomSelectInstances.push(instance);
        return instance;
    }

    document.querySelectorAll('.js-prepedido-searchable-select').forEach(initSearchableSelect);

    document.querySelector('.js-prepedido-reprocessar')?.addEventListener('click', function () {
        processAction(cfg.endpoints.reprocessar, 'POST', {}, true);
    });

    document.querySelector('.js-prepedido-cancelar')?.addEventListener('click', function () {
        processAction(cfg.endpoints.cancelar, 'POST', {}, true);
    });

    document.querySelector('.js-prepedido-aceitar')?.addEventListener('click', function () {
        processAction(cfg.endpoints.aceitar, 'POST', {}, true);
    });

    function atualizarBotaoAceitar() {
        var btn = document.querySelector('.js-prepedido-aceitar');
        if (!btn) return;

        var cnpjOk = cnpjSelect && cnpjSelect.value && cnpjSelect.value.trim() !== '';
        var enderecoOk = enderecoSelect && Number(enderecoSelect.value || 0) > 0;
        var localEntregaOk = localEntregaSelect && Number(localEntregaSelect.value || 0) > 0;
        var cotacaoOk = cfg.cotacaoId === 0;
        var precosOk = cfg.todosItensComPreco === true;
        var totalOk = cfg.totalPedidoValor >= cfg.vlrMinimo;

        btn.style.display = (cnpjOk && enderecoOk && localEntregaOk && cotacaoOk && precosOk && totalOk) ? '' : 'none';
    }

    atualizarBotaoAceitar();

    async function updateCnpj() {
        const value = (cnpjSelect?.value || '').trim();
        const lastValue = cnpjSelect?.dataset.lastValue || '';

        if (!value || value === lastValue)
            return;

        const result = await processAction(cfg.endpoints.atualizarCnpj, 'PUT', { cnpj: value }, false);
        if (result.success) {
            cnpjSelect.dataset.lastValue = value;
            window.location.reload();
        }
    }

    async function updateEndereco() {
        const value = Number(enderecoSelect?.value || 0);
        const lastValue = Number(enderecoSelect?.dataset.lastValue || 0);

        if (!value || value === lastValue)
            return;

        const option = enderecoSelect?.selectedOptions?.[0];
        const clienteEnderecoId = Number(enderecoSelect?.value || 0);
        const result = await processAction(cfg.endpoints.atualizarEndereco, 'PUT', {
            clienteEnderecoID: clienteEnderecoId,
            logradouro: option?.dataset?.logradouro || option?.text || ''
        }, false);

        if (result.success) {
            enderecoSelect.dataset.lastValue = String(clienteEnderecoId);
            window.location.reload();
        }
    }

    async function updateLocalEntrega() {
        const value = Number(localEntregaSelect?.value || 0);
        const lastValue = Number(localEntregaSelect?.dataset.lastValue || 0);

        if (!value || value === lastValue)
            return;

        const option = localEntregaSelect?.selectedOptions?.[0];
        const clienteLocalEntregaId = Number(localEntregaSelect?.value || 0);
        const result = await processAction(cfg.endpoints.atualizarLocalEntrega, 'PUT', {
            clienteLocalEntregaID: clienteLocalEntregaId,
            nmLocalEntrega: option?.dataset?.local || option?.text || ''
        }, false);

        if (result.success) {
            localEntregaSelect.dataset.lastValue = String(clienteLocalEntregaId);
            window.location.reload();
        }
    }

    cnpjSelect?.addEventListener('change', updateCnpj);
    enderecoSelect?.addEventListener('change', updateEndereco);
    localEntregaSelect?.addEventListener('change', updateLocalEntrega);

    // ── Lápis de edição dos campos de mapeamento ──
    document.querySelectorAll('.js-mapping-edit-toggle').forEach(function (btn) {
        btn.addEventListener('click', function () {
            var wrapperId = btn.dataset.target;
            var displayId = btn.dataset.display;
            var wrapper = document.getElementById(wrapperId);
            var display = document.getElementById(displayId);
            if (!wrapper) return;

            wrapper.classList.remove('d-none');
            if (display) display.classList.add('d-none');
            btn.classList.add('d-none');

            // Foca o Tom Select do wrapper se existir
            var select = wrapper.querySelector('select');
            if (select && select.tomselect) {
                select.tomselect.focus();
            } else if (select) {
                select.focus();
            }
        });
    });

    // Ao salvar (change já chama reload), mas se o usuário pressionar Escape
    // no Tom Select, volta ao modo exibição sem salvar
    [cnpjSelect, enderecoSelect, localEntregaSelect].forEach(function (select) {
        if (!select) return;
        select.addEventListener('keydown', function (e) {
            if (e.key !== 'Escape') return;
            var wrapper = select.closest('[id$="Wrapper"]');
            if (!wrapper) return;
            var displayId = wrapper.id.replace('Wrapper', 'Display');
            var display = document.getElementById(displayId);
            var toggleBtn = document.querySelector('.js-mapping-edit-toggle[data-target="' + wrapper.id + '"]');
            wrapper.classList.add('d-none');
            if (display) display.classList.remove('d-none');
            if (toggleBtn) toggleBtn.classList.remove('d-none');
        });
    });

    document.querySelectorAll('.js-prepedido-quantidade').forEach(function (input) {
        async function updateQuantidade() {
            const itemId = input.dataset.itemId;
            const descricao = input.dataset.descricao || '';
            const newValue = Number(input.value || 0);
            const lastValue = Number(input.dataset.lastValue || 0);

            if (newValue < 1 || newValue === lastValue)
                return;

            const result = await processAction(cfg.endpoints.atualizarQuantidadeBase + '/itens/' + itemId + '/quantidade', 'PUT', {
                quantidade: newValue,
                descricao: descricao
            }, false);

            if (result.success) {
                input.dataset.lastValue = String(newValue);
                window.location.reload();
            }
        }

        input.addEventListener('blur', updateQuantidade);
        input.addEventListener('keydown', function (event) {
            if (event.key === 'Enter') {
                event.preventDefault();
                input.blur();
            }
        });
    });

    document.querySelectorAll('.js-prepedido-vlrunit').forEach(function (input) {
        // Máscara monetária: apenas dígitos, formata como 0,00
        function aplicarMascara(raw) {
            var digits = raw.replace(/\D/g, '');
            if (!digits) return '0,00';
            var num = parseInt(digits, 10);
            return (num / 100).toFixed(2).replace('.', ',');
        }

        function parseValor(formatted) {
            return parseFloat((formatted || '0').replace('.', '').replace(',', '.'));
        }

        // Aplica máscara ao digitar
        input.addEventListener('input', function () {
            var pos = input.selectionStart;
            input.value = aplicarMascara(input.value);
        });

        // Ao focar, seleciona tudo para facilitar substituição
        input.addEventListener('focus', function () {
            input.select();
        });

        async function updateVlrUnit() {
            const itemId = input.dataset.itemId;
            const descricao = input.dataset.descricao || '';
            const newValue = parseValor(input.value);
            const lastValue = parseValor(input.dataset.lastValue || '0,00');

            if (isNaN(newValue) || newValue < 0 || newValue === lastValue)
                return;

            const result = await processAction(cfg.endpoints.atualizarVlrUnitBase + '/itens/' + itemId + '/vlr-unit', 'PUT', {
                vlrUnit: newValue,
                descricao: descricao
            }, false);

            if (result.success) {
                input.dataset.lastValue = aplicarMascara(String(Math.round(newValue * 100)));
                window.location.reload();
            }
        }

        input.addEventListener('blur', updateVlrUnit);
        input.addEventListener('keydown', function (event) {
            if (event.key === 'Enter') {
                event.preventDefault();
                input.blur();
            }
        });
    });

    // ── Inline edit: Obs. Nota / Obs. Comprador ──
    document.querySelectorAll('.js-obs-edit-toggle').forEach(function (btn) {
        btn.addEventListener('click', function () {
            const targetId = btn.dataset.target;
            const textarea = document.getElementById(targetId);
            if (!textarea) return;

            const displayId = textarea.dataset.display;
            const display = displayId ? document.getElementById(displayId) : null;

            textarea.classList.remove('d-none');
            if (display) display.classList.add('d-none');
            btn.classList.add('d-none');
            textarea.focus();
            textarea.setSelectionRange(textarea.value.length, textarea.value.length);
        });
    });

    document.querySelectorAll('.js-prepedido-obs').forEach(function (textarea) {
        async function salvarObs() {
            const obsNota = (document.getElementById('inputObsNota')?.value || '').trim();
            const obsComprador = (document.getElementById('inputObsComprador')?.value || '').trim();
            const lastNota = (document.getElementById('inputObsNota')?.dataset.lastValue || '').trim();
            const lastComprador = (document.getElementById('inputObsComprador')?.dataset.lastValue || '').trim();

            const displayId = textarea.dataset.display;
            const display = displayId ? document.getElementById(displayId) : null;
            const toggleBtn = document.querySelector('.js-obs-edit-toggle[data-target="' + textarea.id + '"]');

            // Volta para modo leitura
            textarea.classList.add('d-none');
            if (display) display.classList.remove('d-none');
            if (toggleBtn) toggleBtn.classList.remove('d-none');

            if (obsNota === lastNota && obsComprador === lastComprador)
                return;

            const result = await processAction(cfg.endpoints.atualizarObs, 'PUT', {
                obsNota: obsNota,
                obsComprador: obsComprador
            }, false);

            if (result.success) {
                const notaEl = document.getElementById('inputObsNota');
                const compradorEl = document.getElementById('inputObsComprador');
                const notaDisplay = document.getElementById('inputObsNotaDisplay');
                const compradorDisplay = document.getElementById('inputObsCompradorDisplay');

                if (notaEl) notaEl.dataset.lastValue = obsNota;
                if (compradorEl) compradorEl.dataset.lastValue = obsComprador;
                if (notaDisplay) notaDisplay.textContent = obsNota || '—';
                if (compradorDisplay) compradorDisplay.textContent = obsComprador || '—';
            }
        }

        textarea.addEventListener('blur', salvarObs);
        textarea.addEventListener('keydown', function (event) {
            if (event.key === 'Escape') {
                // Cancela edição sem salvar — restaura valor original
                textarea.value = textarea.dataset.lastValue || '';
                textarea.blur();
            }
        });
    });

    document.querySelectorAll('.js-prepedido-excluir-item').forEach(function (button) {
        button.addEventListener('click', function () {
            processAction(cfg.endpoints.atualizarQuantidadeBase + '/itens/' + button.dataset.itemId + '/excluir', 'POST', {
                descricao: button.dataset.descricao || ''
            }, true);
        });
    });

    // ── Conteúdo do Arquivo (proxy fallback quando não pré-carregado) ──
    if (conteudoModal && !cfg.conteudoPreCarregado) {
        conteudoModal.addEventListener('show.bs.modal', async function () {
            if (conteudoStatus) conteudoStatus.textContent = 'Carregando conteúdo do arquivo...';
            if (conteudoPre) conteudoPre.textContent = 'Aguardando carregamento...';

            try {
                var url = cfg.endpoints.conteudoArquivo
                    + '?cdExtCliente=' + encodeURIComponent(cfg.cdExtCliente)
                    + '&ordemCompra=' + encodeURIComponent(cfg.ordemCompra);
                var response = await fetch(url);
                var data = await response.json();

                if (data.success && data.conteudo) {
                    if (conteudoPre) applyJsonHighlight(conteudoPre, data.conteudo);
                    if (conteudoStatus) conteudoStatus.textContent = 'Conteúdo carregado.';
                } else {
                    if (conteudoStatus) conteudoStatus.textContent = 'Não foi possível carregar o conteúdo do arquivo.';
                    if (conteudoPre) conteudoPre.textContent = '';
                }
            }
            catch {
                if (conteudoStatus) conteudoStatus.textContent = 'Erro ao carregar o conteúdo do arquivo.';
                if (conteudoPre) conteudoPre.textContent = '';
            }
        });
    }

    // ── Conteúdo do Arquivo pré-carregado: aplicar syntax highlight ──
    if (conteudoPre && cfg.conteudoPreCarregado && window.__conteudoArquivoRaw) {
        applyJsonHighlight(conteudoPre, window.__conteudoArquivoRaw);
    }

    // ── Trocar Item ──
    var tomSelectSubstituto = null;
    var promessaItensSubstituto = null;

    if (trocarItemModal) {
        trocarItemModal.addEventListener('show.bs.modal', function (event) {
            var btn = event.relatedTarget;
            if (!btn) return;

            // Info display
            document.getElementById('codigoTrocaItem').textContent = btn.dataset.itemBrsupply || btn.dataset.cdItem || '';
            document.getElementById('quantidadeTrocaItem').textContent = btn.dataset.pdfQtde || '';
            document.getElementById('valorTrocaItem').textContent = btn.dataset.vlrTblPrecoFormat || '';

            // Hidden inputs — old item
            document.getElementById('trocarItemPDFItemID').value = btn.dataset.itemId || '';
            document.getElementById('CdItemAntigo').value = btn.dataset.cdItem || '';
            document.getElementById('ItemIDAntigo').value = btn.dataset.itemInternoId || '';
            document.getElementById('DescricaoAntiga').value = btn.dataset.descricao || '';
            document.getElementById('ValorAntigo').value = btn.dataset.vlrTblPrecoFormat || '';

            // Clear new item
            document.getElementById('itemIDNovo').value = '';
            document.getElementById('nmItemNovo').value = '';
            document.getElementById('tblPrecoNovo').value = '';
            document.getElementById('cdItemNovo').value = '';
            document.getElementById('motivoTrocaItem').value = '';

            // Destruir Tom Select anterior antes de repopular
            if (tomSelectSubstituto) {
                tomSelectSubstituto.destroy();
                tomSelectSubstituto = null;
            }

            var select = document.getElementById('itemSubstituto');
            select.innerHTML = '<option value="">Carregando...</option>';

            var url = cfg.endpoints.trocaItens
                + '?tblPrecoId=' + cfg.tblPrecoId
                + '&estabelecimentoId=' + cfg.estabelecimentoId
                + '&segmentoId=' + (btn.dataset.segmentoId || 0)
                + '&familiaId=' + (btn.dataset.familiaId || 0)
                + '&itemId=' + (btn.dataset.itemInternoId || 0);

            // Guarda a promise para ser aguardada no shown.bs.modal
            promessaItensSubstituto = fetch(url)
                .then(function (resp) { return resp.json(); })
                .then(function (items) {
                    select.innerHTML = '<option value="">Selecione</option>';
                    items.forEach(function (item) {
                        var opt = document.createElement('option');
                        opt.value = item.cdItem;
                        opt.textContent = item.cdItem + ' - ' + item.nmItem + ' (R$ ' + Number(item.vlrTabelaPreco).toFixed(2) + ')';
                        opt.dataset.itemId = item.itemID;
                        opt.dataset.nmItem = item.nmItem;
                        opt.dataset.vlrTabelaPreco = item.vlrTabelaPreco;
                        select.appendChild(opt);
                    });
                })
                .catch(function () {
                    select.innerHTML = '<option value="">Erro ao carregar substitutos</option>';
                });
        });

        // shown.bs.modal — aguarda o AJAX e inicializa Tom Select após options prontas
        trocarItemModal.addEventListener('shown.bs.modal', function () {
            var select = document.getElementById('itemSubstituto');
            Promise.resolve(promessaItensSubstituto).then(function () {
                if (tomSelectSubstituto) {
                    tomSelectSubstituto.destroy();
                    tomSelectSubstituto = null;
                }
                if (typeof TomSelect !== 'undefined') {
                    tomSelectSubstituto = new TomSelect(select, {
                        create: false,
                        allowEmptyOption: true,
                        maxOptions: null,
                        placeholder: 'Digite para pesquisar...',
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
                        onChange: function (value) {
                            var nativeOpt = select.querySelector('option[value="' + String(value).replace(/\\/g, '\\\\').replace(/"/g, '\\"') + '"]');
                            if (value && nativeOpt) {
                                document.getElementById('cdItemNovo').value = value;
                                document.getElementById('itemIDNovo').value = nativeOpt.dataset.itemId || '';
                                document.getElementById('nmItemNovo').value = nativeOpt.dataset.nmItem || '';
                                document.getElementById('tblPrecoNovo').value = nativeOpt.dataset.vlrTabelaPreco || '';
                            } else {
                                document.getElementById('cdItemNovo').value = '';
                                document.getElementById('itemIDNovo').value = '';
                                document.getElementById('nmItemNovo').value = '';
                                document.getElementById('tblPrecoNovo').value = '';
                            }
                        }
                    });
                }
                promessaItensSubstituto = null;
            });
        });

        // Destruir Tom Select ao fechar o modal
        trocarItemModal.addEventListener('hidden.bs.modal', function () {
            if (tomSelectSubstituto) {
                tomSelectSubstituto.destroy();
                tomSelectSubstituto = null;
            }
        });

        // Gravar troca
        document.getElementById('gravarTrocaItem').addEventListener('click', async function () {
            var cdItemNovo = document.getElementById('cdItemNovo').value;
            var motivo = document.getElementById('motivoTrocaItem').value.trim();

            if (!cdItemNovo) {
                showFeedback(false, 'Selecione um item substituto.');
                return;
            }
            if (!motivo) {
                showFeedback(false, 'Informe o motivo da troca.');
                return;
            }

            var pdfItemId = document.getElementById('trocarItemPDFItemID').value;
            var url = cfg.endpoints.atualizarQuantidadeBase + '/itens/' + pdfItemId + '/trocar';

            await processAction(url, 'POST', {
                cdItem: cdItemNovo,
                itemID: Number(document.getElementById('itemIDNovo').value),
                nmItem: document.getElementById('nmItemNovo').value,
                vlrTabelaPreco: Number(document.getElementById('tblPrecoNovo').value),
                cdItemAntigo: document.getElementById('CdItemAntigo').value,
                descricaoAntiga: document.getElementById('DescricaoAntiga').value,
                valorAntigo: document.getElementById('ValorAntigo').value,
                motivoTrocaItem: motivo
            }, true);
        });
    }

    // ── Buscar Catálogo (Adicionar Item) — com paginação client-side ──
    var formSearchProds = document.getElementById('formSearchProds');
    var tbodyItens = document.getElementById('tbodyItens');
    var tfootAdicionarItens = document.getElementById('tfootAdicionarItens');
    var catalogoPaginationEl = document.getElementById('catalogoPagination');

    var CATALOGO_POR_PAGINA = 10;
    var catalogoItens = [];
    var catalogoPagina = 1;
    var catalogoSelecionados = {};

    function catalogoSalvarSelecoes() {
        document.querySelectorAll('.catalogo-check').forEach(function (cb) {
            var key = cb.dataset.cdItem + '_' + cb.dataset.itemId;
            if (cb.checked) {
                catalogoSelecionados[key] = {
                    cdItem: cb.dataset.cdItem,
                    nmItem: cb.dataset.nmItem,
                    vlrTabela: cb.dataset.vlrTabela,
                    itemDePara: cb.dataset.itemDePara,
                    itemId: cb.dataset.itemId
                };
            } else {
                delete catalogoSelecionados[key];
            }
        });
    }

    function catalogoRenderPagina(pagina) {
        catalogoSalvarSelecoes();
        catalogoPagina = pagina;

        var total = catalogoItens.length;
        var totalPaginas = Math.ceil(total / CATALOGO_POR_PAGINA);
        if (catalogoPagina > totalPaginas) catalogoPagina = totalPaginas;
        if (catalogoPagina < 1) catalogoPagina = 1;

        var inicio = (catalogoPagina - 1) * CATALOGO_POR_PAGINA;
        var fim = Math.min(inicio + CATALOGO_POR_PAGINA, total);
        var paginaItens = catalogoItens.slice(inicio, fim);

        tbodyItens.innerHTML = '';
        paginaItens.forEach(function (item) {
            var key = (item.cdItem || '') + '_' + (item.itemID || 0);
            var checked = catalogoSelecionados[key] ? ' checked' : '';
            var tr = document.createElement('tr');
            tr.style.cursor = 'pointer';
            tr.innerHTML =
                '<td class="text-center"><input type="checkbox" class="form-check-input catalogo-check"'
                + ' data-cd-item="' + (item.cdItem || '') + '"'
                + ' data-nm-item="' + (item.nmItem || '').replace(/"/g, '&quot;') + '"'
                + ' data-vlr-tabela="' + (item.vlrTabela || 0) + '"'
                + ' data-item-de-para="' + (item.itemDePara || '') + '"'
                + ' data-item-id="' + (item.itemID || 0) + '"' + checked + ' /></td>'
                + '<td>' + (item.cdItem || '') + '</td>'
                + '<td>' + (item.nmItem || '') + '</td>'
                + '<td class="text-end">R$ ' + (Number(item.vlrTabela) || 0).toFixed(2) + '</td>'
                + '<td class="text-end">' + (item.vlrCustoAquisicao || '-') + '</td>'
                + '<td class="text-end">' + (item.qtdDisponivel != null ? item.qtdDisponivel : '-') + '</td>';
            tr.addEventListener('click', function (e) {
                if (e.target.closest('.catalogo-check')) return;
                iniciarFluxoQuantidade([{
                    codItemBR: item.cdItem || '',
                    descrItemBR: item.nmItem || '',
                    precoTbl: Number(item.vlrTabela) || 0,
                    itemDePara: item.itemDePara || '',
                    itemID: Number(item.itemID) || 0,
                    quantidade: 1
                }]);
            });
            tbodyItens.appendChild(tr);
        });

        catalogoRenderPaginacao(totalPaginas, total);
    }

    function catalogoRenderPaginacao(totalPaginas, totalItens) {
        if (!catalogoPaginationEl) return;

        if (totalPaginas <= 1) {
            catalogoPaginationEl.classList.add('d-none');
            return;
        }

        catalogoPaginationEl.classList.remove('d-none');
        catalogoPaginationEl.innerHTML = '';

        var btnPrev = document.createElement('button');
        btnPrev.type = 'button';
        btnPrev.className = 'page-btn';
        btnPrev.innerHTML = '<i class="fa-solid fa-chevron-left"></i>';
        btnPrev.disabled = catalogoPagina <= 1;
        btnPrev.addEventListener('click', function () { catalogoRenderPagina(catalogoPagina - 1); });
        catalogoPaginationEl.appendChild(btnPrev);

        var maxBotoes = 5;
        var metade = Math.floor(maxBotoes / 2);
        var pInicio = Math.max(1, catalogoPagina - metade);
        var pFim = Math.min(totalPaginas, pInicio + maxBotoes - 1);
        if (pFim - pInicio < maxBotoes - 1) pInicio = Math.max(1, pFim - maxBotoes + 1);

        for (var p = pInicio; p <= pFim; p++) {
            var btn = document.createElement('button');
            btn.type = 'button';
            btn.className = 'page-btn' + (p === catalogoPagina ? ' active' : '');
            btn.textContent = p;
            btn.dataset.page = p;
            btn.addEventListener('click', function () { catalogoRenderPagina(Number(this.dataset.page)); });
            catalogoPaginationEl.appendChild(btn);
        }

        var btnNext = document.createElement('button');
        btnNext.type = 'button';
        btnNext.className = 'page-btn';
        btnNext.innerHTML = '<i class="fa-solid fa-chevron-right"></i>';
        btnNext.disabled = catalogoPagina >= totalPaginas;
        btnNext.addEventListener('click', function () { catalogoRenderPagina(catalogoPagina + 1); });
        catalogoPaginationEl.appendChild(btnNext);

        var info = document.createElement('span');
        info.className = 'page-info';
        info.textContent = totalItens + ' produto(s)';
        catalogoPaginationEl.appendChild(info);
    }

    if (formSearchProds) {
        formSearchProds.addEventListener('submit', async function (e) {
            e.preventDefault();
            var descricao = document.getElementById('inputSearchProds').value.trim();
            if (!descricao) return;

            tbodyItens.innerHTML = '<tr><td colspan="6" class="text-center py-3"><i class="fa-solid fa-spinner fa-spin me-1"></i>Pesquisando...</td></tr>';
            if (tfootAdicionarItens) tfootAdicionarItens.classList.add('d-none');
            if (catalogoPaginationEl) catalogoPaginationEl.classList.add('d-none');

            catalogoItens = [];
            catalogoSelecionados = {};
            catalogoPagina = 1;

            try {
                var url = cfg.endpoints.buscarCatalogo
                    + '?descricao=' + encodeURIComponent(descricao)
                    + '&clienteId=' + cfg.clienteId
                    + '&tblPrecoId=' + cfg.tblPrecoId
                    + '&estabelecimentoId=' + cfg.estabelecimentoId;
                var response = await fetch(url);
                var items = await response.json();

                if (!items || items.length === 0) {
                    tbodyItens.innerHTML = '<tr><td colspan="6" class="text-center py-3" style="color: var(--sic-muted);">Nenhum produto encontrado.</td></tr>';
                    return;
                }

                catalogoItens = items;
                catalogoRenderPagina(1);

                if (tfootAdicionarItens) tfootAdicionarItens.classList.remove('d-none');
            }
            catch {
                tbodyItens.innerHTML = '<tr><td colspan="6" class="text-center py-3 text-danger">Erro ao buscar produtos.</td></tr>';
            }
        });
    }

    // ── Adicionar Itens Marcados (modal quantidade) ──
    var addItensMarcadosBtn = document.getElementById('addItensMarcados');
    var modalQuantidadeEl = document.getElementById('modalQuantidade');
    var btnGravarQuantidade = document.getElementById('btnGravarQuantidade');
    var btnGravarQuantidadeTexto = document.getElementById('btnGravarQuantidadeTexto');
    var itensParaAdicionar = [];
    var itemAtualIndex = 0;

    function catalogoColetarSelecionados() {
        catalogoSalvarSelecoes();
        var selecionados = [];
        for (var key in catalogoSelecionados) {
            var s = catalogoSelecionados[key];
            selecionados.push({
                codItemBR: s.cdItem,
                descrItemBR: s.nmItem,
                precoTbl: Number(s.vlrTabela) || 0,
                itemDePara: s.itemDePara || '',
                itemID: Number(s.itemId) || 0,
                quantidade: 1
            });
        }
        return selecionados;
    }

    function atualizarModalQuantidade() {
        var tbody = document.getElementById('tbodyQuantidadeItens');
        if (!tbody) return;

        tbody.innerHTML = '';

        itensParaAdicionar.forEach(function (item, index) {
            var tr = document.createElement('tr');
            tr.innerHTML =
                '<td class="small fw-medium text-nowrap">'
                    + '<i class="fa-solid fa-barcode me-1" style="color: var(--sic-muted);"></i>'
                    + (item.codItemBR || '\u2014')
                + '</td>'
                + '<td class="small">' + (item.descrItemBR || '\u2014') + '</td>'
                + '<td class="text-end small text-nowrap fw-semibold">R$ ' + (item.precoTbl || 0).toFixed(2) + '</td>'
                + '<td class="text-center">'
                    + '<input type="number"'
                    + ' class="form-control form-control-sm text-center prepedido-qty-input js-modal-qtd-input"'
                    + ' min="1" value="' + (item.quantidade || 1) + '"'
                    + ' data-index="' + index + '"'
                    + ' style="width: 90px; margin: 0 auto; font-weight: 700;" />'
                + '</td>';
            tbody.appendChild(tr);
        });

        var firstInput = tbody.querySelector('.js-modal-qtd-input');
        if (firstInput) firstInput.focus();

        if (btnGravarQuantidadeTexto) {
            btnGravarQuantidadeTexto.textContent = 'Adicionar';
        }
    }

    function iniciarFluxoQuantidade(itens) {
        if (!modalQuantidadeEl || !itens || itens.length === 0) return;
        itensParaAdicionar = itens;
        itemAtualIndex = 0;
        atualizarModalQuantidade();
        bootstrap.Modal.getOrCreateInstance(modalQuantidadeEl).show();
    }

    if (addItensMarcadosBtn) {
        addItensMarcadosBtn.addEventListener('click', function () {
            var selecionados = catalogoColetarSelecionados();
            if (selecionados.length === 0) {
                showFeedback(false, 'Selecione ao menos um item.');
                return;
            }
            iniciarFluxoQuantidade(selecionados);
        });
    }

    if (btnGravarQuantidade && modalQuantidadeEl) {
        btnGravarQuantidade.addEventListener('click', async function () {
            // Coletar todas as quantidades de todos os itens da tabela
            var inputs = document.querySelectorAll('#tbodyQuantidadeItens .js-modal-qtd-input');
            inputs.forEach(function (input) {
                var idx = Number(input.dataset.index);
                itensParaAdicionar[idx].quantidade = Number(input.value) || 1;
            });

            bootstrap.Modal.getOrCreateInstance(modalQuantidadeEl).hide();

            var sucesso = true;
            for (var i = 0; i < itensParaAdicionar.length; i++) {
                var item = itensParaAdicionar[i];
                var result = await processAction(cfg.endpoints.atualizarQuantidadeBase + '/itens/adicionar', 'POST', {
                    codItemBR: item.codItemBR,
                    descrItemBR: item.descrItemBR,
                    quantidade: item.quantidade,
                    precoTbl: item.precoTbl,
                    itemDePara: item.itemDePara,
                    itemID: item.itemID,
                    ordemCompra: cfg.ordemCompra
                }, false);

                if (!result.success) {
                    sucesso = false;
                }
            }

            if (sucesso) {
                showFeedback(true, 'Itens adicionados com sucesso.');
                window.location.reload();
            }
        });
    }

    // Enter em qualquer input de quantidade do modal aciona o botão Adicionar
    if (modalQuantidadeEl && btnGravarQuantidade) {
        modalQuantidadeEl.addEventListener('keydown', function (e) {
            if (e.key === 'Enter' && e.target.classList.contains('js-modal-qtd-input')) {
                e.preventDefault();
                btnGravarQuantidade.click();
            }
        });
    }
});