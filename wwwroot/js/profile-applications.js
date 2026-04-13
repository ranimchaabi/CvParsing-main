// AJAX filtering/pagination for "Mes candidatures" without full reload
(function () {
  function onReady(fn) {
    if (document.readyState === "loading") document.addEventListener("DOMContentLoaded", fn);
    else fn();
  }

  onReady(function () {
    var root = document.getElementById("applications-root");
    if (!root) return;

    var endpoint = root.getAttribute("data-applications-endpoint");
    if (!endpoint) return;

    function buildUrlFromHref(href) {
      try {
        var u = new URL(href, window.location.origin);
        var page = u.searchParams.get("page") || "1";
        var status = u.searchParams.get("status") || "all";
        var tab = u.searchParams.get("tab") || null;

        var out = new URL(endpoint, window.location.origin);
        out.searchParams.set("page", page);
        out.searchParams.set("status", status);
        if (tab) out.searchParams.set("tab", tab);
        return out.toString();
      } catch {
        return null;
      }
    }

    async function loadPartial(url) {
      root.classList.add("cp-loading");
      try {
        var res = await fetch(url, { headers: { "X-Requested-With": "XMLHttpRequest" } });
        if (!res.ok) return;
        var html = await res.text();
        root.innerHTML = html;
      } finally {
        root.classList.remove("cp-loading");
      }
    }

    root.addEventListener("click", function (e) {
      var link = e.target && e.target.closest ? e.target.closest("a") : null;
      if (!link) return;
      if (!link.classList.contains("cp-filter-chip") && !link.classList.contains("cp-page-link")) return;

      var url = buildUrlFromHref(link.getAttribute("href"));
      if (!url) return;

      e.preventDefault();
      loadPartial(url);
    });
  });
})();

