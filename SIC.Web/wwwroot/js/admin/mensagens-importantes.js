(function () {
    'use strict';

    var pathBase = document.querySelector('meta[name="pathbase"]')?.content || '';

    // ── DataTables ─────────────────────────────────────────────
    var table = $('#tblMensagens').DataTable({
        responsive: true,
        order: [[3, 'desc']],
        pageLength: 25,
        language: {
            url: 'https://cdn.datatables.net/plug-ins/1.13.8/i18n/pt-BR.json'
        },
        columnDefs: [
            { targets: [6], orderable: false, searchable: false }
        ]
    });

    // ── Ações de linha ─────────────────────────────────────────
    document.getElementById('tblMensagens').addEventListener('click', function (e) {
        var btn = e.target.closest('[data-action]');
        if (!btn) return;

        var action = btn.getAttribute('data-action');
        var avisoId = btn.getAttribute('data-aviso-id');

        if (action === 'excluir') {
            confirmarExclusao(avisoId, btn);
        } else if (action === 'expirar') {
            confirmarExpiracao(avisoId, btn);
        }
    });

    function confirmarExclusao(avisoId, btn) {
        Swal.fire({
            title: 'Excluir mensagem?',
            text: 'Esta ação não pode ser desfeita. A mensagem será removida permanentemente.',
            icon: 'warning',
            showCancelButton: true,
            confirmButtonColor: '#dc3545',
            confirmButtonText: 'Sim, excluir',
            cancelButtonText: 'Cancelar'
        }).then(function (result) {
            if (result.isConfirmed) {
                executarAcao(pathBase + '/Admin/Admin/ExcluirMensagem', avisoId, btn, 'excluída');
            }
        });
    }

    function confirmarExpiracao(avisoId, btn) {
        Swal.fire({
            title: 'Expirar mensagem?',
            text: 'A mensagem será marcada como expirada e não será mais exibida para os usuários.',
            icon: 'question',
            showCancelButton: true,
            confirmButtonColor: '#ffc107',
            confirmButtonText: 'Sim, expirar',
            cancelButtonText: 'Cancelar'
        }).then(function (result) {
            if (result.isConfirmed) {
                executarAcao(pathBase + '/Admin/Admin/ExpirarMensagem', avisoId, btn, 'expirada');
            }
        });
    }

    function executarAcao(url, avisoId, btn, verbo) {
        var antiForgeryToken = document.querySelector('input[name="__RequestVerificationToken"]')?.value;

        fetch(url, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/x-www-form-urlencoded',
                'RequestVerificationToken': antiForgeryToken || ''
            },
            body: 'avisoId=' + encodeURIComponent(avisoId)
        })
        .then(function (response) { return response.json(); })
        .then(function (data) {
            if (data.success) {
                Swal.fire({
                    title: 'Sucesso!',
                    text: 'Mensagem ' + verbo + ' com sucesso.',
                    icon: 'success',
                    timer: 1500,
                    showConfirmButton: false
                });

                var row = btn.closest('tr');
                table.row(row).remove().draw(false);
            } else {
                Swal.fire('Erro', 'Não foi possível realizar a operação.', 'error');
            }
        })
        .catch(function () {
            Swal.fire('Erro', 'Falha de comunicação com o servidor.', 'error');
        });
    }

    // ── Rich-text toolbar ──────────────────────────────────────
    var toolbar = document.getElementById('richToolbar');
    if (toolbar) {
        toolbar.addEventListener('click', function (e) {
            var btn = e.target.closest('[data-cmd]');
            if (!btn) return;
            e.preventDefault();
            document.execCommand(btn.getAttribute('data-cmd'), false, null);
            document.getElementById('editorDescricao').focus();
        });

        var colorSelect = document.getElementById('cmdForeColor');
        if (colorSelect) {
            colorSelect.addEventListener('change', function () {
                if (this.value) {
                    document.execCommand('foreColor', false, this.value);
                    document.getElementById('editorDescricao').focus();
                }
                this.selectedIndex = 0;
            });
        }
    }

    // ── Destinatário toggle ────────────────────────────────────
    var selDestinatarioTipo = document.getElementById('selDestinatarioTipo');
    var wrapperArea = document.getElementById('wrapperArea');
    var wrapperUsuario = document.getElementById('wrapperUsuario');
    var selArea = document.getElementById('selArea');
    var selUsuario = document.getElementById('selUsuario');
    var areasLoaded = false;
    var usersLoaded = false;

    if (selDestinatarioTipo) {
        selDestinatarioTipo.addEventListener('change', function () {
            var val = this.value;
            wrapperArea.classList.toggle('d-none', val !== 'area');
            wrapperUsuario.classList.toggle('d-none', val !== 'usuario');

            if (val === 'area' && !areasLoaded) {
                loadAreas();
            } else if (val === 'usuario' && !usersLoaded) {
                loadUsuarios();
            }
        });
    }

    function loadAreas() {
        fetch(pathBase + '/Admin/Admin/GetAreas')
            .then(function (r) { return r.json(); })
            .then(function (data) {
                areasLoaded = true;
                selArea.innerHTML = '<option value="">Selecione uma área...</option>';
                data.forEach(function (item) {
                    var opt = document.createElement('option');
                    opt.value = item.intranetAreaID;
                    opt.textContent = item.nmArea;
                    selArea.appendChild(opt);
                });
            })
            .catch(function () {
                selArea.innerHTML = '<option value="">Erro ao carregar áreas</option>';
            });
    }

    function loadUsuarios() {
        fetch(pathBase + '/Admin/Admin/GetUsuarios')
            .then(function (r) { return r.json(); })
            .then(function (data) {
                usersLoaded = true;
                selUsuario.innerHTML = '<option value="">Selecione um usuário...</option>';
                data.forEach(function (item) {
                    var opt = document.createElement('option');
                    opt.value = item.usuarioID;
                    opt.textContent = item.nmUsuario;
                    selUsuario.appendChild(opt);
                });
            })
            .catch(function () {
                selUsuario.innerHTML = '<option value="">Erro ao carregar usuários</option>';
            });
    }

    // ── Salvar nova mensagem ───────────────────────────────────
    var btnSalvar = document.getElementById('btnSalvarMensagem');
    if (btnSalvar) {
        btnSalvar.addEventListener('click', function () {
            var titulo = document.getElementById('txtTitulo').value.trim();
            var descricao = document.getElementById('editorDescricao').innerHTML.trim();
            var prioridade = parseInt(document.getElementById('selPrioridade').value, 10);
            var expiracao = document.getElementById('txtExpiracao').value;
            var tipoDestinatario = selDestinatarioTipo.value;

            // Validações
            if (!titulo) {
                Swal.fire('Atenção', 'Informe o título da mensagem.', 'warning');
                return;
            }
            if (!descricao || descricao === '<br>') {
                Swal.fire('Atenção', 'Informe a descrição da mensagem.', 'warning');
                return;
            }
            if (!expiracao) {
                Swal.fire('Atenção', 'Informe a data de expiração.', 'warning');
                return;
            }
            var dtExp = new Date(expiracao);
            if (dtExp <= new Date()) {
                Swal.fire('Atenção', 'A data de expiração deve ser maior que a data atual.', 'warning');
                return;
            }

            var intranetAreaID = null;
            var usuarioID = null;

            if (tipoDestinatario === 'area') {
                intranetAreaID = selArea.value ? parseInt(selArea.value, 10) : null;
                if (!intranetAreaID) {
                    Swal.fire('Atenção', 'Selecione uma área.', 'warning');
                    return;
                }
            } else if (tipoDestinatario === 'usuario') {
                usuarioID = selUsuario.value ? parseInt(selUsuario.value, 10) : null;
                if (!usuarioID) {
                    Swal.fire('Atenção', 'Selecione um usuário.', 'warning');
                    return;
                }
            }

            var payload = {
                titulo: titulo,
                descricao: descricao,
                prioridade: prioridade,
                dataHoraExpiracao: expiracao,
                intranetAreaID: intranetAreaID,
                usuarioID: usuarioID
            };

            var antiForgeryToken = document.querySelector('input[name="__RequestVerificationToken"]')?.value;

            btnSalvar.disabled = true;

            fetch(pathBase + '/Admin/Admin/CriarMensagem', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'RequestVerificationToken': antiForgeryToken || ''
                },
                body: JSON.stringify(payload)
            })
            .then(function (r) { return r.json(); })
            .then(function (data) {
                btnSalvar.disabled = false;
                if (data.success) {
                    var modal = bootstrap.Modal.getInstance(document.getElementById('modalNovaMensagem'));
                    if (modal) modal.hide();

                    Swal.fire({
                        title: 'Sucesso!',
                        text: 'Mensagem cadastrada com sucesso.',
                        icon: 'success',
                        timer: 2000,
                        showConfirmButton: false
                    }).then(function () {
                        window.location.reload();
                    });
                } else {
                    Swal.fire('Erro', 'Não foi possível cadastrar a mensagem.', 'error');
                }
            })
            .catch(function () {
                btnSalvar.disabled = false;
                Swal.fire('Erro', 'Falha de comunicação com o servidor.', 'error');
            });
        });
    }

    // ── Limpar modal ao fechar ─────────────────────────────────
    var modalEl = document.getElementById('modalNovaMensagem');
    if (modalEl) {
        modalEl.addEventListener('hidden.bs.modal', function () {
            document.getElementById('txtTitulo').value = '';
            document.getElementById('editorDescricao').innerHTML = '';
            document.getElementById('selPrioridade').value = '3';
            document.getElementById('txtExpiracao').value = '';
            selDestinatarioTipo.value = 'todos';
            wrapperArea.classList.add('d-none');
            wrapperUsuario.classList.add('d-none');
        });
    }
})();
