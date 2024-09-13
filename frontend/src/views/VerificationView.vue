<!--
    This Source Code Form is subject to the terms of the Mozilla Public
    License, v. 2.0. If a copy of the MPL was not distributed with this
    file, You can obtain one at https://mozilla.org/MPL/2.0/.
-->

<script setup lang="ts">
import Panel from '@/components/Panel.vue';
import Heading from '@/components/Heading.vue';
import Logo from '@/components/Logo.vue';
import { reactive } from 'vue';
import AccountUtils from '@/utils/account';
import useSession from '@/context/session';
import TextButton from '@/components/TextButton.vue';
import { useRoute, useRouter } from 'vue-router';
import Spinner from '@/components/Spinner.vue';

const session = useSession();
const router = useRouter();
const route = useRoute();

const state = reactive<{
    valid_code: boolean,
    code_sent: boolean,
    code: string,
    error?: string,
    busy: boolean
}>({
    valid_code: false,
    code_sent: false,
    code: "",
    busy: false
})

const checkCode = (e: Event) => {
    const field = e.target as HTMLInputElement
    const match = field.value.match(/(\d+)/g)
    field.value = match?.join("") || ""
    state.valid_code = field.value.length === 6
}

const onResend = () => {
    if (!session.session)
        return;

    state.busy = true;
    AccountUtils.requestVerificationCode(session.session.token)
        .then(res => {
            if (res.ok)
                state.code_sent = true;
            else
                state.error = "verification_resend_failed"
        })
        .finally(() => { state.busy = false })
}

const onCancel = () => {
    session.clear();
    router.push({
        path: "/login",
        replace: true,
        query: route.query
    })
}

const onVerify = () => {
    if (!session.session || !state.valid_code)
        return;

    state.busy = true;
    session.verify(state.code)
        .then(res => {
            if (res)
                router.replace({
                    path: "/auth",
                    replace: true,
                    query: route.query
                })
            else
                state.error = 'verification_failed'
        })
        .finally(() => { state.busy = false })
}
</script>

<template>
    <Logo />
    <Heading>{{ $t('verification_title') }}</Heading>
    <Panel id="verification-view" class="main-panel">
        <div>{{ $t('verification_description') }}</div>
        <div id="verification-error" v-if="state.error && !state.busy">{{ $t(state.error) }}</div>
        <form id="verification-form" @submit.prevent="onVerify">
            <input id="verification-code" type="text" maxlength="6" autocomplete="off" autofocus="true"
                v-model="state.code" :readonly="state.busy" inputmode="numeric" @input="checkCode" />
            <input type="submit" :value="$t('verification_button_confirm')" :disabled="!state.valid_code"
                v-if="!state.busy" />
        </form>
        <Spinner v-if="state.busy" />
        <template v-if="!state.busy">
            <TextButton v-if="!state.code_sent" @click="onResend">
                {{ $t('verification_resend') }}
            </TextButton>
            <span v-if="state.code_sent" id="resend-verification-notify">
                {{ $t('verification_code_sent') }}
            </span>
        </template>
    </Panel>
    <TextButton @click="onCancel" v-if="!state.busy">{{ $t('verification_logout') }}</TextButton>
</template>

<style scoped>
#verification-view {
    display: flex;
    flex-direction: column;
    gap: 1rem;
}

#verification-form {
    display: flex;
    flex-direction: column;
    gap: 1rem;
}

#verification-code {
    text-align: center;
    font-size: 3rem;
}

#verification-error {
    background-color: var(--error-color);
    border: 1px solid var(--error-color);
    border-radius: var(--border-radius);
    color: var(--text-color-dark);
    padding: 1rem;
}
</style>