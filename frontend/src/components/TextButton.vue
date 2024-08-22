<script setup lang="ts">
const emits = defineEmits<{
    click: []
}>();
</script>

<template>
    <div class="text-button" @click="emits('click')">
        <slot></slot>
    </div>
</template>

<style scoped>
.text-button {
    text-decoration: none;
    cursor: pointer;
    color: var(--text-color);
    transition: color 0.5s;
    margin: 0.1rem -0.5rem;
    padding: 0.5rem;

    &:hover {
        color: var(--accent-primary-color);
    }
}
</style>