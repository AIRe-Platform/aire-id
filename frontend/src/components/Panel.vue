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
</style>