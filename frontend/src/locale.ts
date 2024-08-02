const loc: { [key: string]: { [lang: string]: string } } = {
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

export default function localize(key: string) {
    const query = new URLSearchParams(window.location.search)
    const lang = query.get("lang") || "en"
    return loc[key][lang]
}
