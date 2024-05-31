/* Localization */

const loc = {
    "title": {
        "en": "Log in",
        "fi": "Tunnistaudu"
    },
    "username": {
        "en": "Email or username",
        "fi": "Sähköpostiosoite tai käyttäjätunnus"
    },
    "password": {
        "en": "Password",
        "fi": "Salasana"
    },
    "button_login": {
        "en": "Login",
        "fi": "Kirjaudu"
    },
    "login_error": {
        "en": "Failed to log in",
        "fi": "Kirjautuminen epäonnistui"
    }
};

const localize = (key) => {
    const query = new URLSearchParams(window.location.search)
    const lang = query.get("lang") || "en"
    return loc[key][lang]
}

/* Theme */

function getTheme() {
    const query = new URLSearchParams(window.location.search);
    if (query.has("theme")) {
        const theme = query.get("theme");
        if (theme == "dark" || theme == "light")
            return theme;
    }

    return window.matchMedia("(prefers-color-scheme: dark)") ? "dark" : "light";
}

document.documentElement.classList.add(getTheme());

/* Session and login */

function restoreSession() {
    const token = localStorage.getItem("session_token");
    const username = localStorage.getItem("username");
    return { token: token, username: username };
}

function storeSession(session) {
    if (session) {
        localStorage.setItem("session_token", session.token);
        localStorage.setItem("username", session.username);
    }
    else {
        localStorage.clear();
    }
}

async function verifyToken(token) {
    return await fetch("/login/session", {
        method: "POST",
        headers: {
            "Content-Type": "application/json"
        },
        body: JSON.stringify({ token: token })
    })
        .then((res) => {
            return res.ok;
        })
        .catch((err) => {
            console.log(err);
            return false;
        })
}

async function login(username, password) {
    const credentials = {
        username: username,
        password: password
    };

    return await fetch("/login", {
        method: "POST",
        headers: {
            "Content-Type": "application/json",
            "Accept": "application/json"
        },
        body: JSON.stringify(credentials)
    })
        .then(async (res) => {
            if (res.ok) {
                const body = await res.json();
                storeSession({ token: body.token, username: username });
                return body;
            }
            return null;
        })
        .catch((err) => {
            console.log(err);
            return null;
        })
}

async function authorize(session_token, search_params) {
    const params = new URLSearchParams(search_params);

    return await fetch("/api/oauth/authorize", {
        method: "POST",
        headers: {
            "Content-Type": "application/x-www-form-urlencoded",
            "Authorization": `Bearer ${session_token}`
        },
        body: new FormData(params)
    })
        .then((res) => {
            if(res.redirected) {
                window.location.replace(res.location);
            }
        })
        .catch((err) => {
            console.log(err);
        })
}

async function verifyAccount(code) {

}

async function resendVerification(email) {

}
