// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.


import { createRouter, createWebHistory } from "vue-router";
import NotFoundView from "@/views/NotFoundView.vue";
import AuthView from "@/views/AuthView.vue";
import LoginView from "@/views/LoginView.vue";
import LogoutView from "@/views/LogoutView.vue";
import ConsentView from "./views/ConsentView.vue";
import VerificationView from "./views/VerificationView.vue";
import RecoveryView from "./views/RecoveryView.vue";
import ErrorView from "./views/ErrorView.vue";
import useSession from "./context/session";

const router = createRouter({
    history: createWebHistory("app"),
    routes: [
        { path: "/login", component: LoginView, meta: { requireSession: false } },
        { path: "/error", component: ErrorView, meta: { requireSession: false } },
        { path: "/logout", component: LogoutView, meta: { requireSession: false } },
        { path: "/consent", component: ConsentView, meta: { requireSession: true }  },
        { path: "/verify", component: VerificationView, meta: { requireSession: true }  },
        { path: "/recovery", component: RecoveryView, meta: { requireSession: false }  },
        { path: "/auth", component: AuthView, meta: { requireSession: false }  },
        {
            path: "/:pathMatch(.*)*",
            component: NotFoundView,
        },
    ]
});

router.beforeEach((to, _) => {
    const session = useSession();
    if(to.meta.requireSession) {
        if(!session.session)
            return "/auth"
    }
})

export default router;
