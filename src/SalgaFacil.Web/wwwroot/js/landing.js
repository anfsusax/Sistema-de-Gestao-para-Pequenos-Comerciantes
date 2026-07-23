window.ConsuLanding = (function () {
  "use strict";

  let observer = null;
  let toastTimer = null;
  let bound = false;

  function prefersReducedMotion() {
    return window.matchMedia("(prefers-reduced-motion: reduce)").matches;
  }

  function root() {
    return document.querySelector("[data-consu-landing]");
  }

  function showReveals(scope) {
    scope.querySelectorAll("[data-reveal]").forEach(function (el) {
      el.classList.add("is-visible");
    });
  }

  function setupReveals(scope) {
    var els = scope.querySelectorAll("[data-reveal]");
    if (!els.length) return;

    if (prefersReducedMotion() || !("IntersectionObserver" in window)) {
      showReveals(scope);
      return;
    }

    observer = new IntersectionObserver(
      function (entries) {
        entries.forEach(function (entry) {
          if (!entry.isIntersecting) return;
          entry.target.classList.add("is-visible");
          observer.unobserve(entry.target);
        });
      },
      { threshold: 0.15, rootMargin: "0px 0px -6% 0px" }
    );

    els.forEach(function (el) {
      observer.observe(el);
      var rect = el.getBoundingClientRect();
      if (rect.top < window.innerHeight * 0.92) {
        el.classList.add("is-visible");
        observer.unobserve(el);
      }
    });
  }

  function showToast(scope, message) {
    var toast = scope.querySelector("#toast");
    if (!toast) return;
    var p = toast.querySelector("p");
    if (p && message) p.innerHTML = message;
    toast.hidden = false;
    void toast.offsetWidth;
    toast.classList.add("is-visible");
    window.clearTimeout(toastTimer);
    toastTimer = window.setTimeout(function () {
      toast.classList.remove("is-visible");
      window.setTimeout(function () {
        toast.hidden = true;
      }, 280);
    }, 1600);
  }

  function setupCta(scope) {
    var cta = scope.querySelector("#cta-principal");
    if (!cta || bound) return;
    bound = true;

    cta.addEventListener("click", function (event) {
      // deixa o href="/login" navegar; só mostra feedback rápido
      showToast(scope, 'Entrando no sistema <strong>Consu</strong>...');
      cta.classList.add("is-pressed");
      window.setTimeout(function () {
        cta.classList.remove("is-pressed");
      }, 200);
    });
  }

  function setupParallax(scope) {
    if (prefersReducedMotion()) return;
    var scribbles = scope.querySelector(".scribbles");
    var visual = scope.querySelector(".hero__visual");
    if (!scribbles || !visual) return;

    visual.addEventListener("pointermove", function (event) {
      if (window.innerWidth < 1024) {
        scribbles.style.transform = "";
        return;
      }
      var rect = visual.getBoundingClientRect();
      var x = (event.clientX - rect.left) / rect.width - 0.5;
      var y = (event.clientY - rect.top) / rect.height - 0.5;
      scribbles.style.transform = "translate(" + x * 10 + "px, " + y * 8 + "px)";
    });

    visual.addEventListener("pointerleave", function () {
      scribbles.style.transform = "";
    });
  }

  function init() {
    var scope = root();
    if (!scope) return;
    setupReveals(scope);
    setupCta(scope);
    setupParallax(scope);
  }

  function destroy() {
    if (observer) {
      observer.disconnect();
      observer = null;
    }
    window.clearTimeout(toastTimer);
    bound = false;
  }

  return { init: init, destroy: destroy };
})();
