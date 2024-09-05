<!--
    This Source Code Form is subject to the terms of the Mozilla Public
    License, v. 2.0. If a copy of the MPL was not distributed with this
    file, You can obtain one at https://mozilla.org/MPL/2.0/.
-->

<script setup lang="ts">
import Spinner from "@/components/Spinner.vue";
import useSession from "@/context/session";
import OauthUtils from "@/utils/oauth";
import { onMounted } from "vue";
import { useRoute, useRouter } from "vue-router";

const query = new URLSearchParams(window.location.search);
const session = useSession();
const router = useRouter();
const route = useRoute();

const redirectToConsent = () => {
    router.push({
        path: "/consent",
        replace: true,
        query: route.query
    })
}

const redirectToLogin = () => {
    router.push({
        path: "/login",
        replace: true,
        query: route.query
    })
}

const redirectToVerification = () => {
    router.push({
        path: "/verify",
        replace: true,
        query: route.query
    })
}

const onAuthorize = () => {
    if (session.session)
        OauthUtils.authorize(session.session);
}

const onInitSession = () => {
    if (session.restore()) {
        session.validate()
            .then(valid => {
                if (valid) {
                    if (session.session?.verified) {
                        if (["false", "False", "0"].includes(query.get("consent") || "1"))
                            onAuthorize();
                        else
                            redirectToConsent();
                    }
                    else {
                        redirectToVerification();
                    }
                }
                else {
                    redirectToLogin();
                }
            })
    }
    else {
        redirectToLogin();
    }
}

onMounted(onInitSession);
</script>

<template>
    <div class="auth-view">
        <Spinner />
        <div>{{ $t('auth_please_wait') }}</div>
    </div>
</template>

<style scoped>
.auth-view {
    display: flex;
    flex-direction: column;
    align-items: center;
    gap: 1rem;
}
</style>