// Scoped navbar user dropdown + smooth scroll + logout confirm
(function () {
  function onReady(fn) {
    if (document.readyState === "loading") document.addEventListener("DOMContentLoaded", fn);
    else fn();
  }

  onReady(function () {
    var root = document.getElementById("userNavRoot");
    if (!root) return;

    var btn = document.getElementById("userNavAvatarBtn");
    var menu = document.getElementById("userNavMenu");
    var logoutBtn = document.getElementById("logoutConfirmBtn");

    if (!btn || !menu) return;

    function openMenu() {
      menu.hidden = false;
      btn.setAttribute("aria-expanded", "true");
      root.classList.add("user-nav--open");
    }

    function closeMenu() {
      menu.hidden = true;
      btn.setAttribute("aria-expanded", "false");
      root.classList.remove("user-nav--open");
    }

    function toggleMenu() {
      if (menu.hidden) openMenu();
      else closeMenu();
    }

    btn.addEventListener("click", function (e) {
      e.preventDefault();
      e.stopPropagation();
      toggleMenu();
    });

    document.addEventListener("click", function (e) {
      if (!root.contains(e.target)) closeMenu();
    });

    document.addEventListener("keydown", function (e) {
      if (e.key === "Escape") closeMenu();
    });

    // Smooth scroll for Mes candidatures when already on profile
    document.addEventListener("click", function (e) {
      var a = e.target && e.target.closest ? e.target.closest("a[href*=\"#mes-candidatures\"]") : null;
      if (!a) return;
      var isProfile = (window.location.pathname || "").toLowerCase().includes("/profile");
      if (!isProfile) return;
      var target = document.getElementById("mes-candidatures");
      if (!target) return;
      e.preventDefault();
      closeMenu();
      target.scrollIntoView({ behavior: "smooth", block: "start" });
      history.replaceState(null, "", "#mes-candidatures");
    });

    // Logout confirmation modal trigger
    if (logoutBtn) {
      logoutBtn.addEventListener("click", function (e) {
        e.preventDefault();
        closeMenu();
        var modalEl = document.getElementById("logoutConfirmModal");
        if (!modalEl || !window.bootstrap || !window.bootstrap.Modal) return;
        var m = window.bootstrap.Modal.getOrCreateInstance(modalEl);
        m.show();
      });
    }
  });
})();

