(function () {

    var cfg = window.produtoConfig;
    var basePath = cfg.basePath;
    var fotos = cfg.fotos;
    var currentIdx = 0;

    // Thumbnails
    document.querySelectorAll('.produto-thumb').forEach(function (thumb, i) {
        thumb.addEventListener('click', function () {
            document.getElementById('fotoPrincipal').src = thumb.dataset.fotoUrl;
            document.querySelectorAll('.produto-thumb').forEach(function (t) { t.classList.remove('active'); });
            thumb.classList.add('active');
            currentIdx = i;
        });
    });

    // Lightbox
    var lightbox = document.getElementById('produtoLightbox');
    var lightboxImg = document.getElementById('lightboxImg');
    var lightboxCounter = document.getElementById('lightboxCounter');

    function openLightbox(idx) {
        currentIdx = idx;
        updateLightbox();
        lightbox.classList.add('open');
        document.body.style.overflow = 'hidden';
    }

    function closeLightbox() {
        lightbox.classList.remove('open');
        document.body.style.overflow = '';
    }

    function updateLightbox() {
        lightboxImg.src = fotos[currentIdx];
        lightboxCounter.textContent = (currentIdx + 1) + ' / ' + fotos.length;
        document.getElementById('lightboxPrev').style.display = fotos.length > 1 ? '' : 'none';
        document.getElementById('lightboxNext').style.display = fotos.length > 1 ? '' : 'none';
    }

    document.getElementById('btnZoomFoto').addEventListener('click', function () { openLightbox(currentIdx); });
    document.getElementById('fotoPrincipal').addEventListener('click', function () { openLightbox(currentIdx); });
    document.getElementById('lightboxClose').addEventListener('click', closeLightbox);
    lightbox.addEventListener('click', function (e) { if (e.target === lightbox) closeLightbox(); });

    document.getElementById('lightboxPrev').addEventListener('click', function (e) {
        e.stopPropagation();
        currentIdx = (currentIdx - 1 + fotos.length) % fotos.length;
        updateLightbox();
    });

    document.getElementById('lightboxNext').addEventListener('click', function (e) {
        e.stopPropagation();
        currentIdx = (currentIdx + 1) % fotos.length;
        updateLightbox();
    });

    document.addEventListener('keydown', function (e) {
        if (!lightbox.classList.contains('open')) return;
        if (e.key === 'Escape') closeLightbox();
        if (e.key === 'ArrowLeft') { currentIdx = (currentIdx - 1 + fotos.length) % fotos.length; updateLightbox(); }
        if (e.key === 'ArrowRight') { currentIdx = (currentIdx + 1) % fotos.length; updateLightbox(); }
    });

    // Toggle detalhes estoque por estabelecimento
    document.querySelectorAll('.detalhesEstoque').forEach(function (btn) {
        btn.addEventListener('click', function (e) {
            e.preventDefault();
            var container = btn.closest('.produto-estoque-estabelecimento');
            var detalhes = container.querySelector('.divDetalhesEstoque');
            var label = btn.querySelector('.detalhesEstoque-label');
            var isOpen = detalhes.classList.toggle('open');
            btn.classList.toggle('active', isOpen);
            if (label) label.textContent = isOpen ? 'Ocultar' : 'Detalhes';
        });
    });

    // Alocações SIC - modal com DataTables
    var dtAlocacoes = null;
    var modalAlocacoes = document.getElementById('modalVerAlocacoesSIC');

    document.querySelectorAll('.btnVerAlocacoesSIC').forEach(function (btn) {
        btn.addEventListener('click', function (e) {
            e.preventDefault();
            var itemId = btn.dataset.itemid;
            var estabId = btn.dataset.estabelecimentoid;
            var nmEstab = btn.dataset.nmcurto;

            document.getElementById('alocacoesSICEstabelecimento').textContent = nmEstab;
            document.getElementById('verAlocacoesLoader').classList.remove('d-none');
            document.getElementById('verAlocacoesError').classList.add('d-none');
            document.getElementById('verAlocacoesEmpty').classList.add('d-none');
            document.getElementById('divTblAlocacoes').classList.add('d-none');

            if (dtAlocacoes) {
                dtAlocacoes.destroy();
                dtAlocacoes = null;
                document.querySelector('#tblAlocacoes tbody').innerHTML = '';
            }

            var bsModal = new bootstrap.Modal(modalAlocacoes);
            bsModal.show();

            fetch(basePath + '/Produtos/Detalhes/' + itemId + '/Alocacoes/' + estabId)
                .then(function (resp) {
                    if (!resp.ok) throw new Error(resp.status);
                    return resp.json();
                })
                .then(function (data) {
                    document.getElementById('verAlocacoesLoader').classList.add('d-none');

                    if (!data || data.length === 0) {
                        document.getElementById('verAlocacoesEmpty').classList.remove('d-none');
                        return;
                    }

                    document.getElementById('divTblAlocacoes').classList.remove('d-none');

                    dtAlocacoes = new DataTable('#tblAlocacoes', {
                        data: data,
                        responsive: true,
                        paging: data.length > 25,
                        pageLength: 25,
                        order: [[0, 'desc']],
                        language: {
                            emptyTable: 'Nenhuma alocação encontrada',
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
                                data: 'pedido', className: 'text-center fw-semibold',
                                render: function (v) {
                                    return '<a href="' + basePath + '/Pedidos/Detalhes/' + v + '" class="text-decoration-none text-br">' + v + '</a>';
                                }
                            },
                            { data: 'dtPedido', className: 'text-center' },
                            {
                                data: 'dtProgLiberacao', className: 'text-center',
                                render: function (v) { return v || '<span class="text-muted">—</span>'; }
                            },
                            {
                                data: null,
                                render: function (row) {
                                    return '<div>' + row.nmCliente + '</div><small class="text-muted">' + row.nmCanalVenda + '</small>';
                                }
                            },
                            { data: 'qtSolicitada', className: 'text-center fw-semibold' },
                            {
                                data: 'qtRupturas', className: 'text-center',
                                render: function (v) {
                                    return v > 0 ? '<span class="badge bg-danger">' + v + '</span>' : '<span class="text-muted">0</span>';
                                }
                            },
                            { data: 'dsStatusCotacao' },
                            {
                                data: 'ordemVendaSAP', className: 'text-center',
                                render: function (v) {
                                    return v === 'Sem OV' ? '<span class="text-muted">Sem OV</span>' : '<span class="fw-semibold">' + v + '</span>';
                                }
                            }
                        ]
                    });
                })
                .catch(function () {
                    document.getElementById('verAlocacoesLoader').classList.add('d-none');
                    document.getElementById('verAlocacoesError').classList.remove('d-none');
                });
        });
    });

    modalAlocacoes.addEventListener('hidden.bs.modal', function () {
        if (dtAlocacoes) {
            dtAlocacoes.destroy();
            dtAlocacoes = null;
            document.querySelector('#tblAlocacoes tbody').innerHTML = '';
        }
    });

    // Ordens de Compra - lazy load com filtro por estabelecimento e paginação manual
    var ordensCompraLoaded = false;
    var collapseOC = document.getElementById('collapseOrdensCompra');
    if (collapseOC) {
        var ocAllData = [];
        var ocFiltered = [];
        var ocCurrentFilter = null;
        var ocPage = 1;
        var ocPageSize = 10;

        function ocRender() {
            var total = ocFiltered.length;
            var totalPages = Math.ceil(total / ocPageSize) || 1;
            if (ocPage > totalPages) ocPage = totalPages;
            var start = (ocPage - 1) * ocPageSize;
            var pageData = ocFiltered.slice(start, start + ocPageSize);

            var tbody = document.getElementById('tblOrdensCompraBody');
            tbody.innerHTML = '';
            pageData.forEach(function (r) {
                var tr = document.createElement('tr');
                tr.innerHTML =
                    '<td class="text-center fw-semibold">' + r.quantidade + '</td>' +
                    '<td>' + r.nmEstabelecimento + ' <small class="text-muted">(' + r.cdEstabelecimento + ')</small></td>' +
                    '<td class="text-center">' + (r.dtPrevisao || '<span class="text-muted">—</span>') + '</td>' +
                    '<td class="text-center oc-col-hide">' + (r.ordemCompra || '<span class="text-muted">—</span>') + '</td>' +
                    '<td class="text-center oc-col-hide">' + (r.xPed || '<span class="text-muted">—</span>') + '</td>' +
                    '<td>' + r.razaoSocial + '</td>';
                tbody.appendChild(tr);
            });

            // Info
            var info = document.getElementById('ocPaginationInfo');
            if (total === 0) {
                info.textContent = 'Nenhum registro';
            } else {
                info.textContent = (start + 1) + ' – ' + Math.min(start + ocPageSize, total) + ' de ' + total + ' registros';
            }

            // Pagination
            var pag = document.getElementById('ocPagination');
            pag.innerHTML = '';
            var wrap = document.getElementById('ocPaginationWrap');
            if (totalPages <= 1) { wrap.classList.add('d-none'); return; }
            wrap.classList.remove('d-none');

            function addLi(label, page, disabled, active) {
                var li = document.createElement('li');
                li.className = 'page-item' + (disabled ? ' disabled' : '') + (active ? ' active' : '');
                var a = document.createElement('a');
                a.className = 'page-link';
                a.href = '#';
                a.innerHTML = label;
                if (!disabled && !active) {
                    a.addEventListener('click', function (e) { e.preventDefault(); ocPage = page; ocRender(); });
                }
                li.appendChild(a);
                pag.appendChild(li);
            }

            addLi('‹', ocPage - 1, ocPage === 1, false);
            for (var p = 1; p <= totalPages; p++) {
                addLi(p, p, false, p === ocPage);
            }
            addLi('›', ocPage + 1, ocPage === totalPages, false);
        }

        function ocApplyFilter(estab) {
            ocCurrentFilter = estab;
            ocFiltered = estab ? ocAllData.filter(function (r) { return r.cdEstabelecimento === estab; }) : ocAllData;
            ocPage = 1;

            // Update active pill
            document.querySelectorAll('#ocFilterBar .oc-filter-pill').forEach(function (p) {
                p.classList.toggle('active', p.dataset.estab === (estab || ''));
            });

            ocRender();
        }

        function ocBuildFilters(data) {
            var bar = document.getElementById('ocFilterBar');
            var estabs = {};
            data.forEach(function (r) {
                if (!estabs[r.cdEstabelecimento]) estabs[r.cdEstabelecimento] = { nome: r.nmEstabelecimento, count: 0 };
                estabs[r.cdEstabelecimento].count++;
            });

            var keys = Object.keys(estabs);
            if (keys.length <= 1) { bar.classList.add('d-none'); return; }

            // "Todos" pill
            var allBtn = document.createElement('button');
            allBtn.type = 'button';
            allBtn.className = 'oc-filter-pill active';
            allBtn.dataset.estab = '';
            allBtn.innerHTML = 'Todos <span class="oc-filter-count">' + data.length + '</span>';
            allBtn.addEventListener('click', function () { ocApplyFilter(null); });
            bar.appendChild(allBtn);

            keys.forEach(function (cd) {
                var btn = document.createElement('button');
                btn.type = 'button';
                btn.className = 'oc-filter-pill';
                btn.dataset.estab = cd;
                btn.innerHTML = estabs[cd].nome + ' <span class="oc-filter-count">' + estabs[cd].count + '</span>';
                btn.addEventListener('click', function () { ocApplyFilter(cd); });
                bar.appendChild(btn);
            });
        }

        collapseOC.addEventListener('show.bs.collapse', function () {
            if (ordensCompraLoaded) return;
            ordensCompraLoaded = true;

            document.getElementById('ordensCompraLoader').classList.remove('d-none');

            fetch(basePath + '/Produtos/Detalhes/' + cfg.itemId + '/OrdensCompra')
                .then(function (resp) {
                    if (!resp.ok) throw new Error(resp.status);
                    return resp.json();
                })
                .then(function (data) {
                    document.getElementById('ordensCompraLoader').classList.add('d-none');

                    if (!data || data.length === 0) {
                        document.getElementById('ordensCompraEmpty').classList.remove('d-none');
                        return;
                    }

                    ocAllData = data;
                    ocFiltered = data;
                    document.getElementById('divTblOrdensCompra').classList.remove('d-none');
                    ocBuildFilters(data);
                    ocRender();
                })
                .catch(function () {
                    document.getElementById('ordensCompraLoader').classList.add('d-none');
                    document.getElementById('ordensCompraError').classList.remove('d-none');
                    ordensCompraLoaded = false;
                });
        });
    }

    // Produtos Similares - lazy load
    var similaresLoaded = false;
    var collapseSim = document.getElementById('collapseSimilares');
    if (collapseSim) {
        collapseSim.addEventListener('show.bs.collapse', function () {
            if (similaresLoaded) return;
            similaresLoaded = true;

            document.getElementById('similaresLoader').classList.remove('d-none');

            fetch(basePath + '/Produtos/Detalhes/' + cfg.itemId + '/Similares')
                .then(function (resp) {
                    if (!resp.ok) throw new Error(resp.status);
                    return resp.json();
                })
                .then(function (data) {
                    document.getElementById('similaresLoader').classList.add('d-none');

                    if (!data || data.length === 0) {
                        document.getElementById('similaresEmpty').classList.remove('d-none');
                        return;
                    }

                    document.getElementById('divTblSimilares').classList.remove('d-none');
                    var tbody = document.getElementById('tblSimilaresBody');
                    tbody.innerHTML = '';

                    data.forEach(function (r) {
                        var tr = document.createElement('tr');
                        tr.innerHTML =
                            '<td class="text-center"><img src="' + (r.foto || 'https://www.supplymanager.com.br/fotos/semimagem.jpg') + '" alt="' + r.nmItem + '" style="width:48px;height:48px;object-fit:contain;border-radius:4px" onerror="this.src=\'https://www.supplymanager.com.br/fotos/semimagem.jpg\'" /></td>' +
                            '<td class="text-center fw-semibold"><a href="' + basePath + '/Produtos/Detalhes/' + r.itemID + '" class="text-decoration-none text-br">' + r.cdItem + '</a></td>' +
                            '<td>' + r.nmItem + '</td>' +
                            '<td class="text-center">' + (r.ncm || '<span class="text-muted">—</span>') + '</td>' +
                            '<td class="text-center">' + (r.dtCadastro || '<span class="text-muted">—</span>') + '</td>' +
                            '<td class="text-center"><button type="button" class="btn btn-outline-br btnVerEstoquesSimilar" data-itemid="' + r.itemID + '" data-cditem="' + r.cdItem + '"><i class="fa-duotone fa-warehouse me-1"></i></button></td>';
                        tbody.appendChild(tr);
                    });

                    // Bind estoques buttons
                    tbody.querySelectorAll('.btnVerEstoquesSimilar').forEach(function (btn) {
                        btn.addEventListener('click', function () {
                            abrirModalEstoquesSimilar(btn.dataset.itemid, btn.dataset.cditem);
                        });
                    });
                })
                .catch(function () {
                    document.getElementById('similaresLoader').classList.add('d-none');
                    document.getElementById('similaresError').classList.remove('d-none');
                    similaresLoaded = false;
                });
        });
    }

    // Estoques do Similar - modal com DataTables
    var dtEstoquesSimilar = null;
    var modalEstoquesSimilar = document.getElementById('modalVerEstoquesSubstituto');

    function abrirModalEstoquesSimilar(itemSimilarId, cdItem) {
        document.getElementById('modalVerEstoquesSubstitutoLabel').innerHTML =
            '<i class="fa-duotone fa-clone me-2 text-br"></i>Estoques do Item #' + cdItem;

        document.getElementById('verEstoquesSubstitutoLoader').classList.remove('d-none');
        document.getElementById('verEstoquesSubstitutoError').classList.add('d-none');
        document.getElementById('verEstoquesSubstitutoEmpty').classList.add('d-none');
        document.getElementById('divTblVerEstoquesSubstituto').classList.add('d-none');

        if (dtEstoquesSimilar) {
            dtEstoquesSimilar.destroy();
            dtEstoquesSimilar = null;
            document.querySelector('#tblVerEstoquesSubstituto tbody').innerHTML = '';
        }

        var bsModal = new bootstrap.Modal(modalEstoquesSimilar);
        bsModal.show();

        fetch(basePath + '/Produtos/Detalhes/' + cfg.itemId + '/Similares/' + itemSimilarId + '/Estoques')
            .then(function (resp) {
                if (!resp.ok) throw new Error(resp.status);
                return resp.json();
            })
            .then(function (data) {
                document.getElementById('verEstoquesSubstitutoLoader').classList.add('d-none');

                if (!data || data.length === 0) {
                    document.getElementById('verEstoquesSubstitutoEmpty').classList.remove('d-none');
                    return;
                }

                document.getElementById('divTblVerEstoquesSubstituto').classList.remove('d-none');

                dtEstoquesSimilar = new DataTable('#tblVerEstoquesSubstituto', {
                    data: data,
                    responsive: true,
                    paging: data.length > 25,
                    pageLength: 25,
                    order: [[5, 'desc']],
                    language: {
                        emptyTable: 'Nenhum estoque encontrado',
                        info: 'Exibindo _START_ a _END_ de _TOTAL_ registros',
                        infoEmpty: 'Nenhum registro',
                        infoFiltered: '(filtrado de _MAX_ registros)',
                        lengthMenu: '_MENU_ por página',
                        search: 'Buscar:',
                        zeroRecords: 'Nenhum registro encontrado',
                        paginate: { first: 'Primeira', last: 'Última', next: '›', previous: '‹' }
                    },
                    columns: [
                        { data: 'cdEstabelecimento', className: 'text-center' },
                        { data: 'nmEstabelecimento' },
                        {
                            data: 'curva', className: 'text-center',
                            render: function (v) {
                                if (!v || v === '-') return '<span class="text-muted">-</span>';
                                var cls = v === 'A' ? 'produto-curva-a' : v === 'B' ? 'produto-curva-b' : v === 'C' ? 'produto-curva-c' : '';
                                return '<span class="produto-curva ' + cls + '">' + v + '</span>';
                            }
                        },
                        {
                            data: 'criticidade', className: 'text-center',
                            render: function (v) {
                                var map = { X: ['Normal', 'badge-status-info'], Y: ['Outlet', 'badge-status-warning'], Z: ['Sob Demanda', 'badge-status-primary'], F: ['Falta Fabricante', 'badge-status-danger'] };
                                var m = map[v] || [v, 'badge-status-info'];
                                return '<span class="produto-criticidade ' + m[1] + '" title="' + m[0] + '">' + v + '</span>';
                            }
                        },
                        {
                            data: 'situacao', className: 'text-center',
                            render: function (v) {
                                if (v === 'Inativo') return '<span class="badge bg-danger">Inativo</span>';
                                if (v === 'Falta no Fabricante') return '<span class="badge bg-danger">Falta no Fabricante</span>';
                                if (v === 'Outlet') return '<span class="badge bg-warning text-dark">Outlet</span>';
                                if (v === 'Sob Demanda') return '<span class="badge bg-primary">Sob Demanda</span>';
                                return '<span class="badge bg-success">Ativo</span>';
                            }
                        },
                        {
                            data: 'qtDisponivel', className: 'text-center fw-semibold',
                            render: function (v) {
                                return v > 0 ? '<span class="text-success">' + v + '</span>' : '<span class="text-danger">' + v + '</span>';
                            }
                        }
                    ]
                });
            })
            .catch(function () {
                document.getElementById('verEstoquesSubstitutoLoader').classList.add('d-none');
                document.getElementById('verEstoquesSubstitutoError').classList.remove('d-none');
            });
    }

    modalEstoquesSimilar.addEventListener('hidden.bs.modal', function () {
        if (dtEstoquesSimilar) {
            dtEstoquesSimilar.destroy();
            dtEstoquesSimilar = null;
            document.querySelector('#tblVerEstoquesSubstituto tbody').innerHTML = '';
        }
    });

    // Produtos Relacionados - carrossel Swiper
    fetch(basePath + '/Produtos/Detalhes/' + cfg.itemId + '/Relacionados')
        .then(function (resp) { return resp.ok ? resp.json() : []; })
        .then(function (data) {
            if (!data || data.length === 0) return;

            var container = document.getElementById('carrouselRelacionados');
            var semFoto = 'https://www.supplymanager.com.br/fotos/semimagem.jpg';

            var slides = data.map(function (r) {
                return '<div class="swiper-slide">' +
                    '<a href="' + basePath + '/Produtos/Detalhes/' + r.itemID + '" class="relacionado-card">' +
                    '<img src="' + (r.foto || semFoto) + '" alt="' + r.nmItem + '" onerror="this.src=\'' + semFoto + '\'" />' +
                    '<div class="relacionado-info">' +
                    '<span class="relacionado-codigo text-muted">' + r.cdItem + '</span>' +
                    '<span class="relacionado-nome">' + r.nmItem + '</span>' +
                    '</div></a></div>';
            }).join('');

            container.innerHTML =
                '<div class="relacionados-section mt-4">' +
                '<div class="d-flex align-items-center gap-2 mb-3"><i class="fa-duotone fa-link text-br fa-lg"></i><h5 class="mb-0 fw-semibold">Produtos Relacionados</h5></div>' +
                '<div class="swiper relacionados-swiper">' +
                '<div class="swiper-wrapper">' + slides + '</div>' +
                '<div class="swiper-button-prev"></div>' +
                '<div class="swiper-button-next"></div>' +
                '</div></div>';

            new Swiper('.relacionados-swiper', {
                slidesPerView: 2,
                spaceBetween: 16,
                navigation: { nextEl: '.swiper-button-next', prevEl: '.swiper-button-prev' },
                breakpoints: {
                    576: { slidesPerView: 3 },
                    768: { slidesPerView: 4 },
                    992: { slidesPerView: 5 },
                    1200: { slidesPerView: 6 }
                }
            });
        })
        .catch(function () { });

})();
