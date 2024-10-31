<!--
    This Source Code Form is subject to the terms of the Mozilla Public
    License, v. 2.0. If a copy of the MPL was not distributed with this
    file, You can obtain one at https://mozilla.org/MPL/2.0/.
-->

<script setup lang="ts">
import { defineEmits } from "vue";

const emits = defineEmits<{
    click: []
}>();
</script>

<template>
    <div class="text-button"
        tabindex="0"
        @keydown.prevent.space.enter="emits('click')"
        @click="emits('click')">
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