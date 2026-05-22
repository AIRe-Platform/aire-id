// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.


import { Locale } from ".";

const sv: Locale = {
    settings_language: "AIRe-tjänstens språk",

    auth_please_wait: "Vänta lite...",

    login_title: "Inloggning",
    login_username: "Användarnamn eller e-postadress",
    login_password: "Lösenord",
    login_submit: "Logga in",
    login_cancel: "Ångra",
    login_recover_password: "Har du glömt lösenordet?",
    login_failed: "Inloggningen misslyckades! Kontrollera användarnamnet och lösenordet.",
    logout_please_wait: "Loggar ut...",
    logout_logged_out: "Du är utloggad. Du kan nu stänga denna sida.",

    consent_title: "Bekräfta inloggningen",
    consent_logged_in_as: "Du har loggat in som användare {username}.",
    consent_click_to_logout: "Fel användarkonto? Logga ut här.",
    consent_disclaimer: "Du loggar in i tjänsten {service} på plattformen {platform}. Detta ger tjänsten tillgång till dina uppgifter. Vill du fortsätta?",
    consent_button_consent: "Bekräfta",
    consent_button_cancel: "Ångra",

    verification_title: "Bekräftande av konto",
    verification_description: "Ange bekräftelsekoden som vi skickade till din e-postadress här.",
    verification_resend: "Skicka en ny kod",
    verification_code_sent: "En ny kod är på väg!",
    verification_logout: "Logga ut",
    verification_failed: "Det gick inte att bekräfta kontot. Koden kan vara felaktig eller föråldrad.",
    verification_resend_failed: "Det gick inte att skicka en ny kod. Försök igen senare.",
    verification_button_confirm: "Bekräfta",

    recovery_title: "Återställ konto",
    recovery_step1: "Ange din e-postadress. Om kontot finns skickar vi dig en återställningskod via e-post som du behöver i följande skede.",
    recovery_step2: "Ange den återställningskod du fått och ditt nya lösenord.",
    recovery_step3: "Ditt lösenord har ändrats!",
    recovery_label_code: "Återställningskod",
    recovery_label_password: "Nytt lösenord",
    recovery_button_continue: "Fortsätt",
    recovery_button_back: "Tillbaka till inloggningen",
    recovery_failed: "Detta konto kan inte återställas. Kontakta supporten.",
    recovery_password_requirements_not_met: "Lösenordet ska vara minst 8 tecken långt och innehålla stora och små bokstäver.",

    invite_title: "Bjud in ny användare",
    invite_email: "E-postadress",
    invite_failed_generic: "Det gick inte att skicka inbjudan. Försök igen senare.",
    invite_failed_conflict: "Inbjudan kunde inte skickas. E-postadressen kan redan vara i bruk eller inbjuden.",
    invite_failed_not_found: "Felaktig inbjudningskod. Den QR-kod eller inbjudningslänk du använder finns inte längre.",
    invite_failed_forbidden: "Föråldrad inbjudningskod. Den QR-kod eller inbjudningslänk du använder är inte längre i kraft.",
    invite_sent: "Inbjudan har skickats till adressen '{email}'!",
    invite_submit: "Skicka inbjudan",

    error_not_found: "Den begärda sidan hittas inte. (404)",
    error_title: "Identifieringen misslyckades",
    error_details: `Autentiseringen misslyckades. Försök igen senare.

            Fel: {error}
            Beskrivning: {description}
        `,

    en: "Engelska",
    fi: "Finska",
    sv: "Svenska",
    es: "Spanska",
    vi: "Vietnamesiska",
    id: "Indonesiska",
    sw: "Swahili",
    rw: "Kinyarwanda",
};


export default sv;
