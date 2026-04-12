(function () {
    var root = document.getElementById('globalNavSearchRoot');
    var input = document.getElementById('globalNavSearchInput');
    var panel = document.getElementById('globalNavSearchPanel');
    var list = document.getElementById('globalNavSearchList');
    var empty = document.getElementById('globalNavSearchEmpty');
    if (!root || !input || !panel || !list || !empty) return;

    var results = [];
    var activeIndex = -1;
    var debounceTimer = null;
    var open = false;
    var fetchSeq = 0;

    function esc(s) {
        return String(s)
            .replace(/&/g, '&amp;')
            .replace(/</g, '&lt;')
            .replace(/>/g, '&gt;')
            .replace(/"/g, '&quot;');
    }

    function segmentsHtml(segments) {
        if (!segments || !segments.length) return '';
        return segments.map(function (seg) {
            var t = esc(seg.text || '');
            return seg.highlight ? '<mark>' + t + '</mark>' : t;
        }).join('');
    }

    function setOpen(v) {
        open = v;
        panel.hidden = !v;
        panel.classList.toggle('is-open', v);
        input.setAttribute('aria-expanded', v ? 'true' : 'false');
        if (!v) activeIndex = -1;
    }

    function render() {
        list.innerHTML = '';
        empty.hidden = true;
        if (!results.length) {
            empty.hidden = false;
            list.innerHTML = '';
            return;
        }
        results.forEach(function (r, i) {
            var li = document.createElement('li');
            li.className = 'nav-global-search__item';
            li.setAttribute('role', 'presentation');
            var btn = document.createElement('button');
            btn.type = 'button';
            btn.className = 'nav-global-search__btn';
            btn.setAttribute('role', 'option');
            btn.id = 'globalNavSearchOpt' + i;
            btn.setAttribute('data-route', r.route);
            btn.innerHTML =
                '<span class="nav-global-search__label">' + segmentsHtml(r.segments) + '</span>' +
                '<span class="nav-global-search__route">' + esc(r.route) + '</span>';
            btn.addEventListener('mousedown', function (e) {
                e.preventDefault();
            });
            btn.addEventListener('click', function () {
                navigate(r.route);
            });
            li.appendChild(btn);
            list.appendChild(li);
        });
        syncActive();
    }

    function syncActive() {
        var btns = list.querySelectorAll('.nav-global-search__btn');
        btns.forEach(function (b, i) {
            b.classList.toggle('is-active', i === activeIndex);
            b.setAttribute('aria-selected', i === activeIndex ? 'true' : 'false');
        });
        if (activeIndex >= 0 && btns[activeIndex]) {
            input.setAttribute('aria-activedescendant', btns[activeIndex].id);
            try {
                btns[activeIndex].scrollIntoView({ block: 'nearest' });
            } catch (_) { }
        } else
            input.removeAttribute('aria-activedescendant');
    }

    function navigate(route) {
        if (route) window.location.href = route;
        setOpen(false);
    }

    function fetchResults(q) {
        var seq = ++fetchSeq;
        fetch('/api/nav-search?q=' + encodeURIComponent(q), {
            headers: { 'Accept': 'application/json' },
            credentials: 'same-origin'
        })
            .then(function (r) { return r.json(); })
            .then(function (data) {
                if (seq !== fetchSeq) return;
                if (input.value.trim() !== q) return;
                results = (data && data.results) ? data.results : [];
                activeIndex = results.length ? 0 : -1;
                render();
                setOpen(true);
            })
            .catch(function () {
                if (seq !== fetchSeq) return;
                results = [];
                activeIndex = -1;
                render();
                setOpen(true);
            });
    }

    function scheduleFetch() {
        var q = input.value.trim();
        clearTimeout(debounceTimer);
        if (!q.length) {
            results = [];
            render();
            setOpen(false);
            return;
        }
        debounceTimer = setTimeout(function () { fetchResults(q); }, 110);
    }

    input.addEventListener('input', scheduleFetch);
    input.addEventListener('focus', function () {
        if (input.value.trim().length) scheduleFetch();
    });

    input.addEventListener('keydown', function (e) {
        if (!open && (e.key === 'ArrowDown' || e.key === 'Enter') && input.value.trim()) {
            scheduleFetch();
            return;
        }
        if (!open) return;

        if (e.key === 'Escape') {
            e.preventDefault();
            setOpen(false);
            return;
        }
        if (e.key === 'ArrowDown') {
            e.preventDefault();
            if (!results.length) return;
            activeIndex = (activeIndex + 1) % results.length;
            syncActive();
            return;
        }
        if (e.key === 'ArrowUp') {
            e.preventDefault();
            if (!results.length) return;
            activeIndex = activeIndex <= 0 ? results.length - 1 : activeIndex - 1;
            syncActive();
            return;
        }
        if (e.key === 'Enter') {
            e.preventDefault();
            if (results.length === 0) return;
            var idx = activeIndex >= 0 ? activeIndex : 0;
            navigate(results[idx].route);
        }
    });

    document.addEventListener('click', function (e) {
        if (!root.contains(e.target)) setOpen(false);
    });

    window.addEventListener('resize', function () {
        if (!open) return;
    });
})();
