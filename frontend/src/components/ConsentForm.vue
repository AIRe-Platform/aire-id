<script setup lang="ts">
const onConsent = () => {}
const onCancel = () => {}
</script>

<template>
    <div id="consent-form" class="panel">
        <div id="session_user_label"></div>
        <div id="session_user_name"></div>
        <div id="consent_message"></div>
        <div id="app_name"></div>
        <button id="button_consent" @click="onConsent()"></button>
        <button id="button_cancel" @click="onCancel()"></button>
    </div>
</template>