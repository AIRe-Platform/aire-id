// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.


import { createI18n } from 'vue-i18n';
import en from './en';
import fi from './fi';
import es from './es';
import vi from './vi';
import id from './id';
import sw from './sw';
import { LocalizationKey } from './keys';
import { LanguageCode } from 'iso-639-1';

export const l = LocalizationKey;
export type Locale = { [id in LocalizationKey]: string };

export const supportedLocales: LanguageCode[] = [
    "en", "fi", "es", "vi", "id", "sw"
]

export function requested_locale() {
    const query = new URLSearchParams(window.location.search);
    return query.get("locale") || undefined
}

const i18n = initLocale();
export default i18n;

function initLocale() {
    const storedLocale = localStorage.getItem("locale");
    const defaultLocale: LanguageCode = "en";

    const loc = storedLocale ?? defaultLocale;
    document.documentElement.lang = loc;

    return createI18n({
        locale: loc,
        legacy: false,
        messages: {
            en: { ...en },
            fi: { ...fi },
            es: { ...es },
            id: { ...id },
            sw: { ...sw },
            vi: { ...vi }
        },
        fallbackLocale: defaultLocale,
        availableLocales: supportedLocales,
    })
}

export function setUILanguage(lang: LanguageCode) {
    if (!supportedLocales.includes(lang))
        return;

    document.documentElement.lang = lang;

    // Is a ref in non-legacy mode
    const loc = i18n.global.locale as any;
    loc.value = lang;

    localStorage.setItem("locale", lang);
}

export function getUILanguage() {
    const loc = i18n.global.locale as any;
    return loc.value as string;
}
