<!--
    This Source Code Form is subject to the terms of the Mozilla Public
    License, v. 2.0. If a copy of the MPL was not distributed with this
    file, You can obtain one at https://mozilla.org/MPL/2.0/.
-->

<template>
    <div class="panel">
        <slot></slot>
    </div>
</template>

<style>
.panel {
    display: flex;
    flex-direction: column;
    border: 1px solid var(--border-color);
    border-radius: var(--border-radius);
    background-color: var(--panel-background-color);
    padding: 1.6rem;
    margin: 1rem;
    box-shadow: 0px 0px 10px var(--shadow-color);
}

@media screen and (max-height: 400px) and (orientation: landscape) {
    .panel {
        padding: 0.6rem;
        margin: 0rem;
    }
}
</style>