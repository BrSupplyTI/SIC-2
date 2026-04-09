(function () {

    var cfg = window.clienteConfig;
    var basePath = cfg.basePath;
    var fmtBRL = new Intl.NumberFormat('pt-BR', { style: 'currency', currency: 'BRL' });

    function parseDateSort(v) {
        if (!v) return '00000000';
        var parts = v.split('/');
        return parts.length === 3 ? parts[2] + parts[1] + parts[0] : '00000000';
    }

    // Títulos em Aberto — lazy load com filtro por situação + DataTables
    var titulosLoaded = false;
    var collapseTit = document.getElementById('collapseTitulos');
    if (collapseTit) {
        var titAllData = [];
        var dtTitulos = null;

        var situacaoBadge = {
            'Vencido': 'bg-danger',
            'A Vencer': 'bg-info',
            'Crédito': 'bg-success'
        };

        function titApplyFilter(situacao) {
            var filtered = situacao
                ? titAllData.filter(function (r) { return r.situacao === situacao; })
                : titAllData;

            document.querySelectorAll('#titFilterBar .tit-filter-pill').forEach(function (p) {
                p.classList.toggle('active', p.dataset.situacao === (situacao || ''));
            });

            dtTitulos.clear();
            dtTitulos.rows.add(filtered);
            dtTitulos.draw();
        }

        function titBuildFilters(data) {
            var bar = document.getElementById('titFilterBar');
            var situacoes = {};
            data.forEach(function (r) {
                if (!situacoes[r.situacao]) situacoes[r.situacao] = 0;
                situacoes[r.situacao]++;
            });

            // "Todos" pill
            var allBtn = document.createElement('button');
            allBtn.type = 'button';
            allBtn.className = 'tit-filter-pill active';
            allBtn.dataset.situacao = '';
            allBtn.innerHTML = 'Todos <span class="tit-filter-count">' + data.length + '</span>';
            allBtn.addEventListener('click', function () { titApplyFilter(null); });
            bar.appendChild(allBtn);

            var ordem = ['Vencido', 'A Vencer', 'Crédito'];
            ordem.forEach(function (sit) {
                if (!situacoes[sit]) return;
                var btn = document.createElement('button');
                btn.type = 'button';
                btn.className = 'tit-filter-pill';
                btn.dataset.situacao = sit;
                btn.innerHTML = sit + ' <span class="tit-filter-count">' + situacoes[sit] + '</span>';
                btn.addEventListener('click', function () { titApplyFilter(sit); });
                bar.appendChild(btn);
            });
        }

        collapseTit.addEventListener('show.bs.collapse', function () {
            if (titulosLoaded) return;
            titulosLoaded = true;

            document.getElementById('titulosLoader').classList.remove('d-none');

            fetch(basePath + '/Clientes/Detalhes/' + cfg.clienteId + '/Titulos')
                .then(function (resp) {
                    if (!resp.ok) throw new Error(resp.status);
                    return resp.json();
                })
                .then(function (data) {
                    document.getElementById('titulosLoader').classList.add('d-none');

                    if (!data || data.length === 0) {
                        document.getElementById('titulosEmpty').classList.remove('d-none');
                        return;
                    }

                    titAllData = data;
                    document.getElementById('divTblTitulos').classList.remove('d-none');

                    var totais = { qtde: data.length, vencido: 0, aVencer: 0, antecipacao: 0 };
                    data.forEach(function (r) {
                        if (r.situacao === 'Crédito') totais.antecipacao += r.vlrSaldo;
                        else if (r.situacao === 'Vencido') totais.vencido += r.vlrSaldo;
                        else if (r.situacao === 'A Vencer') totais.aVencer += r.vlrSaldo;
                    });
                    document.getElementById('titQtde').textContent = totais.qtde;
                    document.getElementById('titVlrVencido').textContent = fmtBRL.format(totais.vencido);
                    document.getElementById('titVlrAVencer').textContent = fmtBRL.format(totais.aVencer);
                    document.getElementById('titVlrAntecipacao').textContent = fmtBRL.format(totais.antecipacao);

                    titBuildFilters(data);

                    dtTitulos = new DataTable('#tblTitulos', {
                        data: data,
                        responsive: true,
                        paging: true,
                        pageLength: 10,
                        order: [[5, 'asc']],
                        language: {
                            emptyTable: 'Nenhum título encontrado',
                            info: 'Exibindo _START_ a _END_ de _TOTAL_ registros',
                            infoEmpty: 'Nenhum registro',
                            infoFiltered: '(filtrado de _MAX_ registros)',
                            lengthMenu: '_MENU_ por página',
                            search: 'Buscar:',
                            zeroRecords: 'Nenhum registro encontrado',
                            paginate: { first: 'Primeira', last: 'Última', next: '›', previous: '‹' }
                        },
                        columns: [
                            {
                                data: 'dtEmissao', className: 'text-center',
                                render: function (v, type) {
                                    if (type === 'sort' || type === 'type') return parseDateSort(v);
                                    return v || '<span class="text-muted">—</span>';
                                }
                            },
                            { data: 'nrNotaFiscal', className: 'text-center' },
                            {
                                data: 'serie', className: 'text-center',
                                render: function (v) { return v || '<span class="text-muted">—</span>'; }
                            },
                            {
                                data: 'pedido', className: 'text-center',
                                render: function (v) { return v || '<span class="text-muted">—</span>'; }
                            },
                            {
                                data: 'parcela', className: 'text-center',
                                render: function (v) { return v || '<span class="text-muted">—</span>'; }
                            },
                            {
                                data: 'dtVencimento', className: 'text-center',
                                render: function (v, type) {
                                    if (type === 'sort' || type === 'type') return parseDateSort(v);
                                    return v || '<span class="text-muted">—</span>';
                                }
                            },
                            {
                                data: 'situacao', className: 'text-center',
                                render: function (v) {
                                    var cls = situacaoBadge[v] || 'bg-secondary';
                                    return '<span class="badge ' + cls + '">' + v + '</span>';
                                }
                            },
                            {
                                data: 'vlrOriginal', className: 'text-end',
                                render: function (v, type) {
                                    if (type === 'sort' || type === 'type') return v;
                                    return fmtBRL.format(v);
                                }
                            },
                            {
                                data: 'vlrSaldo', className: 'text-end',
                                render: function (v, type) {
                                    if (type === 'sort' || type === 'type') return v;
                                    return fmtBRL.format(v);
                                }
                            }
                        ]
                    });
                })
                .catch(function () {
                    document.getElementById('titulosLoader').classList.add('d-none');
                    document.getElementById('titulosError').classList.remove('d-none');
                    titulosLoaded = false;
                });
        });
    }

    // Saldo de Crédito — lazy load na abertura da modal
    var saldoLoaded = false;
    var modalSaldo = document.getElementById('modalSaldoCredito');
    if (modalSaldo) {
        modalSaldo.addEventListener('show.bs.modal', function () {
            if (saldoLoaded) return;
            saldoLoaded = true;

            document.getElementById('saldoCreditoLoader').classList.remove('d-none');
            document.getElementById('saldoCreditoContent').classList.add('d-none');
            document.getElementById('saldoCreditoError').classList.add('d-none');

            fetch(basePath + '/Clientes/Detalhes/' + cfg.clienteId + '/SaldoCredito')
                .then(function (resp) {
                    if (!resp.ok) throw new Error(resp.status);
                    return resp.json();
                })
                .then(function (data) {
                    document.getElementById('saldoCreditoLoader').classList.add('d-none');
                    document.getElementById('saldoCreditoContent').classList.remove('d-none');

                    var limite = cfg.vlrLimiteCredito || 0;
                    var creditos = data.vlrCreditos || 0;
                    var titulos = data.vlrTitulosEmAberto || 0;
                    var pedidos = data.vlrPedidosNaoFaturados || 0;
                    var saldo = limite + creditos - titulos - pedidos;

                    document.getElementById('scLimite').textContent = fmtBRL.format(limite);
                    document.getElementById('scCreditos').textContent = fmtBRL.format(creditos);
                    document.getElementById('scTitulos').textContent = fmtBRL.format(titulos);
                    document.getElementById('scPedidos').textContent = fmtBRL.format(pedidos);

                    var elSaldo = document.getElementById('scSaldo');
                    elSaldo.textContent = fmtBRL.format(saldo);
                    elSaldo.classList.remove('text-success', 'text-danger');
                    elSaldo.classList.add(saldo >= 0 ? 'text-success' : 'text-danger');
                })
                .catch(function () {
                    document.getElementById('saldoCreditoLoader').classList.add('d-none');
                    document.getElementById('saldoCreditoError').classList.remove('d-none');
                    saldoLoaded = false;
                });
        });
    }

    // Endereços — lazy load com filtro por situação + DataTables
    var enderecosLoaded = false;
    var collapseEnd = document.getElementById('collapseEnderecos');
    if (collapseEnd) {
        var endAllData = [];
        var dtEnderecos = null;

        var endSituacaoBadge = {
            'Ativo': 'bg-success',
            'Inativo': 'bg-secondary',
            'Erro': 'bg-danger'
        };

        function endApplyFilter(situacao) {
            var filtered = situacao
                ? endAllData.filter(function (r) { return r.situacao === situacao; })
                : endAllData;

            document.querySelectorAll('#enderecosFilterBar .tit-filter-pill').forEach(function (p) {
                p.classList.toggle('active', p.dataset.situacao === (situacao || ''));
            });

            dtEnderecos.clear();
            dtEnderecos.rows.add(filtered);
            dtEnderecos.draw();
        }

        function endBuildFilters(data) {
            var bar = document.getElementById('enderecosFilterBar');
            var situacoes = {};
            data.forEach(function (r) {
                if (!situacoes[r.situacao]) situacoes[r.situacao] = 0;
                situacoes[r.situacao]++;
            });

            var allBtn = document.createElement('button');
            allBtn.type = 'button';
            allBtn.className = 'tit-filter-pill active';
            allBtn.dataset.situacao = '';
            allBtn.innerHTML = 'Todos <span class="tit-filter-count">' + data.length + '</span>';
            allBtn.addEventListener('click', function () { endApplyFilter(null); });
            bar.appendChild(allBtn);

            var ordem = ['Ativo', 'Inativo', 'Erro'];
            ordem.forEach(function (sit) {
                if (!situacoes[sit]) return;
                var btn = document.createElement('button');
                btn.type = 'button';
                btn.className = 'tit-filter-pill';
                btn.dataset.situacao = sit;
                btn.innerHTML = sit + ' <span class="tit-filter-count">' + situacoes[sit] + '</span>';
                btn.addEventListener('click', function () { endApplyFilter(sit); });
                bar.appendChild(btn);
            });
        }

        collapseEnd.addEventListener('show.bs.collapse', function () {
            if (enderecosLoaded) return;
            enderecosLoaded = true;

            document.getElementById('enderecosLoader').classList.remove('d-none');

            fetch(basePath + '/Clientes/Detalhes/' + cfg.clienteId + '/Enderecos')
                .then(function (resp) {
                    if (!resp.ok) throw new Error(resp.status);
                    return resp.json();
                })
                .then(function (data) {
                    document.getElementById('enderecosLoader').classList.add('d-none');

                    if (!data || data.length === 0) {
                        document.getElementById('enderecosEmpty').classList.remove('d-none');
                        return;
                    }

                    endAllData = data;
                    document.getElementById('divTblEnderecos').classList.remove('d-none');

                    endBuildFilters(data);

                    dtEnderecos = new DataTable('#tblEnderecos', {
                        data: data,
                        responsive: true,
                        paging: true,
                        pageLength: 10,
                        order: [[2, 'asc']],
                        language: {
                            emptyTable: 'Nenhum endereço encontrado',
                            info: 'Exibindo _START_ a _END_ de _TOTAL_ registros',
                            infoEmpty: 'Nenhum registro',
                            infoFiltered: '(filtrado de _MAX_ registros)',
                            lengthMenu: '_MENU_ por página',
                            search: 'Buscar:',
                            zeroRecords: 'Nenhum registro encontrado',
                            paginate: { first: 'Primeira', last: 'Última', next: '›', previous: '‹' }
                        },
                        columns: [
                            { data: 'clienteEnderecoID', className: 'text-center' },
                            {
                                data: 'situacao', className: 'text-center',
                                render: function (v) {
                                    var cls = endSituacaoBadge[v] || 'bg-secondary';
                                    var title = v === 'Erro' ? ' title="Sem código SAP"' : '';
                                    return '<span class="badge ' + cls + '"' + title + '>' + v + '</span>';
                                }
                            },
                            {
                                data: 'codSAP', className: 'text-center',
                                render: function (v) { return v || '<span class="text-muted">—</span>'; }
                            },
                            { data: 'tipoDocumento', className: 'text-center' },
                            { data: 'cpfcnpj', className: 'text-center' },
                            {
                                data: 'razaoSocial', className: 'text-start',
                                render: function (v) { return v || '<span class="text-muted">—</span>'; }
                            },
                            { data: 'nmCidade', className: 'text-start' },
                            { data: 'cdUF', className: 'text-center' },
                            {
                                data: 'tabelaPreco', className: 'text-start',
                                render: function (v) { return v || '<span class="text-muted">—</span>'; }
                            },
                            {
                                data: 'vlrPedidoMinimo', className: 'text-end',
                                render: function (v, type) {
                                    if (type === 'sort' || type === 'type') return v;
                                    return fmtBRL.format(v);
                                }
                            },
                            {
                                data: 'vlrTaxaEntrega', className: 'text-end',
                                render: function (v, type) {
                                    if (type === 'sort' || type === 'type') return v;
                                    return fmtBRL.format(v);
                                }
                            }
                        ]
                    });
                })
                .catch(function () {
                    document.getElementById('enderecosLoader').classList.add('d-none');
                    document.getElementById('enderecosError').classList.remove('d-none');
                    enderecosLoaded = false;
                });
        });
    }

    // Locais de Entrega — lazy load com filtro por situação + DataTables
    var locaisLoaded = false;
    var collapseLoc = document.getElementById('collapseLocais');
    if (collapseLoc) {
        var locAllData = [];
        var dtLocais = null;

        var locSituacaoBadge = {
            'Ativo': 'bg-success',
            'Inativo': 'bg-secondary',
            'Desabilitado': 'bg-warning text-dark'
        };

        function locApplyFilter(situacao) {
            var filtered = situacao
                ? locAllData.filter(function (r) { return r.situacao === situacao; })
                : locAllData;

            document.querySelectorAll('#locaisFilterBar .tit-filter-pill').forEach(function (p) {
                p.classList.toggle('active', p.dataset.situacao === (situacao || ''));
            });

            dtLocais.clear();
            dtLocais.rows.add(filtered);
            dtLocais.draw();
        }

        function locBuildFilters(data) {
            var bar = document.getElementById('locaisFilterBar');
            var situacoes = {};
            data.forEach(function (r) {
                if (!situacoes[r.situacao]) situacoes[r.situacao] = 0;
                situacoes[r.situacao]++;
            });

            var allBtn = document.createElement('button');
            allBtn.type = 'button';
            allBtn.className = 'tit-filter-pill active';
            allBtn.dataset.situacao = '';
            allBtn.innerHTML = 'Todos <span class="tit-filter-count">' + data.length + '</span>';
            allBtn.addEventListener('click', function () { locApplyFilter(null); });
            bar.appendChild(allBtn);

            var ordem = ['Ativo', 'Inativo', 'Desabilitado'];
            ordem.forEach(function (sit) {
                if (!situacoes[sit]) return;
                var btn = document.createElement('button');
                btn.type = 'button';
                btn.className = 'tit-filter-pill';
                btn.dataset.situacao = sit;
                btn.innerHTML = sit + ' <span class="tit-filter-count">' + situacoes[sit] + '</span>';
                btn.addEventListener('click', function () { locApplyFilter(sit); });
                bar.appendChild(btn);
            });
        }

        collapseLoc.addEventListener('show.bs.collapse', function () {
            if (locaisLoaded) return;
            locaisLoaded = true;

            document.getElementById('locaisLoader').classList.remove('d-none');

            fetch(basePath + '/Clientes/Detalhes/' + cfg.clienteId + '/LocaisEntrega')
                .then(function (resp) {
                    if (!resp.ok) throw new Error(resp.status);
                    return resp.json();
                })
                .then(function (data) {
                    document.getElementById('locaisLoader').classList.add('d-none');

                    if (!data || data.length === 0) {
                        document.getElementById('locaisEmpty').classList.remove('d-none');
                        return;
                    }

                    locAllData = data;
                    document.getElementById('divTblLocais').classList.remove('d-none');

                    locBuildFilters(data);

                    dtLocais = new DataTable('#tblLocais', {
                        data: data,
                        responsive: true,
                        paging: true,
                        pageLength: 10,
                        order: [[2, 'asc']],
                        language: {
                            emptyTable: 'Nenhum local de entrega encontrado',
                            info: 'Exibindo _START_ a _END_ de _TOTAL_ registros',
                            infoEmpty: 'Nenhum registro',
                            infoFiltered: '(filtrado de _MAX_ registros)',
                            lengthMenu: '_MENU_ por página',
                            search: 'Buscar:',
                            zeroRecords: 'Nenhum registro encontrado',
                            paginate: { first: 'Primeira', last: 'Última', next: '›', previous: '‹' }
                        },
                        columns: [
                            { data: 'clienteLocalEntregaID', className: 'text-center' },
                            {
                                data: 'situacao', className: 'text-center',
                                render: function (v) {
                                    var cls = locSituacaoBadge[v] || 'bg-secondary';
                                    return '<span class="badge ' + cls + '">' + v + '</span>';
                                }
                            },
                            { data: 'cdControle', className: 'text-center' },
                            {
                                data: 'nmLocalEntrega', className: 'text-start',
                                render: function (v) { return v || '<span class="text-muted">—</span>'; }
                            },
                            { data: 'tipoDocumento', className: 'text-center' },
                            { data: 'cpfcnpj', className: 'text-center' },
                            {
                                data: null, className: 'text-start',
                                render: function (data) {
                                    var icon = data.tipoEndereco === 'SIM'
                                        ? '<i class="fa-solid fa-home-alt text-warning me-1" title="Endereço no Local de Entrega"></i>'
                                        : '<i class="fa-solid fa-map-location-dot text-success me-1" title="Endereço do CNPJ"></i>';
                                    return icon + (data.nmCidade || '<span class="text-muted">—</span>');
                                }
                            },
                            { data: 'cdUF', className: 'text-center' },
                            {
                                data: 'nmCanalVenda', className: 'text-center',
                                render: function (v) { return v || '<span class="text-muted">—</span>'; }
                            },
                            {
                                data: 'situacaoCredito', className: 'text-center',
                                render: function (v) {
                                    if (v === 'OK') return '<i class="fa-solid fa-check text-success" title="Crédito OK"></i>';
                                    return '<i class="fa-solid fa-xmark text-danger" title="Crédito bloqueado"></i>';
                                }
                            }
                        ]
                    });
                })
                .catch(function () {
                    document.getElementById('locaisLoader').classList.add('d-none');
                    document.getElementById('locaisError').classList.remove('d-none');
                    locaisLoaded = false;
                });
        });
    }

    // Usuários — lazy load com filtro por situação + DataTables
    var usuariosLoaded = false;
    var collapseUsr = document.getElementById('collapseUsuarios');
    if (collapseUsr) {
        var usrAllData = [];
        var dtUsuarios = null;

        var usrSituacaoBadge = {
            'Ativo': 'bg-success',
            'Inativo': 'bg-secondary',
            'Bloqueado': 'bg-danger'
        };

        function usrApplyFilter(situacao) {
            var filtered = situacao
                ? usrAllData.filter(function (r) { return r.situacao === situacao; })
                : usrAllData;

            document.querySelectorAll('#usuariosFilterBar .tit-filter-pill').forEach(function (p) {
                p.classList.toggle('active', p.dataset.situacao === (situacao || ''));
            });

            dtUsuarios.clear();
            dtUsuarios.rows.add(filtered);
            dtUsuarios.draw();
        }

        function usrBuildFilters(data) {
            var bar = document.getElementById('usuariosFilterBar');
            var situacoes = {};
            data.forEach(function (r) {
                if (!situacoes[r.situacao]) situacoes[r.situacao] = 0;
                situacoes[r.situacao]++;
            });

            var allBtn = document.createElement('button');
            allBtn.type = 'button';
            allBtn.className = 'tit-filter-pill active';
            allBtn.dataset.situacao = '';
            allBtn.innerHTML = 'Todos <span class="tit-filter-count">' + data.length + '</span>';
            allBtn.addEventListener('click', function () { usrApplyFilter(null); });
            bar.appendChild(allBtn);

            var ordem = ['Ativo', 'Inativo', 'Bloqueado'];
            ordem.forEach(function (sit) {
                if (!situacoes[sit]) return;
                var btn = document.createElement('button');
                btn.type = 'button';
                btn.className = 'tit-filter-pill';
                btn.dataset.situacao = sit;
                btn.innerHTML = sit + ' <span class="tit-filter-count">' + situacoes[sit] + '</span>';
                btn.addEventListener('click', function () { usrApplyFilter(sit); });
                bar.appendChild(btn);
            });
        }

        collapseUsr.addEventListener('show.bs.collapse', function () {
            if (usuariosLoaded) return;
            usuariosLoaded = true;

            document.getElementById('usuariosLoader').classList.remove('d-none');

            fetch(basePath + '/Clientes/Detalhes/' + cfg.clienteId + '/Usuarios')
                .then(function (resp) {
                    if (!resp.ok) throw new Error(resp.status);
                    return resp.json();
                })
                .then(function (data) {
                    document.getElementById('usuariosLoader').classList.add('d-none');

                    if (!data || data.length === 0) {
                        document.getElementById('usuariosEmpty').classList.remove('d-none');
                        return;
                    }

                    usrAllData = data;
                    document.getElementById('divTblUsuarios').classList.remove('d-none');

                    usrBuildFilters(data);

                    dtUsuarios = new DataTable('#tblUsuarios', {
                        data: data,
                        responsive: true,
                        paging: true,
                        pageLength: 10,
                        order: [[8, 'desc']],
                        language: {
                            emptyTable: 'Nenhum usuário encontrado',
                            info: 'Exibindo _START_ a _END_ de _TOTAL_ registros',
                            infoEmpty: 'Nenhum registro',
                            infoFiltered: '(filtrado de _MAX_ registros)',
                            lengthMenu: '_MENU_ por página',
                            search: 'Buscar:',
                            zeroRecords: 'Nenhum registro encontrado',
                            paginate: { first: 'Primeira', last: 'Última', next: '›', previous: '‹' }
                        },
                        columns: [
                            { data: 'clienteUsuarioID', className: 'text-center' },
                            {
                                data: 'situacao', className: 'text-center',
                                render: function (v) {
                                    var cls = usrSituacaoBadge[v] || 'bg-secondary';
                                    return '<span class="badge ' + cls + '">' + v + '</span>';
                                }
                            },
                            { data: 'login', className: 'text-center' },
                            { data: 'nmUsuario', className: 'text-center' },
                            { data: 'email', className: 'text-center' },
                            { data: 'nmPerfil', className: 'text-start' },
                            { data: 'catalogo', className: 'text-center' },
                            { data: 'permissao', className: 'text-start' },
                            {
                                data: 'dtCadastro', className: 'text-center',
                                render: function (v, type) {
                                    if (type === 'sort' || type === 'type') return parseDateSort(v);
                                    return v || '<span class="text-muted">—</span>';
                                }
                            },
                            {
                                data: 'dtUltimoLogin', className: 'text-center',
                                render: function (v, type) {
                                    if (type === 'sort' || type === 'type') return parseDateSort(v);
                                    return v || '<span class="text-muted">Nunca logou</span>';
                                }
                            }
                        ]
                    });
                })
                .catch(function () {
                    document.getElementById('usuariosLoader').classList.add('d-none');
                    document.getElementById('usuariosError').classList.remove('d-none');
                    usuariosLoaded = false;
                });
        });
    }

})();
