<script setup lang="ts">
import { supportedLocales, setUILanguage, l } from "@/locales";
import ISO6391, { LanguageCode } from 'iso-639-1';

const setLang = async (e: Event) => {
    const el = e.target as HTMLSelectElement;
    setUILanguage(el.value as LanguageCode);
    el.blur();
};
</script>

<template>
    <div class="language-selector-panel">
        <label for="settings-language">{{ $t(l.settings_language) }}</label>
        <select id="settings-language" class="capitalize" @change="setLang" :value="$i18n.locale">
            <option v-for="lang in supportedLocales" :value="lang" :key="lang">
                {{ $t(lang) }} ({{ ISO6391.getName(lang) }})
            </option>
        </select>
    </div>
</template>

<style lang="scss" scoped>
.language-selector-panel {
    display: flex;
    flex-direction: column;
    gap: 0.5rem;
}
</style>