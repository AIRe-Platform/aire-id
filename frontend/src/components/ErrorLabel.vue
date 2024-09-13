<!--
    This Source Code Form is subject to the terms of the Mozilla Public
    License, v. 2.0. If a copy of the MPL was not distributed with this
    file, You can obtain one at https://mozilla.org/MPL/2.0/.
-->

<script setup lang="ts">

</script>

<template>
    <div class="error-label">
        <slot></slot>
    </div>
</template>

<style scopep>
.error-label {
    display: flex;
    flex-direction: column;
    align-items: center;
    padding: 0.4rem 0.8rem;
    background-color: var(--error-color);
    border: 1px solid var(--error-color);
    border-radius: var(--border-radius);
    color: var(--border-color);
}
</style>
