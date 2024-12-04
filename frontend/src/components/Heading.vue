<!--
    This Source Code Form is subject to the terms of the Mozilla Public
    License, v. 2.0. If a copy of the MPL was not distributed with this
    file, You can obtain one at https://mozilla.org/MPL/2.0/.
-->

<script setup lang="ts">
import { PropType } from 'vue';

const props = defineProps({
    level: {
        type: String as PropType<'h1' | 'h2' | 'h3' | 'h4' | 'h5' | 'h6'>,
        default: 'h1'
    }
})
</script>

<template>
    <component :is="props.level" class="heading">
        <slot></slot>
    </component>
</template>

<style scoped>
.heading {
    font-weight: normal;
    margin-top: 1rem;
}

@media screen and (max-height: 400px) and (orientation: landscape) {
    .heading {
        font-weight: var(--font-small);
        margin: 0.7rem;
    }
}
</style>