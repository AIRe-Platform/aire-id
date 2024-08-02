import { createApp } from 'vue'
import './style.css'
import App from './App.vue'
import router from './routes'
import { initTheme } from './theme'

createApp(App)
    .use(router)
    .mount('#app')

initTheme();