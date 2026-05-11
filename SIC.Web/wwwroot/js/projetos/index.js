(function () {
    'use strict';

    // ====== Page loading overlay ======
    function showPageLoading() {
        var overlay = document.getElementById('projetoLoadingOverlay');
        if (!overlay) {
            overlay = document.createElement('div');
            overlay.id = 'projetoLoadingOverlay';
            overlay.className = 'projeto-loading-overlay';
            overlay.innerHTML = '<div class="projeto-loading-spinner"></div>';
            document.body.appendChild(overlay);
        }
        overlay.offsetHeight;
        overlay.classList.add('active');
    }

    window.addEventListener('pageshow', function () {
        var overlay = document.getElementById('projetoLoadingOverlay');
        if (overlay) overlay.classList.remove('active');
    });

    // ====== Navegacao para detalhes ao clicar no card ======
    document.querySelectorAll('.projeto-card[data-url]').forEach(function (card) {
        card.addEventListener('click', function () {
            showPageLoading();
            window.location.href = card.dataset.url;
        });
    });

    // ====== Form de filtros - overlay ao submeter ======
    var formFiltros = document.getElementById('formFiltros');
    if (formFiltros) {
        formFiltros.addEventListener('submit', function () {
            showPageLoading();
        });
        var _origSubmit = HTMLFormElement.prototype.submit;
        formFiltros.submit = function () {
            showPageLoading();
            _origSubmit.call(formFiltros);
        };
    }

    // ====== Limpar filtros ======
    var btnLimpar = document.getElementById('btnLimparFiltros');
    if (btnLimpar) {
        btnLimpar.addEventListener('click', function (e) {
            e.preventDefault();
            var form = document.getElementById('formFiltros');
            if (form) {
                form.querySelectorAll('input[type="text"], input[type="search"]').forEach(function (el) { el.value = ''; });
                form.querySelectorAll('select').forEach(function (el) { el.selectedIndex = 0; });
                form.submit();
            }
        });
    }

    // ====== Remocao de filter tags ======
    document.querySelectorAll('.projeto-filter-tag').forEach(function (tag) {
        tag.addEventListener('click', function () {
            var field = tag.dataset.field;
            var form = document.getElementById('formFiltros');
            if (form && field) {
                var input = form.querySelector('[name="' + field + '"]');
                if (input) {
                    if (input.tagName === 'SELECT') {
                        input.selectedIndex = 0;
                    } else {
                        input.value = '';
                    }
                }
                form.submit();
            }
        });
    });

    // ====== Modal Novo Projeto - submit via AJAX ======
    var formNovoProjeto = document.getElementById('formNovoProjeto');
    if (formNovoProjeto) {
        formNovoProjeto.addEventListener('submit', function (e) {
            e.preventDefault();

            var nmProjeto = document.getElementById('npNmProjeto').value.trim();
            if (!nmProjeto) {
                document.getElementById('npNmProjeto').classList.add('is-invalid');
                document.getElementById('npNmProjeto').focus();
                return;
            }
            document.getElementById('npNmProjeto').classList.remove('is-invalid');

            var dtInicio = document.getElementById('npDtInicio').value;
            var dtPrevisaoFim = document.getElementById('npDtPrevisaoFim').value;
            if (dtInicio && dtPrevisaoFim && dtPrevisaoFim < dtInicio) {
                document.getElementById('npDtPrevisaoFim').classList.add('is-invalid');
                document.getElementById('npDtPrevisaoFim').focus();
                return;
            }
            document.getElementById('npDtPrevisaoFim').classList.remove('is-invalid');

            var payload = {
                NmProjeto: nmProjeto,
                DsProjeto: (document.getElementById('npDsProjeto').value || '').trim(),
                DtInicio: document.getElementById('npDtInicio').value || null,
                DtPrevisaoFim: document.getElementById('npDtPrevisaoFim').value || null,
                CamposExtras: Array.from(document.querySelectorAll('#npCamposExtras .np-campo-extra'))
                    .map(function (row) {
                        return {
                            Ordem: parseInt(row.dataset.ordem, 10),
                            NmCampo: (row.querySelector('.np-campo-nome').value || '').trim(),
                            VlCampo: (row.querySelector('.np-campo-valor').value || '').trim()
                        };
                    })
                    .filter(function (c) { return c.NmCampo.length > 0; })
            };

            var btnSalvar = document.getElementById('btnSalvarNovoProjeto');
            btnSalvar.disabled = true;
            btnSalvar.innerHTML = '<i class="fa-solid fa-spinner fa-spin me-1"></i>Criando...';

            fetch(window.sicUrl('/Projetos/Criar'), {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify(payload)
            })
            .then(function (res) {
                if (!res.ok) return res.json().then(function (err) { throw err; });
                return res.json();
            })
            .then(function (data) {
                var modal = bootstrap.Modal.getInstance(document.getElementById('modalNovoProjeto'));
                if (modal) modal.hide();

                Swal.fire({
                    icon: 'success',
                    title: 'Projeto criado!',
                    text: 'Redirecionando para o projeto...',
                    timer: 1500,
                    showConfirmButton: false
                }).then(function () {
                    window.location.href = window.sicUrl('/Projetos/' + data.projetoId);
                });
            })
            .catch(function (err) {
                Swal.fire({
                    icon: 'error',
                    title: 'Erro',
                    text: (err && err.mensagem) || 'Nao foi possivel criar o projeto.'
                });
            })
            .finally(function () {
                btnSalvar.disabled = false;
                btnSalvar.innerHTML = '<i class="fa-solid fa-check me-1"></i>Criar Projeto';
            });
        });

        // Limpar form ao fechar modal
        document.getElementById('modalNovoProjeto').addEventListener('hidden.bs.modal', function () {
            formNovoProjeto.reset();
            formNovoProjeto.querySelectorAll('.is-invalid').forEach(function (el) {
                el.classList.remove('is-invalid');
            });
        });

        // Limpar validacao ao interagir com campos
        formNovoProjeto.addEventListener('input', function (e) { e.target.classList.remove('is-invalid'); });
        formNovoProjeto.addEventListener('change', function (e) { e.target.classList.remove('is-invalid'); });
    }

})();
