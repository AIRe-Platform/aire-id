// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.


const ValidationUtils = {
    checkPasswordRequirements(password: string) {
        return (
            password.length >= 8 &&
            password.match(/(\d+)/g) != null &&
            password.match(/([A-Z])/g) != null &&
            password.match(/([a-z])/g) != null
        )
    }
}

export default ValidationUtils;