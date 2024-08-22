import AccountUtils from "@/utils/account";
import { apiUrl } from "@/utils/api";
import { reactive } from "vue";

export interface Session {
    token: string;
    username?: string;
    verified?: boolean;
}

interface Credentials {
    username: string;
    password: string;
}

class SessionContext {
    session?: Session
    verification_credentials?: Credentials;

    constructor() { }

    public restore(): boolean {
        const session = localStorage.getItem("session");
        if (session) {
            this.session = JSON.parse(session);
        }
        return (this.session !== undefined);
    }

    public store() {
        if (this.session) {
            localStorage.setItem("session", JSON.stringify(this.session));
        }
        else {
            localStorage.removeItem("session");
        }
    }

    public clear() {
        this.session = undefined;
        this.store();
    }

    public async validate(): Promise<boolean> {
        if (!this.session)
            return false;

        return await fetch(apiUrl("v1/login/auth"), {
            method: "GET",
            headers: {
                "Authorization": `Bearer ${this.session.token}`
            }
        })
            .then((res) => {
                if (!res.ok)
                    this.session = undefined;

                return res.ok;
            })
            .catch((err) => {
                console.log(err);
                this.session = undefined;
                return false;
            })
            .finally(() => {
                this.store();
            })
    }

    public async login(username: string, password: string): Promise<boolean> {
        const credentials: Credentials = {
            username: username,
            password: password
        };

        return await fetch(apiUrl("v1/login"), {
            method: "POST",
            headers: {
                "Content-Type": "application/json",
                "Accept": "application/json"
            },
            body: JSON.stringify(credentials)
        })
            .then(async (res) => {
                if (res.ok) {
                    const body = await res.json();
                    this.session = {
                        token: body.token,
                        verified: body.verified,
                        username: username
                    };

                    if (!this.session.verified)
                        this.verification_credentials = credentials;

                    this.store();
                }
                return res.ok;
            })
            .catch((err) => {
                console.log(err);
                return false;
            })
    }

    public async verify(code: string): Promise<boolean> {
        if (!this.verification_credentials || !this.session)
            return false;

        this.session.verified = await AccountUtils.verify(this.session.token, code)
            .then(res => res.ok)

        if (this.session.verified)
            return this.login(
                this.verification_credentials.username,
                this.verification_credentials.password)
                .then(res => {
                    this.verification_credentials = undefined;
                    return res;
                });
        else
            return false;

    }
}

const context = reactive<SessionContext>(new SessionContext());

export default function useSession() {
    return context;
}
