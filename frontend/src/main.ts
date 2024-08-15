import { createApp } from 'vue'
import './style.css'
import App from './App.vue'
import router from './routes'
import { initTheme } from './theme'
import i18n from './locale'

initTheme();

createApp(App)
    .use(router)
    .use(i18n)
    .mount('#app');
