<script setup lang="ts">
const onLogin = () => {}
const onConsent = () => {}
const onCancel = () => {}
</script>

<template>
    <div class="main">
        <div id="busy_dialog">
            <div class="spinner"></div>
        </div>
        <div id="login_dialog" class="hidden panel">
            <h1 id="title"></h1>
            <form id="login_form" class="form" @submit="onLogin()">
                <input type="hidden" id="code" />
                <div class="form-field">
                    <label for="username" id="label_username"></label>
                    <input type="text" id="username" autocomplete="username" />
                </div>
                <div class="form-field">
                    <label for="password" id="label_password"></label>
                    <input type="password" id="password" autocomplete="current-password" />
                </div>
                <input type="submit" id="button_login" />
            </form>
        </div>
        <div id="consent_dialog" class="panel">
            <div id="session_user_label"></div>
            <div id="session_user_name"></div>
            <div id="consent_message"></div>
            <div id="app_name"></div>
            <button id="button_consent" @click="onConsent()"></button>
            <button id="button_cancel" @click="onCancel()"></button>
        </div>
        <div id="verification_dialog" class="panel">
            <label for="verification_code" id="label_verification"></label>
            <input type="text" id="verification_code" autocomplete="off" />
            <div id="verification_instruction"></div>
        </div>
        <div id="error_dialog" class="hidden panel"></div>
    </div>
</template>

<style scoped>

</style>