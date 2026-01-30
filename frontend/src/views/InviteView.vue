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
import Spinner from "@/components/Spinner.vue";
import LanguageSelector from "@/components/settings/LanguageSelector.vue";
import { reactive } from "vue";
import { useRoute } from "vue-router";
import useInvites from "@/context/invites";
import { useI18n } from "vue-i18n";
import InfoLabel from "@/components/InfoLabel.vue";

const state = reactive<{
    email: string,
    sent_to?: string,
    busy: boolean,
    failed: boolean,
    status: number,
}>({
    email: "",
    busy: false,
    failed: false,
    status: 0
})

//! TODO: Check invite code
const route = useRoute();
const invites = useInvites();
const i18n = useI18n();

const onSend = async (e: Event) => {
    const form = e.target as HTMLFormElement;
    if (!form.reportValidity())
        return;

    state.busy = true;
    state.failed = false;
    state.sent_to = undefined;

    const code = route.params.code as string;
    const lang = i18n.locale.value;

    await invites.sendInvite(code, state.email, lang)
        .then(res => {
            state.failed = !res.ok;
            state.status = res.status;

            if (res.ok) {
                state.sent_to = state.email;
                state.email = "";
            }
        })
        .finally(() => {
            state.busy = false;
        })
}
</script>

<template>
    <Logo />
    <Heading>{{ $t('invite_title') }}</Heading>
    <Panel id="invite-view" class="main-panel">
        <form id="invite-form" class="form" @submit.prevent="onSend">
            <Spinner v-if="state.busy" />
            <template v-else>
                <div class="form-field">
                    <label for="email" id="label-email">{{ $t('invite_email') }}</label>
                    <input type="email" id="email" autocomplete="email" :required="true" v-model="state.email" />
                </div>
                <LanguageSelector />
                <template v-if="state.failed">
                    <ErrorLabel v-if="state.status === 409">{{ $t('invite_failed_conflict') }}</ErrorLabel>
                    <ErrorLabel v-else-if="state.status === 404">{{ $t('invite_failed_not_found') }}</ErrorLabel>
                    <ErrorLabel v-else-if="state.status === 403">{{ $t('invite_failed_forbidden') }}</ErrorLabel>
                    <ErrorLabel v-else>{{ $t('invite_failed_generic') }}</ErrorLabel>
                </template>
                <InfoLabel v-if="state.sent_to">{{ $t('invite_sent', { email: state.sent_to }) }}</InfoLabel>
                <input type="submit" id="button-send" :value="$t('invite_submit')" />
            </template>
        </form>
    </Panel>
</template>

<style scoped>
#invite-view {
    display: flex;
    flex-direction: column;
    gap: 2rem;
    align-items: center;
}

#invite-form {
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

@media screen and (max-height: 400px) and (orientation: landscape) {
    #invite-view {
        gap: 0rem;
    }
}
</style>