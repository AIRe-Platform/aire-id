import { reactive } from "vue";

export interface Session {
    token: string;
    username: string;
}

class SessionContext {
    session?: Session

    constructor() {

    }

    public restore(): Session | undefined {
        const token = localStorage.getItem("session_token");
        const username = localStorage.getItem("session_username");

        if (!token || !username)
            return;

        return { token: token, username: username };
    }

    public store() {
        if (this.session) {
            localStorage.setItem("session_token", this.session.token);
            localStorage.setItem("session_username", this.session.username);
        }
        else {
            localStorage.removeItem("session_token");
            localStorage.removeItem("session_username");
        }
    }

    public async verify(): Promise<boolean> {
        if (!this.session)
            return false;

        return await fetch("/login/session", {
            method: "POST",
            headers: {
                "Content-Type": "application/json"
            },
            body: JSON.stringify({ token: this.session.token })
        })
            .then((res) => {
                return res.ok;
            })
            .catch((err) => {
                console.log(err);
                return false;
            })
    }

    public async login(username: string, password: string) {
        const credentials = {
            username: username,
            password: password
        };

        return await fetch("/login", {
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
                    this.session = { token: body.token, username: username };
                    return body;
                }
                return null;
            })
            .catch((err) => {
                console.log(err);
                return null;
            })
    }

    public async authorize(token: string, params: string) {
        const data = new URLSearchParams(params);

        return await fetch("/api/oauth/authorize", {
            method: "POST",
            headers: {
                "Content-Type": "application/x-www-form-urlencoded",
                "Authorization": `Bearer ${token}`
            },
            body: new FormData(data as any)
        })
            .then((res) => {
                if (res.redirected) {
                    const location = res.headers.get("Location");
                    if (location)
                        window.location.replace(location);
                    else
                        throw Error("Missing Location header from authorization response");
                }
            })
            .catch((err) => {
                console.log(err);
            })
    }
}

const context = reactive<SessionContext>(new SessionContext());

export default function useSession() {
    return context;
}
