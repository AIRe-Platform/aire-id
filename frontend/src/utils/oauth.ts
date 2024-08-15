import { Session } from "@/context/session";
import { apiUrl } from "./api";

const OauthUtils = {
    cancelAuth() {
        const params = new URLSearchParams(window.location.search);
        const redirect_uri = params.get("redirect_uri");
        if (!redirect_uri)
            return;

        const url = new URL(redirect_uri);
        url.searchParams.append("error", "access_denied");
        url.searchParams.append("error_message", "User cancelled");

        const state = params.get("state");
        if (state)
            url.searchParams.append("state", state);

        window.open(url, "_self");
    },
    async authorize(session: Session): Promise<void> {
        const data = new URLSearchParams(window.location.search);

        return await fetch(apiUrl("oauth/authorize"), {
            method: "POST",
            headers: {
                "Content-Type": "application/x-www-form-urlencoded",
                "Authorization": `Bearer ${session.token}`
            },
            body: data.toString()
        })
            .then((res) => {
                if (res.redirected) {
                        window.location.replace(res.url);
                }
            })
            .catch((err) => {
                console.log(err);
            })
    }
}

export default OauthUtils;