<script setup lang="ts">
</script>

<template>
    <div class="spinner"></div>
</template>

<style>
.spinner {
    width: 56px;
    min-width: 56px;
    height: 56px;
    min-height: 56px;
    border-radius: 50%;
    background:
        radial-gradient(farthest-side, var(--accent-primary-color) 94%, #0000) top/9px 9px no-repeat,
        conic-gradient(#0000 30%, var(--accent-primary-color));
    mask: radial-gradient(farthest-side, #0000 calc(100% - 9px), #000 0);
    animation: spinner-frames 1s infinite linear;
    margin: 1rem;
    align-self: center;
}

@keyframes spinner-frames {
    100% {
        transform: rotate(1turn);
    }
}
</style>