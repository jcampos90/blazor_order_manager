const THEME_KEY = "om-theme";

export function toggleTheme() {
    const root = document.documentElement;
    const next = root.getAttribute("data-theme") === "dark" ? "light" : "dark";
    root.setAttribute("data-theme", next);
    try {
        localStorage.setItem(THEME_KEY, next);
    } catch (e) {
        // Storage unavailable — theme still applies for this session.
    }
}
