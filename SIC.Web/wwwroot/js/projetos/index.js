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

    // ====== Navegação para detalhes ao clicar no card ======
    document.querySelectorAll('.projeto-card[data-url]').forEach(function (card) {
        card.addEventListener('click', function () {
            showPageLoading();
            window.location.href = card.dataset.url;
        });
    });

    // ====== Toggle de modo de visualização (Quadro / Lista / Kanban) ======
    document.querySelectorAll('.projeto-view-toggle [data-view-mode]').forEach(function (btn) {
        btn.addEventListener('click', function () {
            var mode = btn.dataset.viewMode;

            // Gravar preferência no cookie (1 ano) e recarregar para o servidor
            // carregar os dados corretos para o modo selecionado
            document.cookie = 'sic_projetos_view=' + mode + ';path=/Projetos;max-age=31536000;SameSite=Lax';
            showPageLoading();
            document.getElementById('formFiltros').submit();
        });
    });

    // ====== Lista: expand/collapse de tarefas e subtarefas ======
    document.addEventListener('click', function (e) {
        var toggleBtn = e.target.closest('.projeto-list-toggle');
        if (!toggleBtn) return;

        var targetId = toggleBtn.dataset.target;
        var targetEl = document.getElementById(targetId);
        if (!targetEl) return;

        var chevron = toggleBtn.querySelector('.projeto-list-chevron');
        var isExpanded = !targetEl.classList.contains('d-none');

        targetEl.classList.toggle('d-none', isExpanded);
        if (chevron) {
            chevron.classList.toggle('rotated', !isExpanded);
        }

        // Add/remove expanded class on parent container
        var parentRow = toggleBtn.closest('.projeto-list-row');
        var parentTarefa = toggleBtn.closest('.projeto-list-tarefa');
        var expandTarget = parentTarefa || parentRow;
        if (expandTarget) {
            expandTarget.classList.toggle('expanded', !isExpanded);
        }
    });

    // ====== Helper: converte dd/MM/yyyy → yyyy-MM-dd para payloads ======
    function brDateToIso(brDate) {
        if (!brDate) return '';
        var parts = brDate.split('/');
        if (parts.length !== 3) return '';
        return parts[2] + '-' + parts[1] + '-' + parts[0];
    }

    // ====== Helper: converte yyyy-MM-dd → dd/MM/yyyy para exibição ======
    function isoToBrDate(iso) {
        if (!iso) return '';
        var parts = iso.split('-');
        if (parts.length !== 3) return '';
        return parts[2] + '/' + parts[1] + '/' + parts[0];
    }

    // ====== Helpers: buscar info de status/prioridade nas listas JSON do viewLista ======
    function getListStatusInfo(statusId) {
        try {
            var list = JSON.parse(document.getElementById('viewLista').dataset.statusList);
            for (var i = 0; i < list.length; i++) {
                if (list[i].id === statusId) return list[i];
            }
        } catch (_) { /* ignore */ }
        return null;
    }

    function getListPrioridadeInfo(prioridadeId) {
        try {
            var list = JSON.parse(document.getElementById('viewLista').dataset.prioridadeList);
            for (var i = 0; i < list.length; i++) {
                if (list[i].id === prioridadeId) return list[i];
            }
        } catch (_) { /* ignore */ }
        return null;
    }

    // ====== Helper: flash de confirmação visual ======
    function flashSuccess(element) {
        if (!element) return;
        element.classList.remove('projeto-list-flash-success');
        void element.offsetWidth;
        element.classList.add('projeto-list-flash-success');
        setTimeout(function () { element.classList.remove('projeto-list-flash-success'); }, 700);
    }

    // ====== Helper: calcula classes de prazo a partir de ISO date ======
    function computeDateClasses(isoDate, isSubtask) {
        var baseClass = 'projeto-list-date' + (isSubtask ? ' projeto-list-date-sub' : '');
        var icon = 'fa-solid fa-calendar-check';
        if (!isoDate) {
            return { cls: baseClass + ' projeto-list-date-placeholder', icon: 'fa-solid fa-calendar', label: 'Sem prazo', title: 'Sem prazo definido', isEmpty: true };
        }
        var brDate = isoToBrDate(isoDate);
        var d = new Date(isoDate + 'T00:00:00');
        var today = new Date(); today.setHours(0, 0, 0, 0);
        var diff = (d - today) / (1000 * 60 * 60 * 24);
        if (diff < 0) {
            baseClass += ' projeto-list-date-overdue';
            icon = 'fa-solid fa-triangle-exclamation';
        } else if (diff <= 3) {
            baseClass += ' projeto-list-date-soon';
            icon = 'fa-solid fa-clock';
        }
        return { cls: baseClass, icon: icon, label: brDate, title: 'Prazo: ' + brDate, isEmpty: false };
    }

    // ====== Helper: reconstrói span de data ======
    function buildDateSpan(isoDate, isSubtask) {
        var info = computeDateClasses(isoDate, isSubtask);
        var span = document.createElement('span');
        span.className = info.cls;
        span.title = info.title;
        span.innerHTML = '<i class="' + info.icon + '"></i> ' + info.label;
        return span;
    }

    // ====== Helper: reconstrói elemento de responsável ======
    function buildResponsavelElement(nome, isSubtask) {
        if (nome && nome.trim()) {
            var parts = nome.trim().split(/\s+/);
            var iniciais = parts.length >= 2
                ? (parts[0][0] + parts[parts.length - 1][0]).toUpperCase()
                : parts[0].substring(0, Math.min(2, parts[0].length)).toUpperCase();
            var span = document.createElement('span');
            span.className = isSubtask ? 'projeto-list-subtarefa-responsavel' : 'projeto-list-responsavel';
            span.title = nome + ' — Clique para alterar';
            span.textContent = iniciais;
            return span;
        } else {
            var btn = document.createElement('button');
            btn.type = 'button';
            btn.className = isSubtask ? 'projeto-list-subtarefa-btn-responsavel' : 'projeto-list-btn-responsavel';
            btn.title = 'Adicionar responsável';
            btn.setAttribute('aria-label', 'Adicionar responsável');
            btn.innerHTML = '<i class="fa-solid fa-user-plus"></i>';
            return btn;
        }
    }

    // ====== Helpers Kanban: buscar info de status/prioridade nas listas JSON do board ======
    function getKanbanStatusInfo(statusId) {
        try {
            var list = JSON.parse(document.querySelector('.kanban-board-global').dataset.statusList);
            for (var i = 0; i < list.length; i++) {
                if (list[i].id === statusId) return list[i];
            }
        } catch (_) { /* ignore */ }
        return null;
    }

    function getKanbanPrioridadeInfo(prioridadeId) {
        try {
            var list = JSON.parse(document.querySelector('.kanban-board-global').dataset.prioridadeList);
            for (var i = 0; i < list.length; i++) {
                if (list[i].id === prioridadeId) return list[i];
            }
        } catch (_) { /* ignore */ }
        return null;
    }

    function buildKanbanCardResponsavelElement(nome) {
        if (nome && nome.trim()) {
            var parts = nome.trim().split(/\s+/);
            var iniciais = parts.length >= 2
                ? (parts[0][0] + parts[parts.length - 1][0])
                : parts[0].substring(0, Math.min(2, parts[0].length));
            var span = document.createElement('span');
            span.className = 'kanban-card-responsavel';
            span.setAttribute('draggable', 'false');
            span.title = nome + ' — Clique para alterar';
            span.textContent = iniciais;
            return span;
        } else {
            var btn = document.createElement('button');
            btn.type = 'button';
            btn.className = 'kanban-card-btn-responsavel';
            btn.setAttribute('draggable', 'false');
            btn.title = 'Adicionar responsável';
            btn.setAttribute('aria-label', 'Adicionar responsável');
            btn.innerHTML = '<i class="fa-solid fa-user-plus"></i>';
            return btn;
        }
    }

    function buildKanbanSubtaskResponsavelElement(nome) {
        if (nome && nome.trim()) {
            var span = document.createElement('span');
            span.className = 'kanban-subtask-responsavel';
            span.title = nome + ' — Clique para alterar';
            span.innerHTML = '<i class="fa-solid fa-user"></i>';
            return span;
        } else {
            var btn = document.createElement('button');
            btn.type = 'button';
            btn.className = 'kanban-subtask-btn-responsavel';
            btn.title = 'Adicionar responsável';
            btn.setAttribute('aria-label', 'Adicionar responsável');
            btn.innerHTML = '<i class="fa-solid fa-user-plus"></i>';
            return btn;
        }
    }

    // ====== Helper: recalcula barra de progresso de subtarefas no card Kanban ======
    function updateKanbanCardSubtaskProgress(card) {
        if (!card) return;
        var section = card.querySelector('.kanban-subtask-section');
        if (!section) return;

        var items = card.querySelectorAll('.kanban-subtask-item');
        var total = items.length;
        if (total === 0) return;

        var board = card.closest('.kanban-board-global');
        var statusList = [];
        try { statusList = JSON.parse(board.dataset.statusList || '[]'); } catch (_) { }
        var lastStatusId = statusList.length > 0 ? statusList[statusList.length - 1].id : 0;

        var done = 0;
        items.forEach(function (item) {
            if (parseInt(item.dataset.subtaskStatusId, 10) === lastStatusId) done++;
        });

        var pct = Math.round(done * 100 / total);

        var progressSpan = section.querySelector('.kanban-subtask-progress');
        if (progressSpan) progressSpan.textContent = 'Subtarefas (' + done + '/' + total + ')';

        var fill = section.querySelector('.kanban-subtask-progress-fill');
        if (fill) fill.style.width = pct + '%';
    }

    // ====== Builders: criar elementos novos para inserção inline ======

    function buildKanbanCardElement(data) {
        var prioInfo = getKanbanPrioridadeInfo(data.prioridadeId);
        var prioCor = prioInfo ? prioInfo.cor : '#6c757d';
        var prioNome = prioInfo ? prioInfo.nome : '';

        var card = document.createElement('div');
        card.className = 'kanban-card kanban-draggable-global';
        card.draggable = true;
        card.style.borderLeftColor = prioCor;
        card.dataset.tarefaId = data.tarefaId;
        card.dataset.projetoId = data.projetoId;
        card.dataset.nmTarefa = data.nmTarefa;
        card.dataset.dsTarefa = '';
        card.dataset.statusId = data.statusId;
        card.dataset.prioridadeId = data.prioridadeId;
        card.dataset.responsavelId = '';
        card.dataset.dtInicio = '';
        card.dataset.dtPrevisaoFim = '';
        card.dataset.dtFimReal = '';

        var actions = document.createElement('div');
        actions.className = 'kanban-card-actions';
        actions.draggable = false;
        actions.innerHTML = '<button type="button" class="kanban-card-menu-trigger" draggable="false" title="Ações" aria-label="Menu de ações" aria-haspopup="true" aria-expanded="false"><i class="fa-solid fa-ellipsis-vertical"></i></button>' +
            '<div class="kanban-card-menu"><a class="kanban-card-menu-item" href="/Projetos/' + data.projetoId + '"><i class="fa-solid fa-arrow-up-right-from-square"></i> Ver detalhes do projeto</a></div>';
        card.appendChild(actions);

        var projeto = document.createElement('div');
        projeto.className = 'kanban-card-projeto';
        projeto.title = data.nmProjeto;
        projeto.innerHTML = '<i class="fa-solid fa-diagram-project"></i> ';
        projeto.appendChild(document.createTextNode(data.nmProjeto));
        card.appendChild(projeto);

        var titleDiv = document.createElement('div');
        titleDiv.className = 'kanban-card-title';
        titleDiv.textContent = data.nmTarefa;
        card.appendChild(titleDiv);

        var meta = document.createElement('div');
        meta.className = 'kanban-card-meta';
        meta.innerHTML = '<button type="button" class="kanban-card-prioridade-trigger" draggable="false" style="background: color-mix(in srgb, ' + prioCor + ' 18%, transparent); color: ' + prioCor + ';" title="Alterar prioridade"><i class="fa-solid fa-flag" style="font-size: 0.6rem;"></i> ' + prioNome + ' <i class="fa-solid fa-caret-down kanban-card-prioridade-caret"></i></button>';
        meta.appendChild(buildKanbanCardResponsavelElement(null));
        card.appendChild(meta);

        var addSubBtn = document.createElement('button');
        addSubBtn.type = 'button';
        addSubBtn.className = 'kanban-btn-add-subtask';
        addSubBtn.title = 'Criar subtarefa';
        addSubBtn.setAttribute('aria-label', 'Criar subtarefa');
        addSubBtn.innerHTML = '<i class="fa-solid fa-plus"></i> Subitem';
        card.appendChild(addSubBtn);

        var inlineForm = document.createElement('div');
        inlineForm.className = 'kanban-inline-form d-none';
        inlineForm.dataset.inlineType = 'subtask';
        inlineForm.dataset.tarefaPaiId = data.tarefaId;
        inlineForm.setAttribute('role', 'form');
        inlineForm.setAttribute('aria-label', 'Criar subtarefa');
        inlineForm.innerHTML = '<input type="text" class="kanban-inline-input" placeholder="Título da subtarefa..." maxlength="200" aria-label="Título da subtarefa" />' +
            '<div class="kanban-inline-actions">' +
            '<button type="button" class="kanban-inline-btn-confirm" title="Confirmar criação"><i class="fa-solid fa-check"></i> Criar</button>' +
            '<button type="button" class="kanban-inline-btn-cancel" title="Cancelar"><i class="fa-solid fa-xmark"></i></button>' +
            '</div>';
        card.appendChild(inlineForm);

        return card;
    }

    function buildKanbanSubtaskItem(data) {
        var statusInfo = getKanbanStatusInfo(data.statusId);
        var cor = statusInfo ? statusInfo.cor : '#6c757d';
        var nome = statusInfo ? statusInfo.nome : '';

        var li = document.createElement('li');
        li.className = 'kanban-subtask-item';
        li.style.borderLeftColor = cor;
        li.dataset.subtaskId = data.subtaskId;
        li.dataset.projetoId = data.projetoId;
        li.dataset.subtaskStatusId = data.statusId;
        li.dataset.subtaskNmTarefa = data.nmTarefa;
        li.dataset.subtaskDsTarefa = '';
        li.dataset.subtaskPrioridadeId = data.prioridadeId;
        li.dataset.subtaskDtInicio = '';
        li.dataset.subtaskDtPrevisaoFim = '';
        li.dataset.subtaskDtFimReal = '';
        li.dataset.subtaskResponsavelId = '';
        li.dataset.subtaskNmResponsavel = '';

        var titleSpan = document.createElement('span');
        titleSpan.className = 'kanban-subtask-title';
        titleSpan.textContent = data.nmTarefa;
        li.appendChild(titleSpan);

        var actionsDiv = document.createElement('div');
        actionsDiv.className = 'kanban-subtask-actions';
        actionsDiv.innerHTML = '<button type="button" class="kanban-subtask-status-trigger" style="color: ' + cor + ';" title="Alterar status"><span class="kanban-subtask-status-dot" style="background: ' + cor + ';"></span> ' + nome + ' <i class="fa-solid fa-caret-down"></i></button>';
        actionsDiv.appendChild(buildKanbanSubtaskResponsavelElement(null));
        li.appendChild(actionsDiv);

        return li;
    }

    function buildListSubtarefaElement(data) {
        var statusInfo = getListStatusInfo(data.statusId);
        var cor = statusInfo ? statusInfo.cor : '#6c757d';
        var nome = statusInfo ? statusInfo.nome : '';

        var div = document.createElement('div');
        div.className = 'projeto-list-subtarefa';
        div.dataset.subtaskId = data.subtaskId;
        div.dataset.projetoId = data.projetoId;
        div.dataset.subtaskStatusId = data.statusId;
        div.dataset.subtaskNmTarefa = data.nmTarefa;
        div.dataset.subtaskDsTarefa = '';
        div.dataset.subtaskPrioridadeId = data.prioridadeId;
        div.dataset.subtaskDtInicio = '';
        div.dataset.subtaskDtPrevisaoFim = '';
        div.dataset.subtaskDtFimReal = '';
        div.dataset.subtaskResponsavelId = '';
        div.dataset.subtaskNmResponsavel = '';

        var nomeSpan = document.createElement('span');
        nomeSpan.className = 'projeto-list-subtarefa-nome';
        nomeSpan.textContent = data.nmTarefa;
        div.appendChild(nomeSpan);

        var metaDiv = document.createElement('div');
        metaDiv.className = 'projeto-list-subtarefa-meta';
        metaDiv.innerHTML = '<span class="projeto-list-date projeto-list-date-sub projeto-list-date-placeholder" title="Sem prazo definido"><i class="fa-solid fa-calendar"></i> Sem prazo</span>' +
            '<button type="button" class="projeto-list-subtarefa-status-trigger" style="color: ' + cor + ';" title="Alterar status">' +
            '<span class="projeto-list-subtarefa-status-dot" style="background: ' + cor + ';"></span> ' + nome +
            ' <i class="fa-solid fa-caret-down"></i></button>';
        metaDiv.appendChild(buildResponsavelElement(null, true));
        div.appendChild(metaDiv);

        return div;
    }

    // ====== Kanban Global: Menu de ações do card (3 pontinhos) ======
    function closeAllCardMenus() {
        document.querySelectorAll('.kanban-card-menu.show').forEach(function (m) {
            m.classList.remove('show');
        });
        document.querySelectorAll('.kanban-card-menu-trigger[aria-expanded="true"]').forEach(function (t) {
            t.setAttribute('aria-expanded', 'false');
        });
    }

    function closeAllCardPriorityMenus() {
        document.querySelectorAll('.kanban-card-prioridade-menu.show').forEach(function (m) {
            m.classList.remove('show');
        });
    }

    function closeAllCardSearchPopovers() {
        document.querySelectorAll('.kanban-card-search-popover.show').forEach(function (p) {
            p.classList.remove('show');
        });
    }

    // Helper: monta payload completo do card para EditarTarefa
    function buildCardPayload(card, overrides) {
        var payload = {
            ProjetoTarefaID: parseInt(card.dataset.tarefaId, 10),
            ProjetoID: parseInt(card.dataset.projetoId, 10),
            NmTarefa: card.dataset.nmTarefa,
            DsTarefa: card.dataset.dsTarefa || null,
            ProjetoTarefaStatusID: parseInt(card.dataset.statusId, 10),
            ProjetoTarefaPrioridadeID: parseInt(card.dataset.prioridadeId, 10),
            UsuarioResponsavelID: card.dataset.responsavelId ? parseInt(card.dataset.responsavelId, 10) : null,
            DtInicio: brDateToIso(card.dataset.dtInicio) || null,
            DtPrevisaoFim: brDateToIso(card.dataset.dtPrevisaoFim) || null,
            DtFimReal: brDateToIso(card.dataset.dtFimReal) || null
        };
        if (overrides) {
            for (var k in overrides) { payload[k] = overrides[k]; }
        }
        return payload;
    }

    // Fechar todos os menus do card
    function closeAllCardInteractions() {
        closeAllCardMenus();
        closeAllCardPriorityMenus();
        closeAllCardSearchPopovers();
    }

    // Toggle do menu ao clicar no botão "⋮"
    document.addEventListener('click', function (e) {
        var trigger = e.target.closest('.kanban-card-menu-trigger');
        if (!trigger) return;

        e.preventDefault();
        e.stopPropagation();

        var menu = trigger.nextElementSibling;
        var isOpen = menu.classList.contains('show');

        // Fecha todos os outros menus (card + subtask)
        closeAllCardInteractions();
        closeAllSubtaskMenus();

        if (!isOpen) {
            menu.classList.add('show');
            trigger.setAttribute('aria-expanded', 'true');
        }
    });

    // Impedir que o drag inicie ao interagir com controles do card
    document.addEventListener('mousedown', function (e) {
        if (e.target.closest('.kanban-card-actions') ||
            e.target.closest('.kanban-card-prioridade-trigger') ||
            e.target.closest('.kanban-card-prioridade-menu') ||
            e.target.closest('.kanban-card-responsavel') ||
            e.target.closest('.kanban-card-btn-responsavel') ||
            e.target.closest('.kanban-card-search-popover')) {
            e.stopPropagation();
        }
    }, true); // capture phase — intercepta antes do drag

    document.addEventListener('dragstart', function (e) {
        if (e.target.closest('.kanban-card-actions') ||
            e.target.closest('.kanban-card-prioridade-trigger') ||
            e.target.closest('.kanban-card-prioridade-menu') ||
            e.target.closest('.kanban-card-responsavel') ||
            e.target.closest('.kanban-card-btn-responsavel') ||
            e.target.closest('.kanban-card-search-popover')) {
            e.preventDefault();
        }
    }, true);

    // Overlay de carregamento ao clicar em "Ver detalhes"
    document.addEventListener('click', function (e) {
        var menuItem = e.target.closest('.kanban-card-menu-item');
        if (menuItem) {
            closeAllCardMenus();
            showPageLoading();
        }
    });

    // ====== Kanban Global: Prioridade inline da tarefa principal ======

    // Toggle: abrir menu de prioridade ao clicar no badge
    document.addEventListener('click', function (e) {
        var trigger = e.target.closest('.kanban-card-prioridade-trigger');
        if (!trigger) return;
        e.stopPropagation();

        var card = trigger.closest('.kanban-card');
        if (!card) return;

        var existing = card.querySelector('.kanban-card-prioridade-menu');
        if (existing && existing.classList.contains('show')) {
            existing.classList.remove('show');
            return;
        }

        closeAllCardInteractions();
        closeAllSubtaskMenus();

        if (existing) existing.remove();

        var board = document.querySelector('#viewKanban .kanban-board-global');
        var prioridadeList = [];
        try { prioridadeList = JSON.parse(board.dataset.prioridadeList); } catch (ex) { /* ignore */ }

        var currentPrioridadeId = parseInt(card.dataset.prioridadeId, 10);

        var menu = document.createElement('div');
        menu.className = 'kanban-card-prioridade-menu show';

        prioridadeList.forEach(function (p) {
            var btn = document.createElement('button');
            btn.type = 'button';
            btn.className = 'kanban-card-prioridade-option' + (p.id === currentPrioridadeId ? ' active' : '');
            btn.dataset.prioridadeId = p.id;
            btn.innerHTML = '<span class="kanban-card-prioridade-dot" style="background: ' + p.cor + ';"></span>' + p.nome;
            menu.appendChild(btn);
        });

        // Posicionar no container .kanban-card-meta (relativo ao trigger)
        var meta = trigger.closest('.kanban-card-meta');
        if (meta) {
            meta.style.position = 'relative';
            meta.appendChild(menu);
        } else {
            trigger.parentNode.appendChild(menu);
        }
    });

    // Seleção: clicar numa opção de prioridade
    document.addEventListener('click', function (e) {
        var option = e.target.closest('.kanban-card-prioridade-option');
        if (!option) return;
        e.stopPropagation();

        var card = option.closest('.kanban-card');
        if (!card) return;

        var newPrioridadeId = parseInt(option.dataset.prioridadeId, 10);
        var currentPrioridadeId = parseInt(card.dataset.prioridadeId, 10);

        var menu = option.closest('.kanban-card-prioridade-menu');
        if (newPrioridadeId === currentPrioridadeId) {
            if (menu) menu.classList.remove('show');
            return;
        }

        var payload = buildCardPayload(card, { ProjetoTarefaPrioridadeID: newPrioridadeId });

        // Feedback visual: spinner no trigger
        var trigger = card.querySelector('.kanban-card-prioridade-trigger');
        var originalHtml = trigger ? trigger.innerHTML : '';
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
            card.dataset.prioridadeId = newPrioridadeId;
            var info = getKanbanPrioridadeInfo(newPrioridadeId);
            if (trigger && info) {
                trigger.style.background = 'color-mix(in srgb, ' + info.cor + ' 18%, transparent)';
                trigger.style.color = info.cor;
                trigger.innerHTML = '<i class="fa-solid fa-flag" style="font-size: 0.6rem;"></i> ' + info.nome + ' <i class="fa-solid fa-caret-down kanban-card-prioridade-caret"></i>';
                card.style.borderLeftColor = info.cor;
            } else if (trigger) {
                trigger.innerHTML = originalHtml;
            }
            flashSuccess(card);
        })
        .catch(function (err) {
            if (trigger) trigger.innerHTML = originalHtml;
            Swal.fire({
                icon: 'error',
                title: 'Erro',
                text: (err && err.mensagem) || 'Não foi possível atualizar a prioridade da tarefa.'
            });
        });
    });

    // ====== Kanban Global: Responsável inline da tarefa principal ======

    // Helper: atualizar responsável do card via EditarTarefa
    function updateCardResponsavel(card, responsavel, popover, onSuccess) {
        var payload = buildCardPayload(card, {
            UsuarioResponsavelID: responsavel ? responsavel.id : null
        });

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
            if (typeof onSuccess === 'function') {
                onSuccess();
            } else {
                showPageLoading();
                window.location.reload();
            }
        })
        .catch(function (err) {
            Swal.fire({
                icon: 'error',
                title: 'Erro',
                text: (err && err.mensagem) || 'Não foi possível atualizar o responsável da tarefa.'
            });
        });
    }

    // Abrir popover de busca de responsável na tarefa principal
    document.addEventListener('click', function (e) {
        var target = e.target.closest('.kanban-card-responsavel, .kanban-card-btn-responsavel');
        if (!target) return;
        // Ignorar se for subtask
        if (target.closest('.kanban-subtask-item')) return;
        e.stopPropagation();

        var card = target.closest('.kanban-card');
        if (!card) return;

        var existing = card.querySelector('.kanban-card-search-popover');
        if (existing && existing.classList.contains('show')) {
            existing.classList.remove('show');
            return;
        }

        closeAllCardInteractions();
        closeAllSubtaskMenus();

        if (existing) existing.remove();

        var popover = document.createElement('div');
        popover.className = 'kanban-card-search-popover show';

        var searchInput = document.createElement('input');
        searchInput.type = 'text';
        searchInput.className = 'kanban-subtask-search-input';
        searchInput.placeholder = 'Buscar usuário...';
        searchInput.setAttribute('autocomplete', 'off');
        popover.appendChild(searchInput);

        var resultsList = document.createElement('ul');
        resultsList.className = 'kanban-subtask-search-results';
        popover.appendChild(resultsList);

        var currentResponsavelId = card.dataset.responsavelId;
        if (currentResponsavelId) {
            var removeBtn = document.createElement('button');
            removeBtn.type = 'button';
            removeBtn.className = 'kanban-subtask-search-btn-remove';
            removeBtn.innerHTML = '<i class="fa-solid fa-user-xmark"></i> Remover responsável';
            removeBtn.addEventListener('click', function (ev) {
                ev.stopPropagation();
                updateCardResponsavel(card, null, popover, function () {
                    card.dataset.responsavelId = '';
                    var metaDiv = card.querySelector('.kanban-card-meta');
                    if (metaDiv) {
                        var oldEl = metaDiv.querySelector('.kanban-card-responsavel, .kanban-card-btn-responsavel');
                        var newEl = buildKanbanCardResponsavelElement(null);
                        if (oldEl) oldEl.parentNode.replaceChild(newEl, oldEl); else metaDiv.appendChild(newEl);
                    }
                    if (popover) popover.remove();
                    flashSuccess(card);
                });
            });
            popover.appendChild(removeBtn);
        }

        // Posicionar no meta do card (junto da prioridade e responsável)
        var meta = card.querySelector('.kanban-card-meta');
        if (meta) {
            meta.style.position = 'relative';
            meta.appendChild(popover);
        } else {
            card.appendChild(popover);
        }
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
                                var resp = { id: u.usuarioID, nome: u.nmUsuario };
                                updateCardResponsavel(card, resp, popover, function () {
                                    card.dataset.responsavelId = String(resp.id);
                                    var metaDiv = card.querySelector('.kanban-card-meta');
                                    if (metaDiv) {
                                        var oldEl = metaDiv.querySelector('.kanban-card-responsavel, .kanban-card-btn-responsavel');
                                        var newEl = buildKanbanCardResponsavelElement(resp.nome);
                                        if (oldEl) oldEl.parentNode.replaceChild(newEl, oldEl); else metaDiv.appendChild(newEl);
                                    }
                                    if (popover) popover.remove();
                                    flashSuccess(card);
                                });
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

    // ====== Kanban Global: Drag & Drop ======
    var draggedCard = null;

    function attachKanbanDragHandlers(card) {
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
    }

    document.querySelectorAll('.kanban-draggable-global').forEach(function (card) {
        attachKanbanDragHandlers(card);
    });

    document.querySelectorAll('.kanban-droppable-global').forEach(function (zone) {
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
            var projetoId = parseInt(draggedCard.dataset.projetoId, 10);

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

            // Guardar posição original para reverter em caso de erro
            var originalParent = draggedCard.parentNode;
            var originalNext = draggedCard.nextSibling;

            // Otimismo visual: mover o card imediatamente
            var placeholder = zone.querySelector('.kanban-empty-placeholder');
            if (placeholder) placeholder.remove();
            zone.appendChild(draggedCard);
            draggedCard.dataset.statusId = newStatusId;
            updateKanbanGlobalCounters();

            var movedCard = draggedCard;

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
                flashSuccess(movedCard);
            })
            .catch(function (err) {
                // Reverter: devolver card à coluna original
                movedCard.dataset.statusId = currentStatusId;
                if (originalParent) {
                    originalParent.insertBefore(movedCard, originalNext);
                }
                updateKanbanGlobalCounters();
                Swal.fire({
                    icon: 'error',
                    title: 'Erro',
                    text: (err && err.mensagem) || 'Não foi possível atualizar o status da tarefa.'
                });
            });
        });
    });

    function updateKanbanGlobalCounters() {
        document.querySelectorAll('#viewKanban .kanban-column').forEach(function (col) {
            var cards = col.querySelectorAll('.kanban-card').length;
            var badge = col.querySelector('.kanban-column-header .badge');
            if (badge) badge.textContent = cards;
        });
    }

    // ====== Kanban Global Inline Creation ======

    function closeAllKanbanGlobalInlineForms() {
        document.querySelectorAll('#viewKanban .kanban-inline-form').forEach(function (form) {
            form.classList.add('d-none');
            var input = form.querySelector('.kanban-inline-input');
            if (input) {
                input.value = '';
                input.classList.remove('is-invalid');
            }
            var select = form.querySelector('.kanban-inline-select-projeto');
            if (select) {
                select.selectedIndex = 0;
                select.classList.remove('is-invalid');
            }
        });
        document.querySelectorAll('#viewKanban .kanban-btn-add-task').forEach(function (btn) {
            btn.classList.remove('d-none');
        });
        document.querySelectorAll('#viewKanban .kanban-btn-add-subtask').forEach(function (btn) {
            btn.classList.remove('d-none');
        });
    }

    // "+ Tarefa" button in column footer
    document.addEventListener('click', function (e) {
        var btn = e.target.closest('#viewKanban .kanban-btn-add-task');
        if (!btn) return;
        closeAllKanbanGlobalInlineForms();
        var footer = btn.closest('.kanban-column-footer');
        if (!footer) return;
        var form = footer.querySelector('.kanban-inline-form');
        if (!form) return;
        form.classList.remove('d-none');
        btn.classList.add('d-none');
        var select = form.querySelector('.kanban-inline-select-projeto');
        if (select) select.focus();
    });

    // "+ Subitem" button inside card
    document.addEventListener('click', function (e) {
        var btn = e.target.closest('#viewKanban .kanban-btn-add-subtask');
        if (!btn) return;
        closeAllKanbanGlobalInlineForms();
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
        var btn = e.target.closest('#viewKanban .kanban-inline-btn-cancel');
        if (!btn) return;
        closeAllKanbanGlobalInlineForms();
    });

    // Escape key closes inline forms
    document.addEventListener('keydown', function (e) {
        if (e.key !== 'Escape') return;
        var active = document.activeElement;
        if (active && active.closest('#viewKanban .kanban-inline-form')) {
            closeAllKanbanGlobalInlineForms();
        }
    });

    // Clear is-invalid on typing/change
    document.addEventListener('input', function (e) {
        if (e.target.matches('#viewKanban .kanban-inline-input')) {
            e.target.classList.remove('is-invalid');
        }
    });
    document.addEventListener('change', function (e) {
        if (e.target.matches('#viewKanban .kanban-inline-select-projeto')) {
            e.target.classList.remove('is-invalid');
        }
    });

    // Click outside closes inline forms
    document.addEventListener('click', function (e) {
        if (!e.target.closest('#viewKanban .kanban-inline-form') &&
            !e.target.closest('#viewKanban .kanban-btn-add-task') &&
            !e.target.closest('#viewKanban .kanban-btn-add-subtask')) {
            closeAllKanbanGlobalInlineForms();
        }
    });

    var kanbanGlobalInlineSubmitting = false;

    function submitKanbanGlobalInlineForm(form) {
        if (kanbanGlobalInlineSubmitting) return;

        var input = form.querySelector('.kanban-inline-input');
        var title = input ? input.value.trim() : '';

        var inlineType = form.dataset.inlineType; // "task" or "subtask"
        var projetoId = 0;
        var statusId = 1;
        var tarefaPaiId = null;

        if (inlineType === 'task') {
            var select = form.querySelector('.kanban-inline-select-projeto');
            if (select && !select.value) {
                select.classList.add('is-invalid');
                select.focus();
                return;
            }
            if (select) projetoId = parseInt(select.value, 10);
            var statusAttr = form.dataset.statusId;
            if (statusAttr) statusId = parseInt(statusAttr, 10);
        } else if (inlineType === 'subtask') {
            var card = form.closest('.kanban-card');
            if (card) {
                projetoId = parseInt(card.dataset.projetoId, 10) || 0;
                statusId = parseInt(card.dataset.statusId, 10) || 1;
            }
            var paiAttr = form.dataset.tarefaPaiId;
            if (paiAttr) tarefaPaiId = parseInt(paiAttr, 10);
        }

        if (!title) {
            if (input) { input.classList.add('is-invalid'); input.focus(); }
            return;
        }

        if (!projetoId) return;

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
        kanbanGlobalInlineSubmitting = true;

        fetch('/Projetos/CriarTarefa', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(payload)
        })
        .then(function (res) {
            if (!res.ok) return res.json().then(function (err) { throw err; });
            return res.json();
        })
        .then(function (data) {
            kanbanGlobalInlineSubmitting = false;
            var newId = data.projetoTarefaId;
            var createdEl = null;

            if (inlineType === 'subtask') {
                var card = form.closest('.kanban-card');
                if (card) {
                    var newItem = buildKanbanSubtaskItem({
                        subtaskId: newId,
                        projetoId: projetoId,
                        statusId: statusId,
                        nmTarefa: title,
                        prioridadeId: 2
                    });

                    var section = card.querySelector('.kanban-subtask-section');
                    if (section) {
                        var list = section.querySelector('.kanban-subtask-list');
                        if (list) {
                            list.appendChild(newItem);
                            list.classList.remove('collapsed');
                        }
                        var toggle = section.querySelector('.kanban-subtask-toggle');
                        if (toggle) toggle.setAttribute('aria-expanded', 'true');
                        var allItems = list ? list.querySelectorAll('.kanban-subtask-item').length : 1;
                        var progressSpan = section.querySelector('.kanban-subtask-progress');
                        if (progressSpan) progressSpan.textContent = 'Subtarefas (0/' + allItems + ')';
                        var fill = section.querySelector('.kanban-subtask-progress-fill');
                        if (fill) fill.style.width = '0%';
                    } else {
                        section = document.createElement('div');
                        section.className = 'kanban-subtask-section';

                        var header = document.createElement('div');
                        header.className = 'kanban-subtask-header';
                        header.title = 'Expandir/recolher subtarefas';
                        header.innerHTML = '<button type="button" class="kanban-subtask-toggle" aria-expanded="true" aria-label="Expandir subtarefas"><i class="fa-solid fa-caret-down"></i></button>' +
                            '<span class="kanban-subtask-progress">Subtarefas (0/1)</span>' +
                            '<div class="kanban-subtask-progress-bar"><div class="kanban-subtask-progress-fill" style="width: 0%;"></div></div>';
                        section.appendChild(header);

                        var list = document.createElement('ul');
                        list.className = 'kanban-subtask-list';
                        list.appendChild(newItem);
                        section.appendChild(list);

                        var addBtn = card.querySelector('.kanban-btn-add-subtask');
                        if (addBtn) card.insertBefore(section, addBtn);
                        else card.appendChild(section);
                    }
                    createdEl = newItem;
                }
            } else {
                var select = form.querySelector('.kanban-inline-select-projeto');
                var nmProjeto = select && select.selectedIndex > 0 ? select.options[select.selectedIndex].text : '';

                var newCard = buildKanbanCardElement({
                    tarefaId: newId,
                    projetoId: projetoId,
                    nmTarefa: title,
                    statusId: statusId,
                    prioridadeId: 2,
                    nmProjeto: nmProjeto
                });

                var footer = form.closest('.kanban-column-footer');
                var column = footer ? footer.closest('.kanban-column') : null;
                var body = column ? column.querySelector('.kanban-column-body') : null;
                if (body) {
                    var placeholder = body.querySelector('.kanban-empty-placeholder');
                    if (placeholder) placeholder.remove();
                    body.appendChild(newCard);
                    attachKanbanDragHandlers(newCard);
                }
                updateKanbanGlobalCounters();
                createdEl = newCard;
            }

            closeAllKanbanGlobalInlineForms();
            if (createdEl) flashSuccess(createdEl);
        })
        .catch(function (err) {
            kanbanGlobalInlineSubmitting = false;
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
        var btn = e.target.closest('#viewKanban .kanban-inline-btn-confirm');
        if (!btn) return;
        var form = btn.closest('.kanban-inline-form');
        if (form) submitKanbanGlobalInlineForm(form);
    });

    // Enter key submits
    document.addEventListener('keydown', function (e) {
        if (e.key !== 'Enter') return;
        var input = e.target.closest('#viewKanban .kanban-inline-input');
        if (!input) return;
        e.preventDefault();
        var form = input.closest('.kanban-inline-form');
        if (form) submitKanbanGlobalInlineForm(form);
    });

    // ====== Kanban Global: Subtask Interactions ======

    // Close any open subtask status menu or search popover
    function closeAllSubtaskMenus() {
        document.querySelectorAll('#viewKanban .kanban-subtask-status-menu.show').forEach(function (menu) {
            menu.classList.remove('show');
        });
        document.querySelectorAll('#viewKanban .kanban-subtask-search-popover.show').forEach(function (pop) {
            pop.classList.remove('show');
        });
    }

    // Toggle: expand/collapse subtask list
    document.addEventListener('click', function (e) {
        var header = e.target.closest('#viewKanban .kanban-subtask-header');
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
        var trigger = e.target.closest('#viewKanban button.kanban-subtask-status-trigger');
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

        var board = document.querySelector('#viewKanban .kanban-board-global');
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
        var option = e.target.closest('#viewKanban .kanban-subtask-status-option');
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

        var projetoId = parseInt(item.dataset.projetoId, 10);
        if (!projetoId) return;

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
            item.dataset.subtaskStatusId = newStatusId;
            var info = getKanbanStatusInfo(newStatusId);
            if (trigger && info) {
                trigger.style.color = info.cor;
                trigger.innerHTML = '<span class="kanban-subtask-status-dot" style="background: ' + info.cor + ';"></span>' + info.nome + ' <i class="fa-solid fa-caret-down"></i>';
                item.style.borderLeftColor = info.cor;
            } else if (trigger) {
                trigger.innerHTML = originalTriggerHtml;
            }
            var parentCard = item.closest('.kanban-card');
            updateKanbanCardSubtaskProgress(parentCard);
            flashSuccess(item);
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
    function updateSubtaskResponsavel(item, responsavel, popover, onSuccess) {
        var projetoId = parseInt(item.dataset.projetoId, 10);
        if (!projetoId) return;

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
            if (typeof onSuccess === 'function') {
                onSuccess();
            } else {
                showPageLoading();
                window.location.reload();
            }
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
        var target = e.target.closest('#viewKanban .kanban-subtask-responsavel, #viewKanban .kanban-subtask-btn-responsavel');
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
                updateSubtaskResponsavel(item, null, popover, function () {
                    item.dataset.subtaskResponsavelId = '';
                    item.dataset.subtaskNmResponsavel = '';
                    var actionsDiv = item.querySelector('.kanban-subtask-actions');
                    if (actionsDiv) {
                        var oldEl = actionsDiv.querySelector('.kanban-subtask-responsavel, .kanban-subtask-btn-responsavel');
                        var newEl = buildKanbanSubtaskResponsavelElement(null);
                        if (oldEl) oldEl.parentNode.replaceChild(newEl, oldEl); else actionsDiv.appendChild(newEl);
                    }
                    if (popover) popover.remove();
                    flashSuccess(item);
                });
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
                                var resp = { id: u.usuarioID, nome: u.nmUsuario };
                                updateSubtaskResponsavel(item, resp, popover, function () {
                                    item.dataset.subtaskResponsavelId = String(resp.id);
                                    item.dataset.subtaskNmResponsavel = resp.nome;
                                    var actionsDiv = item.querySelector('.kanban-subtask-actions');
                                    if (actionsDiv) {
                                        var oldEl = actionsDiv.querySelector('.kanban-subtask-responsavel, .kanban-subtask-btn-responsavel');
                                        var newEl = buildKanbanSubtaskResponsavelElement(resp.nome);
                                        if (oldEl) oldEl.parentNode.replaceChild(newEl, oldEl); else actionsDiv.appendChild(newEl);
                                    }
                                    if (popover) popover.remove();
                                    flashSuccess(item);
                                });
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

    // Close all menus when clicking outside
    document.addEventListener('click', function (e) {
        if (!e.target.closest('.kanban-subtask-status-menu') &&
            !e.target.closest('.kanban-subtask-status-trigger') &&
            !e.target.closest('.kanban-subtask-search-popover') &&
            !e.target.closest('.kanban-subtask-responsavel') &&
            !e.target.closest('.kanban-subtask-btn-responsavel')) {
            closeAllSubtaskMenus();
        }
        if (!e.target.closest('.kanban-card-menu') &&
            !e.target.closest('.kanban-card-menu-trigger')) {
            closeAllCardMenus();
        }
        if (!e.target.closest('.kanban-card-prioridade-menu') &&
            !e.target.closest('.kanban-card-prioridade-trigger')) {
            closeAllCardPriorityMenus();
        }
        if (!e.target.closest('.kanban-card-search-popover') &&
            !e.target.closest('.kanban-card-responsavel') &&
            !e.target.closest('.kanban-card-btn-responsavel')) {
            closeAllCardSearchPopovers();
        }
    });

    // Escape closes all menus
    document.addEventListener('keydown', function (e) {
        if (e.key === 'Escape') {
            closeAllSubtaskMenus();
            closeAllCardInteractions();
        }
    });

    // ====== Modo Lista: Ações Interativas ======

    function closeAllListNameEdits() {
        document.querySelectorAll('#viewLista .projeto-list-nome-input').forEach(function (inp) {
            var wrapper = inp.closest('.projeto-list-nome-editing');
            if (!wrapper) { inp.remove(); return; }
            var origSpan = wrapper._origNameSpan;
            if (origSpan) {
                wrapper.parentNode.insertBefore(origSpan, wrapper);
            }
            wrapper.remove();
        });
    }

    function closeAllListDatePickers() {
        document.querySelectorAll('#viewLista .projeto-list-date-input').forEach(function (inp) {
            var wrapper = inp.closest('.projeto-list-date-editing');
            if (!wrapper) { inp.remove(); return; }
            var origSpan = wrapper._origDateSpan;
            if (origSpan) {
                wrapper.parentNode.insertBefore(origSpan, wrapper);
            }
            wrapper.remove();
        });
    }

    function closeAllListMenus() {
        document.querySelectorAll('#viewLista .kanban-subtask-status-menu.show').forEach(function (m) { m.classList.remove('show'); });
        document.querySelectorAll('#viewLista .kanban-card-prioridade-menu.show').forEach(function (m) { m.classList.remove('show'); });
        document.querySelectorAll('#viewLista .kanban-subtask-search-popover.show').forEach(function (p) { p.classList.remove('show'); });
        closeAllListNameEdits();
        closeAllListDatePickers();
    }

    function closeAllListInlineForms() {
        document.querySelectorAll('#viewLista .projeto-list-inline-form').forEach(function (f) {
            f.classList.add('d-none');
            var inp = f.querySelector('.projeto-list-inline-input');
            if (inp) { inp.value = ''; inp.classList.remove('is-invalid'); }
        });
        document.querySelectorAll('#viewLista .projeto-list-btn-add-subtask').forEach(function (b) {
            b.classList.remove('d-none');
        });
    }

    function buildListSubtaskPayload(item, overrides) {
        var payload = {
            ProjetoTarefaID: parseInt(item.dataset.subtaskId, 10),
            ProjetoID: parseInt(item.dataset.projetoId, 10),
            NmTarefa: item.dataset.subtaskNmTarefa,
            DsTarefa: item.dataset.subtaskDsTarefa || null,
            ProjetoTarefaStatusID: parseInt(item.dataset.subtaskStatusId, 10),
            ProjetoTarefaPrioridadeID: parseInt(item.dataset.subtaskPrioridadeId, 10),
            DtInicio: brDateToIso(item.dataset.subtaskDtInicio) || null,
            DtPrevisaoFim: brDateToIso(item.dataset.subtaskDtPrevisaoFim) || null,
            DtFimReal: brDateToIso(item.dataset.subtaskDtFimReal) || null,
            UsuarioResponsavelID: item.dataset.subtaskResponsavelId ? parseInt(item.dataset.subtaskResponsavelId, 10) : null
        };
        if (overrides) { for (var k in overrides) { payload[k] = overrides[k]; } }
        return payload;
    }

    // Lista: Tarefa — Status dropdown (abrir)
    document.addEventListener('click', function (e) {
        var trigger = e.target.closest('#viewLista .projeto-list-status-trigger');
        if (!trigger) return;
        if (e.target.closest('.kanban-subtask-status-option')) return;
        e.stopPropagation();

        var tarefa = trigger.closest('.projeto-list-tarefa');
        if (!tarefa) return;

        var existing = trigger.querySelector('.kanban-subtask-status-menu');
        if (existing && existing.classList.contains('show')) { existing.classList.remove('show'); return; }

        closeAllListMenus();
        if (existing) existing.remove();

        var statusList = [];
        try { statusList = JSON.parse(document.getElementById('viewLista').dataset.statusList); } catch (ex) { /* ignore */ }

        var currentId = parseInt(tarefa.dataset.statusId, 10);
        var menu = document.createElement('div');
        menu.className = 'kanban-subtask-status-menu show';
        menu.style.top = '100%';
        menu.style.left = '0';
        menu.style.marginTop = '0.15rem';

        statusList.forEach(function (s) {
            var btn = document.createElement('button');
            btn.type = 'button';
            btn.className = 'kanban-subtask-status-option' + (s.id === currentId ? ' active' : '');
            btn.dataset.statusId = s.id;
            btn.innerHTML = '<span class="kanban-subtask-status-dot" style="background:' + s.cor + ';"></span>' + s.nome;
            menu.appendChild(btn);
        });

        trigger.appendChild(menu);
    });

    // Lista: Tarefa — selecionar status
    document.addEventListener('click', function (e) {
        var option = e.target.closest('#viewLista .kanban-subtask-status-option');
        if (!option || option.closest('.projeto-list-subtarefa')) return;
        e.stopPropagation();

        var tarefa = option.closest('.projeto-list-tarefa');
        if (!tarefa) return;

        var newId = parseInt(option.dataset.statusId, 10);
        var curId = parseInt(tarefa.dataset.statusId, 10);
        var menu = option.closest('.kanban-subtask-status-menu');
        if (newId === curId) { if (menu) menu.classList.remove('show'); return; }

        var payload = buildCardPayload(tarefa, { ProjetoTarefaStatusID: newId });
        var trigger = tarefa.querySelector('.projeto-list-status-trigger');
        var origHtml = trigger ? trigger.innerHTML : '';
        if (trigger) trigger.innerHTML = '<i class="fa-solid fa-spinner fa-spin" style="font-size:0.55rem;"></i>';
        if (menu) menu.classList.remove('show');

        fetch('/Projetos/EditarTarefa', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(payload)
        })
        .then(function (r) { if (!r.ok) return r.json().then(function (err) { throw err; }); return r.json(); })
        .then(function () {
            tarefa.dataset.statusId = newId;
            var info = getListStatusInfo(newId);
            if (trigger && info) {
                trigger.style.background = 'color-mix(in srgb, ' + info.cor + ' 18%, transparent)';
                trigger.style.color = info.cor;
                trigger.innerHTML = '<i class="fa-solid fa-circle" style="font-size: 0.4rem;"></i> ' + info.nome + ' <i class="fa-solid fa-caret-down projeto-list-trigger-caret"></i>';
            } else if (trigger) {
                trigger.innerHTML = origHtml;
            }
            flashSuccess(tarefa);
        })
        .catch(function (err) {
            if (trigger) trigger.innerHTML = origHtml;
            Swal.fire({ icon: 'error', title: 'Erro', text: (err && err.mensagem) || 'Não foi possível atualizar o status.' });
        });
    });

    // Lista: Tarefa — Prioridade dropdown (abrir)
    document.addEventListener('click', function (e) {
        var trigger = e.target.closest('#viewLista .projeto-list-prioridade-trigger');
        if (!trigger) return;
        if (e.target.closest('.kanban-card-prioridade-option')) return;
        e.stopPropagation();

        var tarefa = trigger.closest('.projeto-list-tarefa');
        if (!tarefa) return;

        var existing = trigger.querySelector('.kanban-card-prioridade-menu');
        if (existing && existing.classList.contains('show')) { existing.classList.remove('show'); return; }

        closeAllListMenus();
        if (existing) existing.remove();

        var prioridadeList = [];
        try { prioridadeList = JSON.parse(document.getElementById('viewLista').dataset.prioridadeList); } catch (ex) { /* ignore */ }

        var currentId = parseInt(tarefa.dataset.prioridadeId, 10);
        var menu = document.createElement('div');
        menu.className = 'kanban-card-prioridade-menu show';
        menu.style.top = '100%';
        menu.style.left = '0';
        menu.style.marginTop = '0.15rem';

        prioridadeList.forEach(function (p) {
            var btn = document.createElement('button');
            btn.type = 'button';
            btn.className = 'kanban-card-prioridade-option' + (p.id === currentId ? ' active' : '');
            btn.dataset.prioridadeId = p.id;
            btn.innerHTML = '<span class="kanban-card-prioridade-dot" style="background:' + p.cor + ';"></span>' + p.nome;
            menu.appendChild(btn);
        });

        trigger.appendChild(menu);
    });

    // Lista: Tarefa — selecionar prioridade
    document.addEventListener('click', function (e) {
        var option = e.target.closest('#viewLista .kanban-card-prioridade-option');
        if (!option) return;
        e.stopPropagation();

        var tarefa = option.closest('.projeto-list-tarefa');
        if (!tarefa) return;

        var newId = parseInt(option.dataset.prioridadeId, 10);
        var curId = parseInt(tarefa.dataset.prioridadeId, 10);
        var menu = option.closest('.kanban-card-prioridade-menu');
        if (newId === curId) { if (menu) menu.classList.remove('show'); return; }

        var payload = buildCardPayload(tarefa, { ProjetoTarefaPrioridadeID: newId });
        var trigger = tarefa.querySelector('.projeto-list-prioridade-trigger');
        var origHtml = trigger ? trigger.innerHTML : '';
        if (trigger) trigger.innerHTML = '<i class="fa-solid fa-spinner fa-spin" style="font-size:0.55rem;"></i>';
        if (menu) menu.classList.remove('show');

        fetch('/Projetos/EditarTarefa', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(payload)
        })
        .then(function (r) { if (!r.ok) return r.json().then(function (err) { throw err; }); return r.json(); })
        .then(function () {
            tarefa.dataset.prioridadeId = newId;
            var info = getListPrioridadeInfo(newId);
            if (trigger && info) {
                trigger.style.background = 'color-mix(in srgb, ' + info.cor + ' 18%, transparent)';
                trigger.style.color = info.cor;
                trigger.innerHTML = '<i class="fa-solid fa-flag" style="font-size: 0.5rem;"></i> ' + info.nome + ' <i class="fa-solid fa-caret-down projeto-list-trigger-caret"></i>';
            } else if (trigger) {
                trigger.innerHTML = origHtml;
            }
            flashSuccess(tarefa);
        })
        .catch(function (err) {
            if (trigger) trigger.innerHTML = origHtml;
            Swal.fire({ icon: 'error', title: 'Erro', text: (err && err.mensagem) || 'Não foi possível atualizar a prioridade.' });
        });
    });

    // Lista: Tarefa — Responsável popover (abrir)
    document.addEventListener('click', function (e) {
        var target = e.target.closest('#viewLista .projeto-list-responsavel, #viewLista .projeto-list-btn-responsavel');
        if (!target) return;
        e.stopPropagation();

        var tarefa = target.closest('.projeto-list-tarefa');
        if (!tarefa) return;

        var existing = tarefa.querySelector('.kanban-subtask-search-popover');
        if (existing && existing.classList.contains('show')) { existing.classList.remove('show'); return; }

        closeAllListMenus();
        if (existing) existing.remove();

        var popover = document.createElement('div');
        popover.className = 'kanban-subtask-search-popover show';
        popover.style.top = '100%';
        popover.style.right = '0';
        popover.style.marginTop = '0.15rem';

        var searchInput = document.createElement('input');
        searchInput.type = 'text';
        searchInput.className = 'kanban-subtask-search-input';
        searchInput.placeholder = 'Buscar usuário...';
        searchInput.setAttribute('autocomplete', 'off');
        popover.appendChild(searchInput);

        var resultsList = document.createElement('ul');
        resultsList.className = 'kanban-subtask-search-results';
        popover.appendChild(resultsList);

        if (tarefa.dataset.responsavelId) {
            var removeBtn = document.createElement('button');
            removeBtn.type = 'button';
            removeBtn.className = 'kanban-subtask-search-btn-remove';
            removeBtn.innerHTML = '<i class="fa-solid fa-user-xmark"></i> Remover responsável';
            removeBtn.addEventListener('click', function (ev) {
                ev.stopPropagation();
                updateCardResponsavel(tarefa, null, popover, function () {
                    tarefa.dataset.responsavelId = '';
                    tarefa.dataset.nmResponsavel = '';
                    var infoDiv = tarefa.querySelector('.projeto-list-tarefa-info');
                    if (infoDiv) {
                        var oldEl = infoDiv.querySelector('.projeto-list-responsavel, .projeto-list-btn-responsavel');
                        var newEl = buildResponsavelElement(null, false);
                        if (oldEl) oldEl.parentNode.replaceChild(newEl, oldEl); else infoDiv.appendChild(newEl);
                    }
                    if (popover) popover.remove();
                    flashSuccess(tarefa);
                });
            });
            popover.appendChild(removeBtn);
        }

        var actionsRow = target.closest('.ms-auto');
        if (actionsRow) {
            actionsRow.style.position = 'relative';
            actionsRow.appendChild(popover);
        } else {
            tarefa.style.position = 'relative';
            tarefa.appendChild(popover);
        }
        searchInput.focus();

        var timer = null;
        searchInput.addEventListener('input', function () {
            var texto = searchInput.value.trim();
            if (texto.length < 2) { resultsList.innerHTML = ''; return; }
            clearTimeout(timer);
            timer = setTimeout(function () {
                fetch('/Projetos/BuscarUsuarios?texto=' + encodeURIComponent(texto))
                    .then(function (r) { return r.json(); })
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
                                var resp = { id: u.usuarioID, nome: u.nmUsuario };
                                updateCardResponsavel(tarefa, resp, popover, function () {
                                    tarefa.dataset.responsavelId = String(resp.id);
                                    tarefa.dataset.nmResponsavel = resp.nome;
                                    var infoDiv = tarefa.querySelector('.projeto-list-tarefa-info');
                                    if (infoDiv) {
                                        var oldEl = infoDiv.querySelector('.projeto-list-responsavel, .projeto-list-btn-responsavel');
                                        var newEl = buildResponsavelElement(resp.nome, false);
                                        if (oldEl) oldEl.parentNode.replaceChild(newEl, oldEl); else infoDiv.appendChild(newEl);
                                    }
                                    if (popover) popover.remove();
                                    flashSuccess(tarefa);
                                });
                            });
                            resultsList.appendChild(li);
                        });
                    })
                    .catch(function () { resultsList.innerHTML = ''; });
            }, 300);
        });
    });

    // Lista: Subtarefa — Status dropdown (abrir)
    document.addEventListener('click', function (e) {
        var trigger = e.target.closest('#viewLista .projeto-list-subtarefa-status-trigger');
        if (!trigger) return;
        if (e.target.closest('.kanban-subtask-status-option')) return;
        e.stopPropagation();

        var sub = trigger.closest('.projeto-list-subtarefa');
        if (!sub) return;

        var existing = sub.querySelector('.kanban-subtask-status-menu');
        if (existing && existing.classList.contains('show')) { existing.classList.remove('show'); return; }

        closeAllListMenus();
        if (existing) existing.remove();

        var statusList = [];
        try { statusList = JSON.parse(document.getElementById('viewLista').dataset.statusList); } catch (ex) { /* ignore */ }

        var currentId = parseInt(sub.dataset.subtaskStatusId, 10);
        var menu = document.createElement('div');
        menu.className = 'kanban-subtask-status-menu show';
        menu.style.top = '100%';
        menu.style.right = '0';
        menu.style.marginTop = '0.15rem';

        statusList.forEach(function (s) {
            var btn = document.createElement('button');
            btn.type = 'button';
            btn.className = 'kanban-subtask-status-option' + (s.id === currentId ? ' active' : '');
            btn.dataset.statusId = s.id;
            btn.innerHTML = '<span class="kanban-subtask-status-dot" style="background:' + s.cor + ';"></span>' + s.nome;
            menu.appendChild(btn);
        });

        sub.style.position = 'relative';
        sub.appendChild(menu);
    });

    // Lista: Subtarefa — selecionar status
    document.addEventListener('click', function (e) {
        var option = e.target.closest('#viewLista .projeto-list-subtarefa .kanban-subtask-status-option');
        if (!option) return;
        e.stopPropagation();

        var sub = option.closest('.projeto-list-subtarefa');
        if (!sub) return;

        var newId = parseInt(option.dataset.statusId, 10);
        var curId = parseInt(sub.dataset.subtaskStatusId, 10);
        var menu = option.closest('.kanban-subtask-status-menu');
        if (newId === curId) { if (menu) menu.classList.remove('show'); return; }

        var payload = buildListSubtaskPayload(sub, { ProjetoTarefaStatusID: newId });
        var trigger = sub.querySelector('.projeto-list-subtarefa-status-trigger');
        var origHtml = trigger ? trigger.innerHTML : '';
        if (trigger) trigger.innerHTML = '<i class="fa-solid fa-spinner fa-spin" style="font-size:0.55rem;"></i>';
        if (menu) menu.classList.remove('show');

        fetch('/Projetos/EditarTarefa', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(payload)
        })
        .then(function (r) { if (!r.ok) return r.json().then(function (err) { throw err; }); return r.json(); })
            .then(function () {
                sub.dataset.subtaskStatusId = newId;
                var info = getListStatusInfo(newId);
                if (trigger && info) {
                    trigger.style.color = info.cor;
                    trigger.innerHTML = '<span class="projeto-list-subtarefa-status-dot" style="background: ' + info.cor + ';"></span> ' + info.nome + ' <i class="fa-solid fa-caret-down"></i>';
                } else if (trigger) {
                    trigger.innerHTML = origHtml;
                }
                flashSuccess(sub);
            })
        .catch(function (err) {
            if (trigger) trigger.innerHTML = origHtml;
            Swal.fire({ icon: 'error', title: 'Erro', text: (err && err.mensagem) || 'Não foi possível atualizar o status da subtarefa.' });
        });
    });

    // Lista: Subtarefa — Responsável popover (abrir)
    document.addEventListener('click', function (e) {
        var target = e.target.closest('#viewLista .projeto-list-subtarefa-responsavel, #viewLista .projeto-list-subtarefa-btn-responsavel');
        if (!target) return;
        e.stopPropagation();

        var sub = target.closest('.projeto-list-subtarefa');
        if (!sub) return;

        var existing = sub.querySelector('.kanban-subtask-search-popover');
        if (existing && existing.classList.contains('show')) { existing.classList.remove('show'); return; }

        closeAllListMenus();
        if (existing) existing.remove();

        var popover = document.createElement('div');
        popover.className = 'kanban-subtask-search-popover show';
        popover.style.top = '100%';
        popover.style.right = '0';
        popover.style.marginTop = '0.15rem';

        var searchInput = document.createElement('input');
        searchInput.type = 'text';
        searchInput.className = 'kanban-subtask-search-input';
        searchInput.placeholder = 'Buscar usuário...';
        searchInput.setAttribute('autocomplete', 'off');
        popover.appendChild(searchInput);

        var resultsList = document.createElement('ul');
        resultsList.className = 'kanban-subtask-search-results';
        popover.appendChild(resultsList);

        if (sub.dataset.subtaskResponsavelId) {
            var removeBtn = document.createElement('button');
            removeBtn.type = 'button';
            removeBtn.className = 'kanban-subtask-search-btn-remove';
            removeBtn.innerHTML = '<i class="fa-solid fa-user-xmark"></i> Remover responsável';
            removeBtn.addEventListener('click', function (ev) {
                ev.stopPropagation();
                updateSubtaskResponsavel(sub, null, popover, function () {
                    sub.dataset.subtaskResponsavelId = '';
                    sub.dataset.subtaskNmResponsavel = '';
                    var metaDiv = sub.querySelector('.projeto-list-subtarefa-meta');
                    if (metaDiv) {
                        var oldEl = metaDiv.querySelector('.projeto-list-subtarefa-responsavel, .projeto-list-subtarefa-btn-responsavel');
                        var newEl = buildResponsavelElement(null, true);
                        if (oldEl) oldEl.parentNode.replaceChild(newEl, oldEl); else metaDiv.appendChild(newEl);
                    }
                    if (popover) popover.remove();
                    flashSuccess(sub);
                });
            });
            popover.appendChild(removeBtn);
        }

        sub.style.position = 'relative';
        sub.appendChild(popover);
        searchInput.focus();

        var timer = null;
        searchInput.addEventListener('input', function () {
            var texto = searchInput.value.trim();
            if (texto.length < 2) { resultsList.innerHTML = ''; return; }
            clearTimeout(timer);
            timer = setTimeout(function () {
                fetch('/Projetos/BuscarUsuarios?texto=' + encodeURIComponent(texto))
                    .then(function (r) { return r.json(); })
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
                                var resp = { id: u.usuarioID, nome: u.nmUsuario };
                                updateSubtaskResponsavel(sub, resp, popover, function () {
                                    sub.dataset.subtaskResponsavelId = String(resp.id);
                                    sub.dataset.subtaskNmResponsavel = resp.nome;
                                    var metaDiv = sub.querySelector('.projeto-list-subtarefa-meta');
                                    if (metaDiv) {
                                        var oldEl = metaDiv.querySelector('.projeto-list-subtarefa-responsavel, .projeto-list-subtarefa-btn-responsavel');
                                        var newEl = buildResponsavelElement(resp.nome, true);
                                        if (oldEl) oldEl.parentNode.replaceChild(newEl, oldEl); else metaDiv.appendChild(newEl);
                                    }
                                    if (popover) popover.remove();
                                    flashSuccess(sub);
                                });
                            });
                            resultsList.appendChild(li);
                        });
                    })
                    .catch(function () { resultsList.innerHTML = ''; });
            }, 300);
        });
    });

    // ====== Lista: Edição inline de prazo — Tarefa ======
    document.addEventListener('click', function (e) {
        var dateSpan = e.target.closest('#viewLista .projeto-list-tarefa-info > .projeto-list-date');
        if (!dateSpan) return;
        if (e.target.closest('.projeto-list-date-editing')) return;
        e.stopImmediatePropagation();

        var tarefa = dateSpan.closest('.projeto-list-tarefa');
        if (!tarefa) return;

        closeAllListDatePickers();

        var currentBr = tarefa.dataset.dtPrevisaoFim || '';
        var currentIso = brDateToIso(currentBr) || '';

        var wrapper = document.createElement('span');
        wrapper.className = 'projeto-list-date-editing';
        wrapper._origDateSpan = dateSpan;

        var input = document.createElement('input');
        input.type = 'date';
        input.className = 'projeto-list-date-input';
        input.value = currentIso;
        wrapper.appendChild(input);

        dateSpan.parentNode.insertBefore(wrapper, dateSpan);
        dateSpan.remove();
        input.focus();
        try { input.showPicker(); } catch (_) { /* not supported in all browsers */ }

        input.addEventListener('change', function () {
            var newIso = input.value || null;
            var payload = buildCardPayload(tarefa, { DtPrevisaoFim: newIso });

            wrapper.innerHTML = '<i class="fa-solid fa-spinner fa-spin" style="font-size:0.6rem;color:var(--sic-muted);"></i>';

            fetch('/Projetos/EditarTarefa', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify(payload)
            })
            .then(function (r) { if (!r.ok) return r.json().then(function (err) { throw err; }); return r.json(); })
            .then(function () {
                var brDate = newIso ? isoToBrDate(newIso) : '';
                tarefa.dataset.dtPrevisaoFim = brDate;
                var newSpan = buildDateSpan(newIso, false);
                wrapper.parentNode.insertBefore(newSpan, wrapper);
                wrapper.remove();
                flashSuccess(tarefa);
            })
            .catch(function (err) {
                if (wrapper._origDateSpan) {
                    wrapper.parentNode.insertBefore(wrapper._origDateSpan, wrapper);
                }
                wrapper.remove();
                Swal.fire({ icon: 'error', title: 'Erro', text: (err && err.mensagem) || 'Não foi possível alterar o prazo.' });
            });
        });

        input.addEventListener('keydown', function (ev) {
            if (ev.key === 'Escape') {
                ev.stopPropagation();
                closeAllListDatePickers();
            }
        });
    });

    // ====== Lista: Edição inline de prazo — Subtarefa ======
    document.addEventListener('click', function (e) {
        var dateSpan = e.target.closest('#viewLista .projeto-list-subtarefa-meta > .projeto-list-date');
        if (!dateSpan) return;
        if (e.target.closest('.projeto-list-date-editing')) return;
        e.stopImmediatePropagation();

        var sub = dateSpan.closest('.projeto-list-subtarefa');
        if (!sub) return;

        closeAllListDatePickers();

        var currentBr = sub.dataset.subtaskDtPrevisaoFim || '';
        var currentIso = brDateToIso(currentBr) || '';

        var wrapper = document.createElement('span');
        wrapper.className = 'projeto-list-date-editing';
        wrapper._origDateSpan = dateSpan;

        var input = document.createElement('input');
        input.type = 'date';
        input.className = 'projeto-list-date-input projeto-list-date-input-sub';
        input.value = currentIso;
        wrapper.appendChild(input);

        dateSpan.parentNode.insertBefore(wrapper, dateSpan);
        dateSpan.remove();
        input.focus();
        try { input.showPicker(); } catch (_) { /* not supported in all browsers */ }

        input.addEventListener('change', function () {
            var newIso = input.value || null;
            var payload = buildListSubtaskPayload(sub, { DtPrevisaoFim: newIso });

            wrapper.innerHTML = '<i class="fa-solid fa-spinner fa-spin" style="font-size:0.5rem;color:var(--sic-muted);"></i>';

            fetch('/Projetos/EditarTarefa', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify(payload)
            })
            .then(function (r) { if (!r.ok) return r.json().then(function (err) { throw err; }); return r.json(); })
            .then(function () {
                var brDate = newIso ? isoToBrDate(newIso) : '';
                sub.dataset.subtaskDtPrevisaoFim = brDate;
                var newSpan = buildDateSpan(newIso, true);
                wrapper.parentNode.insertBefore(newSpan, wrapper);
                wrapper.remove();
                flashSuccess(sub);
            })
            .catch(function (err) {
                if (wrapper._origDateSpan) {
                    wrapper.parentNode.insertBefore(wrapper._origDateSpan, wrapper);
                }
                wrapper.remove();
                Swal.fire({ icon: 'error', title: 'Erro', text: (err && err.mensagem) || 'Não foi possível alterar o prazo.' });
            });
        });

        input.addEventListener('keydown', function (ev) {
            if (ev.key === 'Escape') {
                ev.stopPropagation();
                closeAllListDatePickers();
            }
        });
    });

    // ====== Lista: Edição inline de nome — Tarefa ======
    document.addEventListener('click', function (e) {
        var nomeSpan = e.target.closest('#viewLista .projeto-list-tarefa-nome');
        if (!nomeSpan) return;
        if (e.target.closest('.projeto-list-nome-editing')) return;
        e.stopImmediatePropagation();

        var tarefa = nomeSpan.closest('.projeto-list-tarefa');
        if (!tarefa) return;

        closeAllListNameEdits();
        closeAllListDatePickers();

        var currentName = tarefa.dataset.nmTarefa || '';

        var wrapper = document.createElement('span');
        wrapper.className = 'projeto-list-nome-editing';
        wrapper._origNameSpan = nomeSpan;

        var input = document.createElement('input');
        input.type = 'text';
        input.className = 'projeto-list-nome-input';
        input.value = currentName;
        input.maxLength = 200;
        wrapper.appendChild(input);

        nomeSpan.parentNode.insertBefore(wrapper, nomeSpan);
        nomeSpan.remove();
        input.focus();
        input.select();

        var saving = false;
        function saveTaskName() {
            if (saving) return;
            var newName = input.value.trim();
            if (!newName) {
                closeAllListNameEdits();
                return;
            }
            if (newName === currentName) {
                closeAllListNameEdits();
                return;
            }
            saving = true;
            var payload = buildCardPayload(tarefa, { NmTarefa: newName });

            wrapper.innerHTML = '<i class="fa-solid fa-spinner fa-spin" style="font-size:0.7rem;color:var(--sic-muted);"></i>';

            fetch('/Projetos/EditarTarefa', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify(payload)
            })
            .then(function (r) { if (!r.ok) return r.json().then(function (err) { throw err; }); return r.json(); })
            .then(function () {
                tarefa.dataset.nmTarefa = newName;
                if (wrapper.parentNode) {
                    var newSpan = document.createElement('span');
                    newSpan.className = 'projeto-list-tarefa-nome';
                    newSpan.textContent = newName;
                    wrapper.parentNode.insertBefore(newSpan, wrapper);
                    wrapper.remove();
                } else {
                    var existingSpan = tarefa.querySelector('.projeto-list-tarefa-nome');
                    if (existingSpan) existingSpan.textContent = newName;
                }
                flashSuccess(tarefa);
            })
            .catch(function (err) {
                saving = false;
                if (wrapper.parentNode) {
                    if (wrapper._origNameSpan) {
                        wrapper.parentNode.insertBefore(wrapper._origNameSpan, wrapper);
                    }
                    wrapper.remove();
                }
                Swal.fire({ icon: 'error', title: 'Erro', text: (err && err.mensagem) || 'Não foi possível alterar o nome da tarefa.' });
            });
        }

        input.addEventListener('blur', function () {
            setTimeout(function () { if (!saving) saveTaskName(); }, 150);
        });
        input.addEventListener('keydown', function (ev) {
            if (ev.key === 'Enter') { ev.preventDefault(); saveTaskName(); }
            if (ev.key === 'Escape') { ev.stopPropagation(); closeAllListNameEdits(); }
        });
    });

    // ====== Lista: Edição inline de nome — Subtarefa ======
    document.addEventListener('click', function (e) {
        var nomeSpan = e.target.closest('#viewLista .projeto-list-subtarefa-nome');
        if (!nomeSpan) return;
        if (e.target.closest('.projeto-list-nome-editing')) return;
        e.stopImmediatePropagation();

        var sub = nomeSpan.closest('.projeto-list-subtarefa');
        if (!sub) return;

        closeAllListNameEdits();
        closeAllListDatePickers();

        var currentName = sub.dataset.subtaskNmTarefa || '';

        var wrapper = document.createElement('span');
        wrapper.className = 'projeto-list-nome-editing';
        wrapper._origNameSpan = nomeSpan;

        var input = document.createElement('input');
        input.type = 'text';
        input.className = 'projeto-list-nome-input projeto-list-nome-input-sub';
        input.value = currentName;
        input.maxLength = 200;
        wrapper.appendChild(input);

        nomeSpan.parentNode.insertBefore(wrapper, nomeSpan);
        nomeSpan.remove();
        input.focus();
        input.select();

        var saving = false;
        function saveSubtaskName() {
            if (saving) return;
            var newName = input.value.trim();
            if (!newName) {
                closeAllListNameEdits();
                return;
            }
            if (newName === currentName) {
                closeAllListNameEdits();
                return;
            }
            saving = true;
            var payload = buildListSubtaskPayload(sub, { NmTarefa: newName });

            wrapper.innerHTML = '<i class="fa-solid fa-spinner fa-spin" style="font-size:0.6rem;color:var(--sic-muted);"></i>';

            fetch('/Projetos/EditarTarefa', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify(payload)
            })
            .then(function (r) { if (!r.ok) return r.json().then(function (err) { throw err; }); return r.json(); })
            .then(function () {
                sub.dataset.subtaskNmTarefa = newName;
                if (wrapper.parentNode) {
                    var newSpan = document.createElement('span');
                    newSpan.className = 'projeto-list-subtarefa-nome';
                    newSpan.textContent = newName;
                    wrapper.parentNode.insertBefore(newSpan, wrapper);
                    wrapper.remove();
                } else {
                    var existingSpan = sub.querySelector('.projeto-list-subtarefa-nome');
                    if (existingSpan) existingSpan.textContent = newName;
                }
                flashSuccess(sub);
            })
            .catch(function (err) {
                saving = false;
                if (wrapper.parentNode) {
                    if (wrapper._origNameSpan) {
                        wrapper.parentNode.insertBefore(wrapper._origNameSpan, wrapper);
                    }
                    wrapper.remove();
                }
                Swal.fire({ icon: 'error', title: 'Erro', text: (err && err.mensagem) || 'Não foi possível alterar o nome da subtarefa.' });
            });
        }

        input.addEventListener('blur', function () {
            setTimeout(function () { if (!saving) saveSubtaskName(); }, 150);
        });
        input.addEventListener('keydown', function (ev) {
            if (ev.key === 'Enter') { ev.preventDefault(); saveSubtaskName(); }
            if (ev.key === 'Escape') { ev.stopPropagation(); closeAllListNameEdits(); }
        });
    });

    // Lista: Criação rápida de subtarefa — abrir formulário
    document.addEventListener('click', function (e) {
        var btn = e.target.closest('#viewLista .projeto-list-btn-add-subtask');
        if (!btn) return;
        closeAllListMenus();
        closeAllListInlineForms();
        var tarefa = btn.closest('.projeto-list-tarefa');
        if (!tarefa) return;
        var form = tarefa.querySelector('.projeto-list-inline-form');
        if (!form) return;
        form.classList.remove('d-none');
        btn.classList.add('d-none');
        var input = form.querySelector('.projeto-list-inline-input');
        if (input) input.focus();
    });

    // Lista: Criação rápida — cancelar
    document.addEventListener('click', function (e) {
        if (e.target.closest('#viewLista .projeto-list-inline-btn-cancel')) {
            closeAllListInlineForms();
        }
    });

    // Lista: Criação rápida — limpar validação ao digitar
    document.addEventListener('input', function (e) {
        if (e.target.matches('#viewLista .projeto-list-inline-input')) {
            e.target.classList.remove('is-invalid');
        }
    });

    var listInlineSubmitting = false;

    function submitListInlineForm(form) {
        if (listInlineSubmitting) return;

        var input = form.querySelector('.projeto-list-inline-input');
        var title = input ? input.value.trim() : '';
        if (!title) {
            if (input) { input.classList.add('is-invalid'); input.focus(); }
            return;
        }

        var projetoId = parseInt(form.dataset.projetoId, 10) || 0;
        var tarefaPaiId = parseInt(form.dataset.tarefaPaiId, 10) || null;
        var tarefa = form.closest('.projeto-list-tarefa');
        var statusId = tarefa ? (parseInt(tarefa.dataset.statusId, 10) || 1) : 1;

        if (!projetoId) return;

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

        var confirmBtn = form.querySelector('.projeto-list-inline-btn-confirm');
        if (confirmBtn) { confirmBtn.disabled = true; confirmBtn.innerHTML = '<i class="fa-solid fa-spinner fa-spin"></i>'; }
        if (input) input.readOnly = true;
        listInlineSubmitting = true;

        fetch('/Projetos/CriarTarefa', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(payload)
        })
        .then(function (r) { if (!r.ok) return r.json().then(function (err) { throw err; }); return r.json(); })
        .then(function (data) {
            listInlineSubmitting = false;
            var newId = data.projetoTarefaId;

            var newSub = buildListSubtarefaElement({
                subtaskId: newId,
                projetoId: projetoId,
                statusId: statusId,
                nmTarefa: title,
                prioridadeId: 2
            });

            var containerId = 'projeto-subtarefas-' + tarefaPaiId;
            var container = document.getElementById(containerId);
            if (container) {
                container.appendChild(newSub);
                container.classList.remove('d-none');
            } else {
                container = document.createElement('div');
                container.className = 'projeto-list-subtarefas';
                container.id = containerId;
                container.appendChild(newSub);

                var content = tarefa.querySelector('.projeto-list-tarefa-content');
                if (content) content.after(container);

                var spacer = tarefa.querySelector('.projeto-list-spacer-sm');
                if (spacer) {
                    var toggleBtn = document.createElement('button');
                    toggleBtn.type = 'button';
                    toggleBtn.className = 'btn btn-sm btn-link p-0 projeto-list-toggle';
                    toggleBtn.dataset.target = containerId;
                    toggleBtn.title = 'Expandir subtarefas (1)';
                    toggleBtn.innerHTML = '<i class="fa-solid fa-chevron-right text-muted projeto-list-chevron projeto-list-chevron-sm rotated"></i>';
                    spacer.parentNode.replaceChild(toggleBtn, spacer);
                }
            }

            var allSubs = container.querySelectorAll('.projeto-list-subtarefa');
            var existingToggle = tarefa.querySelector('.projeto-list-toggle[data-target="' + containerId + '"]');
            if (existingToggle) existingToggle.title = 'Expandir subtarefas (' + allSubs.length + ')';

            var chevron = existingToggle ? existingToggle.querySelector('.projeto-list-chevron') : null;
            if (chevron) chevron.classList.add('rotated');
            tarefa.classList.add('expanded');

            closeAllListInlineForms();
            flashSuccess(newSub);
        })
        .catch(function (err) {
            listInlineSubmitting = false;
            if (confirmBtn) { confirmBtn.disabled = false; confirmBtn.innerHTML = '<i class="fa-solid fa-check"></i> Criar'; }
            if (input) input.readOnly = false;
            Swal.fire({ icon: 'error', title: 'Erro', text: (err && err.mensagem) || 'Não foi possível criar a subtarefa.' });
        });
    }

    // Lista: Criação rápida — botão confirmar
    document.addEventListener('click', function (e) {
        var btn = e.target.closest('#viewLista .projeto-list-inline-btn-confirm');
        if (!btn) return;
        var form = btn.closest('.projeto-list-inline-form');
        if (form) submitListInlineForm(form);
    });

    // Lista: Criação rápida — Enter submete
    document.addEventListener('keydown', function (e) {
        if (e.key !== 'Enter') return;
        var input = e.target.closest('#viewLista .projeto-list-inline-input');
        if (!input) return;
        e.preventDefault();
        var form = input.closest('.projeto-list-inline-form');
        if (form) submitListInlineForm(form);
    });

    // Lista: Escape fecha menus e formulários inline
    document.addEventListener('keydown', function (e) {
        if (e.key === 'Escape') {
            closeAllListMenus();
            closeAllListInlineForms();
        }
    });

    // Lista: Click fora fecha menus e formulários inline
    document.addEventListener('click', function (e) {
        if (!e.target.closest('#viewLista .kanban-subtask-status-menu') &&
            !e.target.closest('#viewLista .kanban-card-prioridade-menu') &&
            !e.target.closest('#viewLista .kanban-subtask-search-popover') &&
            !e.target.closest('#viewLista .projeto-list-status-trigger') &&
            !e.target.closest('#viewLista .projeto-list-prioridade-trigger') &&
            !e.target.closest('#viewLista .projeto-list-responsavel') &&
            !e.target.closest('#viewLista .projeto-list-btn-responsavel') &&
            !e.target.closest('#viewLista .projeto-list-subtarefa-status-trigger') &&
            !e.target.closest('#viewLista .projeto-list-subtarefa-responsavel') &&
            !e.target.closest('#viewLista .projeto-list-subtarefa-btn-responsavel') &&
            !e.target.closest('#viewLista .projeto-list-date-editing') &&
            !e.target.closest('#viewLista .projeto-list-nome-editing')) {
            closeAllListMenus();
        }
        if (!e.target.closest('#viewLista .projeto-list-inline-form') &&
            !e.target.closest('#viewLista .projeto-list-btn-add-subtask')) {
            closeAllListInlineForms();
        }
    });

    // ====== Form de filtros — overlay ao submeter ======
    var formFiltros = document.getElementById('formFiltros');
    if (formFiltros) {
        // Native submit (botão de busca)
        formFiltros.addEventListener('submit', function () {
            showPageLoading();
        });
        // Programmatic submit (paginação, filter tags, onchange selects)
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

    // ====== Remoção de filter tags ======
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

    // ====== Modal Novo Projeto — submit via AJAX ======
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
                DtPrevisaoFim: document.getElementById('npDtPrevisaoFim').value || null
            };

            var btnSalvar = document.getElementById('btnSalvarNovoProjeto');
            btnSalvar.disabled = true;
            btnSalvar.innerHTML = '<i class="fa-solid fa-spinner fa-spin me-1"></i>Criando...';

            fetch('/Projetos/Criar', {
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
                    window.location.href = '/Projetos/' + data.projetoId;
                });
            })
            .catch(function (err) {
                Swal.fire({
                    icon: 'error',
                    title: 'Erro',
                    text: (err && err.mensagem) || 'Não foi possível criar o projeto.'
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

        // Limpar validação ao interagir com campos
        formNovoProjeto.addEventListener('input', function (e) { e.target.classList.remove('is-invalid'); });
        formNovoProjeto.addEventListener('change', function (e) { e.target.classList.remove('is-invalid'); });
    }

})();
