(() => {
  "use strict";

  const prefersReducedMotion = window.matchMedia("(prefers-reduced-motion: reduce)").matches;

  /* ---------- Reveal on scroll / load ---------- */
  const revealElements = document.querySelectorAll("[data-reveal]");

  const showAllReveals = () => {
    revealElements.forEach((el) => el.classList.add("is-visible"));
  };

  if (prefersReducedMotion || !("IntersectionObserver" in window)) {
    showAllReveals();
  } else {
    const observer = new IntersectionObserver(
      (entries, obs) => {
        entries.forEach((entry) => {
          if (!entry.isIntersecting) return;
          entry.target.classList.add("is-visible");
          obs.unobserve(entry.target);
        });
      },
      {
        threshold: 0.18,
        rootMargin: "0px 0px -8% 0px",
      }
    );

    revealElements.forEach((el) => observer.observe(el));

    // Garante entrada imediata dos elementos acima da dobra
    window.requestAnimationFrame(() => {
      revealElements.forEach((el) => {
        const rect = el.getBoundingClientRect();
        if (rect.top < window.innerHeight * 0.92) {
          el.classList.add("is-visible");
          observer.unobserve(el);
        }
      });
    });
  }

  /* ---------- CTA interaction ---------- */
  const cta = document.getElementById("cta-principal");
  const toast = document.getElementById("toast");
  const produtos = document.getElementById("produtos");
  let toastTimer;

  const showToast = (message) => {
    if (!toast) return;
    const paragraph = toast.querySelector("p");
    if (paragraph && message) paragraph.innerHTML = message;
    toast.hidden = false;
    // force reflow for transition
    void toast.offsetWidth;
    toast.classList.add("is-visible");

    window.clearTimeout(toastTimer);
    toastTimer = window.setTimeout(() => {
      toast.classList.remove("is-visible");
      window.setTimeout(() => {
        toast.hidden = true;
      }, 350);
    }, 2800);
  };

  const smoothScrollTo = (target) => {
    if (!target) return;
    target.scrollIntoView({
      behavior: prefersReducedMotion ? "auto" : "smooth",
      block: "start",
    });
  };

  if (cta) {
    cta.addEventListener("click", () => {
      showToast('Ótima escolha! Conheça a qualidade <strong>Consu</strong>.');
      smoothScrollTo(produtos);

      cta.classList.add("is-pressed");
      window.setTimeout(() => cta.classList.remove("is-pressed"), 220);
    });

    cta.addEventListener("keydown", (event) => {
      if (event.key === "Enter" || event.key === " ") {
        event.preventDefault();
        cta.click();
      }
    });
  }

  /* ---------- Subtle parallax on decorative scribbles (desktop) ---------- */
  const scribbles = document.querySelector(".scribbles");
  const visual = document.querySelector(".hero__visual");

  if (scribbles && visual && !prefersReducedMotion) {
    const onMove = (event) => {
      if (window.innerWidth < 1024) {
        scribbles.style.transform = "";
        return;
      }
      const rect = visual.getBoundingClientRect();
      const x = (event.clientX - rect.left) / rect.width - 0.5;
      const y = (event.clientY - rect.top) / rect.height - 0.5;
      scribbles.style.transform = `translate(${x * 10}px, ${y * 8}px)`;
    };

    visual.addEventListener("pointermove", onMove);
    visual.addEventListener("pointerleave", () => {
      scribbles.style.transform = "";
    });
  }

  /* ---------- Soft entrance stagger helper for benefits ---------- */
  const benefits = document.querySelectorAll(".benefit");
  benefits.forEach((item, index) => {
    item.style.setProperty("--i", String(index));
  });

  /* ---------- Year / tiny polish: keyboard focus ring on interactive links ---------- */
  document.querySelectorAll(".footer-block a").forEach((link) => {
    link.addEventListener("focus", () => link.classList.add("is-focused"));
    link.addEventListener("blur", () => link.classList.remove("is-focused"));
  });
})();
