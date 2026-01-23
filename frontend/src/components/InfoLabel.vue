<!--
    This Source Code Form is subject to the terms of the Mozilla Public
    License, v. 2.0. If a copy of the MPL was not distributed with this
    file, You can obtain one at https://mozilla.org/MPL/2.0/.
-->

<script setup lang="ts">

</script>

<template>
    <div class="info-label">
        <slot></slot>
    </div>
</template>

<style scopep>
.info-label {
    display: flex;
    flex-direction: column;
    align-items: center;
    padding: 0.4rem 0.8rem;
    background-color: var(--accent-primary-color);
    border: 1px solid var(--accent-secondary-color);
    border-radius: var(--border-radius);
    color: var(--button-text-color);
}
</style>
