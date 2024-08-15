import { apiUrl } from "@/utils/api";
import { reactive } from "vue";

export interface Session {
    token: string;
    username?: string;
    verified?: boolean;
}

class SessionContext {
    session?: Session

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

    public async verifyAuth(): Promise<boolean> {
        if (!this.session)
            return false;

        return await fetch(apiUrl("v1/login/auth"), {
            method: "GET",
            headers: {
                "Authorization": `Bearer ${this.session.token}`
            }
        })
            .then((res) => {
                if (!res.ok) {
                    this.session = undefined;
                }
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
        const credentials = {
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
                    this.store();
                }
                return res.ok;
            })
            .catch((err) => {
                console.log(err);
                return false;
            })
    }
}

const context = reactive<SessionContext>(new SessionContext());

export default function useSession() {
    return context;
}
