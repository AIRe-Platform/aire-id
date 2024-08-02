import { createMemoryHistory, createRouter } from "vue-router"
import LoginPrompt from "@/components/LoginPrompt.vue"

const router = createRouter({
    history: createMemoryHistory(),
    routes: [
        { path: "/login", component: LoginPrompt }
    ]
});

export default router;
