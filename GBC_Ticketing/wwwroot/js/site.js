const debounce = (fn, delay = 250) => {
  let timer;
  return (...args) => {
    clearTimeout(timer);
    timer = setTimeout(() => fn.apply(null, args), delay);
  };
};

document.addEventListener("DOMContentLoaded", () => {
  initLiveSearch();
  initPurchaseForm();
  initRatingButtons();
});

function initLiveSearch() {
  const input = document.getElementById("searchString");
  const resultsContainer = document.getElementById("searchResults");
  const status = document.getElementById("liveSearchStatus");
  const spinner = document.getElementById("liveSearchSpinner");

  if (!input || !resultsContainer || !input.dataset.liveSearchUrl) {
    return;
  }

  const performSearch = debounce(async () => {
    const query = input.value.trim();
    const url = new URL(input.dataset.liveSearchUrl, window.location.origin);
    if (query) {
      url.searchParams.append("searchString", query);
    }

    spinner?.classList.remove("d-none");
    status?.classList.add("d-none");

    try {
      const response = await fetch(url, {
        headers: { "X-Requested-With": "XMLHttpRequest" },
      });
      if (!response.ok) {
        throw new Error("Failed to load results");
      }
      const html = await response.text();
      resultsContainer.innerHTML = html;
      if (status) {
        status.textContent = query
          ? `Showing results for "${query}"`
          : "Showing all upcoming events";
        status.classList.remove("d-none");
      }
    } catch (error) {
      console.error(error);
      if (status) {
        status.textContent =
          "Unable to fetch live results. Please try again later.";
        status.classList.remove("d-none");
        status.classList.add("text-danger");
      }
    } finally {
      spinner?.classList.add("d-none");
    }
  }, 300);

  input.addEventListener("input", performSearch);
}

function initPurchaseForm() {
  const form = document.getElementById("purchaseForm");
  const quantityInput = document.getElementById("ticketQuantity");
  const hint = document.getElementById("quantityHint");
  const total = document.getElementById("quantityTotal");
  const cartBadge = document.getElementById("cartBadge");
  const cartHintInline = document.getElementById("cartHintInline");
  const cartTotalDisplay = document.getElementById("cartTotalDisplay");
  const cartWarning = document.getElementById("cartWarning");
  const celebrationModalEl = document.getElementById(
    "purchaseCelebrationModal"
  );
  const celebrationModal =
    celebrationModalEl && typeof bootstrap !== "undefined"
      ? new bootstrap.Modal(celebrationModalEl)
      : null;

  if (!form || !quantityInput) {
    return;
  }

  const price = parseFloat(form.dataset.price ?? "0");
  const max = parseInt(form.dataset.remaining ?? "0", 10);

  const updateSummary = () => {
    const qty = parseInt(quantityInput.value || "0", 10);
    if (Number.isNaN(qty) || qty <= 0) {
      total.textContent = "";
      cartBadge && (cartBadge.textContent = "Cart • 0");
      cartTotalDisplay && (cartTotalDisplay.textContent = "Total: $0.00");
      cartHintInline &&
        (cartHintInline.textContent = "Choose a quantity to begin");
      cartWarning && (cartWarning.textContent = "");
      return;
    }
    if (hint && max > 0) {
      const remainingText =
        qty > max
          ? `Only ${max} ticket${max === 1 ? "" : "s"} remaining`
          : `${max - qty} ticket${max - qty === 1 ? " left" : "s left"}`;
      hint.textContent = remainingText;
    }
    const cost = (qty * price).toFixed(2);
    if (total) {
      total.textContent = `Total: $${cost}`;
    }
    if (cartBadge) {
      cartBadge.textContent = `Cart • ${qty}`;
    }
    if (cartTotalDisplay) {
      cartTotalDisplay.textContent = `Total: $${cost}`;
    }
    if (cartHintInline) {
      cartHintInline.textContent =
        qty === 1 ? "1 ticket selected" : `${qty} tickets selected`;
    }
    if (cartWarning) {
      cartWarning.textContent =
        qty > max
          ? `Only ${max} ticket${max === 1 ? "" : "s"} available`
          : max - qty <= 4
          ? `Hurry! Only ${max - qty} ticket${max - qty === 1 ? "" : "s"} left`
          : "";
    }
  };

  quantityInput.addEventListener("input", updateSummary);
  updateSummary();

  const validatePurchaseForm = () => {
    if (window.jQuery && typeof window.jQuery === "function") {
      const validator = window.jQuery(form).valid;
      if (typeof validator === "function") {
        return window.jQuery(form).valid();
      }
    }

    if (typeof form.checkValidity === "function") {
      const isValid = form.checkValidity();
      if (!isValid && typeof form.reportValidity === "function") {
        form.reportValidity();
      }
      return isValid;
    }

    return true;
  };

  const runConfettiAndSubmit = () => {
    launchConfetti();
    setTimeout(() => {
      form.dataset.celebrating = "done";
      form.submit();
    }, 300);
  };

  form.addEventListener("submit", (event) => {
    if (form.dataset.celebrating === "done") {
      return;
    }

    if (!validatePurchaseForm()) {
      return;
    }

    event.preventDefault();
    if (form.dataset.celebrating === "pending") {
      return;
    }

    if (!celebrationModal) {
      form.dataset.celebrating = "pending";
      runConfettiAndSubmit();
      return;
    }

    form.dataset.celebrating = "pending";

    const handleHidden = () => {
      celebrationModalEl.removeEventListener("hidden.bs.modal", handleHidden);
      runConfettiAndSubmit();
    };

    celebrationModalEl.addEventListener("hidden.bs.modal", handleHidden);
    celebrationModal.show();

    setTimeout(() => {
      celebrationModal.hide();
    }, 1500);
  });
}

function initRatingButtons() {
  const historyTab = document.getElementById("history");
  if (!historyTab) {
    return;
  }

  const rateUrl = historyTab.dataset.rateUrl;
  if (!rateUrl) {
    return;
  }

  const tokenInput = document.querySelector(
    '#ajaxTokens input[name="__RequestVerificationToken"]'
  );
  const token = tokenInput?.value ?? "";

  historyTab.addEventListener("click", async (event) => {
    const button = event.target.closest(".rate-btn");
    if (!button) {
      return;
    }

    const purchaseId = button.dataset.purchaseId;
    const rating = button.dataset.rating;
    if (!purchaseId || !rating) {
      return;
    }

    button.disabled = true;
    try {
      const response = await fetch(rateUrl, {
        method: "POST",
        headers: {
          "Content-Type": "application/json",
          RequestVerificationToken: token,
        },
        body: JSON.stringify({ purchaseId, rating }),
      });

      if (response.ok) {
        window.location.reload();
      } else {
        console.warn("Failed to submit rating");
        button.disabled = false;
      }
    } catch (error) {
      console.error(error);
      button.disabled = false;
    }
  });
}

function initConfirmationCelebration() {
  const confirmationCard = document.querySelector("[data-confirmation-card]");
  if (!confirmationCard) {
    return;
  }

  launchConfetti();
}

function launchConfetti() {
  const colors = ["#f87171", "#60a5fa", "#34d399", "#fbbf24", "#c084fc"];
  const existingContainer = document.getElementById("confetti-container");
  if (existingContainer) {
    existingContainer.remove();
  }
  const confettiContainer = document.createElement("div");
  confettiContainer.id = "confetti-container";
  confettiContainer.style.position = "fixed";
  confettiContainer.style.inset = "0";
  confettiContainer.style.pointerEvents = "none";
  document.body.appendChild(confettiContainer);

  for (let i = 0; i < 60; i += 1) {
    const piece = document.createElement("span");
    piece.textContent = "•";
    piece.style.position = "absolute";
    piece.style.left = `${Math.random() * 100}%`;
    piece.style.top = "-10px";
    piece.style.fontSize = `${Math.random() * 18 + 12}px`;
    piece.style.color = colors[Math.floor(Math.random() * colors.length)];
    piece.style.animation = `fall ${Math.random() * 2 + 2}s linear forwards`;
    confettiContainer.appendChild(piece);
  }

  setTimeout(() => confettiContainer.remove(), 4500);
}

const style = document.createElement("style");
style.textContent = `
@keyframes fall {
  to {
    transform: translateY(110vh) rotate(720deg);
    opacity: 0;
  }
}`;
document.head.appendChild(style);
