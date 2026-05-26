/* ============================================
   Cotações — Adicionar (validação client-side)
   ============================================ */
(() => {
    const form = document.getElementById('formCotacaoAdd');
    if (!form) return;

    const campos = [
        { name: 'Tipo',        msg: 'O campo Tipo é obrigatório.' },
        { name: 'Estabelecimento', msg: 'O campo Estabelecimento é obrigatório.' },
        { name: 'CondPagtoId', msg: 'O campo Condição de Pagamento é obrigatório.' },
        { name: 'FormaPagtoId', msg: 'O campo Forma de Pagamento é obrigatório.' }
    ];

    function mostrarErro(input, mensagem) {
        input.classList.add('input-validation-error');
        let span = input.parentElement.querySelector('.field-validation-error');
        if (!span) {
            span = document.createElement('span');
            span.className = 'field-validation-error';
            input.parentElement.appendChild(span);
        }
        span.textContent = mensagem;
    }

    function limparErro(input) {
        input.classList.remove('input-validation-error');
        const span = input.parentElement.querySelector('.field-validation-error');
        if (span) span.textContent = '';
    }

    // Limpa erro ao interagir com o campo
    campos.forEach(function (c) {
        const el = form.elements[c.name];
        if (!el) return;
        el.addEventListener('change', function () { limparErro(el); });
        el.addEventListener('input', function () { limparErro(el); });
    });

    form.addEventListener('submit', function (e) {
        let valido = true;

        campos.forEach(function (c) {
            const el = form.elements[c.name];
            if (!el) return;

            const valor = el.value;
            if (!valor || valor === '') {
                mostrarErro(el, c.msg);
                valido = false;
            } else {
                limparErro(el);
            }
        });

        if (!valido) {
            e.preventDefault();
        }
    });
})();
