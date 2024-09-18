// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.


import { Locale } from ".";

const fi: Locale = {
    settings_language: "Käyttöliittymän kieli",

    auth_please_wait: "Hetkinen...",
    
    login_title: "Kirjautuminen",
    login_username: "Käyttäjätunnus tai sähköpostiosoite",
    login_password: "Salasana",
    login_submit: "Kirjaudu",
    login_cancel: "Peruuta",
    login_recover_password: "Unohtuiko salasana?",
    login_failed: "Kirjautuminen epäonnistui! Tarkista käyttäjätunnus ja salasana.",

    logout_please_wait: "Kirjaudutaan ulos...",
    logout_logged_out: "Olet kirjautunut ulos. Voit nyt sulkea tämän sivun.",

    consent_title: "Vahvista kirjautuminen",
    consent_logged_in_as: "Olet kirjautunut käyttäjänä {username}.",
    consent_click_to_logout: "Etkö ole sinä? Kirjaudu ulos klikkaamalla tästä.",
    consent_disclaimer: "Olet kirjautumassa palveluun {service}. Tämä antaa palvelulle pääsyn tietoihisi. Haluatko jatkaa?",
    consent_button_consent: "Vahvista",
    consent_button_cancel: "Peruuta",

    verification_title: "Tilin vahvistaminen",
    verification_description: "Syötä tähän vahvistuskoodi, jonka lähetimme sähköpostiosoitteeseesi.",
    verification_resend: "Lähetä uusi koodi",
    verification_code_sent: "Uusi koodi on matkalla!",
    verification_logout: "Kirjaudu ulos",
    verification_failed: "Tilin vahvistaminen epäonnistui. Koodi voi olla virheellinen tai vanhentunut.",
    verification_resend_failed: "Uuden koodin lähettäminen epäonnistui. Yritä myöhemmin uudelleen.",
    verification_button_confirm: "Vahvista",

    recovery_title: "Tilin palauttaminen",
    recovery_step1: "Syötä sähköpostiosoitteesi. Jos tili on olemassa, lähetämme sinulle sähköpostitse palautuskoodin, jonka tarvitset seuraavassa vaiheessa.",
    recovery_step2: "Syötä saamasi palautuskoodi ja uusi salasanasi.",
    recovery_step3: "Salasanasi on vaihdettu onnistuneesti!",
    recovery_label_code: "Palautuskoodi",
    recovery_label_password: "Uusi salasana",
    recovery_button_continue: "Jatka",
    recovery_button_back: "Takaisin kirjautumiseen",
    recovery_failed: "Tätä tiliä ei voida palauttaa. Ota yhteyttä tukeen.",
    recovery_password_requirements_not_met: "Salasanan tulee olla vähintään 8 merkkiä pitkä ja sisältää isoja ja pieniä kirjaimia.",

    error_not_found: "Pyydettyä sivua ei löydy. (404)",
    error_title: "Tunnistautuminen epäonnistui",
    error_details: `Tunnistautuminen epäonnistui. Yritä myöhemmin uudelleen.

            Virhe: {error}
            Kuvaus: {description}
        `,
    
    en: "Englanti",
    fi: "Suomi",
    es: "Espanja",
    vi: "Vietnam",
    id: "Indonesia",
    sw: "Swahili",
    rw: "Ruandan kieli"
};


export default fi;
