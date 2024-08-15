<script setup lang="ts">
import Heading from '@/components/Heading.vue';
import Logo from '@/components/Logo.vue';
import Panel from '@/components/Panel.vue';
import Spinner from '@/components/Spinner.vue';
import TextButton from '@/components/TextButton.vue';
import { requested_locale } from '@/locale';
import AccountUtils from '@/utils/account';
import ValidationUtils from '@/utils/validation';
import { reactive } from 'vue';
import { useRoute, useRouter } from 'vue-router';

const router = useRouter();
const route = useRoute();

enum Steps {
    Step1_Email,
    Step2_PasswordReset,
    Step3_Completed
}

const state = reactive<{
    step: Steps,
    valid_code: boolean,
    busy: boolean,
    email: string,
    code: string,
    password: string,
    error?: string
}>({
    step: Steps.Step1_Email,
    valid_code: false,
    busy: false,
    email: "",
    code: "",
    password: "",
});

const checkCode = (e: Event) => {
    const field = e.target as HTMLInputElement
    const match = field.value.match(/(\d+)/g)
    field.value = match?.join("").substring(0, 6) || ""
    state.valid_code = field.value.length === 6
}

const onSubmitEmail = (e: Event) => {
    const form = e.target as HTMLFormElement;
    if (!form.reportValidity())
        return;

    state.busy = true;
    state.error = undefined;

    AccountUtils.requestRecoveryCode(state.email, requested_locale())
        .then(res => {
            if (res.ok)
                state.step = Steps.Step2_PasswordReset;
            else
                state.error = "recovery.failed";
        })
        .finally(() => { state.busy = false; })
}

const onSubmitPassword = (e: Event) => {
    const form = e.target as HTMLFormElement;
    if (!form.reportValidity())
        return;

    if (!ValidationUtils.checkPasswordRequirements(state.password)) {
        state.error = "recovery.password_requirements_not_met";
        return;
    }

    state.busy = true;
    state.error = undefined;

    AccountUtils.recover(state.code, state.email, state.password, requested_locale())
        .then(res => {
            if (res.ok)
                state.step = Steps.Step3_Completed;
            else
                state.error = "recovery.failed"
        })
        .finally(() => { state.busy = false; })
}

const onCancel = () => {
    router.push({
        path: "/login",
        replace: false,
        query: route.query
    })
}
</script>

<template>
    <Logo />
    <Heading>{{ $t('recovery.title') }}</Heading>
    <Panel id="recovery-view" class="main-panel">
        <div class="recovery-error" v-if="state.error">{{ $t(state.error) }}</div>
        <form class="recovery-form" @submit.prevent="onSubmitEmail" v-if="state.step == Steps.Step1_Email">
            <div class="recovery-instruction">{{ $t('recovery.step1') }}</div>
            <input id="recovery-email" type="email" autocomplete="email" :required="true" :readonly="state.busy"
                v-model="state.email" />
            <input type="submit" :value="$t('recovery.button_continue')" v-if="!state.busy" />
        </form>
        <form class="recovery-form" @submit.prevent="onSubmitPassword" v-if="state.step == Steps.Step2_PasswordReset">
            <div class="recovery-instruction">{{ $t('recovery.step2') }}</div>

            <label for="recovery-code">{{ $t('recovery.label_code') }}</label>
            <input id="recovery-code" type="text" autocomplete="off" :required="true" :readonly="state.busy"
                @input="checkCode" v-model="state.code" />

            <label for="recovery-password">{{ $t('recovery.label_password') }}</label>
            <input id="recovery-password" type="password" autocomplete="new-password" :required="true"
                :readonly="state.busy" v-model="state.password" minlength="8" />

            <input type="submit" :value="$t('recovery.button_continue')" v-if="!state.busy" />
        </form>
        <div class="recovery-completed" v-if="state.step == Steps.Step3_Completed">{{ $t('recovery.step3') }}</div>
        <Spinner v-if="state.busy" />
    </Panel>
    <TextButton @click="onCancel" v-if="!state.busy">{{ $t('recovery.button_back') }}</TextButton>
</template>

<style scoped>
#recovery-view {
    display: flex;
    flex-direction: column;
    gap: 1rem;
}

.recovery-form {
    display: flex;
    flex-direction: column;
    gap: 1rem;

    input {
        font-size: large;
    }
}

.recovery-error {
    display: flex;
    flex-direction: row;
    padding: 1rem;
    background-color: var(--error-color);
    border: 1px solid var(--error-color);
    border-radius: var(--border-radius);
    color: var(--text-color-dark);
}
</style>