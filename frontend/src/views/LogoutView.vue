<!--
    This Source Code Form is subject to the terms of the Mozilla Public
    License, v. 2.0. If a copy of the MPL was not distributed with this
    file, You can obtain one at https://mozilla.org/MPL/2.0/.
-->

<script setup lang="ts">
import Heading from "@/components/Heading.vue";
import Spinner from "@/components/Spinner.vue";
import useSession from "@/context/session";
import { onMounted, reactive } from "vue";
import { useRoute } from "vue-router";

const session = useSession();
const route = useRoute();
const state = reactive<{
    logged_out: boolean
}>({
    logged_out: false
});

const onLogout = () => {
    session.clear();
    if (route.query.return_url) {
        const url = new URL(route.query.return_url as string);
        window.open(url, "_self");
    }
    state.logged_out = true;
}

onMounted(onLogout);
</script>

<template>
    <div class="logout-view">
        <template v-if="!state.logged_out">
            <Spinner />
            <Heading>{{ $t('logout_please_wait') }}</Heading>
        </template>
        <div v-if="state.logged_out">{{ $t('logout_logged_out') }}</div>
    </div>
</template>

<style scoped>
.logout-view {
    display: flex;
    flex-direction: column;
    align-items: center;
    gap: 1rem;
}
</style>