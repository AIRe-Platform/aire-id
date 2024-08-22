<!--
    This Source Code Form is subject to the terms of the Mozilla Public
    License, v. 2.0. If a copy of the MPL was not distributed with this
    file, You can obtain one at https://mozilla.org/MPL/2.0/.
-->

<script setup lang="ts">
import Heading from '@/components/Heading.vue';
import Logo from '@/components/Logo.vue';
import Panel from '@/components/Panel.vue';
import TextButton from '@/components/TextButton.vue';
import useSession from '@/context/session';
import OauthUtils from '@/utils/oauth';
import { reactive } from 'vue';
import { useRoute, useRouter } from 'vue-router';

const session = useSession();
const router = useRouter();
const route = useRoute();

const params = new URLSearchParams(window.location.search);
const state = reactive<{
    busy: boolean
}>({
    busy: false
})

const onLogout = () => {
    if (state.busy)
        return;

    session.clear();

    router.push({
        path: "/login",
        replace: true,
        query: route.query
    })
}

const onConsent = () => {
    if(session.session)
        OauthUtils.authorize(session.session);
}

const onCancel = () => {
    OauthUtils.cancelAuth();
}
</script>

<template>
    <Logo />
    <Heading>{{ $t('consent.title') }}</Heading>
    <Panel id="consent-view" class="main-panel">
        <div id="consent-user">
            <p>{{ $t('consent.logged_in_as', { username: session.session?.username }) }}</p>
            <TextButton @click="onLogout">{{ $t('consent.click_to_logout') }}</TextButton>
        </div>
        <div id="consent-message">{{ $t('consent.disclaimer', { service: params.get("service") }) }}</div>
        <div id="consent-buttons">
            <button @click="onConsent()" class="positive" :disabled="state.busy">{{
                $t('consent.button_consent') }}</button>
            <button @click="onCancel()" class="negative" :disabled="state.busy">{{ $t('consent.button_cancel')
                }}</button>
        </div>
    </Panel>
</template>

<style scoped>
#consent-view {
    display: flex;
    flex-direction: column;
    gap: 1rem;
}

#consent-user {
    font-size: small;
}

#consent-message {
    font-size: large;
    margin: 2rem 0;
    line-height: 1.8rem;
}

#consent-buttons {
    display: flex;
    flex-direction: row;
    align-items: center;
    justify-content: space-between;
    gap: 1rem;

    * {
        flex-grow: 1;
    }
}
</style>