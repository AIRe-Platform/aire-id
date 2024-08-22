import { Session } from "@/context/session";
import { apiUrl } from "./api";
import router from "@/routes";
import ErrorResponse from "@/models/error_response";

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
            .then(async (res) => {
                if (res.redirected) {
                    window.location.replace(res.url);
                }
                else {
                    const err = await res.json() as ErrorResponse;
                    router.push({
                        path: "/error",
                        query: {
                            error: err.error,
                            description: err.error_description
                        },
                        replace: true
                    })
                }
            })
            .catch((err) => {
                console.log(err);
            })
    }
}

export default OauthUtils;