// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.


import { apiUrl } from "@/utils/api";
import { reactive } from "vue";

class InviteContext {
    public async sendInvite(code: string, email: string, lang: string): Promise<{ ok: boolean, status: number }> {
        const invitation = {
            code: code,
            email: email,
            language: lang
        };

        return await fetch(apiUrl("v1/invite"), {
            method: "POST",
            headers: {
                "Content-Type": "application/json"
            },
            body: JSON.stringify(invitation)
        })
            .then((res) => {
                return { ok: res.ok, status: res.status }
            })
            .catch(() => {
                return { ok: false, status: 0 }
            })
    }
}

const context = reactive(new InviteContext());

export default function useInvites() {
    return context;
}
