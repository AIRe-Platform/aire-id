// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.


import { Locale } from ".";

const en: Locale = {
    settings_language: "The language of the AIRe Service",

    auth_please_wait: "Please wait...",

    login_title: "Login",
    login_username: "Username or email",
    login_password: "Password",
    login_submit: "Login",
    login_cancel: "Cancel",
    login_recover_password: "Forgot password?",
    login_failed: "Login failed! Please check the username and password.",

    logout_please_wait: "Logging out...",
    logout_logged_out: "You have been logged out. You can now close this page.",

    consent_title: "Confirm Login",
    consent_logged_in_as: "You are logged in as {username}.",
    consent_click_to_logout: "Wrong user account? Log out here.",
    consent_disclaimer: "You are logging into {service} on {platform}. This gives the service access to your information. Do you wish to proceed?",
    consent_button_consent: "Confirm",
    consent_button_cancel: "Cancel",

    verification_title: "Account Verification",
    verification_description: "Please enter the verification code we have sent to your email address.",
    verification_resend: "Send a new code",
    verification_code_sent: "A new code is on its way!",
    verification_logout: "Log out",
    verification_failed: "Failed to verify the account. The code may be invalid or expired.",
    verification_resend_failed: "Could not send a new code. Please try again later.",
    verification_button_confirm: "Verify",

    recovery_title: "Account Recovery",
    recovery_step1: "Please enter your email address. If the account exists, you'll receive an email from us with the recovery code you need to enter in the next step.",
    recovery_step2: "Please enter the recovery code you received and a new password you wish to use.",
    recovery_step3: "Your password has been changed successfully!",
    recovery_label_code: "Recovery code",
    recovery_label_password: "New password",
    recovery_button_continue: "Continue",
    recovery_button_back: "Back to login",
    recovery_failed: "This account cannot be recovered. Please contact support.",
    recovery_password_requirements_not_met: "The password must contain at least 8 characters as well as upper and lower case letters",

    invite_title: "Invite a new user",
    invite_email: "Email address",
    invite_failed_generic: "Failed to send an invite. Try again later.",
    invite_failed_conflict: "Cannot send an invite to this email. It may already be in use or invited already.",
    invite_failed_not_found: "Invalid invite code. The QR code or invitation link you used may not exist.",
    invite_failed_forbidden: "Expired invite code. The QR code or invitation link you used might be expired.",
    invite_sent: "Invitation sent to '{email}' successfully!",
    invite_submit: "Send invitation",

    error_not_found: "The requested page does not exist. (404)",
    error_title: "Authentication failed",
    error_details: `Failed to authenticate. Please try again later.

            Error: {error}
            Description: {description}
        `,

    en: "English",
    fi: "Finnish",
    sv: "Swedish",
    es: "Spanish",
    vi: "Vietnamese",
    id: "Indonesian",
    sw: "Swahili",
    rw: "Kinyarwanda"
};

export default en;
