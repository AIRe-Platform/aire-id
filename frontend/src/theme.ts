// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.


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
