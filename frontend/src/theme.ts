function getTheme() {
    const query = new URLSearchParams(window.location.search);
    if (query.has("theme")) {
        const theme = query.get("theme");
        if (theme == "dark" || theme == "light")
            return theme;
    }

    return window.matchMedia("(prefers-color-scheme: dark)") ? "dark" : "light";
}

export function initTheme() {
    document.documentElement.classList.add(getTheme());
}
