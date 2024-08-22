<script setup lang="ts">
import Panel from '@/components/Panel.vue';
</script>

<template>
    <Panel id="error-panel" class="main-panel">
        <h1 class="error-title">{{ $t('error.title') }}</h1>
        <code class="error-details">
            {{
                $t('error.details', {
                    error: $route.query.error,
                    description: $route.query.description
                })
            }}
        </code>
    </Panel>
</template>

<style scoped>
.error-title {
    margin-bottom: 1rem;
    text-align: center;
    color: var(--error-color);
}

.error-details {
    background-color: var(--background-color);
    border: 1px solid var(--border-color);
    padding: 1rem;
    white-space: pre-line;
}
</style>