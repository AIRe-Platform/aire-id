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