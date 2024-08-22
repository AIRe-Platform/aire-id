import { createI18n } from "vue-i18n";

const STRINGS = {
    en: {
        auth: {
            please_wait: "Please wait..."
        },
        login: {
            title: "Login",
            username: "Username or email",
            password: "Password",
            submit: "Login",
            cancel: "Cancel",
            recover_password: "Forgot password?",
            failed: "Login failed! Please check the username and password."
        },
        logout: {
            please_wait: "Logging out...",
            logged_out: "You have been logged out. You can now close this page."
        },
        consent: {
            title: "Confirm Login",
            logged_in_as: "You are logged in as {username}.",
            click_to_logout: "Not you? Click here to logout.",
            disclaimer: "You are logging into {service}. This gives the service access to your information. Do you wish proceed?",
            button_consent: "Confirm",
            button_cancel: "Cancel"
        },
        verification: {
            title: "Account Verification",
            description: "Please enter the verification code we have sent to your email address.",
            resend: "Send a new code",
            code_sent: "A new code is on its way!",
            logout: "Log out",
            failed: "Failed to verify the account. The code may be invalid or expired.",
            resend_failed: "Could not send a new code. Please try again later.",
            button_confirm: "Verify"
        },
        recovery: {
            title: "Account Recovery",
            step1: "Please enter your email address. If the account exists, you'll receive an email from us with the recovery code you need to enter in the next step.",
            step2: "Please enter the recovery code you received and a new password you wish to use.",
            step3: "Your password has been changed successfully!",
            label_code: "Recovery code",
            label_password: "New password",
            button_continue: "Continue",
            button_back: "Back to login",
            failed: "This account cannot be recovered. Please contact support.",
            password_requirements_not_met: "The password must contain at least 8 characters as well as upper and lower case letters"
        },
        not_found: "The requested page does not exist. (404)",
        error: {
            title: "Authentication failed",
            details: `Failed to authenticate. Please try again later.

                Error: {error}
                Description: {description}
            `
        }
    },
    fi: {
        auth: {
            please_wait: "Hetkinen..."
        },
        login: {
            title: "Kirjautuminen",
            username: "Käyttäjätunnus",
            password: "Salasana",
            submit: "Kirjaudu",
            cancel: "Peruuta",
            failed: "Kirjautuminen epäonnistui! Tarkista käyttäjätunnus ja salasana.",
            recover_password: "Salasana unohtunut?"
        },
        logout: {
            please_wait: "Kirjaudutaan ulos...",
            logged_out: "Sinut on kirjattu ulos. Voit sulkea tämän sivun."
        },
        consent: {
            title: "Vahvista kirjautuminen",
            logged_in_as: "Olet kirjautunut käyttäjänä {username}.",
            click_to_logout: "Etkö ole hän? Kirjaudu ulos klikkaamalla tästä.",
            disclaimer: "Olet kirjautumassa palveluun {service}. Tämä antaa palvelulle pääsyn tietoihisi. Haluatko jatkaa?",
            button_consent: "Vahvista",
            button_cancel: "Peruuta"
        },
        verification: {
            title: "Tilin vahvistaminen",
            description: "Syötä tähän vahvistuskoodi, jonka lähetimme antamaasi sähköpostiosoitteeseen.",
            resend: "Lähetä uusi koodi",
            code_sent: "Uusi koodi on matkalla!",
            logout: "Kirjaudu ulos",
            failed: "Tilin vahvistaminen epäonnistui. Koodi voi olla väärä tai vanhentunut.",
            resend_failed: "Koodin lähettäminen epäonnistui. Yritä myöhemmin uudelleen.",
            button_confirm: "Vahvista"
        },
        recovery: {
            title: "Tilin palauttaminen",
            step1: "Syötä käyttäjätilisi sähköpostiosoite. Mikäli tili on olemassa, lähetämme palautuskoodin, jota tarvitset seuraavassa vaiheessa.",
            step2: "Syötä saamasi palautuskoodi sekä uusi salasana.",
            step3: "Salasanasi on vaihdettu onnistuneesti!",
            label_code: "Palautuskoodi",
            label_password: "Uusi salasana",
            button_continue: "Jatka",
            button_back: "Takaisin kirjautumiseen",
            failed: "Tätä tiliä ei voida palauttaa. Ole hyvä ja ota yhteys tukeen.",
            password_requirements_not_met: "Salasanan tulee olla vähintään 8 merkkiä pitkä sekä sisältää isoja ja pieniä kirjaimia."
        },
        not_found: "Pyydettyä sivua ei löydy. (404)",
        error: {
            title: "Tunnistautuminen epäonnistui",
            details: `Tunnistautuminen ei onnistunut. Yritä myöhemmin uudelleen.

                Virhe: {error}
                Lisätiedot: {description}
            `
        }
    }
};

const DEFAULT_LOCALE = "en";
const AVAILABLE_LOCALES = [
    "en", "fi"
]

export function requested_locale() {
    const query = new URLSearchParams(window.location.search);
    return query.get("locale") || undefined
}

function get_ui_locale() {
    const requested = requested_locale() || DEFAULT_LOCALE;
    return AVAILABLE_LOCALES.includes(requested) ? requested : DEFAULT_LOCALE;
}

const i18n = createI18n({
    locale: get_ui_locale(),
    messages: STRINGS,
    fallbackLocale: DEFAULT_LOCALE,
    availableLocales: AVAILABLE_LOCALES
});

export default i18n;
