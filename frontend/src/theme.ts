function getTheme() {
    const query = new URLSearchParams(window.location.search);
    if (query.has("theme")) {
        return query.get("theme") == "dark" ? "dark-mode" : "light-mode";
    }
    else {
        return window.matchMedia("(prefers-color-scheme: dark)") ? "dark-mode" : "light-mode";
    }
}

export function initTheme() {
    document.documentElement.classList.add(getTheme());
}
