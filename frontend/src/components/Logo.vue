<!--
    This Source Code Form is subject to the terms of the Mozilla Public
    License, v. 2.0. If a copy of the MPL was not distributed with this
    file, You can obtain one at https://mozilla.org/MPL/2.0/.
-->
<script setup lang="ts">
import { initTheme } from '@/theme';
const theme = initTheme();
const isLightMode = theme === "light-mode";

</script>

<template>
    <div class="aire-logo-container">
        <img v-if="isLightMode" src="/assets/aire-logo-letter.svg" class="aire-logo" alt="AIRe logo" />
        <img v-else src="/assets/aire-logo-letter-dark-mode.svg" class="aire-logo" alt="AIRe logo dark mode" />
    </div>
</template>

<style scoped>
.aire-logo-container {
    display: flex;
    justify-content: center;
    align-items: center;
    align-self: center;
    justify-self: center;
}

.aire-logo {
    width: 6rem;
    height: auto;
}
</style>