// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.


export function apiUrl(path: string) {
    const url = new URL(window.location.origin + "/api/" + path);
    return url
}

export interface ApiResponse {
    ok: boolean,
    status: number
}
