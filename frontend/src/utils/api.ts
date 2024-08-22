export function apiUrl(path: string) {
    const url = new URL(window.location.origin + "/api/" + path);
    return url
}

export interface ApiResponse {
    ok: boolean,
    status: number
}
