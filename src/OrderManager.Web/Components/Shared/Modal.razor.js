export function openModal(root) {
    root._previousFocus = document.activeElement;

    const focusable = getFocusable(root);
    const initial = focusable.find((el) => !el.classList.contains("modal-close"));
    (initial ?? focusable[0] ?? root).focus();

    root._trapHandler = (event) => {
        if (event.key === "Tab") trapFocus(root, event);
    };
    document.addEventListener("keydown", root._trapHandler, true);

    root._previousBodyOverflow = document.body.style.overflow;
    document.body.style.overflow = "hidden";
}

export function closeModal(root) {
    if (root._trapHandler) {
        document.removeEventListener("keydown", root._trapHandler, true);
        root._trapHandler = null;
    }

    if (root._previousBodyOverflow !== undefined) {
        document.body.style.overflow = root._previousBodyOverflow;
        root._previousBodyOverflow = undefined;
    }

    const previous = root._previousFocus;
    root._previousFocus = null;
    if (previous && previous.isConnected && typeof previous.focus === "function") {
        previous.focus();
    }
}

function trapFocus(root, event) {
    const focusable = getFocusable(root);
    if (focusable.length === 0) {
        event.preventDefault();
        return;
    }
    const first = focusable[0];
    const last = focusable[focusable.length - 1];
    const active = document.activeElement;

    if (event.shiftKey) {
        if (active === first || !root.contains(active)) {
            event.preventDefault();
            last.focus();
        }
    } else if (active === last || !root.contains(active)) {
        event.preventDefault();
        first.focus();
    }
}

function getFocusable(root) {
    const selector = [
        "a[href]",
        "button:not([disabled])",
        "input:not([disabled])",
        "select:not([disabled])",
        "textarea:not([disabled])",
        '[tabindex]:not([tabindex="-1"]):not([disabled])',
    ].join(",");
    return [...root.querySelectorAll(selector)].filter((el) => el.getClientRects().length > 0);
}