<script setup lang="ts">
import { defineEmits } from "vue";

// Define the click event
const emits = defineEmits<{
    click: [];
}>();
</script>

<template>
    <a href="#" class="text-button" @keydown.space="emits('click')" @click="emits('click')">
        <slot></slot>
    </a>
</template>

<style scoped>
.text-button {
    cursor: pointer;
    color: var(--text-color);
    transition: color 0.5s;
    margin: 0.1rem -0.5rem;
    padding: 0.5rem;
}

.text-button:hover {
    color: var(--hover-link-text);
}

.text-button:focus {
    outline: 2px solid;
    outline-offset: 2px;
}

@media screen and (max-height: 400px) and (orientation: landscape) {
    .text-button {
        font-size: small;
        padding: 0rem;
    }
}
</style>
