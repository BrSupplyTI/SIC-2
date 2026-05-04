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

    // ====== Toggle subtarefas ======
    document.querySelectorAll('.tarefa-toggle-subtarefas').forEach(function (btn) {
        btn.addEventListener('click', function (e) {
            e.stopPropagation();
            var tarefaId = btn.dataset.tarefaId;
            var container = document.getElementById('subtarefas-' + tarefaId);
            if (!container) return;

            var isOpen = container.classList.toggle('d-none');
            var icon = btn.querySelector('i');
            if (icon) {
                icon.classList.toggle('fa-chevron-down', isOpen);
                icon.classList.toggle('fa-chevron-up', !isOpen);
            }
        });
    });

    // ====== Accordion sections (tarefas, participantes, histórico) ======
    document.querySelectorAll('[data-toggle-section]').forEach(function (btn) {
        btn.addEventListener('click', function () {
            var targetId = btn.dataset.toggleSection;
            var section = document.getElementById(targetId);
            if (!section) return;

            section.classList.toggle('d-none');
            var icon = btn.querySelector('.section-chevron');
            if (icon) {
                icon.classList.toggle('fa-chevron-down');
                icon.classList.toggle('fa-chevron-up');
            }
        });
    });

    // ====== Modal Editar Projeto — submit via AJAX ======
    var formEditarProjeto = document.getElementById('formEditarProjeto');
    if (formEditarProjeto) {
        formEditarProjeto.addEventListener('submit', function (e) {
            e.preventDefault();

            var nmProjeto = document.getElementById('epNmProjeto').value.trim();
            if (!nmProjeto) {
                document.getElementById('epNmProjeto').classList.add('is-invalid');
                document.getElementById('epNmProjeto').focus();
                return;
            }
            document.getElementById('epNmProjeto').classList.remove('is-invalid');

            if (!validarDatas('epDtInicio', 'epDtPrevisaoFim')) return;

            var payload = {
                ProjetoID: parseInt(document.getElementById('epProjetoID').value, 10),
                NmProjeto: nmProjeto,
                DsProjeto: (document.getElementById('epDsProjeto').value || '').trim(),
                ProjetoStatusID: parseInt(document.getElementById('epProjetoStatusID').value, 10),
                DtInicio: document.getElementById('epDtInicio').value || null,
                DtPrevisaoFim: document.getElementById('epDtPrevisaoFim').value || null,
                DtFimReal: document.getElementById('epDtFimReal').value || null
            };

            var btnSalvar = document.getElementById('btnSalvarEditarProjeto');
            btnSalvar.disabled = true;
            btnSalvar.innerHTML = '<i class="fa-solid fa-spinner fa-spin me-1"></i>Salvando...';

            fetch('/Projetos/Editar', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify(payload)
            })
            .then(function (res) {
                if (!res.ok) return res.json().then(function (err) { throw err; });
                return res.json();
            })
            .then(function (data) {
                var modal = bootstrap.Modal.getInstance(document.getElementById('modalEditarProjeto'));
                if (modal) modal.hide();

                Swal.fire({
                    icon: 'success',
                    title: 'Projeto atualizado!',
                    text: 'A página será recarregada...',
                    timer: 1500,
                    showConfirmButton: false
                }).then(function () {
                    showPageLoading();
                    window.location.reload();
                });
            })
            .catch(function (err) {
                Swal.fire({
                    icon: 'error',
                    title: 'Erro',
                    text: (err && err.mensagem) || 'Não foi possível atualizar o projeto.'
                });
            })
            .finally(function () {
                            btnSalvar.disabled = false;
                                btnSalvar.innerHTML = '<i class="fa-solid fa-check me-1"></i>Salvar Alterações';
                            });
                        });
                    }

                    // ====== Modal Nova Tarefa — submit via AJAX ======
                    var formNovaTarefa = document.getElementById('formNovaTarefa');
                    if (formNovaTarefa) {
                        formNovaTarefa.addEventListener('submit', function (e) {
                            e.preventDefault();

                            var nmTarefa = document.getElementById('ntNmTarefa').value.trim();
                            if (!nmTarefa) {
                                document.getElementById('ntNmTarefa').classList.add('is-invalid');
                                document.getElementById('ntNmTarefa').focus();
                                return;
                            }
                            document.getElementById('ntNmTarefa').classList.remove('is-invalid');

                            if (!validarDatas('ntDtInicio', 'ntDtPrevisaoFim')) return;

                            var tarefaPaiVal = document.getElementById('ntProjetoTarefaPaiID').value;

                            var payload = {
                                ProjetoID: parseInt(document.getElementById('ntProjetoID').value, 10),
                                NmTarefa: nmTarefa,
                                DsTarefa: (document.getElementById('ntDsTarefa').value || '').trim() || null,
                                ProjetoTarefaStatusID: parseInt(document.getElementById('ntProjetoTarefaStatusID').value, 10),
                                ProjetoTarefaPrioridadeID: parseInt(document.getElementById('ntProjetoTarefaPrioridadeID').value, 10),
                                DtInicio: document.getElementById('ntDtInicio').value || null,
                                DtPrevisaoFim: document.getElementById('ntDtPrevisaoFim').value || null,
                                ProjetoTarefaPaiID: tarefaPaiVal ? parseInt(tarefaPaiVal, 10) : null
                            };

                            var btnSalvar = document.getElementById('btnSalvarNovaTarefa');
                            btnSalvar.disabled = true;
                            btnSalvar.innerHTML = '<i class="fa-solid fa-spinner fa-spin me-1"></i>Criando...';

                            fetch('/Projetos/CriarTarefa', {
                                method: 'POST',
                                headers: { 'Content-Type': 'application/json' },
                                body: JSON.stringify(payload)
                            })
                            .then(function (res) {
                                if (!res.ok) return res.json().then(function (err) { throw err; });
                                return res.json();
                            })
                            .then(function () {
                                var modal = bootstrap.Modal.getInstance(document.getElementById('modalNovaTarefa'));
                                if (modal) modal.hide();

                                Swal.fire({
                                     icon: 'success',
                                     title: 'Tarefa criada!',
                                     text: 'A página será recarregada...',
                                     timer: 1500,
                                     showConfirmButton: false
                                 }).then(function () {
                                     showPageLoading();
                                     window.location.reload();
                                 });
                            })
                            .catch(function (err) {
                                Swal.fire({
                                    icon: 'error',
                                    title: 'Erro',
                                    text: (err && err.mensagem) || 'Não foi possível criar a tarefa.'
                                });
                            })
                            .finally(function () {
                                btnSalvar.disabled = false;
                                btnSalvar.innerHTML = '<i class="fa-solid fa-check me-1"></i>Criar Tarefa';
                            });
                        });

                        // Limpar form ao fechar modal
                        document.getElementById('modalNovaTarefa').addEventListener('hidden.bs.modal', function () {
                            formNovaTarefa.reset();
                            formNovaTarefa.querySelectorAll('.is-invalid').forEach(function (el) {
                                el.classList.remove('is-invalid');
                            });
                        });
                    }

    // ====== Helper: converte dd/MM/yyyy → yyyy-MM-dd para inputs date ======
    function brDateToIso(brDate) {
        if (!brDate) return '';
        var parts = brDate.split('/');
        if (parts.length !== 3) return '';
        return parts[2] + '-' + parts[1] + '-' + parts[0];
    }

    // ====== Helper: valida intervalo de datas (previsão >= início) ======
    function validarDatas(dtInicioId, dtPrevisaoFimId) {
        var dtInicio = document.getElementById(dtInicioId).value;
        var dtPrevisaoFim = document.getElementById(dtPrevisaoFimId).value;
        if (dtInicio && dtPrevisaoFim && dtPrevisaoFim < dtInicio) {
            document.getElementById(dtPrevisaoFimId).classList.add('is-invalid');
            document.getElementById(dtPrevisaoFimId).focus();
            return false;
        }
        document.getElementById(dtPrevisaoFimId).classList.remove('is-invalid');
        return true;
    }

    // ====== Modal Editar Tarefa — abrir e popular ======
    document.querySelectorAll('.btn-editar-tarefa').forEach(function (btn) {
        btn.addEventListener('click', function (e) {
            e.stopPropagation();

            document.getElementById('etProjetoTarefaID').value = btn.dataset.tarefaId;
            document.getElementById('etNmTarefa').value = btn.dataset.nmTarefa || '';
            document.getElementById('etDsTarefa').value = btn.dataset.dsTarefa || '';
            document.getElementById('etProjetoTarefaStatusID').value = btn.dataset.statusId || '';
            document.getElementById('etProjetoTarefaPrioridadeID').value = btn.dataset.prioridadeId || '';
            document.getElementById('etDtInicio').value = brDateToIso(btn.dataset.dtInicio);
            document.getElementById('etDtPrevisaoFim').value = brDateToIso(btn.dataset.dtPrevisaoFim);
            document.getElementById('etDtFimReal').value = brDateToIso(btn.dataset.dtFimReal);

            var modal = new bootstrap.Modal(document.getElementById('modalEditarTarefa'));
            modal.show();
        });
    });

    // ====== Modal Editar Tarefa — submit via AJAX ======
    var formEditarTarefa = document.getElementById('formEditarTarefa');
    if (formEditarTarefa) {
        formEditarTarefa.addEventListener('submit', function (e) {
            e.preventDefault();

            var nmTarefa = document.getElementById('etNmTarefa').value.trim();
            if (!nmTarefa) {
                document.getElementById('etNmTarefa').classList.add('is-invalid');
                document.getElementById('etNmTarefa').focus();
                return;
            }
            document.getElementById('etNmTarefa').classList.remove('is-invalid');

            if (!validarDatas('etDtInicio', 'etDtPrevisaoFim')) return;

            var payload = {
                ProjetoTarefaID: parseInt(document.getElementById('etProjetoTarefaID').value, 10),
                ProjetoID: parseInt(document.getElementById('ntProjetoID').value, 10),
                NmTarefa: nmTarefa,
                DsTarefa: (document.getElementById('etDsTarefa').value || '').trim() || null,
                ProjetoTarefaStatusID: parseInt(document.getElementById('etProjetoTarefaStatusID').value, 10),
                ProjetoTarefaPrioridadeID: parseInt(document.getElementById('etProjetoTarefaPrioridadeID').value, 10),
                DtInicio: document.getElementById('etDtInicio').value || null,
                DtPrevisaoFim: document.getElementById('etDtPrevisaoFim').value || null,
                DtFimReal: document.getElementById('etDtFimReal').value || null
            };

            var btnSalvar = document.getElementById('btnSalvarEditarTarefa');
            btnSalvar.disabled = true;
            btnSalvar.innerHTML = '<i class="fa-solid fa-spinner fa-spin me-1"></i>Salvando...';

            fetch('/Projetos/EditarTarefa', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify(payload)
            })
            .then(function (res) {
                if (!res.ok) return res.json().then(function (err) { throw err; });
                return res.json();
            })
            .then(function () {
                var modal = bootstrap.Modal.getInstance(document.getElementById('modalEditarTarefa'));
                if (modal) modal.hide();

                Swal.fire({
                    icon: 'success',
                    title: 'Tarefa atualizada!',
                    text: 'A página será recarregada...',
                    timer: 1500,
                    showConfirmButton: false
                }).then(function () {
                    showPageLoading();
                    window.location.reload();
                });
            })
            .catch(function (err) {
                Swal.fire({
                    icon: 'error',
                    title: 'Erro',
                    text: (err && err.mensagem) || 'Não foi possível atualizar a tarefa.'
                });
            })
            .finally(function () {
                            btnSalvar.disabled = false;
                                btnSalvar.innerHTML = '<i class="fa-solid fa-check me-1"></i>Salvar Alterações';
                            });
                        });
                    }

                    // ====== Exclusão de tarefa — SweetAlert2 confirm → POST ======
                    document.querySelectorAll('.btn-excluir-tarefa').forEach(function (btn) {
                        btn.addEventListener('click', function (e) {
                            e.stopPropagation();

                            var tarefaId = parseInt(btn.dataset.tarefaId, 10);
                            var nmTarefa = btn.dataset.nmTarefa || 'esta tarefa';

                            Swal.fire({
                                icon: 'warning',
                                title: 'Excluir tarefa?',
                                html: 'Tem certeza que deseja excluir <strong>' + nmTarefa + '</strong>?<br><small class="text-muted">Esta ação não poderá ser desfeita.</small>',
                                showCancelButton: true,
                                confirmButtonColor: '#dc3545',
                                confirmButtonText: '<i class="fa-solid fa-trash-can me-1"></i>Excluir',
                                cancelButtonText: 'Cancelar',
                                focusCancel: true
                            }).then(function (result) {
                                if (!result.isConfirmed) return;

                                Swal.fire({
                                    title: 'Excluindo...',
                                    allowOutsideClick: false,
                                    allowEscapeKey: false,
                                    didOpen: function () { Swal.showLoading(); }
                                });

                                fetch('/Projetos/ExcluirTarefa', {
                                    method: 'POST',
                                    headers: { 'Content-Type': 'application/json' },
                                    body: JSON.stringify({ ProjetoTarefaID: tarefaId, ProjetoID: parseInt(document.getElementById('ntProjetoID').value, 10) })
                                })
                                .then(function (res) {
                                    if (!res.ok) return res.json().then(function (err) { throw err; });
                                    return res.json();
                                })
                                .then(function () {
                                    Swal.fire({
                                        icon: 'success',
                                        title: 'Tarefa excluída!',
                                        text: 'A página será recarregada...',
                                        timer: 1500,
                                        showConfirmButton: false
                                    }).then(function () {
                                        showPageLoading();
                                        window.location.reload();
                                    });
                                })
                                .catch(function (err) {
                                    Swal.fire({
                                        icon: 'error',
                                        title: 'Erro',
                                        text: (err && err.mensagem) || 'Não foi possível excluir a tarefa.'
                                    });
                                });
                            });
                        });
                    });

    // ====== Atualização rápida de status da tarefa ======
    document.addEventListener('click', function (e) {
        var btn = e.target.closest('.btn-alterar-status-rapido');
        if (!btn) return;

        var tarefaId = parseInt(btn.dataset.tarefaId, 10);
        var newStatusId = parseInt(btn.dataset.statusId, 10);
        var projetoIdEl = document.getElementById('ntProjetoID');
        if (!projetoIdEl) return;
        var projetoId = parseInt(projetoIdEl.value, 10);

        var editBtn = document.querySelector('.btn-editar-tarefa[data-tarefa-id="' + tarefaId + '"]');
        if (!editBtn) return;

        var payload = {
            ProjetoTarefaID: tarefaId,
            ProjetoID: projetoId,
            NmTarefa: editBtn.dataset.nmTarefa,
            DsTarefa: editBtn.dataset.dsTarefa || null,
            ProjetoTarefaStatusID: newStatusId,
            ProjetoTarefaPrioridadeID: parseInt(editBtn.dataset.prioridadeId, 10),
            DtInicio: brDateToIso(editBtn.dataset.dtInicio) || null,
            DtPrevisaoFim: brDateToIso(editBtn.dataset.dtPrevisaoFim) || null,
            DtFimReal: brDateToIso(editBtn.dataset.dtFimReal) || null
        };

        var trigger = btn.closest('.dropdown').querySelector('.tarefa-status-trigger');
        var originalHtml = trigger ? trigger.innerHTML : '';
        if (trigger) trigger.innerHTML = '<i class="fa-solid fa-spinner fa-spin" style="font-size: 0.65rem;"></i>';

        fetch('/Projetos/EditarTarefa', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(payload)
        })
        .then(function (res) {
            if (!res.ok) return res.json().then(function (err) { throw err; });
            return res.json();
        })
        .then(function () {
            showPageLoading();
            window.location.reload();
        })
        .catch(function (err) {
            if (trigger) trigger.innerHTML = originalHtml;
            Swal.fire({
                icon: 'error',
                title: 'Erro',
                text: (err && err.mensagem) || 'Não foi possível atualizar o status da tarefa.'
            });
        });
    });

    // ====== Limpar validação ao interagir com campos ======
    ['formEditarProjeto', 'formNovaTarefa', 'formEditarTarefa'].forEach(function (formId) {
        var form = document.getElementById(formId);
        if (form) {
            form.addEventListener('input', function (e) { e.target.classList.remove('is-invalid'); });
            form.addEventListener('change', function (e) { e.target.classList.remove('is-invalid'); });
        }
    });

    // ====== Limpar validação ao fechar modais de edição ======
    ['modalEditarProjeto', 'modalEditarTarefa'].forEach(function (modalId) {
        var modal = document.getElementById(modalId);
        if (modal) {
            modal.addEventListener('hidden.bs.modal', function () {
                modal.querySelectorAll('.is-invalid').forEach(function (el) {
                    el.classList.remove('is-invalid');
                });
            });
        }
    });

    // ====== Toggle Lista / Kanban ======
    var btnLista = document.getElementById('btnViewLista');
    var btnKanban = document.getElementById('btnViewKanban');
    var viewLista = document.getElementById('tarefasViewLista');
    var viewKanban = document.getElementById('tarefasViewKanban');

    if (btnLista && btnKanban && viewLista && viewKanban) {
        btnLista.addEventListener('click', function () {
            btnLista.classList.add('active');
            btnKanban.classList.remove('active');
            viewLista.classList.remove('d-none');
            viewKanban.classList.add('d-none');
        });

        btnKanban.addEventListener('click', function () {
            btnKanban.classList.add('active');
            btnLista.classList.remove('active');
            viewKanban.classList.remove('d-none');
            viewLista.classList.add('d-none');
        });
    }

    // ====== Kanban Drag & Drop ======
    var draggedCard = null;

    document.querySelectorAll('.kanban-draggable').forEach(function (card) {
        card.addEventListener('dragstart', function (e) {
            draggedCard = card;
            card.classList.add('kanban-dragging');
            e.dataTransfer.effectAllowed = 'move';
            e.dataTransfer.setData('text/plain', card.dataset.tarefaId);
        });

        card.addEventListener('dragend', function () {
            card.classList.remove('kanban-dragging');
            draggedCard = null;
            document.querySelectorAll('.kanban-drag-over').forEach(function (el) {
                el.classList.remove('kanban-drag-over');
            });
        });
    });

    document.querySelectorAll('.kanban-droppable').forEach(function (zone) {
        zone.addEventListener('dragover', function (e) {
            e.preventDefault();
            e.dataTransfer.dropEffect = 'move';
            zone.classList.add('kanban-drag-over');
        });

        zone.addEventListener('dragleave', function (e) {
            if (!zone.contains(e.relatedTarget)) {
                zone.classList.remove('kanban-drag-over');
            }
        });

        zone.addEventListener('drop', function (e) {
            e.preventDefault();
            zone.classList.remove('kanban-drag-over');

            if (!draggedCard) return;

            var newStatusId = parseInt(zone.dataset.statusId, 10);
            var currentStatusId = parseInt(draggedCard.dataset.statusId, 10);
            if (newStatusId === currentStatusId) return;

            var tarefaId = parseInt(draggedCard.dataset.tarefaId, 10);
            var projetoIdEl = document.getElementById('ntProjetoID');
            if (!projetoIdEl) return;
            var projetoId = parseInt(projetoIdEl.value, 10);

            var payload = {
                ProjetoTarefaID: tarefaId,
                ProjetoID: projetoId,
                NmTarefa: draggedCard.dataset.nmTarefa,
                DsTarefa: draggedCard.dataset.dsTarefa || null,
                ProjetoTarefaStatusID: newStatusId,
                ProjetoTarefaPrioridadeID: parseInt(draggedCard.dataset.prioridadeId, 10),
                UsuarioResponsavelID: draggedCard.dataset.responsavelId ? parseInt(draggedCard.dataset.responsavelId, 10) : null,
                DtInicio: brDateToIso(draggedCard.dataset.dtInicio) || null,
                DtPrevisaoFim: brDateToIso(draggedCard.dataset.dtPrevisaoFim) || null,
                DtFimReal: brDateToIso(draggedCard.dataset.dtFimReal) || null
            };

            // Move card visually (optimistic)
            var placeholder = zone.querySelector('.kanban-empty-placeholder');
            if (placeholder) placeholder.remove();
            zone.appendChild(draggedCard);
            draggedCard.dataset.statusId = newStatusId;

            // Update column counters
            updateKanbanCounters();

            fetch('/Projetos/EditarTarefa', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify(payload)
            })
            .then(function (res) {
                if (!res.ok) return res.json().then(function (err) { throw err; });
                return res.json();
            })
            .then(function () {
                showPageLoading();
                window.location.reload();
            })
            .catch(function (err) {
                Swal.fire({
                    icon: 'error',
                    title: 'Erro',
                    text: (err && err.mensagem) || 'Não foi possível atualizar o status da tarefa.'
                });
                showPageLoading();
                window.location.reload();
            });
        });
    });

    function updateKanbanCounters() {
        document.querySelectorAll('.kanban-column').forEach(function (col) {
            var cards = col.querySelectorAll('.kanban-card').length;
            var badge = col.querySelector('.kanban-column-header .badge');
            if (badge) badge.textContent = cards;
        });
    }

    // ====== Kanban Inline Creation ======

    function closeAllKanbanInlineForms() {
        document.querySelectorAll('#tarefasViewKanban .kanban-inline-form').forEach(function (form) {
            form.classList.add('d-none');
            var input = form.querySelector('.kanban-inline-input');
            if (input) {
                input.value = '';
                input.classList.remove('is-invalid');
            }
        });
        document.querySelectorAll('#tarefasViewKanban .kanban-btn-add-task').forEach(function (btn) {
            btn.classList.remove('d-none');
        });
        document.querySelectorAll('#tarefasViewKanban .kanban-btn-add-subtask').forEach(function (btn) {
            btn.classList.remove('d-none');
        });
    }

    // "+ Tarefa" button in column footer
    document.addEventListener('click', function (e) {
        var btn = e.target.closest('#tarefasViewKanban .kanban-btn-add-task');
        if (!btn) return;
        closeAllKanbanInlineForms();
        var footer = btn.closest('.kanban-column-footer');
        if (!footer) return;
        var form = footer.querySelector('.kanban-inline-form');
        if (!form) return;
        form.classList.remove('d-none');
        btn.classList.add('d-none');
        var input = form.querySelector('.kanban-inline-input');
        if (input) input.focus();
    });

    // "+ Subitem" button inside card
    document.addEventListener('click', function (e) {
        var btn = e.target.closest('#tarefasViewKanban .kanban-btn-add-subtask');
        if (!btn) return;
        closeAllKanbanInlineForms();
        var card = btn.closest('.kanban-card');
        if (!card) return;
        var form = card.querySelector('.kanban-inline-form');
        if (!form) return;
        form.classList.remove('d-none');
        btn.classList.add('d-none');
        var input = form.querySelector('.kanban-inline-input');
        if (input) input.focus();
    });

    // Cancel button
    document.addEventListener('click', function (e) {
        var btn = e.target.closest('#tarefasViewKanban .kanban-inline-btn-cancel');
        if (!btn) return;
        closeAllKanbanInlineForms();
    });

    // Escape key closes inline forms
    document.addEventListener('keydown', function (e) {
        if (e.key !== 'Escape') return;
        var active = document.activeElement;
        if (active && active.closest('#tarefasViewKanban .kanban-inline-form')) {
            closeAllKanbanInlineForms();
        }
    });

    // Clear is-invalid on typing
    document.addEventListener('input', function (e) {
        if (e.target.matches('#tarefasViewKanban .kanban-inline-input')) {
            e.target.classList.remove('is-invalid');
        }
    });

    // Click outside closes inline forms
    document.addEventListener('click', function (e) {
        if (!e.target.closest('#tarefasViewKanban .kanban-inline-form') &&
            !e.target.closest('#tarefasViewKanban .kanban-btn-add-task') &&
            !e.target.closest('#tarefasViewKanban .kanban-btn-add-subtask')) {
            closeAllKanbanInlineForms();
        }
    });

    var kanbanInlineSubmitting = false;

    function submitKanbanInlineForm(form) {
        if (kanbanInlineSubmitting) return;

        var input = form.querySelector('.kanban-inline-input');
        var title = input ? input.value.trim() : '';
        if (!title) {
            if (input) { input.classList.add('is-invalid'); input.focus(); }
            return;
        }

        var projetoIdEl = document.getElementById('ntProjetoID');
        if (!projetoIdEl) return;
        var projetoId = parseInt(projetoIdEl.value, 10);

        var inlineType = form.dataset.inlineType; // "task" or "subtask"
        var statusId = 1; // default
        var tarefaPaiId = null;

        if (inlineType === 'task') {
            var statusAttr = form.dataset.statusId;
            if (statusAttr) statusId = parseInt(statusAttr, 10);
        } else if (inlineType === 'subtask') {
            var card = form.closest('.kanban-card');
            if (card) {
                statusId = parseInt(card.dataset.statusId, 10) || 1;
            }
            var paiAttr = form.dataset.tarefaPaiId;
            if (paiAttr) tarefaPaiId = parseInt(paiAttr, 10);
        }

        var payload = {
            ProjetoID: projetoId,
            NmTarefa: title,
            DsTarefa: null,
            ProjetoTarefaStatusID: statusId,
            ProjetoTarefaPrioridadeID: 2,
            DtInicio: null,
            DtPrevisaoFim: null,
            ProjetoTarefaPaiID: tarefaPaiId
        };

        var confirmBtn = form.querySelector('.kanban-inline-btn-confirm');
        if (confirmBtn) {
            confirmBtn.disabled = true;
            confirmBtn.innerHTML = '<i class="fa-solid fa-spinner fa-spin"></i>';
        }
        if (input) input.readOnly = true;
        kanbanInlineSubmitting = true;

        fetch('/Projetos/CriarTarefa', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(payload)
        })
        .then(function (res) {
            if (!res.ok) return res.json().then(function (err) { throw err; });
            return res.json();
        })
        .then(function () {
            Swal.fire({
                icon: 'success',
                title: inlineType === 'subtask' ? 'Subtarefa criada!' : 'Tarefa criada!',
                text: 'A página será recarregada...',
                timer: 1200,
                showConfirmButton: false
            }).then(function () {
                showPageLoading();
                window.location.reload();
            });
        })
        .catch(function (err) {
            kanbanInlineSubmitting = false;
            if (confirmBtn) {
                confirmBtn.disabled = false;
                confirmBtn.innerHTML = '<i class="fa-solid fa-check"></i> Criar';
            }
            if (input) input.readOnly = false;
            Swal.fire({
                icon: 'error',
                title: 'Erro',
                text: (err && err.mensagem) || 'Não foi possível criar a tarefa.'
            });
        });
    }

    // Confirm button
    document.addEventListener('click', function (e) {
        var btn = e.target.closest('#tarefasViewKanban .kanban-inline-btn-confirm');
        if (!btn) return;
        var form = btn.closest('.kanban-inline-form');
        if (form) submitKanbanInlineForm(form);
    });

    // Enter key submits
    document.addEventListener('keydown', function (e) {
        if (e.key !== 'Enter') return;
        var input = e.target.closest('#tarefasViewKanban .kanban-inline-input');
        if (!input) return;
        e.preventDefault();
        var form = input.closest('.kanban-inline-form');
        if (form) submitKanbanInlineForm(form);
    });

    // ====== Kanban Subtask Interactions ======

    // Close any open subtask status menu or search popover
    function closeAllSubtaskMenus() {
        document.querySelectorAll('.kanban-subtask-status-menu.show').forEach(function (menu) {
            menu.classList.remove('show');
        });
        document.querySelectorAll('.kanban-subtask-search-popover.show').forEach(function (pop) {
            pop.classList.remove('show');
        });
    }

    // Toggle: expand/collapse subtask list
    document.addEventListener('click', function (e) {
        var header = e.target.closest('#tarefasViewKanban .kanban-subtask-header');
        if (!header) return;
        var toggle = header.querySelector('.kanban-subtask-toggle');
        var section = header.closest('.kanban-subtask-section');
        var list = section ? section.querySelector('.kanban-subtask-list') : null;
        if (!toggle || !list) return;
        var expanded = toggle.getAttribute('aria-expanded') === 'true';
        toggle.setAttribute('aria-expanded', expanded ? 'false' : 'true');
        list.classList.toggle('collapsed', expanded);
    });

    // Status dropdown: open menu on trigger click
    document.addEventListener('click', function (e) {
        var trigger = e.target.closest('#tarefasViewKanban button.kanban-subtask-status-trigger');
        if (!trigger) return;
        e.stopPropagation();

        var item = trigger.closest('.kanban-subtask-item');
        if (!item) return;

        var existing = item.querySelector('.kanban-subtask-status-menu');
        if (existing && existing.classList.contains('show')) {
            existing.classList.remove('show');
            return;
        }

        closeAllSubtaskMenus();

        var board = document.querySelector('#tarefasViewKanban .kanban-board');
        var statusList = [];
        try { statusList = JSON.parse(board.dataset.statusList); } catch (ex) { /* ignore */ }

        var currentStatusId = parseInt(item.dataset.subtaskStatusId, 10);

        if (existing) existing.remove();

        var menu = document.createElement('div');
        menu.className = 'kanban-subtask-status-menu show';

        statusList.forEach(function (s) {
            var btn = document.createElement('button');
            btn.type = 'button';
            btn.className = 'kanban-subtask-status-option' + (s.id === currentStatusId ? ' active' : '');
            btn.dataset.statusId = s.id;
            btn.innerHTML = '<span class="kanban-subtask-status-dot" style="background: ' + s.cor + ';"></span>' + s.nome;
            menu.appendChild(btn);
        });

        item.appendChild(menu);
    });

    // Status dropdown: select a status option
    document.addEventListener('click', function (e) {
        var option = e.target.closest('.kanban-subtask-status-option');
        if (!option) return;
        e.stopPropagation();

        var menu = option.closest('.kanban-subtask-status-menu');
        var item = option.closest('.kanban-subtask-item');
        if (!item) return;

        var newStatusId = parseInt(option.dataset.statusId, 10);
        var currentStatusId = parseInt(item.dataset.subtaskStatusId, 10);
        if (newStatusId === currentStatusId) {
            if (menu) menu.classList.remove('show');
            return;
        }

        var projetoIdEl = document.getElementById('ntProjetoID');
        if (!projetoIdEl) return;
        var projetoId = parseInt(projetoIdEl.value, 10);

        var payload = {
            ProjetoTarefaID: parseInt(item.dataset.subtaskId, 10),
            ProjetoID: projetoId,
            NmTarefa: item.dataset.subtaskNmTarefa,
            DsTarefa: item.dataset.subtaskDsTarefa || null,
            ProjetoTarefaStatusID: newStatusId,
            ProjetoTarefaPrioridadeID: parseInt(item.dataset.subtaskPrioridadeId, 10),
            DtInicio: brDateToIso(item.dataset.subtaskDtInicio) || null,
            DtPrevisaoFim: brDateToIso(item.dataset.subtaskDtPrevisaoFim) || null,
            DtFimReal: brDateToIso(item.dataset.subtaskDtFimReal) || null,
            UsuarioResponsavelID: item.dataset.subtaskResponsavelId ? parseInt(item.dataset.subtaskResponsavelId, 10) : null
        };

        var trigger = item.querySelector('.kanban-subtask-status-trigger');
        var originalTriggerHtml = trigger ? trigger.innerHTML : '';
        if (trigger) trigger.innerHTML = '<i class="fa-solid fa-spinner fa-spin" style="font-size: 0.55rem;"></i>';
        if (menu) menu.classList.remove('show');

        fetch('/Projetos/EditarTarefa', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(payload)
        })
        .then(function (res) {
            if (!res.ok) return res.json().then(function (err) { throw err; });
            return res.json();
        })
        .then(function () {
            showPageLoading();
            window.location.reload();
        })
        .catch(function (err) {
            if (trigger) trigger.innerHTML = originalTriggerHtml;
            Swal.fire({
                icon: 'error',
                title: 'Erro',
                text: (err && err.mensagem) || 'Não foi possível atualizar o status da subtarefa.'
            });
        });
    });

    // Helper: update subtask assignee via EditarTarefa
    function updateSubtaskResponsavel(item, responsavel, popover) {
        var projetoIdEl = document.getElementById('ntProjetoID');
        if (!projetoIdEl) return;
        var projetoId = parseInt(projetoIdEl.value, 10);

        var payload = {
            ProjetoTarefaID: parseInt(item.dataset.subtaskId, 10),
            ProjetoID: projetoId,
            NmTarefa: item.dataset.subtaskNmTarefa,
            DsTarefa: item.dataset.subtaskDsTarefa || null,
            ProjetoTarefaStatusID: parseInt(item.dataset.subtaskStatusId, 10),
            ProjetoTarefaPrioridadeID: parseInt(item.dataset.subtaskPrioridadeId, 10),
            DtInicio: brDateToIso(item.dataset.subtaskDtInicio) || null,
            DtPrevisaoFim: brDateToIso(item.dataset.subtaskDtPrevisaoFim) || null,
            DtFimReal: brDateToIso(item.dataset.subtaskDtFimReal) || null,
            UsuarioResponsavelID: responsavel ? responsavel.id : null
        };

        if (popover) popover.classList.remove('show');

        fetch('/Projetos/EditarTarefa', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(payload)
        })
        .then(function (res) {
            if (!res.ok) return res.json().then(function (err) { throw err; });
            return res.json();
        })
        .then(function () {
            showPageLoading();
            window.location.reload();
        })
        .catch(function (err) {
            Swal.fire({
                icon: 'error',
                title: 'Erro',
                text: (err && err.mensagem) || 'Não foi possível atualizar o responsável da subtarefa.'
            });
        });
    }

    // Assignee: open search popover
    document.addEventListener('click', function (e) {
        var target = e.target.closest('#tarefasViewKanban .kanban-subtask-responsavel, #tarefasViewKanban .kanban-subtask-btn-responsavel');
        if (!target) return;
        e.stopPropagation();

        var item = target.closest('.kanban-subtask-item');
        if (!item) return;

        var existing = item.querySelector('.kanban-subtask-search-popover');
        if (existing && existing.classList.contains('show')) {
            existing.classList.remove('show');
            return;
        }

        closeAllSubtaskMenus();

        if (existing) existing.remove();

        var popover = document.createElement('div');
        popover.className = 'kanban-subtask-search-popover show';

        var searchInput = document.createElement('input');
        searchInput.type = 'text';
        searchInput.className = 'kanban-subtask-search-input';
        searchInput.placeholder = 'Buscar usuário...';
        searchInput.setAttribute('autocomplete', 'off');
        popover.appendChild(searchInput);

        var resultsList = document.createElement('ul');
        resultsList.className = 'kanban-subtask-search-results';
        popover.appendChild(resultsList);

        var currentResponsavelId = item.dataset.subtaskResponsavelId;
        if (currentResponsavelId) {
            var removeBtn = document.createElement('button');
            removeBtn.type = 'button';
            removeBtn.className = 'kanban-subtask-search-btn-remove';
            removeBtn.innerHTML = '<i class="fa-solid fa-user-xmark"></i> Remover responsável';
            removeBtn.addEventListener('click', function (ev) {
                ev.stopPropagation();
                updateSubtaskResponsavel(item, null, popover);
            });
            popover.appendChild(removeBtn);
        }

        item.appendChild(popover);
        searchInput.focus();

        var searchTimer = null;
        searchInput.addEventListener('input', function () {
            var texto = searchInput.value.trim();
            if (texto.length < 2) {
                resultsList.innerHTML = '';
                return;
            }
            clearTimeout(searchTimer);
            searchTimer = setTimeout(function () {
                fetch('/Projetos/BuscarUsuarios?texto=' + encodeURIComponent(texto))
                    .then(function (res) { return res.json(); })
                    .then(function (data) {
                        resultsList.innerHTML = '';
                        if (!data || data.length === 0) {
                            var li = document.createElement('li');
                            li.className = 'kanban-subtask-search-item text-muted';
                            li.textContent = 'Nenhum usuário encontrado';
                            resultsList.appendChild(li);
                            return;
                        }
                        data.forEach(function (u) {
                            var li = document.createElement('li');
                            li.className = 'kanban-subtask-search-item';
                            li.textContent = u.nmUsuario;
                            li.addEventListener('click', function (ev) {
                                ev.stopPropagation();
                                updateSubtaskResponsavel(item, { id: u.usuarioID, nome: u.nmUsuario }, popover);
                            });
                            resultsList.appendChild(li);
                        });
                    })
                    .catch(function () {
                        resultsList.innerHTML = '';
                    });
            }, 300);
        });
    });

    // Close subtask menus when clicking outside
    document.addEventListener('click', function (e) {
        if (!e.target.closest('.kanban-subtask-status-menu') &&
            !e.target.closest('.kanban-subtask-status-trigger') &&
            !e.target.closest('.kanban-subtask-search-popover') &&
            !e.target.closest('.kanban-subtask-responsavel') &&
            !e.target.closest('.kanban-subtask-btn-responsavel')) {
            closeAllSubtaskMenus();
        }
    });

    // Escape closes subtask menus
    document.addEventListener('keydown', function (e) {
        if (e.key === 'Escape') {
            closeAllSubtaskMenus();
        }
    });

    // ====== Participantes ======
    var apBuscaInput = document.getElementById('apBuscaUsuario');
    var apResultados = document.getElementById('apResultadosBusca');
    var apUsuarioID = document.getElementById('apUsuarioID');
    var apProjetoID = document.getElementById('apProjetoID');
    var apDebounceTimer = null;

    if (apBuscaInput && apResultados) {
        apBuscaInput.addEventListener('input', function () {
            var texto = apBuscaInput.value.trim();
            apUsuarioID.value = '';
            apBuscaInput.classList.remove('is-invalid');

            if (texto.length < 2) {
                apResultados.classList.add('d-none');
                apResultados.innerHTML = '';
                return;
            }

            clearTimeout(apDebounceTimer);
            apDebounceTimer = setTimeout(function () {
                fetch('/Projetos/BuscarUsuarios?texto=' + encodeURIComponent(texto))
                    .then(function (res) { return res.json(); })
                    .then(function (data) {
                        apResultados.innerHTML = '';
                        if (!data || data.length === 0) {
                            apResultados.innerHTML = '<div class="ap-autocomplete-item text-muted">Nenhum usuário encontrado</div>';
                            apResultados.classList.remove('d-none');
                            return;
                        }
                        data.forEach(function (u) {
                            var item = document.createElement('div');
                            item.className = 'ap-autocomplete-item';
                            item.textContent = u.nmUsuario;
                            item.dataset.usuarioId = u.usuarioID;
                            item.addEventListener('click', function () {
                                apBuscaInput.value = u.nmUsuario;
                                apUsuarioID.value = u.usuarioID;
                                apResultados.classList.add('d-none');
                                apBuscaInput.classList.remove('is-invalid');
                            });
                            apResultados.appendChild(item);
                        });
                        apResultados.classList.remove('d-none');
                    })
                    .catch(function () {
                        apResultados.classList.add('d-none');
                    });
            }, 300);
        });

        document.addEventListener('click', function (e) {
            if (!apBuscaInput.contains(e.target) && !apResultados.contains(e.target)) {
                apResultados.classList.add('d-none');
            }
        });
    }

    // Salvar novo participante
    var btnSalvarParticipante = document.getElementById('btnSalvarParticipante');
    if (btnSalvarParticipante) {
        btnSalvarParticipante.addEventListener('click', function () {
            var usuarioId = parseInt(apUsuarioID.value, 10);
            if (!usuarioId) {
                apBuscaInput.classList.add('is-invalid');
                return;
            }

            var payload = {
                ProjetoID: parseInt(apProjetoID.value, 10),
                UsuarioID: usuarioId,
                NmPapel: document.getElementById('apNmPapel').value.trim()
            };

            btnSalvarParticipante.disabled = true;
            fetch('/Projetos/AdicionarParticipante', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify(payload)
            })
            .then(function (res) {
                if (!res.ok) return res.json().then(function (err) { throw err; });
                return res.json();
            })
            .then(function () {
                showPageLoading();
                window.location.reload();
            })
            .catch(function (err) {
                btnSalvarParticipante.disabled = false;
                Swal.fire({
                    icon: 'error',
                    title: 'Erro',
                    text: (err && err.mensagem) || 'Não foi possível adicionar o participante.'
                });
            });
        });
    }

    // Limpar modal ao fechar
    var modalAP = document.getElementById('modalAdicionarParticipante');
    if (modalAP) {
        modalAP.addEventListener('hidden.bs.modal', function () {
            apBuscaInput.value = '';
            apUsuarioID.value = '';
            document.getElementById('apNmPapel').value = '';
            apResultados.classList.add('d-none');
            apResultados.innerHTML = '';
            apBuscaInput.classList.remove('is-invalid');
        });
    }

    // Editar papel do participante
    document.querySelectorAll('.btn-editar-papel').forEach(function (btn) {
        btn.addEventListener('click', function () {
            var participanteId = parseInt(btn.dataset.participanteId, 10);
            var papelAtual = btn.dataset.nmPapel || '';
            var projetoId = apProjetoID ? parseInt(apProjetoID.value, 10) : parseInt(document.getElementById('ntProjetoID').value, 10);

            Swal.fire({
                title: 'Editar Papel',
                input: 'text',
                inputLabel: 'Papel no projeto',
                inputValue: papelAtual,
                inputAttributes: { maxlength: 100 },
                showCancelButton: true,
                confirmButtonText: 'Salvar',
                cancelButtonText: 'Cancelar',
                inputValidator: function (value) {
                    if (!value || !value.trim()) return 'Informe o papel do participante.';
                }
            }).then(function (result) {
                if (!result.isConfirmed) return;

                var payload = {
                    ProjetoParticipanteID: participanteId,
                    ProjetoID: projetoId,
                    NmPapel: result.value.trim()
                };

                fetch('/Projetos/AtualizarPapelParticipante', {
                    method: 'POST',
                    headers: { 'Content-Type': 'application/json' },
                    body: JSON.stringify(payload)
                })
                .then(function (res) {
                    if (!res.ok) return res.json().then(function (err) { throw err; });
                    return res.json();
                })
                .then(function () {
                    showPageLoading();
                    window.location.reload();
                })
                .catch(function (err) {
                    Swal.fire({
                        icon: 'error',
                        title: 'Erro',
                        text: (err && err.mensagem) || 'Não foi possível atualizar o papel.'
                    });
                });
            });
        });
    });

    // Remover participante
    document.querySelectorAll('.btn-remover-participante').forEach(function (btn) {
        btn.addEventListener('click', function () {
            var participanteId = parseInt(btn.dataset.participanteId, 10);
            var nmUsuario = btn.dataset.nmUsuario;
            var projetoId = apProjetoID ? parseInt(apProjetoID.value, 10) : parseInt(document.getElementById('ntProjetoID').value, 10);

            Swal.fire({
                title: 'Remover participante?',
                html: 'Deseja remover <strong>' + nmUsuario + '</strong> do projeto?',
                icon: 'warning',
                showCancelButton: true,
                confirmButtonColor: '#dc3545',
                confirmButtonText: 'Sim, remover',
                cancelButtonText: 'Cancelar'
            }).then(function (result) {
                if (!result.isConfirmed) return;

                fetch('/Projetos/RemoverParticipante', {
                    method: 'POST',
                    headers: { 'Content-Type': 'application/json' },
                    body: JSON.stringify({
                        ProjetoParticipanteID: participanteId,
                        ProjetoID: projetoId
                    })
                })
                .then(function (res) {
                    if (!res.ok) return res.json().then(function (err) { throw err; });
                    return res.json();
                })
                .then(function () {
                    showPageLoading();
                    window.location.reload();
                })
                .catch(function (err) {
                    Swal.fire({
                        icon: 'error',
                        title: 'Erro',
                        text: (err && err.mensagem) || 'Não foi possível remover o participante.'
                    });
                });
            });
        });
    });

                })();
