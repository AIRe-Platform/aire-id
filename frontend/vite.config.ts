import { defineConfig } from 'vite'
import { fileURLToPath, URL } from 'node:url'
import vue from '@vitejs/plugin-vue'

// https://vitejs.dev/config/
export default defineConfig({
    plugins: [vue()],
    server: {
        port: 8082
    },
    resolve: {
        alias: {
            '@': fileURLToPath(new URL('./src', import.meta.url))
        }
    },
    build: {
        rollupOptions: {
            output: {
                manualChunks: manualChunks
            }
        }
    }
})

function manualChunks(id: any) {
    if (id.includes('node_modules'))
        return 'vendor';

    if (id.includes('aire-typescript-sdk'))
        return 'aire';

    return null;
}
