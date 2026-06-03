/* ============================================
   Cotações — Cadastro (interações mínimas)
   ============================================ */
(() => {
    'use strict';

    const btnVoltar = document.getElementById('btnVoltar');
    if (btnVoltar) {
        btnVoltar.addEventListener('click', () => history.back());
    }

    // ── Condição de Pagamento: sincroniza hidden + estado inicial ──
    const $condPagtoSelect = $('#CondPagtoId');
    const $condPagtoHidden = $('#CondPagtoId_hidden');

    if ($condPagtoSelect.length) {
        $condPagtoSelect.select2({
            theme: 'bootstrap-5',
            language: 'pt-BR',
            placeholder: 'Selecione',
            allowClear: true
        });
    }

    function syncCondPagtoHidden() {
        $condPagtoHidden.val($condPagtoSelect.val());
    }

    $condPagtoSelect.on('change', syncCondPagtoHidden);

    // Estado inicial: bloqueado até selecionar cliente (a menos que já tenha valor pré-selecionado)
    (function () {
        var isAdmin = $condPagtoSelect.data('is-admin') === true || $condPagtoSelect.data('is-admin') === 'true';
        var isBackOffice = $condPagtoSelect.data('is-backoffice') === true || $condPagtoSelect.data('is-backoffice') === 'true';
        if (!isAdmin && !isBackOffice) {
            $condPagtoSelect.prop('disabled', true);
        }
        syncCondPagtoHidden();
    })();

    // ── MargemPadrao: sincroniza hidden para garantir POST quando disabled ──
    const $margemInput = $('#MargemPadrao');
    const $margemHidden = $('#MargemPadrao_hidden');

    function syncMargemHidden() {
        $margemHidden.val($margemInput.val());
    }

    $margemInput.on('input change', syncMargemHidden);
    syncMargemHidden();

    // ── Validação: Número Chamado aceita apenas números ──
    $(document).on('input', '#NrChamado', function () {
        this.value = this.value.replace(/[^0-9]/g, '');
    });

    // ── Validação do submit para campos de bonificação ──
    $(document).on('submit', '#formCotacaoCadastro', function (e) {
        // Garante que TipoNome tenha o texto do tipo selecionado
        var $tipoSelect = $('#Tipo');
        $('#TipoNome').val($tipoSelect.find('option:selected').text().trim());
        if ($('#colMotivoBonificacao').is(':visible')) {
            var valido = true;

            var $motivo = $('#MotivoBonificacaoId');
            var $spanMotivo = $motivo.siblings('.field-validation-error');
            if (!$motivo.val()) {
                $motivo.addClass('input-validation-error');
                $spanMotivo.text('O campo Motivo é obrigatório.');
                valido = false;
            } else {
                $motivo.removeClass('input-validation-error');
                $spanMotivo.text('');
            }

            var $nrChamado = $('#NrChamado');
            var $spanNrChamado = $nrChamado.siblings('.field-validation-error');
            var nrVal = $nrChamado.val().trim();
            if (!nrVal || !/^[0-9]+$/.test(nrVal)) {
                $nrChamado.addClass('input-validation-error');
                $spanNrChamado.text('O campo Número Chamado é obrigatório e deve conter apenas números.');
                valido = false;
            } else {
                $nrChamado.removeClass('input-validation-error');
                $spanNrChamado.text('');
            }

            if (!valido) e.preventDefault();
        }

        if ($('#colPedidoOriginal').is(':visible')) {
            var $pedidoOriginal = $('#PedidoOriginal');
            var $spanPedido = $pedidoOriginal.siblings('.field-validation-error');
            var pedidoVal = $pedidoOriginal.val().trim();
            if (!pedidoVal || isNaN(pedidoVal) || parseInt(pedidoVal) <= 0) {
                $pedidoOriginal.addClass('input-validation-error');
                $spanPedido.text('O campo Pedido Original é obrigatório e deve ser um número válido.');
                e.preventDefault();
            } else {
                $pedidoOriginal.removeClass('input-validation-error');
                $spanPedido.text('');
            }
        }
    });

    // ── Select2: Cliente (busca AJAX) ──
    const $cliente = $('#Cliente');
    const $estabelecimento = $('#Estabelecimento');
    const $endereco = $('#Endereco');
    const $localEntrega = $('#LocalEntrega');

    if ($cliente.length) {
        $cliente.select2({
            theme: 'bootstrap-5',
            language: 'pt-BR',
            placeholder: 'Digite para pesquisar...',
            allowClear: true,
            minimumInputLength: 2,
            ajax: {
                url: window.cotacaoUrls.searchClientes,
                dataType: 'json',
                delay: 300,
                data: function (params) {
                    return { term: params.term, estabelecimentoId: $estabelecimento.val() };
                },
                processResults: function (data) {
                    return { results: data.results };
                },
                cache: true
            }
        });

        $cliente.on('select2:opening', function (e) {
            if (!$estabelecimento.val()) {
                e.preventDefault();
                var $validationSpan = $estabelecimento.siblings('.field-validation-error');
                $validationSpan.text('Favor selecionar o Estabelecimento primeiro.').show();
                $estabelecimento.addClass('input-validation-error').focus();
            }
        });

        if ($endereco.length) {
            $endereco.select2({
                theme: 'bootstrap-5',
                language: 'pt-BR',
                placeholder: 'Selecione',
                allowClear: true,
                matcher: function (params, data) {
                    if ($.trim(params.term) === '') return data;
                    if (typeof data.text === 'undefined') return null;

                    var term = params.term.replace(/[.\-\/]/g, '').toLowerCase();
                    var text = data.text.replace(/[.\-\/]/g, '').toLowerCase();

                    if (text.indexOf(term) > -1) return data;

                    return null;
                }
            });
        }

        $cliente.on('change', function () {
            var clienteId = $(this).val();
            $endereco.empty().append('<option value="">Selecione</option>').trigger('change');
            $localEntrega.empty().append('<option value="">Selecione</option>').trigger('change');
            $nrContrato.empty().append('<option value="">Selecione</option>');
            $btnNovoContrato.attr('href', '#');

            // Limpa tabela de preço
            $('#TabelaPreco').val('');
            $('#TabelaPrecoId').val('');

            // Regra geral: se margem for 0,00, define como 30,00
            if ($margem.length && !$margem.prop('disabled')) {
                var margemVal = $margem.val().replace(',', '.');
                if (!margemVal || parseFloat(margemVal) === 0) {
                    $margem.val('30,00');
                }
            }

            if (clienteId) {
                $.getJSON(window.cotacaoUrls.getEnderecos, { clienteId: clienteId }, function (data) {
                    $.each(data, function (i, item) {
                        $endereco.append($('<option></option>').val(item.id).text(item.text));
                    });
                    if (data.length === 1) {
                        $endereco.val(data[0].id).trigger('change');
                    } else {
                        $endereco.trigger('change');
                    }
                });

                $.getJSON(window.cotacaoUrls.getTabelaPreco, { clienteId: clienteId }, function (data) {
                    if (data.found) {
                        $('#TabelaPreco').val(data.text);
                        $('#TabelaPrecoId').val(data.id);
                    }
                });

                $.getJSON(window.cotacaoUrls.getFormaPagtoCliente, { clienteId: clienteId }, function (data) {
                    if (data.found) {
                        $('#FormaPagtoId').val(data.id).trigger('change');
                    }
                });

                var tipoTexto = $tipo.find('option:selected').text().trim();
                if (tipoTexto === 'Comodato') {
                    carregarContratos(clienteId);
                    atualizarUrlContrato();
                }
            } else {
                // Sem cliente: limpa e bloqueia
                $('#CondPagtoId').val('').trigger('change').prop('disabled', true);
                $('#FormaPagtoId').val('').trigger('change');
            }
        });

        $endereco.on('change', function () {
            var enderecoId = $(this).val();
            console.log('Endereço selecionado:', enderecoId);
            $localEntrega.empty().append('<option value="">Selecione</option>').trigger('change');

            if (enderecoId) {
                $.getJSON(window.cotacaoUrls.getLocaisEntrega, { clienteEnderecoId: enderecoId }, function (data) {
                    $.each(data, function (i, item) {
                        $localEntrega.append(
                            $('<option></option>')
                                .val(item.id)
                                .text(item.text)
                                .attr('data-cond-pagto-id', item.condPagtoId || '')
                                .attr('data-tipo-ovsap', item.tipoOVSAP || '')
                        );
                    });
                    if (data.length === 1) {
                        $localEntrega.val(data[0].id).trigger('change');
                    } else {
                        $localEntrega.trigger('change');
                    }
                });
            }
        });

        // ── Local de Entrega: preenche Condição de Pagamento e Tipo de Ordem automaticamente ──
        $localEntrega.on('change', function () {
            var $selectedOption = $(this).find('option:selected');
            var condPagtoId = $selectedOption.attr('data-cond-pagto-id');
            var tipoOVSAP = $selectedOption.attr('data-tipo-ovsap');

            if (condPagtoId) {
                $condPagtoSelect.val(condPagtoId).trigger('change');
            }

            if (tipoOVSAP) {
                // Procura a opção do TipoOrdem que contém o tipoOVSAP no texto
                var $tipoOrdem = $('#TipoOrdem');
                var encontrou = false;

                $tipoOrdem.find('option').each(function () {
                    var optionText = $(this).text().trim().toUpperCase();
                    if (optionText.includes(tipoOVSAP.toUpperCase())) {
                        $tipoOrdem.val($(this).val()).trigger('change');
                        encontrou = true;
                        console.log('[TipoOrdem] Preenchido com sucesso:', tipoOVSAP, '→ Valor:', $(this).val());
                        return false; // break
                    }
                });

                if (!encontrou) {
                    console.warn('[TipoOrdem] Não encontrada opção para:', tipoOVSAP);
                }
            }
        });

        // ── Tipo de Cotação: margem + visibilidade de pagamento ──
        const $tipo = $('#Tipo');
        const $margem = $('#MargemPadrao');
        const $colCondPagto = $('#colCondPagto');
        const $colFormaPagto = $('#colFormaPagto');

        const TIPOS_SEM_PAGAMENTO = [];

        // Tipos que travam a condição de pagamento em "Não gera duplicata" e desabilitam o campo
        const TIPOS_TRAVA_COND_PAGTO = [
            'Comodato',
            'Pedido - Bonificação',
            'Pedido - Remessa Bonificação',
            'Pedido - Remessa Reposição'
        ];
        const TIPOS_MARGEM_FIXA = [
            'Cotação - Televendas',
            'Cotação - Revenda'
        ];
        const $colNrContrato = $('#colNrContrato');
        const $nrContrato = $('#NrContrato');
        const $btnNovoContrato = $('#btnNovoContrato');

        function atualizarUrlContrato() {
            var clienteId = $cliente.val();
            if (clienteId) {
                $btnNovoContrato.attr('href', 'https://intranet.brsupply.com.br/NewSIC/GestaoContratos/index/' + clienteId);
            } else {
                $btnNovoContrato.attr('href', '#');
            }
        }

        function carregarContratos(clienteId) {
            $nrContrato.empty().append('<option value="">Selecione</option>');
            if (clienteId) {
                $.getJSON(window.cotacaoUrls.getContratos, { clienteId: clienteId }, function (data) {
                    $.each(data, function (i, item) {
                        $nrContrato.append($('<option></option>').val(item.id).text(item.text));
                    });
                });
            }
        }

        function aplicarRegrasTipo(tipoTexto) {
            var semPagamento = TIPOS_SEM_PAGAMENTO.includes(tipoTexto);
            $colCondPagto.toggle(!semPagamento);
            $colFormaPagto.toggle(!semPagamento);

            var $selectCondPagto = $colCondPagto.find('select');

            // Trava condição
            var travaCond = TIPOS_TRAVA_COND_PAGTO.includes(tipoTexto);
            if (travaCond) {
                // Localiza a opção cujo texto contém "não gera duplicata" (case-insensitive)
                var $opcao = $selectCondPagto.find('option').filter(function () {
                    return $(this).text().trim().toLowerCase().indexOf('não gera duplicata') > -1;
                });
                if ($opcao.length) {
                    $selectCondPagto.val($opcao.val());
                }
                // Usa pointer-events:none ao invés de disabled para o valor ser enviado
                $selectCondPagto.css('pointer-events', 'none').css('cursor', 'not-allowed');
            } else {
                $selectCondPagto.css('pointer-events', '').css('cursor', '');
            }

            var isComodato = tipoTexto === 'Comodato';
            $colNrContrato.toggle(isComodato);
            if (isComodato) {
                carregarContratos($cliente.val());
                atualizarUrlContrato();
            }

            // Exibe campos de bonificação para "Pedido - Bonificação", "Pedido - Remessa Bonificação" e "Pedido - Remessa Reposição"
            var isBonificacao = tipoTexto === 'Pedido - Bonificação' || tipoTexto === 'Pedido - Remessa Bonificação' || tipoTexto === 'Pedido - Remessa Reposição';
            $('#colMotivoBonificacao').toggle(isBonificacao);
            $('#colNrChamado').toggle(isBonificacao);
            if (!isBonificacao) {
                $('#MotivoBonificacaoId').val('');
                $('#NrChamado').val('');
            }

            // Exibe campo Pedido Original para "Pedido - Remessa Reposição" e "Pedido - Remessa Bonificação"
            var isRemessaComPedido = tipoTexto === 'Pedido - Remessa Reposição' || tipoTexto === 'Pedido - Remessa Bonificação';
            $('#colPedidoOriginal').toggle(isRemessaComPedido);
            if (!isRemessaComPedido) {
                $('#PedidoOriginal').val('');
            }

            if (TIPOS_MARGEM_FIXA.includes(tipoTexto)) {
                $margem.val('30,00').prop('disabled', true);
            } else {
                $margem.prop('disabled', false);
            }
            syncMargemHidden();
        }

        // Função para carregar opções de Tipo de Ordem
        function carregarTiposOrdem(cotacaoTipoId, valorParaSelecionar) {
            var $tipoOrdem = $('#TipoOrdem');
            var valorAtual = valorParaSelecionar !== undefined ? valorParaSelecionar : $tipoOrdem.val();
            $tipoOrdem.empty().append('<option value="">Selecione</option>');

            if (cotacaoTipoId) {
                $.getJSON(window.cotacaoUrls.getTiposOrdem, { cotacaoTipoId: cotacaoTipoId }, function (data) {
                    $.each(data, function (i, item) {
                        $tipoOrdem.append($('<option></option>').val(item.id).text(item.text));
                    });
                    // Se temos um valor para selecionar, seleciona após carregar
                    if (valorAtual) {
                        $tipoOrdem.val(valorAtual);
                    }
                });
            }
        }

        $tipo.on('change', function () {
            var selectedText = $(this).find('option:selected').text().trim();
            aplicarRegrasTipo(selectedText);

            var cotacaoTipoId = $(this).val();
            carregarTiposOrdem(cotacaoTipoId);
        });

        // Aplica ao carregar se já houver tipo selecionado
        aplicarRegrasTipo($tipo.find('option:selected').text().trim());

        // Carrega tipos de ordem ao iniciar se já houver tipo selecionado
        if ($tipo.val()) {
            carregarTiposOrdem($tipo.val(), $('#TipoOrdem').val());
        }

        $estabelecimento.on('change', function () {
            if ($(this).val()) {
                $(this).removeClass('input-validation-error');
                $(this).siblings('.field-validation-error').text('').hide();
            }

            var estabId = $(this).val();
            var ufOrigem = $('#UfOrigem');
            if (estabId && window.estabelecimentoUfMap && window.estabelecimentoUfMap[estabId]) {
                ufOrigem.val(window.estabelecimentoUfMap[estabId]);
            } else {
                ufOrigem.val('');
            }
        });

        // ── UF Destino → Cidade Destino (select2 chosen-style) ──
        const $ufDestino = $('#UfDestino');
        const $cidadeDestino = $('#CidadeDestino');

        if ($cidadeDestino.length) {
            $cidadeDestino.select2({
                theme: 'bootstrap-5',
                language: 'pt-BR',
                placeholder: 'Selecione',
                allowClear: true
            });
        }

        function carregarCidades(cdUf, valorParaSelecionar) {
            var valorAtual = valorParaSelecionar !== undefined ? valorParaSelecionar : $cidadeDestino.val();
            $cidadeDestino.empty().append('<option value="">Selecione</option>');
            if (!cdUf) {
                $cidadeDestino.trigger('change');
                return;
            }
            $.getJSON(window.cotacaoUrls.getCidadesByUf, { cdUf: cdUf }, function (data) {
                $.each(data, function (i, item) {
                    $cidadeDestino.append($('<option></option>').val(item.id).text(item.text));
                });
                if (valorAtual) {
                    $cidadeDestino.val(valorAtual);
                }
                $cidadeDestino.trigger('change');
            });
        }

        $ufDestino.on('change', function () {
            carregarCidades($(this).val());
        });

        // Carrega ao iniciar se já houver UF selecionada, preservando a cidade já selecionada
        if ($ufDestino.val()) {
            carregarCidades($ufDestino.val(), $cidadeDestino.val());
        }
    }
})();
