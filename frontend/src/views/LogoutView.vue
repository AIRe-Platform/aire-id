<script setup lang="ts">
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
            <div>{{ $t('logout.please_wait') }}</div>
        </template>
        <div v-if="state.logged_out">{{ $t('logout.logged_out') }}</div>
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