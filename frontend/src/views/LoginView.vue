<!--
    This Source Code Form is subject to the terms of the Mozilla Public
    License, v. 2.0. If a copy of the MPL was not distributed with this
    file, You can obtain one at https://mozilla.org/MPL/2.0/.
-->

<script setup lang="ts">
import Logo from "@/components/Logo.vue";
import Panel from "@/components/Panel.vue";
import Heading from "@/components/Heading.vue";
import ErrorLabel from "@/components/ErrorLabel.vue";
import useSession from "@/context/session";
import { reactive } from "vue";
import { useRoute, useRouter } from "vue-router";
import Spinner from "@/components/Spinner.vue";
import TextButton from "@/components/TextButton.vue";
import OauthUtils from "@/utils/oauth";
import LanguageSelector from "@/components/settings/LanguageSelector.vue";

const session = useSession();
const router = useRouter();
const route = useRoute();

const state = reactive<{
    username: string,
    password: string,
    failed: boolean,
    busy: boolean
}>({
    username: "",
    password: "",
    failed: false,
    busy: false
});

const redirectToAuth = () => {
    router.push({
        path: "/auth",
        replace: true,
        query: route.query
    })
}

const onLogin = (e: Event) => {
    const form = e.target as HTMLFormElement;
    if (!form.reportValidity())
        return;

    state.failed = false;
    state.busy = true;

    session
        .login(state.username, state.password)
        .then(ok => {
            if (ok)
                redirectToAuth()
            else
                state.failed = true;
        })
        .finally(() => {
            state.busy = false;
        })

}

const onCancel = () => {
    OauthUtils.cancelAuth();
}

const recoverPassword = () => {
    router.push({
        path: "/recovery",
        replace: false,
        query: route.query
    })
}
</script>

<template>
    <Logo />
    <Heading>{{ $t('login_title') }}</Heading>
    <Panel id="login-view" class="main-panel">
        <form id="credentials" class="form" @submit.prevent="onLogin">
            <div class="form-field">
                <label for="username" id="label-username">{{ $t('login_username') }}</label>
                <input type="text" id="username" autocomplete="username" :required="true" v-model="state.username"
                    :readonly="state.busy" />
            </div>
            <div class="form-field">
                <label for="password" id="label-password">{{ $t('login_password') }}</label>
                <input type="password" id="password" autocomplete="current-password" :required="true"
                    v-model="state.password" :readonly="state.busy" />
            </div>
            <ErrorLabel v-if="state.failed">{{ $t('login.failed') }}</ErrorLabel>
            <input type="submit" id="button-login" :value="$t('login_submit')" v-if="!state.busy" />
            <Spinner v-if="state.busy" />
        </form>
        <LanguageSelector />
    </Panel>
    <TextButton @click="recoverPassword">{{ $t('login_recover_password') }}</TextButton>
    <TextButton @click="onCancel">{{ $t('login_cancel') }}</TextButton>
</template>

<style scoped>
#login-view {
    display: flex;
    flex-direction: column;
    gap: 2rem;
    align-items: center;
}

#credentials {
    display: flex;
    flex-direction: column;
    gap: 1rem;
    width: 100%;
}

.form-field {
    display: flex;
    flex-direction: column;
    gap: 0.5rem;

    input {
        font-size: 1.2rem;
        font-weight: lighter;
    }
}
</style>