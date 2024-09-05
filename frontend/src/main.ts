// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.


import { createApp } from 'vue'
import './style.css'
import App from './App.vue'
import router from './routes'
import { initTheme } from './theme'
import i18n from "./locales";

initTheme();

createApp(App)
    .use(router)
    .use(i18n)
    .mount('#app');
