(function () {

    var cfg = window.clienteConfig;
    var basePath = cfg.basePath;
    var fmtBRL = new Intl.NumberFormat('pt-BR', { style: 'currency', currency: 'BRL' });

    // Títulos em Aberto — lazy load com filtro por situação e paginação manual
    var titulosLoaded = false;
    var collapseTit = document.getElementById('collapseTitulos');
    if (collapseTit) {
        var titAllData = [];
        var titFiltered = [];
        var titCurrentFilter = null;
        var titPage = 1;
        var titPageSize = 10;

        var situacaoBadge = {
            'Vencido': 'bg-danger',
            'A Vencer': 'bg-info',
            'Crédito': 'bg-success'
        };

        function titRender() {
            var total = titFiltered.length;
            var totalPages = Math.ceil(total / titPageSize) || 1;
            if (titPage > totalPages) titPage = totalPages;
            var start = (titPage - 1) * titPageSize;
            var pageData = titFiltered.slice(start, start + titPageSize);

            var tbody = document.getElementById('tblTitulosBody');
            tbody.innerHTML = '';
            pageData.forEach(function (r) {
                var badgeClass = situacaoBadge[r.situacao] || 'bg-secondary';
                var tr = document.createElement('tr');
                tr.innerHTML =
                    '<td class="text-center">' + (r.dtEmissao || '<span class="text-muted">—</span>') + '</td>' +
                    '<td class="text-center fw-semibold">' + r.nrNotaFiscal + '</td>' +
                    '<td class="text-center">' + (r.serie || '<span class="text-muted">—</span>') + '</td>' +
                    '<td class="text-center">' + (r.parcela || '<span class="text-muted">—</span>') + '</td>' +
                    '<td class="text-center">' + (r.dtVencimento || '<span class="text-muted">—</span>') + '</td>' +
                    '<td class="text-center"><span class="badge ' + badgeClass + '">' + r.situacao + '</span></td>' +
                    '<td class="text-end">' + fmtBRL.format(r.vlrOriginal) + '</td>' +
                    '<td class="text-end">' + fmtBRL.format(r.vlrSaldo) + '</td>';
                tbody.appendChild(tr);
            });

            // Info
            var info = document.getElementById('titPaginationInfo');
            if (total === 0) {
                info.textContent = 'Nenhum registro';
            } else {
                info.textContent = (start + 1) + ' – ' + Math.min(start + titPageSize, total) + ' de ' + total + ' registros';
            }

            // Pagination
            var pag = document.getElementById('titPagination');
            pag.innerHTML = '';
            var wrap = document.getElementById('titPaginationWrap');
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
                    a.addEventListener('click', function (e) { e.preventDefault(); titPage = page; titRender(); });
                }
                li.appendChild(a);
                pag.appendChild(li);
            }

            addLi('‹', titPage - 1, titPage === 1, false);
            for (var p = 1; p <= totalPages; p++) {
                addLi(p, p, false, p === titPage);
            }
            addLi('›', titPage + 1, titPage === totalPages, false);
        }

        function titApplyFilter(situacao) {
            titCurrentFilter = situacao;
            titFiltered = situacao ? titAllData.filter(function (r) { return r.situacao === situacao; }) : titAllData;
            titPage = 1;

            document.querySelectorAll('#titFilterBar .tit-filter-pill').forEach(function (p) {
                p.classList.toggle('active', p.dataset.situacao === (situacao || ''));
            });

            titRender();
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
                    titFiltered = data;
                    document.getElementById('divTblTitulos').classList.remove('d-none');
                    titBuildFilters(data);
                    titRender();
                })
                .catch(function () {
                    document.getElementById('titulosLoader').classList.add('d-none');
                    document.getElementById('titulosError').classList.remove('d-none');
                    titulosLoaded = false;
                });
        });
    }

})();
