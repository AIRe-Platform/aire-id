<script setup lang="ts">
const onLogin = () => {}
</script>

<template>
    <div id="login-form" class="panel">
        <h2 id="title"></h2>
        <form id="credentials" class="form" @submit="onLogin">
            <input type="hidden" id="code" />
            <div class="form-field">
                <label for="username" id="label-username"></label>
                <input type="text" id="username" autocomplete="username" />
            </div>
            <div class="form-field">
                <label for="password" id="label-password"></label>
                <input type="password" id="password" autocomplete="current-password" />
            </div>
            <input type="submit" id="button-login" />
        </form>
    </div>
</template>