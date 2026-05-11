(function () {
    'use strict';

    // ====== Helpers ======
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

    function hidePageLoading() {
        var overlay = document.getElementById('projetoLoadingOverlay');
        if (overlay) overlay.classList.remove('active');
    }

    window.addEventListener('pageshow', hidePageLoading);

    function brDateToIso(brDate) {
        if (!brDate) return '';
        var parts = brDate.split('/');
        if (parts.length !== 3) return '';
        return parts[2] + '-' + parts[1] + '-' + parts[0];
    }

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

    function getProjetoId() {
        var el = document.getElementById('detalhesConteudo');
        return el ? parseInt(el.dataset.projetoId, 10) : 0;
    }

    function getConcluidoStatusId() {
        var el = document.getElementById('detalhesConteudo');
        return el ? el.dataset.concluidoStatusId || '' : '';
    }

    function temSubtarefasAbertas(tarefaId) {
        var concluidoId = getConcluidoStatusId();
        if (!concluidoId) return false;
        var container = document.getElementById('subtarefas-' + tarefaId);
        if (!container) return false;
        var subRows = container.querySelectorAll('.tarefa-row-sub');
        for (var i = 0; i < subRows.length; i++) {
            if (subRows[i].dataset.statusId !== concluidoId) return true;
        }
        return false;
    }

    function confirmarConcluirComSubtarefasAbertas(callback) {
        Swal.fire({
            icon: 'warning',
            title: 'Subtarefas em aberto',
            html: 'Esta tarefa possui subtarefas que ainda não foram concluídas.<br><small class="text-muted">Deseja concluir mesmo assim?</small>',
            showCancelButton: true,
            confirmButtonColor: '#198754',
            confirmButtonText: '<i class="fa-solid fa-check me-1"></i>Sim, concluir',
            cancelButtonText: 'Cancelar',
            focusCancel: true
        }).then(function (result) {
            if (result.isConfirmed) callback();
        });
    }

    function contarSubtarefas(tarefaId) {
        var container = document.getElementById('subtarefas-' + tarefaId);
        if (!container) return { total: 0, abertas: 0, nomes: [] };
        var subRows = container.querySelectorAll('.tarefa-row-sub');
        var concluidoId = getConcluidoStatusId();
        var abertas = 0;
        var nomes = [];
        subRows.forEach(function (r) {
            nomes.push(r.dataset.nmTarefa || '');
            if (concluidoId && r.dataset.statusId !== concluidoId) abertas++;
        });
        return { total: subRows.length, abertas: abertas, nomes: nomes };
    }

    function confirmarExclusaoTarefa(tarefaId, nmTarefa, onConfirm) {
        var info = contarSubtarefas(tarefaId);
        var totalSub = info.total;
        var abertasSub = info.abertas;
        var nomesSub = info.nomes;

        var nomeSeguro = escapeHtmlSafe(nmTarefa || 'esta tarefa');

        var html = 'Tem certeza que deseja excluir <strong>' + nomeSeguro + '</strong>?';

        if (totalSub > 0) {
            html += '<div class="alert alert-warning mt-3 mb-2 text-start" style="font-size: 0.85rem;">';
            html += '<div class="d-flex align-items-start gap-2">';
            html += '<i class="fa-solid fa-triangle-exclamation mt-1"></i>';
            html += '<div class="flex-grow-1">';
            html += '<strong>' + totalSub + ' subtarefa' + (totalSub === 1 ? '' : 's') + '</strong> ';
            html += 'ser' + (totalSub === 1 ? 'á' : 'ão') + ' excluída' + (totalSub === 1 ? '' : 's') + ' junto.';
            if (abertasSub > 0) {
                html += '<br><span class="text-danger"><i class="fa-solid fa-circle-exclamation me-1"></i>' + abertasSub + ' ainda ' + (abertasSub === 1 ? 'está aberta' : 'estão abertas') + '.</span>';
            }
            // Show first 5 subtask names
            var visiveis = nomesSub.slice(0, 5);
            html += '<ul class="mb-0 mt-1 ps-3" style="font-size: 0.8rem;">';
            visiveis.forEach(function (n) {
                html += '<li>' + escapeHtmlSafe(n) + '</li>';
            });
            if (nomesSub.length > 5) {
                html += '<li class="text-muted">... e mais ' + (nomesSub.length - 5) + '</li>';
            }
            html += '</ul>';
            html += '</div></div></div>';
        }

        html += '<small class="text-muted d-block mt-2"><i class="fa-solid fa-info-circle me-1"></i>Esta ação não poderá ser desfeita.</small>';

        // If many subtasks (>=3), require typing the name for extra safety
        var requireTypeConfirm = totalSub >= 3;

        var swalOpts = {
            icon: 'warning',
            title: 'Excluir tarefa?',
            html: html,
            showCancelButton: true,
            confirmButtonColor: '#dc3545',
            confirmButtonText: '<i class="fa-solid fa-trash-can me-1"></i>Excluir',
            cancelButtonText: 'Cancelar',
            focusCancel: true,
            reverseButtons: true,
            width: 520
        };

        if (requireTypeConfirm) {
            swalOpts.input = 'text';
            swalOpts.inputPlaceholder = 'Digite o nome da tarefa para confirmar';
            swalOpts.inputAttributes = { autocapitalize: 'off', autocomplete: 'off' };
            swalOpts.inputValidator = function (value) {
                if (!value || value.trim() !== (nmTarefa || '').trim()) {
                    return 'O nome não confere. Digite exatamente: ' + (nmTarefa || '');
                }
            };
        }

        Swal.fire(swalOpts).then(function (result) {
            if (!result.isConfirmed) return;
            onConfirm();
        });
    }

    function escapeHtmlSafe(str) {
        if (str == null) return '';
        return String(str)
            .replace(/&/g, '&amp;')
            .replace(/</g, '&lt;')
            .replace(/>/g, '&gt;')
            .replace(/"/g, '&quot;')
            .replace(/'/g, '&#39;');
    }

    function executarExclusaoTarefa(tarefaId, nmTarefa, onSuccess) {
        Swal.fire({
            title: 'Excluindo...',
            html: 'Removendo <strong>' + escapeHtmlSafe(nmTarefa) + '</strong>...',
            allowOutsideClick: false,
            allowEscapeKey: false,
            didOpen: function () { Swal.showLoading(); }
        });

        fetch(window.sicUrl('/Projetos/ExcluirTarefa'), {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ ProjetoTarefaID: tarefaId, ProjetoID: getProjetoId() })
        })
        .then(function (res) { if (!res.ok) return res.json().then(function (err) { throw err; }); return res.json(); })
        .then(function () {
            Swal.close();
            showToast('success', 'Tarefa excluída!').then(function () {
                if (typeof onSuccess === 'function') onSuccess();
            });
        })
        .catch(function (err) {
            Swal.fire({
                icon: 'error',
                title: 'Erro ao excluir',
                html: (err && err.mensagem) || 'Não foi possível excluir a tarefa.',
                confirmButtonText: 'Entendi'
            });
        });
    }

    // ====== AJAX Content Reload ======
    function reloadContent(activeTabId) {
        var container = document.getElementById('detalhesConteudo');
        if (!container) return;

        var projetoId = getProjetoId();
        if (!projetoId) return;

        // Preserve state
        if (!activeTabId) {
            var activeTab = document.querySelector('#projetoTabs .nav-link.active');
            activeTabId = activeTab ? activeTab.id : 'tab-tarefas';
        }
        var kanbanActive = false;
        var btnK = document.getElementById('btnViewKanban');
        if (btnK && btnK.classList.contains('active')) kanbanActive = true;

        fetch(window.sicUrl('/Projetos/' + projetoId + '/Partial'), {
            headers: { 'X-Requested-With': 'XMLHttpRequest' }
        })
        .then(function (res) {
            if (!res.ok) throw new Error('Erro ao recarregar conteúdo');
            return res.text();
        })
        .then(function (html) {
            container.innerHTML = html;

            // Restore active tab
            if (activeTabId && activeTabId !== 'tab-tarefas') {
                var tabBtn = document.getElementById(activeTabId);
                if (tabBtn) {
                    var tab = new bootstrap.Tab(tabBtn);
                    tab.show();
                }
            }

            // Restore Kanban view
            if (kanbanActive) {
                var bLista = document.getElementById('btnViewLista');
                var bKanban = document.getElementById('btnViewKanban');
                var vLista = document.getElementById('tarefasViewLista');
                var vKanban = document.getElementById('tarefasViewKanban');
                if (bLista && bKanban && vLista && vKanban) {
                    bKanban.classList.add('active');
                    bLista.classList.remove('active');
                    vKanban.classList.remove('d-none');
                    vLista.classList.add('d-none');
                }
            }

            // Sync "Tarefa Pai" dropdown in modal Nova Tarefa
            syncTarefaPaiDropdown();

            // Re-init D&D (dragstart/dragend need per-element binding)
            initDragAndDrop();
            initListaDragAndDrop();

            // Restore group collapse state
            restoreGroupCollapseState();

            // Restore column visibility
            applyColumnVisibility();

            // Restore lista ordering (client-side only)
            restoreListaOrder();

            hidePageLoading();
        })
        .catch(function () {
            // Fallback: full page reload
            window.location.reload();
        });
    }

    // ====== Partial DOM updates (avoid full reload) ======
    // Sync a task row's visible state after an update, without calling reloadContent.
    // Updates: data-attributes, status badge, prioridade badge, responsável, prazo, progresso bar.
    // Also moves row to correct group when status changes, updates group counters,
    // and updates kanban card/column when applicable.
    function atualizarTarefaDom(tarefaId, novosDados) {
        if (!tarefaId) return;
        var idStr = String(tarefaId);

        // Update ALL occurrences of this task (row pai, row sub, kanban card)
        var row = document.querySelector('.tarefa-row[data-tarefa-id="' + idStr + '"]');
        var kanbanCard = document.querySelector('.kanban-card[data-tarefa-id="' + idStr + '"]');
        var editBtn = document.querySelector('.btn-editar-tarefa[data-tarefa-id="' + idStr + '"]');

        [row, kanbanCard, editBtn].forEach(function (el) {
            if (!el) return;
            if (novosDados.NmTarefa !== undefined) el.dataset.nmTarefa = novosDados.NmTarefa || '';
            if (novosDados.DsTarefa !== undefined) el.dataset.dsTarefa = novosDados.DsTarefa || '';
            if (novosDados.ProjetoTarefaStatusID !== undefined) el.dataset.statusId = String(novosDados.ProjetoTarefaStatusID);
            if (novosDados.ProjetoTarefaPrioridadeID !== undefined) el.dataset.prioridadeId = String(novosDados.ProjetoTarefaPrioridadeID);
            if (novosDados.UsuarioResponsavelID !== undefined) el.dataset.responsavelId = novosDados.UsuarioResponsavelID == null ? '' : String(novosDados.UsuarioResponsavelID);
            if (novosDados.DtInicioBr !== undefined) el.dataset.dtInicio = novosDados.DtInicioBr || '';
            if (novosDados.DtPrevisaoFimBr !== undefined) el.dataset.dtPrevisaoFim = novosDados.DtPrevisaoFimBr || '';
            if (novosDados.DtFimRealBr !== undefined) el.dataset.dtFimReal = novosDados.DtFimRealBr || '';
        });

        // Update the status badge text and color (row)
        if (row && novosDados.NmStatus !== undefined) {
            var statusBadge = row.querySelector('.tarefa-col-status .tarefa-badge-status');
            if (statusBadge) {
                var iconHtml = '<i class="fa-solid fa-circle" style="font-size: 0.45rem;"></i> ';
                var caretHtml = statusBadge.classList.contains('tarefa-status-trigger') ? ' <i class="fa-solid fa-caret-down ms-1" style="font-size: 0.5rem; opacity: 0.7;"></i>' : '';
                statusBadge.innerHTML = iconHtml + escapeHtmlSafe(novosDados.NmStatus) + caretHtml;
                if (novosDados.CdCorStatus) {
                    statusBadge.style.background = 'color-mix(in srgb, ' + novosDados.CdCorStatus + ' 18%, transparent)';
                    statusBadge.style.color = novosDados.CdCorStatus;
                }
            }
        }

        // Update prioridade badge
        if (row && novosDados.NmPrioridade !== undefined) {
            var prioBadge = row.querySelector('.tarefa-col-prioridade .tarefa-badge-prioridade');
            if (prioBadge) {
                var caretHtmlP = prioBadge.classList.contains('tarefa-prioridade-trigger') ? ' <i class="fa-solid fa-caret-down ms-1" style="font-size: 0.5rem; opacity: 0.7;"></i>' : '';
                prioBadge.innerHTML = '<i class="fa-solid fa-flag" style="font-size: 0.55rem;"></i> ' + escapeHtmlSafe(novosDados.NmPrioridade) + caretHtmlP;
                if (novosDados.CdCorPrioridade) {
                    prioBadge.style.background = 'color-mix(in srgb, ' + novosDados.CdCorPrioridade + ' 18%, transparent)';
                    prioBadge.style.color = novosDados.CdCorPrioridade;
                }
            }
        }

        // Update responsável
        if (row && novosDados.NmResponsavel !== undefined) {
            var respTrigger = row.querySelector('.tarefa-col-responsavel .tarefa-responsavel-trigger');
            if (respTrigger) {
                var respHtml;
                if (novosDados.NmResponsavel && novosDados.NmResponsavel.trim().length > 0) {
                    respHtml = '<span class="text-muted"><i class="fa-solid fa-user me-1"></i>' + escapeHtmlSafe(novosDados.NmResponsavel) + '</span>';
                } else {
                    respHtml = '<span class="text-muted" style="opacity: 0.5;"><i class="fa-solid fa-user-plus me-1"></i>Atribuir</span>';
                }
                respHtml += '<i class="fa-solid fa-caret-down ms-1 text-muted" style="font-size: 0.5rem; opacity: 0.7;"></i>';
                respTrigger.innerHTML = respHtml;
            }
        }

        // Update prazo
        if (row && novosDados.DtPrevisaoFimIso !== undefined) {
            var dateInput = row.querySelector('.tarefa-col-prazo .tarefa-inline-date-input');
            if (dateInput) {
                dateInput.value = novosDados.DtPrevisaoFimIso || '';
                dateInput.disabled = false;
            }
        }

        // Update task name text
        if (row && novosDados.NmTarefa !== undefined) {
            var nomeTextEl = row.querySelector('.tarefa-col-nome .tarefa-nome-text');
            if (nomeTextEl) nomeTextEl.textContent = novosDados.NmTarefa;
        }

        // Rebuild status dropdown check-marks if status changed
        if (row && novosDados.ProjetoTarefaStatusID !== undefined) {
            var statusItems = row.querySelectorAll('.tarefa-col-status .dropdown-menu li');
            statusItems.forEach(function (li) {
                var btn = li.querySelector('.btn-alterar-status-rapido');
                var span = li.querySelector('.dropdown-item.disabled');
                // Reset: any item becomes clickable when not the current one
                if (btn && parseInt(btn.dataset.statusId, 10) === novosDados.ProjetoTarefaStatusID) {
                    // Swap to disabled span
                    var newSpan = document.createElement('span');
                    newSpan.className = 'dropdown-item d-flex align-items-center gap-2 disabled';
                    newSpan.innerHTML = btn.innerHTML + ' <i class="fa-solid fa-check ms-auto text-success" style="font-size: 0.7rem;"></i>';
                    btn.replaceWith(newSpan);
                } else if (span && !span.querySelector('.btn-alterar-status-rapido')) {
                    // Need to convert back to button - skip complex re-render; rely on next reload
                }
            });
        }

        // Move row to the correct group if status changed
        if (row && novosDados.ProjetoTarefaStatusID !== undefined) {
            var currentGroup = row.closest('.tarefa-group');
            var targetGroup = document.querySelector('.tarefa-group[data-status-id="' + novosDados.ProjetoTarefaStatusID + '"]');
            if (targetGroup && currentGroup !== targetGroup) {
                var targetTable = targetGroup.querySelector('.tarefa-table');
                var targetBody = targetGroup.querySelector('.tarefa-group-body');
                var subContainer = document.getElementById('subtarefas-' + idStr);

                if (!targetTable) {
                    // Empty group: create table from another group's header
                    var emptyEl = targetBody ? targetBody.querySelector('.tarefa-group-empty') : null;
                    if (emptyEl) emptyEl.remove();
                    var sampleHeader = document.querySelector('.tarefa-table-header');
                    targetTable = document.createElement('div');
                    targetTable.className = 'tarefa-table';
                    if (sampleHeader) targetTable.appendChild(sampleHeader.cloneNode(true));
                    if (targetBody) targetBody.appendChild(targetTable);
                }

                targetTable.appendChild(row);
                if (subContainer) targetTable.appendChild(subContainer);

                // If source group became empty, show empty placeholder
                var sourceTable = currentGroup ? currentGroup.querySelector('.tarefa-table') : null;
                if (sourceTable) {
                    var remainingRows = sourceTable.querySelectorAll('.tarefa-row:not(.tarefa-row-sub)').length;
                    if (remainingRows === 0) {
                        var sourceBody = currentGroup.querySelector('.tarefa-group-body');
                        sourceTable.remove();
                        if (sourceBody && !sourceBody.querySelector('.tarefa-group-empty')) {
                            var emptyDiv = document.createElement('div');
                            emptyDiv.className = 'tarefa-group-empty';
                            emptyDiv.innerHTML = '<small class="text-muted">Nenhuma tarefa</small>';
                            sourceBody.appendChild(emptyDiv);
                        }
                    }
                }

                atualizarContadoresGrupos();
            }
        }

        // Update kanban card (if exists and visible)
        if (kanbanCard) {
            if (novosDados.NmTarefa !== undefined) {
                var cardTitle = kanbanCard.querySelector('.kanban-card-title');
                if (cardTitle) cardTitle.textContent = novosDados.NmTarefa;
            }
            // Status change in kanban requires moving the card between columns (skip: safer to reload kanban when needed)
        }

        // Update edit button data attributes (already done above)
    }

    function atualizarContadoresGrupos() {
        document.querySelectorAll('.tarefa-group').forEach(function (group) {
            var countEl = group.querySelector('.tarefa-group-count');
            var table = group.querySelector('.tarefa-table');
            var rows = table ? table.querySelectorAll('.tarefa-row:not(.tarefa-row-sub)') : [];
            if (countEl) countEl.textContent = rows.length;
        });
    }

    // Lookup helper: find status info from server-side data serialized in the page
    function encontrarStatusInfo(statusId) {
        var id = parseInt(statusId, 10);
        // Try to extract from any existing dropdown item (color + name)
        var el = document.querySelector('.tarefa-col-status .dropdown-menu .btn-alterar-status-rapido[data-status-id="' + id + '"], .tarefa-col-status .dropdown-menu li .dropdown-item.disabled');
        // Fallback: look across all rows' dropdown items
        var items = document.querySelectorAll('.btn-alterar-status-rapido[data-status-id="' + id + '"]');
        for (var i = 0; i < items.length; i++) {
            var item = items[i];
            var icon = item.querySelector('i.fa-circle');
            var span = item.querySelector('span');
            if (icon && span) {
                return { NmStatus: span.textContent.trim(), CdCor: icon.style.color || '' };
            }
        }
        // Try currently selected (disabled) item
        var anyGroup = document.querySelector('.tarefa-group[data-status-id="' + id + '"]');
        if (anyGroup) {
            var colorDot = anyGroup.querySelector('.tarefa-group-color-dot');
            var title = anyGroup.querySelector('.tarefa-group-title');
            return {
                NmStatus: title ? title.textContent.trim() : '',
                CdCor: colorDot ? (colorDot.style.background || '') : ''
            };
        }
        return { NmStatus: '', CdCor: '' };
    }

    function encontrarPrioridadeInfo(prioId) {
        var id = parseInt(prioId, 10);
        var items = document.querySelectorAll('.btn-alterar-prioridade-rapido[data-prioridade-id="' + id + '"]');
        for (var i = 0; i < items.length; i++) {
            var item = items[i];
            var icon = item.querySelector('i.fa-flag');
            var span = item.querySelector('span');
            if (icon && span) {
                return { NmPrioridade: span.textContent.trim(), CdCor: icon.style.color || '' };
            }
        }
        return { NmPrioridade: '', CdCor: '' };
    }

    function encontrarResponsavelNome(usuarioId) {
        if (!usuarioId) return '';
        var items = document.querySelectorAll('.btn-alterar-responsavel-rapido[data-responsavel-id="' + usuarioId + '"]');
        for (var i = 0; i < items.length; i++) {
            var span = items[i].querySelector('span');
            if (span) return span.textContent.trim();
        }
        // Fallback: look in participantesDataJson
        try {
            var jsonEl = document.getElementById('participantesDataJson');
            if (jsonEl) {
                var list = JSON.parse(jsonEl.textContent) || [];
                for (var j = 0; j < list.length; j++) {
                    if (String(list[j].id) === String(usuarioId)) return list[j].nome || '';
                }
            }
        } catch (ex) { /* ignore */ }
        return '';
    }

    function syncTarefaPaiDropdown() {
        var select = document.getElementById('ntProjetoTarefaPaiID');
        var jsonEl = document.getElementById('tarefasDataJson');
        if (!select || !jsonEl) return;
        try {
            var tarefas = JSON.parse(jsonEl.textContent);
            var currentVal = select.value;
            select.innerHTML = '<option value="">Nenhuma (tarefa raiz)</option>';
            tarefas.forEach(function (t) {
                var opt = document.createElement('option');
                opt.value = t.id;
                opt.textContent = t.nome;
                select.appendChild(opt);
            });
            if (currentVal) select.value = currentVal;
        } catch (ex) { /* ignore */ }
    }

    function showToast(icon, title, text) {
        return Swal.fire({
            icon: icon,
            title: title,
            text: text || undefined,
            timer: 1200,
            showConfirmButton: false
        });
    }

    // ====== Tabs: restore active tab from hash ======
    var tabHash = window.location.hash;
    if (tabHash) {
        var tabBtn = document.querySelector('#projetoTabs button[data-bs-target="' + tabHash.replace('#', '#pane-') + '"], #projetoTabs button[data-bs-target="' + tabHash + '"]');
        if (tabBtn) {
            var tab = new bootstrap.Tab(tabBtn);
            tab.show();
        }
    }

    // ====== Drawer: open & populate ======
    function abrirDrawerTarefa(data) {
        document.getElementById('etProjetoTarefaID').value = data.tarefaId;
        document.getElementById('etNmTarefa').value = data.nmTarefa || '';
        document.getElementById('etDsTarefa').value = data.dsTarefa || '';
        document.getElementById('etProjetoTarefaStatusID').value = data.statusId || '';
        document.getElementById('etProjetoTarefaPrioridadeID').value = data.prioridadeId || '';
        document.getElementById('etDtInicio').value = brDateToIso(data.dtInicio);
        document.getElementById('etDtPrevisaoFim').value = brDateToIso(data.dtPrevisaoFim);
        document.getElementById('etDtFimReal').value = brDateToIso(data.dtFimReal);

        var selResp = document.getElementById('etUsuarioResponsavelID');
        if (selResp) selResp.value = data.responsavelId || '';

        // Populate subtasks section
        var subSection = document.getElementById('drawerSubtarefasSection');
        var subList = document.getElementById('drawerSubtarefasList');
        if (subSection && subList) {
            var subtarefas = [];
            // Try from lista DOM (subtarefas container)
            var subContainer = document.getElementById('subtarefas-' + data.tarefaId);
            if (subContainer) {
                var subRows = subContainer.querySelectorAll('.tarefa-row-sub');
                subRows.forEach(function (row) {
                    var statusBadge = row.querySelector('.tarefa-badge-status');
                    subtarefas.push({
                        id: row.dataset.tarefaId,
                        nome: row.dataset.nmTarefa,
                        statusId: row.dataset.statusId,
                        nmStatus: statusBadge ? statusBadge.textContent.trim() : '',
                        corStatus: statusBadge ? statusBadge.style.color : ''
                    });
                });
            }
            // Try from kanban card data attribute
            if (subtarefas.length === 0 && data.subtarefas) {
                try { subtarefas = JSON.parse(data.subtarefas); } catch (ex) { /* ignore */ }
            }

            if (subtarefas.length > 0) {
                var conclId = getConcluidoStatusId();
                var html = '';
                subtarefas.forEach(function (s) {
                    var isDone = conclId && String(s.statusId) === conclId;
                    html += '<div class="drawer-subtarefa-item' + (isDone ? ' done' : '') + '">';
                    html += '<i class="fa-solid ' + (isDone ? 'fa-circle-check text-success' : 'fa-circle text-muted') + '" style="font-size: 0.6rem;"></i> ';
                    html += '<span class="drawer-subtarefa-nome">' + (s.nome || '') + '</span>';
                    html += '<span class="drawer-subtarefa-status" style="color: ' + (s.corStatus || '') + ';">' + (s.nmStatus || '') + '</span>';
                    html += '</div>';
                });
                subList.innerHTML = html;
                subSection.classList.remove('d-none');
            } else {
                subList.innerHTML = '';
                subSection.classList.add('d-none');
            }
        }

        var drawer = document.getElementById('drawerEditarTarefa');
        drawer.dataset.nmTarefa = data.nmTarefa || '';

        // Populate histórico da tarefa
        popularHistoricoTarefa(data.nmTarefa || '', data.tarefaId);

        var offcanvas = bootstrap.Offcanvas.getOrCreateInstance(drawer);
        offcanvas.show();
    }

    function popularHistoricoTarefa(nmTarefa, tarefaId) {
        var list = document.getElementById('drawerHistoricoList');
        if (!list) return;

        var historicoEl = document.getElementById('historicoDataJson');
        if (!historicoEl) {
            list.innerHTML = '<p class="text-muted small mb-0">Nenhum registro de histórico.</p>';
            return;
        }

        var historico = [];
        try { historico = JSON.parse(historicoEl.textContent) || []; } catch (ex) { historico = []; }

        if (historico.length === 0) {
            list.innerHTML = '<p class="text-muted small mb-0">Nenhum registro de histórico.</p>';
            return;
        }

        // Filter by tarefa name (case-insensitive substring match in DsAcao)
        var nome = (nmTarefa || '').trim().toLowerCase();
        var filtered = [];
        if (nome.length > 0) {
            filtered = historico.filter(function (h) {
                return (h.acao || '').toLowerCase().indexOf(nome) !== -1;
            });
        }

        // Sort desc by id (most recent first)
        filtered.sort(function (a, b) { return (b.id || 0) - (a.id || 0); });

        if (filtered.length === 0) {
            list.innerHTML = '<p class="text-muted small mb-0"><i class="fa-solid fa-circle-info me-1"></i>Nenhuma alteração registrada para esta tarefa.</p>';
            return;
        }

        var html = '<div class="drawer-historico-timeline">';
        filtered.forEach(function (h) {
            var data = h.data ? formatarDataHistorico(h.data) : '';
            html += '<div class="drawer-historico-item">';
            html += '  <div class="drawer-historico-acao">';
            html += '    <span class="drawer-historico-usuario">' + escapeHtml(h.usuario || '') + '</span> ';
            html += '    <span class="drawer-historico-texto">' + escapeHtml(h.acao || '') + '</span>';
            html += '  </div>';
            html += '  <div class="drawer-historico-data"><i class="fa-solid fa-clock me-1"></i>' + data + '</div>';
            html += '</div>';
        });
        html += '</div>';

        list.innerHTML = html;
    }

    function formatarDataHistorico(isoDate) {
        if (!isoDate) return '';
        try {
            var d = new Date(isoDate);
            if (isNaN(d.getTime())) return isoDate;
            var dd = String(d.getDate()).padStart(2, '0');
            var mm = String(d.getMonth() + 1).padStart(2, '0');
            var yyyy = d.getFullYear();
            var hh = String(d.getHours()).padStart(2, '0');
            var mi = String(d.getMinutes()).padStart(2, '0');
            return dd + '/' + mm + '/' + yyyy + ' ' + hh + ':' + mi;
        } catch (ex) { return isoDate; }
    }

    function escapeHtml(str) {
        if (str == null) return '';
        return String(str)
            .replace(/&/g, '&amp;')
            .replace(/</g, '&lt;')
            .replace(/>/g, '&gt;')
            .replace(/"/g, '&quot;')
            .replace(/'/g, '&#39;');
    }

    // ====== Group by Status: collapse/expand ======
    function getGroupCollapseKey() {
        return 'sic_projeto_' + getProjetoId() + '_groupCollapsed';
    }

    function getCollapsedGroups() {
        try {
            var raw = sessionStorage.getItem(getGroupCollapseKey());
            return raw ? JSON.parse(raw) : {};
        } catch (ex) { return {}; }
    }

    function saveCollapsedGroups(map) {
        try { sessionStorage.setItem(getGroupCollapseKey(), JSON.stringify(map)); } catch (ex) { /* ignore */ }
    }

    function restoreGroupCollapseState() {
        var map = getCollapsedGroups();
        document.querySelectorAll('.tarefa-group').forEach(function (group) {
            var header = group.querySelector('.tarefa-group-header');
            if (!header) return;
            var statusId = header.dataset.statusId;
            if (map[statusId]) {
                group.classList.add('collapsed');
                header.setAttribute('aria-expanded', 'false');
            }
        });
    }

    restoreGroupCollapseState();

    // ====== Column Toggle (show/hide columns) ======
    var COL_TOGGLE_KEY = 'sic_tarefa_col_visibility';

    function getColumnVisibility() {
        try {
            var raw = localStorage.getItem(COL_TOGGLE_KEY);
            return raw ? JSON.parse(raw) : {};
        } catch (ex) { return {}; }
    }

    function saveColumnVisibility(map) {
        try { localStorage.setItem(COL_TOGGLE_KEY, JSON.stringify(map)); } catch (ex) { /* ignore */ }
    }

    function applyColumnVisibility() {
        var map = getColumnVisibility();
        var container = document.getElementById('tarefasViewLista');
        if (!container) return;

        var cols = ['status', 'prioridade', 'responsavel', 'prazo', 'progresso'];
        cols.forEach(function (col) {
            var hidden = map[col] === false;
            container.classList.toggle('tarefa-hide-' + col, hidden);
            // Sync checkbox state
            var cb = document.querySelector('.tarefa-col-toggle[data-col="' + col + '"]');
            if (cb) cb.checked = !hidden;
        });
    }

    applyColumnVisibility();

    document.addEventListener('change', function (e) {
        var toggle = e.target.closest('.tarefa-col-toggle');
        if (toggle) {
            var col = toggle.dataset.col;
            var map = getColumnVisibility();
            map[col] = toggle.checked;
            saveColumnVisibility(map);
            applyColumnVisibility();
            return;
        }

        // Inline date (prazo) change
        var dateInput = e.target.closest('.tarefa-inline-date-input');
        if (dateInput) {
            var dtTarefaId = parseInt(dateInput.dataset.tarefaId, 10);
            var newDateIso = dateInput.value || null;

            var dtSrc = document.querySelector('.tarefa-row[data-tarefa-id="' + dtTarefaId + '"]');
            if (!dtSrc) return;

            var dtPayload = {
                ProjetoTarefaID: dtTarefaId,
                ProjetoID: getProjetoId(),
                NmTarefa: dtSrc.dataset.nmTarefa,
                DsTarefa: dtSrc.dataset.dsTarefa || null,
                ProjetoTarefaStatusID: parseInt(dtSrc.dataset.statusId, 10),
                ProjetoTarefaPrioridadeID: parseInt(dtSrc.dataset.prioridadeId, 10),
                DtInicio: brDateToIso(dtSrc.dataset.dtInicio) || null,
                DtPrevisaoFim: newDateIso,
                DtFimReal: brDateToIso(dtSrc.dataset.dtFimReal) || null
            };

            dateInput.disabled = true;
            fetch(window.sicUrl('/Projetos/EditarTarefa'), {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify(dtPayload)
            })
            .then(function (res) { if (!res.ok) return res.json().then(function (err) { throw err; }); return res.json(); })
            .then(function () {
                // Partial DOM update: just refresh data-attributes and re-enable input
                var newDateBr = '';
                if (newDateIso) {
                    var pts = newDateIso.split('-');
                    if (pts.length === 3) newDateBr = pts[2] + '/' + pts[1] + '/' + pts[0];
                }
                atualizarTarefaDom(dtTarefaId, {
                    DtPrevisaoFimBr: newDateBr,
                    DtPrevisaoFimIso: newDateIso || ''
                });
                showToast('success', 'Prazo atualizado');
            })
            .catch(function (err) {
                dateInput.disabled = false;
                Swal.fire({ icon: 'error', title: 'Erro', text: (err && err.mensagem) || 'Não foi possível atualizar o prazo.' });
            });
            return;
        }
    });

    // ====== Event Delegation for dynamic content ======
    document.addEventListener('click', function (e) {
        // Toggle group collapse
        var groupHeader = e.target.closest('.tarefa-group-header');
        if (groupHeader && !e.target.closest('button, a, select, input')) {
            var group = groupHeader.closest('.tarefa-group');
            if (group) {
                var isCollapsed = group.classList.toggle('collapsed');
                groupHeader.setAttribute('aria-expanded', isCollapsed ? 'false' : 'true');
                var map = getCollapsedGroups();
                var sid = groupHeader.dataset.statusId;
                if (isCollapsed) { map[sid] = true; } else { delete map[sid]; }
                saveCollapsedGroups(map);
            }
            return;
        }

        // Toggle subtarefas
        var toggleBtn = e.target.closest('.tarefa-toggle-subtarefas');
        if (toggleBtn) {
            e.stopPropagation();
            var tarefaId = toggleBtn.dataset.tarefaId;
            var container = document.getElementById('subtarefas-' + tarefaId);
            if (container) {
                var isNowHidden = container.classList.toggle('d-none');
                var icon = toggleBtn.querySelector('i');
                if (icon) icon.classList.toggle('rotated', !isNowHidden);
            }
            return;
        }

        // Edit tarefa button → open drawer
        var editBtn = e.target.closest('.btn-editar-tarefa');
        if (editBtn) {
            e.stopPropagation();
            abrirDrawerTarefa(editBtn.dataset);
            return;
        }

        // Tarefa nome text click → open drawer (but not when it's in edit mode)
        var nomeText = e.target.closest('.tarefa-nome-text');
        if (nomeText && !nomeText.classList.contains('d-none')) {
            var row = nomeText.closest('.tarefa-row');
            if (row) {
                abrirDrawerTarefa(row.dataset);
            }
            return;
        }

        // Tarefa descrição text click → open drawer
        var descText = e.target.closest('.tarefa-descricao-text');
        if (descText) {
            var row = descText.closest('.tarefa-row');
            if (row) {
                abrirDrawerTarefa(row.dataset);
            }
            return;
        }

        // Tarefa row click → open drawer (on empty areas)
        var row = e.target.closest('.tarefa-row');
        if (row && !e.target.closest('button, a, select, input, .dropdown, .dropdown-menu, .tarefa-toggle-subtarefas, .tarefa-prazo-inline, .tarefa-nome-editable-wrapper, .tarefa-drag-handle')) {
            abrirDrawerTarefa(row.dataset);
            return;
        }

        // Kanban card click → open drawer
        var card = e.target.closest('.kanban-card');
        if (card && !e.target.closest('button, a, select, .dropdown, .dropdown-menu, input, .kanban-inline-form')) {
            abrirDrawerTarefa(card.dataset);
            return;
        }

        // Kanban subtask badge click → open drawer showing subtasks
        var subBadge = e.target.closest('.kanban-btn-ver-subtarefas');
        if (subBadge) {
            e.stopPropagation();
            var parentCard = subBadge.closest('.kanban-card');
            if (parentCard) abrirDrawerTarefa(parentCard.dataset);
            return;
        }

        // Toggle Lista / Kanban
        if (e.target.closest('#btnViewLista')) {
            var bL = document.getElementById('btnViewLista');
            var bK = document.getElementById('btnViewKanban');
            var vL = document.getElementById('tarefasViewLista');
            var vK = document.getElementById('tarefasViewKanban');
            if (bL && bK && vL && vK) {
                bL.classList.add('active'); bK.classList.remove('active');
                vL.classList.remove('d-none'); vK.classList.add('d-none');
                saveViewPreference('lista');
            }
            return;
        }
        if (e.target.closest('#btnViewKanban')) {
            var bL2 = document.getElementById('btnViewLista');
            var bK2 = document.getElementById('btnViewKanban');
            var vL2 = document.getElementById('tarefasViewLista');
            var vK2 = document.getElementById('tarefasViewKanban');
            if (bL2 && bK2 && vL2 && vK2) {
                bK2.classList.add('active'); bL2.classList.remove('active');
                vK2.classList.remove('d-none'); vL2.classList.add('d-none');
                saveViewPreference('kanban');
            }
            return;
        }

        // Delete tarefa button (in lista)
        var excluirBtn = e.target.closest('.btn-excluir-tarefa');
        if (excluirBtn) {
            e.stopPropagation();
            var delTarefaId = parseInt(excluirBtn.dataset.tarefaId, 10);
            var delNmTarefa = excluirBtn.dataset.nmTarefa || 'esta tarefa';

            confirmarExclusaoTarefa(delTarefaId, delNmTarefa, function () {
                executarExclusaoTarefa(delTarefaId, delNmTarefa, function () {
                    reloadContent();
                });
            });
            return;
        }

        // Inline edit name trigger
        var nomeEditBtn = e.target.closest('.tarefa-nome-edit-trigger');
        if (nomeEditBtn) {
            e.stopPropagation();
            var nmTarefaId = parseInt(nomeEditBtn.dataset.tarefaId, 10);
            ativarEdicaoNome(nmTarefaId);
            return;
        }

        // Quick status change
        var statusBtn = e.target.closest('.btn-alterar-status-rapido');
        if (statusBtn) {
            var stTarefaId = parseInt(statusBtn.dataset.tarefaId, 10);
            var newStatusId = parseInt(statusBtn.dataset.statusId, 10);

            // Find task data from row or card or edit button
            var dataSource = document.querySelector('.tarefa-row[data-tarefa-id="' + stTarefaId + '"]') ||
                             document.querySelector('.kanban-card[data-tarefa-id="' + stTarefaId + '"]');
            var stEditBtn = document.querySelector('.btn-editar-tarefa[data-tarefa-id="' + stTarefaId + '"]');
            var src = dataSource || (stEditBtn ? stEditBtn : null);
            if (!src) return;

            function executarTrocaStatus() {
                var stPayload = {
                    ProjetoTarefaID: stTarefaId,
                    ProjetoID: getProjetoId(),
                    NmTarefa: src.dataset.nmTarefa,
                    DsTarefa: src.dataset.dsTarefa || null,
                    ProjetoTarefaStatusID: newStatusId,
                    ProjetoTarefaPrioridadeID: parseInt(src.dataset.prioridadeId, 10),
                    DtInicio: brDateToIso(src.dataset.dtInicio) || null,
                    DtPrevisaoFim: brDateToIso(src.dataset.dtPrevisaoFim) || null,
                    DtFimReal: brDateToIso(src.dataset.dtFimReal) || null
                };

                // Auto-fill DtFimReal when concluding
                var concluidoId = getConcluidoStatusId();
                if (concluidoId && String(newStatusId) === concluidoId && !stPayload.DtFimReal) {
                    stPayload.DtFimReal = new Date().toISOString().slice(0, 10);
                } else if (concluidoId && String(newStatusId) !== concluidoId) {
                    stPayload.DtFimReal = null;
                }

                var stTrigger = statusBtn.closest('.dropdown') ? statusBtn.closest('.dropdown').querySelector('.tarefa-status-trigger') : null;
                var stOriginal = stTrigger ? stTrigger.innerHTML : '';
                if (stTrigger) stTrigger.innerHTML = '<i class="fa-solid fa-spinner fa-spin" style="font-size: 0.65rem;"></i>';

                fetch(window.sicUrl('/Projetos/EditarTarefa'), {
                    method: 'POST',
                    headers: { 'Content-Type': 'application/json' },
                    body: JSON.stringify(stPayload)
                })
                .then(function (res) { if (!res.ok) return res.json().then(function (err) { throw err; }); return res.json(); })
                .then(function () {
                    // Restore trigger appearance (will be replaced by atualizarTarefaDom)
                    if (stTrigger) stTrigger.innerHTML = stOriginal;

                    var statusInfo = encontrarStatusInfo(newStatusId);
                    atualizarTarefaDom(stTarefaId, {
                        ProjetoTarefaStatusID: newStatusId,
                        NmStatus: statusInfo.NmStatus,
                        CdCorStatus: statusInfo.CdCor,
                        DtFimRealBr: stPayload.DtFimReal ? (function () {
                            var p = stPayload.DtFimReal.split('-');
                            return p.length === 3 ? (p[2] + '/' + p[1] + '/' + p[0]) : '';
                        })() : ''
                    });

                    // If moved across groups, kanban view won't know — full reload only if kanban is active
                    var kanbanActive = document.getElementById('btnViewKanban');
                    if (kanbanActive && kanbanActive.classList.contains('active')) {
                        reloadContent();
                    }
                })
                .catch(function (err) {
                    if (stTrigger) stTrigger.innerHTML = stOriginal;
                    Swal.fire({ icon: 'error', title: 'Erro', text: (err && err.mensagem) || 'Não foi possível atualizar o status.' });
                });
            }

            // Check for open subtasks when concluding a parent task
            var conclId = getConcluidoStatusId();
            if (conclId && String(newStatusId) === conclId && temSubtarefasAbertas(stTarefaId)) {
                confirmarConcluirComSubtarefasAbertas(executarTrocaStatus);
            } else {
                executarTrocaStatus();
            }
            return;
        }

        // Quick prioridade change
        var prioBtn = e.target.closest('.btn-alterar-prioridade-rapido');
        if (prioBtn) {
            var prioTarefaId = parseInt(prioBtn.dataset.tarefaId, 10);
            var newPrioId = parseInt(prioBtn.dataset.prioridadeId, 10);

            var prioSrc = document.querySelector('.tarefa-row[data-tarefa-id="' + prioTarefaId + '"]') ||
                          document.querySelector('.kanban-card[data-tarefa-id="' + prioTarefaId + '"]');
            if (!prioSrc) return;

            var prioTrigger = prioBtn.closest('.dropdown') ? prioBtn.closest('.dropdown').querySelector('.tarefa-prioridade-trigger') : null;
            var prioOriginal = prioTrigger ? prioTrigger.innerHTML : '';
            if (prioTrigger) prioTrigger.innerHTML = '<i class="fa-solid fa-spinner fa-spin" style="font-size: 0.65rem;"></i>';

            var prioPayload = {
                ProjetoTarefaID: prioTarefaId,
                ProjetoID: getProjetoId(),
                NmTarefa: prioSrc.dataset.nmTarefa,
                DsTarefa: prioSrc.dataset.dsTarefa || null,
                ProjetoTarefaStatusID: parseInt(prioSrc.dataset.statusId, 10),
                ProjetoTarefaPrioridadeID: newPrioId,
                DtInicio: brDateToIso(prioSrc.dataset.dtInicio) || null,
                DtPrevisaoFim: brDateToIso(prioSrc.dataset.dtPrevisaoFim) || null,
                DtFimReal: brDateToIso(prioSrc.dataset.dtFimReal) || null
            };

            fetch(window.sicUrl('/Projetos/EditarTarefa'), {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify(prioPayload)
            })
            .then(function (res) { if (!res.ok) return res.json().then(function (err) { throw err; }); return res.json(); })
            .then(function () {
                if (prioTrigger) prioTrigger.innerHTML = prioOriginal;

                var prioInfo = encontrarPrioridadeInfo(newPrioId);
                atualizarTarefaDom(prioTarefaId, {
                    ProjetoTarefaPrioridadeID: newPrioId,
                    NmPrioridade: prioInfo.NmPrioridade,
                    CdCorPrioridade: prioInfo.CdCor
                });
            })
            .catch(function (err) {
                if (prioTrigger) prioTrigger.innerHTML = prioOriginal;
                Swal.fire({ icon: 'error', title: 'Erro', text: (err && err.mensagem) || 'Não foi possível atualizar a prioridade.' });
            });
            return;
        }

        // Quick responsável change
        var respBtn = e.target.closest('.btn-alterar-responsavel-rapido');
        if (respBtn) {
            var respTarefaId = parseInt(respBtn.dataset.tarefaId, 10);
            var newRespId = respBtn.dataset.responsavelId ? parseInt(respBtn.dataset.responsavelId, 10) : null;

            var respSrc = document.querySelector('.tarefa-row[data-tarefa-id="' + respTarefaId + '"]') ||
                          document.querySelector('.kanban-card[data-tarefa-id="' + respTarefaId + '"]');
            if (!respSrc) return;

            var respTrigger = respBtn.closest('.dropdown') ? respBtn.closest('.dropdown').querySelector('.tarefa-responsavel-trigger') : null;
            var respOriginal = respTrigger ? respTrigger.innerHTML : '';
            if (respTrigger) respTrigger.innerHTML = '<i class="fa-solid fa-spinner fa-spin text-muted" style="font-size: 0.65rem;"></i>';

            var respPayload = {
                ProjetoTarefaID: respTarefaId,
                ProjetoID: getProjetoId(),
                NmTarefa: respSrc.dataset.nmTarefa,
                DsTarefa: respSrc.dataset.dsTarefa || null,
                ProjetoTarefaStatusID: parseInt(respSrc.dataset.statusId, 10),
                ProjetoTarefaPrioridadeID: parseInt(respSrc.dataset.prioridadeId, 10),
                DtInicio: brDateToIso(respSrc.dataset.dtInicio) || null,
                DtPrevisaoFim: brDateToIso(respSrc.dataset.dtPrevisaoFim) || null,
                DtFimReal: brDateToIso(respSrc.dataset.dtFimReal) || null,
                UsuarioResponsavelID: newRespId
            };

            fetch(window.sicUrl('/Projetos/EditarTarefa'), {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify(respPayload)
            })
            .then(function (res) { if (!res.ok) return res.json().then(function (err) { throw err; }); return res.json(); })
            .then(function () {
                if (respTrigger) respTrigger.innerHTML = respOriginal;

                var respNome = encontrarResponsavelNome(newRespId);
                atualizarTarefaDom(respTarefaId, {
                    UsuarioResponsavelID: newRespId,
                    NmResponsavel: respNome
                });
            })
            .catch(function (err) {
                if (respTrigger) respTrigger.innerHTML = respOriginal;
                Swal.fire({ icon: 'error', title: 'Erro', text: (err && err.mensagem) || 'Não foi possível atualizar o responsável.' });
            });
            return;
        }

        // Participantes: remover
        var removerPartBtn = e.target.closest('.btn-remover-participante');
        if (removerPartBtn) {
            var rpId = parseInt(removerPartBtn.dataset.participanteId, 10);
            var rpNome = removerPartBtn.dataset.nmUsuario;

            Swal.fire({
                title: 'Remover participante?',
                html: 'Deseja remover <strong>' + rpNome + '</strong> do projeto?',
                icon: 'warning',
                showCancelButton: true,
                confirmButtonColor: '#dc3545',
                confirmButtonText: 'Sim, remover',
                cancelButtonText: 'Cancelar'
            }).then(function (result) {
                if (!result.isConfirmed) return;

                fetch(window.sicUrl('/Projetos/RemoverParticipante'), {
                    method: 'POST',
                    headers: { 'Content-Type': 'application/json' },
                    body: JSON.stringify({ ProjetoParticipanteID: rpId, ProjetoID: getProjetoId() })
                })
                .then(function (res) { if (!res.ok) return res.json().then(function (err) { throw err; }); return res.json(); })
                .then(function () { reloadContent('tab-participantes'); })
                .catch(function (err) { Swal.fire({ icon: 'error', title: 'Erro', text: (err && err.mensagem) || 'Não foi possível remover o participante.' }); });
            });
            return;
        }

        // Quick filter chips
        var quickBtn = e.target.closest('.tarefa-quick-filter');
        if (quickBtn) {
            var filter = quickBtn.dataset.quickFilter || '';
            activeQuickFilter = (activeQuickFilter === filter) ? '' : filter;
            applyTarefaFilters();
            return;
        }

        // Filters clear
        if (e.target.closest('#tarefaFilterClear') || e.target.closest('#btnLimparFiltrosEmpty')) {
            limparFiltrosTarefas();
            return;
        }

        // Kanban inline creation: "+ Tarefa"
        var addTaskBtn = e.target.closest('#tarefasViewKanban .kanban-btn-add-task');
        if (addTaskBtn) {
            closeAllKanbanInlineForms();
            var footer = addTaskBtn.closest('.kanban-column-footer');
            if (!footer) return;
            var form = footer.querySelector('.kanban-inline-form');
            if (!form) return;
            form.classList.remove('d-none');
            addTaskBtn.classList.add('d-none');
            var inp = form.querySelector('.kanban-inline-input');
            if (inp) inp.focus();
            return;
        }

        // Kanban inline creation: "+ Subitem"
        var addSubBtn = e.target.closest('#tarefasViewKanban .kanban-btn-add-subtask');
        if (addSubBtn) {
            closeAllKanbanInlineForms();
            var kCard = addSubBtn.closest('.kanban-card');
            if (!kCard) return;
            var subForm = kCard.querySelector('.kanban-inline-form');
            if (!subForm) return;
            subForm.classList.remove('d-none');
            addSubBtn.classList.add('d-none');
            var subInp = subForm.querySelector('.kanban-inline-input');
            if (subInp) subInp.focus();
            return;
        }

        // Kanban inline: cancel
        var cancelBtn = e.target.closest('#tarefasViewKanban .kanban-inline-btn-cancel');
        if (cancelBtn) { closeAllKanbanInlineForms(); return; }

        // Kanban inline: confirm
        var confirmBtn = e.target.closest('#tarefasViewKanban .kanban-inline-btn-confirm');
        if (confirmBtn) {
            var cForm = confirmBtn.closest('.kanban-inline-form');
            if (cForm) submitKanbanInlineForm(cForm);
            return;
        }

        // Close inline forms on outside click
        if (!e.target.closest('#tarefasViewKanban .kanban-inline-form') &&
            !e.target.closest('#tarefasViewKanban .kanban-btn-add-task') &&
            !e.target.closest('#tarefasViewKanban .kanban-btn-add-subtask')) {
            closeAllKanbanInlineForms();
        }
    });

    // Unified filter inputs (delegated via change/input)
    document.addEventListener('input', function (e) {
        if (e.target.matches('#tarefaFilterSearch')) applyTarefaFilters();
        if (e.target.matches('#tarefasViewKanban .kanban-inline-input')) e.target.classList.remove('is-invalid');
    });
    document.addEventListener('change', function (e) {
        if (e.target.matches('#tarefaFilterStatus, #tarefaFilterPrioridade, #tarefaFilterResponsavel')) applyTarefaFilters();
    });

    // Escape key handlers
    document.addEventListener('keydown', function (e) {
        if (e.key === 'Escape') {
            var active = document.activeElement;
            if (active && active.closest('#tarefasViewKanban .kanban-inline-form')) {
                closeAllKanbanInlineForms();
            }
        }
        if (e.key === 'Enter') {
            var kInput = e.target.closest('#tarefasViewKanban .kanban-inline-input');
            if (kInput) {
                e.preventDefault();
                var kForm = kInput.closest('.kanban-inline-form');
                if (kForm) submitKanbanInlineForm(kForm);
            }
        }
    });

    // ====== Unified Filters (Lista + Kanban) ======
    var activeQuickFilter = '';

    function getTodayIso() {
        return new Date().toISOString().slice(0, 10);
    }

    function getUsuarioLogadoId() {
        var el = document.getElementById('detalhesConteudo');
        return el ? el.dataset.usuarioLogadoId || '' : '';
    }

    function matchesQuickFilter(el) {
        if (!activeQuickFilter) return true;
        var today = getTodayIso();
        var concluidoId = getConcluidoStatusId();
        switch (activeQuickFilter) {
            case 'atrasadas':
                var prazo = brDateToIso(el.dataset.dtPrevisaoFim) || el.dataset.dtPrevisaoFim || '';
                var statusDone = concluidoId && String(el.dataset.statusId) === concluidoId;
                return prazo && prazo < today && !statusDone;
            case 'sem-prazo':
                var p = brDateToIso(el.dataset.dtPrevisaoFim) || el.dataset.dtPrevisaoFim || '';
                return !p;
            case 'sem-responsavel':
                return !el.dataset.responsavelId || el.dataset.responsavelId === '';
            case 'minhas':
                return el.dataset.responsavelId === getUsuarioLogadoId();
            default:
                return true;
        }
    }

    function limparFiltrosTarefas() {
        var fs = document.getElementById('tarefaFilterSearch');
        var fst = document.getElementById('tarefaFilterStatus');
        var fp = document.getElementById('tarefaFilterPrioridade');
        var fr = document.getElementById('tarefaFilterResponsavel');
        if (fs) fs.value = '';
        if (fst) fst.value = '';
        if (fp) fp.value = '';
        if (fr) fr.value = '';
        activeQuickFilter = '';
        applyTarefaFilters();
    }

    function applyTarefaFilters() {
        var searchEl = document.getElementById('tarefaFilterSearch');
        var statusEl = document.getElementById('tarefaFilterStatus');
        var prioEl = document.getElementById('tarefaFilterPrioridade');
        var respEl = document.getElementById('tarefaFilterResponsavel');
        var clearEl = document.getElementById('tarefaFilterClear');
        var searchTerm = (searchEl ? searchEl.value : '').toLowerCase().trim();
        var statusId = statusEl ? statusEl.value : '';
        var prioridadeId = prioEl ? prioEl.value : '';
        var responsavelId = respEl ? respEl.value : '';

        var hasFilter = !!(searchTerm || statusId || prioridadeId || responsavelId || activeQuickFilter);
        if (clearEl) clearEl.classList.toggle('d-none', !hasFilter);

        // Update quick filter chip active state
        document.querySelectorAll('.tarefa-quick-filter').forEach(function (btn) {
            btn.classList.toggle('active', btn.dataset.quickFilter === activeQuickFilter);
        });

        // Filter Kanban cards
        document.querySelectorAll('.kanban-card').forEach(function (c) {
            var match = true;
            if (searchTerm) {
                var nome = (c.dataset.nmTarefa || '').toLowerCase();
                var desc = (c.dataset.dsTarefa || '').toLowerCase();
                if (nome.indexOf(searchTerm) === -1 && desc.indexOf(searchTerm) === -1) match = false;
            }
            if (match && statusId && c.dataset.statusId !== statusId) match = false;
            if (match && prioridadeId && c.dataset.prioridadeId !== prioridadeId) match = false;
            if (match && responsavelId && c.dataset.responsavelId !== responsavelId) match = false;
            if (match) match = matchesQuickFilter(c);
            c.classList.toggle('kanban-filtered-out', !match);
        });

        // Filter Lista rows (parent + subtask rows)
        document.querySelectorAll('#tarefasViewLista .tarefa-row').forEach(function (row) {
            var match = true;
            if (searchTerm) {
                var nome = (row.dataset.nmTarefa || '').toLowerCase();
                var desc = (row.dataset.dsTarefa || '').toLowerCase();
                if (nome.indexOf(searchTerm) === -1 && desc.indexOf(searchTerm) === -1) match = false;
            }
            if (match && statusId && row.dataset.statusId !== statusId) match = false;
            if (match && prioridadeId && row.dataset.prioridadeId !== prioridadeId) match = false;
            if (match && responsavelId && row.dataset.responsavelId !== responsavelId) match = false;
            if (match) match = matchesQuickFilter(row);
            row.classList.toggle('tarefa-filtered-out', !match);
        });

        // Show parent if any subtask matches, show subtask if parent matches
        document.querySelectorAll('#tarefasViewLista .tarefa-subtarefas-container').forEach(function (subContainer) {
            var parentRow = subContainer.previousElementSibling;
            while (parentRow && !parentRow.classList.contains('tarefa-row')) parentRow = parentRow.previousElementSibling;
            if (!parentRow) return;

            var subRows = subContainer.querySelectorAll('.tarefa-row-sub');
            var anySubVisible = false;
            subRows.forEach(function (sub) { if (!sub.classList.contains('tarefa-filtered-out')) anySubVisible = true; });

            // If parent is hidden but a sub matches, show the parent
            if (parentRow.classList.contains('tarefa-filtered-out') && anySubVisible) {
                parentRow.classList.remove('tarefa-filtered-out');
            }
            // If parent is visible, show all its subs (unless sub itself is filtered)
            if (!parentRow.classList.contains('tarefa-filtered-out') && !hasFilter) {
                subRows.forEach(function (sub) { sub.classList.remove('tarefa-filtered-out'); });
            }
        });

        updateKanbanCounters();
        updateGroupCountersFiltered();
        updateFiltersEmptyState(hasFilter);
    }

    function updateGroupCountersFiltered() {
        document.querySelectorAll('.tarefa-group').forEach(function (group) {
            var countEl = group.querySelector('.tarefa-group-count');
            if (!countEl) return;
            var visibleRows = group.querySelectorAll('.tarefa-table > .tarefa-row:not(.tarefa-row-sub):not(.tarefa-filtered-out)').length;
            countEl.textContent = visibleRows;
            // Hide group entirely when filtered and no rows visible
            var allRows = group.querySelectorAll('.tarefa-table > .tarefa-row:not(.tarefa-row-sub)').length;
            group.classList.toggle('tarefa-group-hidden-by-filter', allRows > 0 && visibleRows === 0);
        });
    }

    function updateFiltersEmptyState(hasFilter) {
        var listaView = document.getElementById('tarefasViewLista');
        if (!listaView) return;
        var visibleRows = listaView.querySelectorAll('.tarefa-row:not(.tarefa-row-sub):not(.tarefa-filtered-out)').length;
        var existingEmpty = listaView.querySelector('.tarefa-filter-empty');

        if (hasFilter && visibleRows === 0) {
            if (!existingEmpty) {
                var emptyDiv = document.createElement('div');
                emptyDiv.className = 'tarefa-filter-empty';
                emptyDiv.innerHTML = '<div class="text-center py-4">' +
                    '<i class="fa-solid fa-magnifying-glass text-muted mb-2" style="font-size: 2rem; opacity: 0.5;"></i>' +
                    '<p class="text-muted mb-2">Nenhuma tarefa corresponde aos filtros aplicados.</p>' +
                    '<button type="button" class="btn btn-sm btn-outline-secondary" id="btnLimparFiltrosEmpty">' +
                    '<i class="fa-solid fa-filter-circle-xmark me-1"></i>Limpar filtros</button>' +
                    '</div>';
                listaView.appendChild(emptyDiv);
            }
        } else if (existingEmpty) {
            existingEmpty.remove();
        }
    }

    function updateKanbanCounters() {
        document.querySelectorAll('.kanban-column').forEach(function (col) {
            var visibleCards = col.querySelectorAll('.kanban-card:not(.kanban-filtered-out)').length;
            var badge = col.querySelector('.kanban-column-header .kanban-column-count');
            if (badge) badge.textContent = visibleCards;
        });
    }

    // ====== Kanban Inline Creation ======
    function closeAllKanbanInlineForms() {
        document.querySelectorAll('#tarefasViewKanban .kanban-inline-form').forEach(function (f) {
            f.classList.add('d-none');
            var i = f.querySelector('.kanban-inline-input');
            if (i) { i.value = ''; i.classList.remove('is-invalid'); }
        });
        document.querySelectorAll('#tarefasViewKanban .kanban-btn-add-task').forEach(function (b) { b.classList.remove('d-none'); });
        document.querySelectorAll('#tarefasViewKanban .kanban-btn-add-subtask').forEach(function (b) { b.classList.remove('d-none'); });
    }

    var kanbanInlineSubmitting = false;

    function submitKanbanInlineForm(form) {
        if (kanbanInlineSubmitting) return;

        var input = form.querySelector('.kanban-inline-input');
        var title = input ? input.value.trim() : '';
        if (!title) { if (input) { input.classList.add('is-invalid'); input.focus(); } return; }

        var inlineType = form.dataset.inlineType;
        var statusId = 1;
        var tarefaPaiId = null;

        if (inlineType === 'task') {
            var sa = form.dataset.statusId;
            if (sa) statusId = parseInt(sa, 10);
        } else if (inlineType === 'subtask') {
            var kc = form.closest('.kanban-card');
            if (kc) statusId = parseInt(kc.dataset.statusId, 10) || 1;
            var pa = form.dataset.tarefaPaiId;
            if (pa) tarefaPaiId = parseInt(pa, 10);
        }

        var payload = {
            ProjetoID: getProjetoId(),
            NmTarefa: title,
            DsTarefa: null,
            ProjetoTarefaStatusID: statusId,
            ProjetoTarefaPrioridadeID: 2,
            DtInicio: null,
            DtPrevisaoFim: null,
            ProjetoTarefaPaiID: tarefaPaiId
        };

        var cfBtn = form.querySelector('.kanban-inline-btn-confirm');
        if (cfBtn) { cfBtn.disabled = true; cfBtn.innerHTML = '<i class="fa-solid fa-spinner fa-spin"></i>'; }
        if (input) input.readOnly = true;
        kanbanInlineSubmitting = true;

        fetch(window.sicUrl('/Projetos/CriarTarefa'), {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(payload)
        })
        .then(function (res) { if (!res.ok) return res.json().then(function (err) { throw err; }); return res.json(); })
        .then(function () {
            kanbanInlineSubmitting = false;
            showToast('success', inlineType === 'subtask' ? 'Subtarefa criada!' : 'Tarefa criada!').then(function () { reloadContent(); });
        })
        .catch(function (err) {
            kanbanInlineSubmitting = false;
            if (cfBtn) { cfBtn.disabled = false; cfBtn.innerHTML = '<i class="fa-solid fa-check"></i> Criar'; }
            if (input) input.readOnly = false;
            Swal.fire({ icon: 'error', title: 'Erro', text: (err && err.mensagem) || 'Não foi possível criar a tarefa.' });
        });
    }

    // ====== Kanban Drag & Drop ======
    var draggedCard = null;

    function initDragAndDrop() {
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
                document.querySelectorAll('.kanban-drag-over').forEach(function (el) { el.classList.remove('kanban-drag-over'); });
            });
        });
    }

    // These use event delegation since drop zones are dynamic
    document.addEventListener('dragover', function (e) {
        var zone = e.target.closest('.kanban-droppable');
        if (!zone) return;
        e.preventDefault();
        e.dataTransfer.dropEffect = 'move';
        zone.classList.add('kanban-drag-over');
    });

    document.addEventListener('dragleave', function (e) {
        var zone = e.target.closest('.kanban-droppable');
        if (!zone) return;
        if (!zone.contains(e.relatedTarget)) zone.classList.remove('kanban-drag-over');
    });

    document.addEventListener('drop', function (e) {
        var zone = e.target.closest('.kanban-droppable');
        if (!zone) return;
        e.preventDefault();
        zone.classList.remove('kanban-drag-over');

        if (!draggedCard) return;

        var newStatusId = parseInt(zone.dataset.statusId, 10);
        var currentStatusId = parseInt(draggedCard.dataset.statusId, 10);
        if (newStatusId === currentStatusId) return;

        var payload = {
            ProjetoTarefaID: parseInt(draggedCard.dataset.tarefaId, 10),
            ProjetoID: getProjetoId(),
            NmTarefa: draggedCard.dataset.nmTarefa,
            DsTarefa: draggedCard.dataset.dsTarefa || null,
            ProjetoTarefaStatusID: newStatusId,
            ProjetoTarefaPrioridadeID: parseInt(draggedCard.dataset.prioridadeId, 10),
            UsuarioResponsavelID: draggedCard.dataset.responsavelId ? parseInt(draggedCard.dataset.responsavelId, 10) : null,
            DtInicio: brDateToIso(draggedCard.dataset.dtInicio) || null,
            DtPrevisaoFim: brDateToIso(draggedCard.dataset.dtPrevisaoFim) || null,
            DtFimReal: brDateToIso(draggedCard.dataset.dtFimReal) || null
        };

        // Optimistic move
        var placeholder = zone.querySelector('.kanban-empty-placeholder');
        if (placeholder) placeholder.remove();
        zone.appendChild(draggedCard);
        draggedCard.dataset.statusId = newStatusId;
        updateKanbanCounters();

        fetch(window.sicUrl('/Projetos/EditarTarefa'), {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(payload)
        })
        .then(function (res) { if (!res.ok) return res.json().then(function (err) { throw err; }); return res.json(); })
        .then(function () { reloadContent(); })
        .catch(function (err) {
            Swal.fire({ icon: 'error', title: 'Erro', text: (err && err.mensagem) || 'Não foi possível atualizar o status da tarefa.' });
            reloadContent();
        });
    });

    // Initialize D&D on page load
    initDragAndDrop();

    // ====== Lista Drag & Drop (reorder tasks in list view) ======
    // Ordering is persisted client-side in sessionStorage (no backend change).
    // Cross-group drag triggers status change via EditarTarefa (same as kanban).
    var draggedRow = null;

    function getListaOrderKey() {
        return 'sic_projeto_' + getProjetoId() + '_listaOrder';
    }

    function getListaOrderMap() {
        try {
            var raw = sessionStorage.getItem(getListaOrderKey());
            return raw ? JSON.parse(raw) : {};
        } catch (ex) { return {}; }
    }

    function saveListaOrderMap(map) {
        try { sessionStorage.setItem(getListaOrderKey(), JSON.stringify(map)); } catch (ex) { /* ignore */ }
    }

    function captureGroupOrder(groupBody) {
        if (!groupBody) return;
        var statusId = groupBody.closest('.tarefa-group')?.dataset?.statusId;
        if (!statusId) return;
        var ids = [];
        groupBody.querySelectorAll(':scope .tarefa-table > .tarefa-row.tarefa-draggable').forEach(function (r) {
            ids.push(String(r.dataset.tarefaId));
        });
        var map = getListaOrderMap();
        map[statusId] = ids;
        saveListaOrderMap(map);
    }

    function restoreListaOrder() {
        var map = getListaOrderMap();
        Object.keys(map).forEach(function (statusId) {
            var group = document.querySelector('.tarefa-group[data-status-id="' + statusId + '"]');
            if (!group) return;
            var table = group.querySelector('.tarefa-table');
            if (!table) return;
            var header = table.querySelector('.tarefa-table-header');
            var ids = map[statusId] || [];
            ids.forEach(function (id) {
                var row = table.querySelector('.tarefa-row.tarefa-draggable[data-tarefa-id="' + id + '"]');
                if (row) {
                    // Also move associated subtarefas container
                    var subContainer = document.getElementById('subtarefas-' + id);
                    table.appendChild(row);
                    if (subContainer && subContainer.parentElement === table) {
                        table.appendChild(subContainer);
                    }
                }
            });
            // Keep header at top
            if (header && header.previousElementSibling !== null) {
                table.insertBefore(header, table.firstChild);
            }
        });
    }

    function initListaDragAndDrop() {
        document.querySelectorAll('.tarefa-row.tarefa-draggable').forEach(function (row) {
            // Avoid double-binding
            if (row.dataset.dndBound === '1') return;
            row.dataset.dndBound = '1';

            row.addEventListener('dragstart', function (e) {
                // Only allow drag when initiated from the handle
                var fromHandle = e.target && (e.target.classList?.contains('tarefa-drag-handle') || e.target.closest('.tarefa-drag-handle'));
                // Some browsers start drag on the whole row; we still allow but set source
                draggedRow = row;
                row.classList.add('tarefa-dragging');
                e.dataTransfer.effectAllowed = 'move';
                try { e.dataTransfer.setData('text/plain', row.dataset.tarefaId); } catch (ex) { /* ignore */ }
            });

            row.addEventListener('dragend', function () {
                row.classList.remove('tarefa-dragging');
                draggedRow = null;
                document.querySelectorAll('.tarefa-drag-over-before, .tarefa-drag-over-after, .tarefa-group-body.tarefa-drag-over, .tarefa-group-empty.tarefa-drag-over').forEach(function (el) {
                    el.classList.remove('tarefa-drag-over-before', 'tarefa-drag-over-after', 'tarefa-drag-over');
                });
            });
        });
    }

    // Delegated dragover on lista rows (to show insertion indicator)
    document.addEventListener('dragover', function (e) {
        if (!draggedRow) return;
        var overRow = e.target.closest('.tarefa-row.tarefa-draggable');
        var overEmpty = e.target.closest('.tarefa-group-empty');
        var overBody = e.target.closest('.tarefa-group-body');

        if (!overRow && !overEmpty && !overBody) return;
        // Do not interfere with kanban D&D
        if (e.target.closest('.kanban-droppable')) return;

        e.preventDefault();
        e.dataTransfer.dropEffect = 'move';

        // Clear previous indicators
        document.querySelectorAll('.tarefa-drag-over-before, .tarefa-drag-over-after, .tarefa-group-body.tarefa-drag-over, .tarefa-group-empty.tarefa-drag-over').forEach(function (el) {
            el.classList.remove('tarefa-drag-over-before', 'tarefa-drag-over-after', 'tarefa-drag-over');
        });

        if (overRow && overRow !== draggedRow) {
            var rect = overRow.getBoundingClientRect();
            var midY = rect.top + rect.height / 2;
            if (e.clientY < midY) {
                overRow.classList.add('tarefa-drag-over-before');
            } else {
                overRow.classList.add('tarefa-drag-over-after');
            }
        } else if (overEmpty) {
            overEmpty.classList.add('tarefa-drag-over');
        } else if (overBody) {
            overBody.classList.add('tarefa-drag-over');
        }
    });

    document.addEventListener('drop', function (e) {
        if (!draggedRow) return;
        // Let kanban handler deal with its own zones
        if (e.target.closest('.kanban-droppable')) return;

        var overRow = e.target.closest('.tarefa-row.tarefa-draggable');
        var overEmpty = e.target.closest('.tarefa-group-empty');
        var overBody = e.target.closest('.tarefa-group-body');
        if (!overRow && !overEmpty && !overBody) return;

        e.preventDefault();

        var sourceGroup = draggedRow.closest('.tarefa-group');
        var sourceStatusId = sourceGroup ? parseInt(sourceGroup.dataset.statusId, 10) : null;

        var targetGroup = (overRow || overEmpty || overBody).closest('.tarefa-group');
        var targetStatusId = targetGroup ? parseInt(targetGroup.dataset.statusId, 10) : null;
        if (!targetStatusId) return;

        var subContainer = document.getElementById('subtarefas-' + draggedRow.dataset.tarefaId);

        // Cross-group drag: change status (same as kanban)
        if (sourceStatusId !== targetStatusId) {
            var statusPayload = {
                ProjetoTarefaID: parseInt(draggedRow.dataset.tarefaId, 10),
                ProjetoID: getProjetoId(),
                NmTarefa: draggedRow.dataset.nmTarefa,
                DsTarefa: draggedRow.dataset.dsTarefa || null,
                ProjetoTarefaStatusID: targetStatusId,
                ProjetoTarefaPrioridadeID: parseInt(draggedRow.dataset.prioridadeId, 10),
                UsuarioResponsavelID: draggedRow.dataset.responsavelId ? parseInt(draggedRow.dataset.responsavelId, 10) : null,
                DtInicio: brDateToIso(draggedRow.dataset.dtInicio) || null,
                DtPrevisaoFim: brDateToIso(draggedRow.dataset.dtPrevisaoFim) || null,
                DtFimReal: brDateToIso(draggedRow.dataset.dtFimReal) || null
            };

            // Auto-fill DtFimReal when concluding
            var concluidoId = getConcluidoStatusId();
            if (concluidoId && String(targetStatusId) === concluidoId && !statusPayload.DtFimReal) {
                statusPayload.DtFimReal = new Date().toISOString().slice(0, 10);
            } else if (concluidoId && String(targetStatusId) !== concluidoId) {
                statusPayload.DtFimReal = null;
            }

            fetch(window.sicUrl('/Projetos/EditarTarefa'), {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify(statusPayload)
            })
            .then(function (res) { if (!res.ok) return res.json().then(function (err) { throw err; }); return res.json(); })
            .then(function () { reloadContent(); })
            .catch(function (err) {
                Swal.fire({ icon: 'error', title: 'Erro', text: (err && err.mensagem) || 'Não foi possível atualizar o status da tarefa.' });
                reloadContent();
            });
            return;
        }

        // Same-group drag: reorder visually + persist in sessionStorage
        var targetTable = targetGroup.querySelector('.tarefa-table');
        if (!targetTable) {
            // Group was empty → create table from empty state
            var groupBody = targetGroup.querySelector('.tarefa-group-body');
            if (!groupBody) return;
            var emptyEl = groupBody.querySelector('.tarefa-group-empty');
            if (emptyEl) emptyEl.remove();
            // Create new table with header (cloned from another group if available)
            var sampleHeader = document.querySelector('.tarefa-table-header');
            var newTable = document.createElement('div');
            newTable.className = 'tarefa-table';
            if (sampleHeader) newTable.appendChild(sampleHeader.cloneNode(true));
            groupBody.appendChild(newTable);
            targetTable = newTable;
        }

        if (overRow && overRow !== draggedRow) {
            if (overRow.classList.contains('tarefa-drag-over-after') || overRow.nextElementSibling === draggedRow) {
                // Insert after overRow (and after its subtarefas container if any)
                var overSub = document.getElementById('subtarefas-' + overRow.dataset.tarefaId);
                var insertAfter = overSub && overSub.parentElement === targetTable ? overSub : overRow;
                insertAfter.insertAdjacentElement('afterend', draggedRow);
            } else {
                overRow.insertAdjacentElement('beforebegin', draggedRow);
            }
        } else {
            // Dropped on empty area or group body → append
            targetTable.appendChild(draggedRow);
        }

        // Move subtarefas container along with the parent row
        if (subContainer && subContainer.parentElement) {
            draggedRow.insertAdjacentElement('afterend', subContainer);
        }

        // Cleanup indicators
        document.querySelectorAll('.tarefa-drag-over-before, .tarefa-drag-over-after, .tarefa-group-body.tarefa-drag-over, .tarefa-group-empty.tarefa-drag-over').forEach(function (el) {
            el.classList.remove('tarefa-drag-over-before', 'tarefa-drag-over-after', 'tarefa-drag-over');
        });

        // Persist new order in sessionStorage
        var groupBody2 = targetGroup.querySelector('.tarefa-group-body');
        captureGroupOrder(groupBody2);

        showToast('success', 'Ordem atualizada');
    });

    initListaDragAndDrop();
    restoreListaOrder();

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
                DtFimReal: document.getElementById('epDtFimReal').value || null,
                CamposExtras: Array.from(document.querySelectorAll('#epCamposExtras .ep-campo-extra'))
                    .map(function (row) {
                        return {
                            Ordem: parseInt(row.dataset.ordem, 10),
                            NmCampo: (row.querySelector('.ep-campo-nome').value || '').trim(),
                            VlCampo: (row.querySelector('.ep-campo-valor').value || '').trim()
                        };
                    })
                    .filter(function (c) { return c.NmCampo.length > 0; })
            };

            var btnSalvar = document.getElementById('btnSalvarEditarProjeto');
            btnSalvar.disabled = true;
            btnSalvar.innerHTML = '<i class="fa-solid fa-spinner fa-spin me-1"></i>Salvando...';

            fetch(window.sicUrl('/Projetos/Editar'), {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify(payload)
            })
            .then(function (res) { if (!res.ok) return res.json().then(function (err) { throw err; }); return res.json(); })
            .then(function () {
                var modal = bootstrap.Modal.getInstance(document.getElementById('modalEditarProjeto'));
                if (modal) modal.hide();

                showToast('success', 'Projeto atualizado!').then(function () { reloadContent(); });
            })
            .catch(function (err) {
                Swal.fire({ icon: 'error', title: 'Erro', text: (err && err.mensagem) || 'Não foi possível atualizar o projeto.' });
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
                ProjetoID: getProjetoId(),
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

            fetch(window.sicUrl('/Projetos/CriarTarefa'), {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify(payload)
            })
            .then(function (res) { if (!res.ok) return res.json().then(function (err) { throw err; }); return res.json(); })
            .then(function () {
                var modal = bootstrap.Modal.getInstance(document.getElementById('modalNovaTarefa'));
                if (modal) modal.hide();

                showToast('success', 'Tarefa criada!').then(function () { reloadContent(); });
            })
            .catch(function (err) {
                Swal.fire({ icon: 'error', title: 'Erro', text: (err && err.mensagem) || 'Não foi possível criar a tarefa.' });
            })
            .finally(function () {
                btnSalvar.disabled = false;
                btnSalvar.innerHTML = '<i class="fa-solid fa-check me-1"></i>Criar Tarefa';
            });
        });

        document.getElementById('modalNovaTarefa').addEventListener('hidden.bs.modal', function () {
            formNovaTarefa.reset();
            formNovaTarefa.querySelectorAll('.is-invalid').forEach(function (el) { el.classList.remove('is-invalid'); });
        });
    }

    // ====== Drawer Editar Tarefa — submit via AJAX ======
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
                ProjetoID: getProjetoId(),
                NmTarefa: nmTarefa,
                DsTarefa: (document.getElementById('etDsTarefa').value || '').trim() || null,
                ProjetoTarefaStatusID: parseInt(document.getElementById('etProjetoTarefaStatusID').value, 10),
                ProjetoTarefaPrioridadeID: parseInt(document.getElementById('etProjetoTarefaPrioridadeID').value, 10),
                UsuarioResponsavelID: document.getElementById('etUsuarioResponsavelID').value ? parseInt(document.getElementById('etUsuarioResponsavelID').value, 10) : null,
                DtInicio: document.getElementById('etDtInicio').value || null,
                DtPrevisaoFim: document.getElementById('etDtPrevisaoFim').value || null,
                DtFimReal: document.getElementById('etDtFimReal').value || null
            };

            function enviarEdicaoTarefa() {
                var btnSalvar = document.getElementById('btnSalvarEditarTarefa');
                btnSalvar.disabled = true;
                btnSalvar.innerHTML = '<i class="fa-solid fa-spinner fa-spin me-1"></i>Salvando...';

                fetch(window.sicUrl('/Projetos/EditarTarefa'), {
                    method: 'POST',
                    headers: { 'Content-Type': 'application/json' },
                    body: JSON.stringify(payload)
                })
                .then(function (res) { if (!res.ok) return res.json().then(function (err) { throw err; }); return res.json(); })
                .then(function () {
                    var offcanvas = bootstrap.Offcanvas.getInstance(document.getElementById('drawerEditarTarefa'));
                    if (offcanvas) offcanvas.hide();

                    showToast('success', 'Tarefa atualizada!').then(function () { reloadContent(); });
                })
                .catch(function (err) {
                    Swal.fire({ icon: 'error', title: 'Erro', text: (err && err.mensagem) || 'Não foi possível atualizar a tarefa.' });
                })
                .finally(function () {
                    btnSalvar.disabled = false;
                    btnSalvar.innerHTML = '<i class="fa-solid fa-check me-1"></i>Salvar Alterações';
                });
            }

            // Check for open subtasks when concluding
            var conclId = getConcluidoStatusId();
            if (conclId && String(payload.ProjetoTarefaStatusID) === conclId && temSubtarefasAbertas(payload.ProjetoTarefaID)) {
                confirmarConcluirComSubtarefasAbertas(enviarEdicaoTarefa);
            } else {
                enviarEdicaoTarefa();
            }
        });
    }

    // ====== Drawer — botão Excluir ======
    var btnExcluirDrawer = document.getElementById('btnExcluirTarefaDrawer');
    if (btnExcluirDrawer) {
        btnExcluirDrawer.addEventListener('click', function () {
            var tarefaId = parseInt(document.getElementById('etProjetoTarefaID').value, 10);
            var nmTarefa = document.getElementById('drawerEditarTarefa').dataset.nmTarefa || 'esta tarefa';

            confirmarExclusaoTarefa(tarefaId, nmTarefa, function () {
                executarExclusaoTarefa(tarefaId, nmTarefa, function () {
                    var offcanvas = bootstrap.Offcanvas.getInstance(document.getElementById('drawerEditarTarefa'));
                    if (offcanvas) offcanvas.hide();
                    reloadContent();
                });
            });
        });
    }

    // ====== Limpar validação ======

    // ====== Auto "Concluído" when Término Real is filled ======
    var etDtFimReal = document.getElementById('etDtFimReal');
    var etStatusSel = document.getElementById('etProjetoTarefaStatusID');
    if (etDtFimReal && etStatusSel) {
        etDtFimReal.addEventListener('change', function () {
            var container = document.getElementById('detalhesConteudo');
            var concluidoId = container ? container.dataset.concluidoStatusId : '';
            if (!concluidoId) return;
            if (etDtFimReal.value) {
                etStatusSel.value = concluidoId;
            }
        });
        etStatusSel.addEventListener('change', function () {
            var container = document.getElementById('detalhesConteudo');
            var concluidoId = container ? container.dataset.concluidoStatusId : '';
            if (!concluidoId) return;
            if (etStatusSel.value === concluidoId && !etDtFimReal.value) {
                etDtFimReal.value = new Date().toISOString().slice(0, 10);
            } else if (etStatusSel.value !== concluidoId && etDtFimReal.value) {
                etDtFimReal.value = '';
            }
        });
    }
    ['formEditarProjeto', 'formNovaTarefa', 'formEditarTarefa'].forEach(function (formId) {
        var form = document.getElementById(formId);
        if (form) {
            form.addEventListener('input', function (e) { e.target.classList.remove('is-invalid'); });
            form.addEventListener('change', function (e) { e.target.classList.remove('is-invalid'); });
        }
    });

    ['modalEditarProjeto'].forEach(function (modalId) {
        var modal = document.getElementById(modalId);
        if (modal) {
            modal.addEventListener('hidden.bs.modal', function () {
                modal.querySelectorAll('.is-invalid').forEach(function (el) { el.classList.remove('is-invalid'); });
            });
        }
    });

    var drawerEl = document.getElementById('drawerEditarTarefa');
    if (drawerEl) {
        drawerEl.addEventListener('hidden.bs.offcanvas', function () {
            drawerEl.querySelectorAll('.is-invalid').forEach(function (el) { el.classList.remove('is-invalid'); });
        });
    }

    // ====== Participantes: busca autocomplete (modal — fora do partial) ======
    var apBuscaInput = document.getElementById('apBuscaUsuario');
    var apResultados = document.getElementById('apResultadosBusca');
    var apUsuarioID = document.getElementById('apUsuarioID');
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
                fetch(window.sicUrl('/Projetos/BuscarUsuarios?texto=' + encodeURIComponent(texto)))
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
                    .catch(function () { apResultados.classList.add('d-none'); });
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
            if (!usuarioId) { apBuscaInput.classList.add('is-invalid'); return; }

            var payload = {
                ProjetoID: getProjetoId(),
                UsuarioID: usuarioId,
                NmPapel: document.getElementById('apNmPapel').value.trim()
            };

            btnSalvarParticipante.disabled = true;
            fetch(window.sicUrl('/Projetos/AdicionarParticipante'), {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify(payload)
            })
            .then(function (res) { if (!res.ok) return res.json().then(function (err) { throw err; }); return res.json(); })
            .then(function () {
                var modal = bootstrap.Modal.getInstance(document.getElementById('modalAdicionarParticipante'));
                if (modal) modal.hide();
                reloadContent('tab-participantes');
            })
            .catch(function (err) {
                btnSalvarParticipante.disabled = false;
                Swal.fire({ icon: 'error', title: 'Erro', text: (err && err.mensagem) || 'Não foi possível adicionar o participante.' });
            });
        });
    }

    // Limpar modal participante ao fechar
    var modalAP = document.getElementById('modalAdicionarParticipante');
    if (modalAP) {
        modalAP.addEventListener('hidden.bs.modal', function () {
            if (apBuscaInput) apBuscaInput.value = '';
            if (apUsuarioID) apUsuarioID.value = '';
            var nmPapel = document.getElementById('apNmPapel');
            if (nmPapel) nmPapel.value = '';
            if (apResultados) { apResultados.classList.add('d-none'); apResultados.innerHTML = ''; }
            if (apBuscaInput) apBuscaInput.classList.remove('is-invalid');
            if (btnSalvarParticipante) btnSalvarParticipante.disabled = false;
            // Reset do preview do papel
            atualizarPreviewPapel();
        });
    }

    // Atualiza o ícone e a descrição do papel selecionado no modal
    function atualizarPreviewPapel() {
        var sel = document.getElementById('apNmPapel');
        var previewIcon = document.querySelector('#apNmPapelPreview i');
        var descSpan = document.querySelector('#apNmPapelHelp span');
        if (!sel || !previewIcon) return;

        var opt = sel.options[sel.selectedIndex];
        var iconClass = (opt && opt.getAttribute('data-icone')) || 'fa-solid fa-user-tag';
        var descricao = (opt && opt.getAttribute('data-descricao')) || 'Escolha o papel que este participante terá no projeto.';

        previewIcon.className = iconClass;
        if (descSpan) descSpan.textContent = descricao;
    }

    var apNmPapelSelect = document.getElementById('apNmPapel');
    if (apNmPapelSelect) {
        apNmPapelSelect.addEventListener('change', atualizarPreviewPapel);
    }

    // Initial sync of tarefa pai dropdown
    syncTarefaPaiDropdown();

    // ====== Inline name editing ======
    function ativarEdicaoNome(tarefaId) {
        var wrapper = document.querySelector('.tarefa-nome-editable-wrapper .tarefa-nome-text[data-tarefa-id="' + tarefaId + '"]');
        if (!wrapper) return;
        var wrapperParent = wrapper.closest('.tarefa-nome-editable-wrapper');
        if (!wrapperParent) return;

        var nomeText = wrapper.textContent.trim();
        var input = wrapperParent.querySelector('.tarefa-nome-input');
        var editBtn = wrapperParent.querySelector('.tarefa-nome-edit-trigger');
        if (!input || !editBtn) return;

        // Show input, hide text and button
        wrapper.classList.add('d-none');
        editBtn.classList.add('d-none');
        input.classList.remove('d-none');
        input.value = nomeText;
        input.focus();
        input.select();

        var originalValue = nomeText;

        function cancelEdit() {
            input.classList.add('d-none');
            wrapper.classList.remove('d-none');
            editBtn.classList.remove('d-none');
            input.value = '';
        }

        function saveEdit() {
            var newName = input.value.trim();
            if (!newName) {
                input.classList.add('is-invalid');
                input.focus();
                return;
            }
            if (newName === originalValue) {
                cancelEdit();
                return;
            }

            // Get task data
            var row = document.querySelector('.tarefa-row[data-tarefa-id="' + tarefaId + '"]');
            if (!row) return;

            input.disabled = true;

            var payload = {
                ProjetoTarefaID: tarefaId,
                ProjetoID: getProjetoId(),
                NmTarefa: newName,
                DsTarefa: row.dataset.dsTarefa || null,
                ProjetoTarefaStatusID: parseInt(row.dataset.statusId, 10),
                ProjetoTarefaPrioridadeID: parseInt(row.dataset.prioridadeId, 10),
                DtInicio: brDateToIso(row.dataset.dtInicio) || null,
                DtPrevisaoFim: brDateToIso(row.dataset.dtPrevisaoFim) || null,
                DtFimReal: brDateToIso(row.dataset.dtFimReal) || null
            };

            fetch(window.sicUrl('/Projetos/EditarTarefa'), {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify(payload)
            })
            .then(function (res) { if (!res.ok) return res.json().then(function (err) { throw err; }); return res.json(); })
            .then(function () { 
                showToast('success', 'Nome atualizado!');
                reloadContent(); 
            })
            .catch(function (err) {
                input.disabled = false;
                input.focus();
                Swal.fire({ icon: 'error', title: 'Erro', text: (err && err.mensagem) || 'Não foi possível atualizar o nome.' });
            });
        }

        // Event handlers
        function handleKeyDown(e) {
            if (e.key === 'Enter') {
                e.preventDefault();
                saveEdit();
            } else if (e.key === 'Escape') {
                e.preventDefault();
                cancelEdit();
                cleanup();
            }
        }

        function handleBlur() {
            // Small delay to allow button clicks to register
            setTimeout(function() {
                if (!input.classList.contains('d-none')) {
                    saveEdit();
                }
            }, 150);
        }

        function cleanup() {
            input.removeEventListener('keydown', handleKeyDown);
            input.removeEventListener('blur', handleBlur);
        }

        input.addEventListener('keydown', handleKeyDown);
        input.addEventListener('blur', handleBlur);
    }

    // ====== Drawer Histórico: toggle collapse/expand ======
    var btnToggleHist = document.getElementById('btnToggleHistoricoDrawer');
    if (btnToggleHist) {
        btnToggleHist.addEventListener('click', function (e) {
            e.preventDefault();
            var list = document.getElementById('drawerHistoricoList');
            var icon = document.getElementById('iconToggleHistoricoDrawer');
            if (!list || !icon) return;
            var hidden = list.classList.toggle('d-none');
            icon.classList.toggle('fa-chevron-down', !hidden);
            icon.classList.toggle('fa-chevron-right', hidden);
        });
    }

    // ====== View preference persistence (lista/kanban) ======
    function getViewPreferenceKey() {
        return 'sic_projeto_' + getProjetoId() + '_view';
    }

    function saveViewPreference(view) {
        try { localStorage.setItem(getViewPreferenceKey(), view); } catch (ex) { /* ignore */ }
    }

    function restoreViewPreference() {
        try {
            var saved = localStorage.getItem(getViewPreferenceKey());
            if (saved === 'kanban') {
                var btnK = document.getElementById('btnViewKanban');
                if (btnK) btnK.click();
            }
        } catch (ex) { /* ignore */ }
    }

    restoreViewPreference();

    // ====== Keyboard shortcuts ======
    // Ctrl+K / Cmd+K: focus search filter
    // Esc: clear filters (when search is focused or filters are active)
    document.addEventListener('keydown', function (e) {
        // Ignore when typing in editable elements (except search input itself)
        var active = document.activeElement;
        var isEditable = active && (
            active.tagName === 'INPUT' ||
            active.tagName === 'TEXTAREA' ||
            active.tagName === 'SELECT' ||
            active.isContentEditable
        );

        // Ctrl+K / Cmd+K to focus search
        if ((e.ctrlKey || e.metaKey) && e.key.toLowerCase() === 'k') {
            var searchInput = document.getElementById('tarefaFilterSearch');
            if (searchInput) {
                e.preventDefault();
                searchInput.focus();
                searchInput.select();
            }
            return;
        }

        // Esc clears filters when search is focused
        if (e.key === 'Escape' && active && active.id === 'tarefaFilterSearch') {
            if (active.value) {
                e.preventDefault();
                limparFiltrosTarefas();
                active.blur();
            }
        }
    });

})();
