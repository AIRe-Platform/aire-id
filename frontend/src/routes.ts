import { createRouter, createWebHistory } from "vue-router";
import NotFoundView from "@/views/NotFoundView.vue";
import AuthView from "@/views/AuthView.vue";

const router = createRouter({
    history: createWebHistory("app"),
    routes: [
        { path: "/auth", component: AuthView },
        {
            path: "/:pathMatch(.*)*",
            component: NotFoundView,
        },
    ]
});

export default router;
