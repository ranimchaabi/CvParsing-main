/**
 * Adds show/hide control to all password fields (no backend changes).
 */
(function () {
    function attach(input) {
        if (!input || input.closest('.password-toggle-wrap')) return;

        var wrap = document.createElement('div');
        wrap.className = 'password-toggle-wrap';
        input.parentNode.insertBefore(wrap, input);
        wrap.appendChild(input);

        var btn = document.createElement('button');
        btn.type = 'button';
        btn.className = 'password-toggle-btn';
        btn.setAttribute('aria-label', 'Afficher le mot de passe');
        btn.setAttribute('aria-pressed', 'false');
        btn.setAttribute('tabindex', '0');
        btn.innerHTML = "<i class='bx bx-show' aria-hidden='true'></i>";

        btn.addEventListener('click', function () {
            var visible = input.type === 'text';
            input.type = visible ? 'password' : 'text';
            btn.setAttribute('aria-pressed', visible ? 'false' : 'true');
            btn.setAttribute('aria-label', visible ? 'Afficher le mot de passe' : 'Masquer le mot de passe');
            var icon = btn.querySelector('i');
            if (icon) {
                icon.className = visible ? 'bx bx-show' : 'bx bx-hide';
            }
        });

        wrap.appendChild(btn);
    }

    function init() {
        document.querySelectorAll('input[type="password"]').forEach(attach);
    }

    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', init);
    } else {
        init();
    }
})();
