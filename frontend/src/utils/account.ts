import { ApiResponse, apiUrl } from "./api";

const AccountUtils = {
    async verify(token: string, code: string): Promise<ApiResponse> {
        return fetch(apiUrl("v1/verify/" + code), {
            method: "POST",
            headers: {
                "Authorization": `Bearer ${token}`
            }
        }).then(res => {
            return { ok: res.ok, status: res.status }
        })
    },
    async requestVerificationCode(token: string): Promise<ApiResponse> {
        return fetch(apiUrl("v1/verify/resend"), {
            method: "POST",
            headers: {
                "Authorization": `Bearer ${token}`
            }
        }).then(res => {
            return { ok: res.ok, status: res.status }
        })
    },
    async requestRecoveryCode(email: string, lang?: string): Promise<ApiResponse> {
        const body = {
            email: email,
            language: lang
        };

        return fetch(apiUrl("v1/recovery/code"), {
            method: "POST",
            headers: {
                "Content-Type": "application/json"
            },
            body: JSON.stringify(body)
        }).then(res => {
            return { ok: res.ok, status: res.status }
        })
    },
    async recover(code: string, email: string, password: string, lang?: string): Promise<ApiResponse> {
        const body = {
            email: email,
            code: code,
            password: password,
            language: lang
        };

        return fetch(apiUrl("v1/recovery/password"), {
            method: "POST",
            headers: {
                "Content-Type": "application/json"
            },
            body: JSON.stringify(body)
        }).then(res => {
            return { ok: res.ok, status: res.status }
        })
    }
}

export default AccountUtils;