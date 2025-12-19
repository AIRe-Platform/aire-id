// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.


import { Locale } from ".";

const es: Locale = {
    settings_language: "Idioma de la interfaz de usuario",

    auth_please_wait: "Por favor, espere...",
    
    login_title: "Iniciar sesión",
    login_username: "Nombre de usuario o correo electrónico",
    login_password: "Contraseña",
    login_submit: "Iniciar sesión",
    login_cancel: "Cancelar",
    login_recover_password: "¿Olvidó su contraseña?",
    login_failed: "¡Error al iniciar sesión! Por favor, verifica tu nombre de usuario y contraseña.",

    logout_please_wait: "Cerrando sesión...",
    logout_logged_out: "Has cerrado sesión. Ahora puedes cerrar esta página.",

    consent_title: "Confirmar inicio de sesión",
    consent_logged_in_as: "Has iniciado sesión como {username}.",
    consent_click_to_logout: "¿No eres tú? Haz clic aquí para cerrar sesión.",
    consent_disclaimer: "Estás iniciando sesión en {service} en {platform}. Esto le da acceso al servicio a tu información. ¿Deseas continuar?",
    consent_button_consent: "Confirmar",
    consent_button_cancel: "Cancelar",

    verification_title: "Verificación de cuenta",
    verification_description: "Por favor, ingresa el código de verificación que hemos enviado a tu dirección de correo electrónico.",
    verification_resend: "Enviar un nuevo código",
    verification_code_sent: "¡Un nuevo código está en camino!",
    verification_logout: "Cerrar sesión",
    verification_failed: "Error al verificar la cuenta. El código puede ser inválido o haber expirado.",
    verification_resend_failed: "No se pudo enviar un nuevo código. Por favor, inténtalo de nuevo más tarde.",
    verification_button_confirm: "Verificar",

    recovery_title: "Recuperación de cuenta",
    recovery_step1: "Por favor, ingresa tu dirección de correo electrónico. Si la cuenta existe, recibirás un correo electrónico con el código de recuperación que necesitas ingresar en el siguiente paso.",
    recovery_step2: "Por favor, ingresa el código de recuperación que recibiste y la nueva contraseña que deseas usar.",
    recovery_step3: "¡Tu contraseña ha sido cambiada exitosamente!",
    recovery_label_code: "Código de recuperación",
    recovery_label_password: "Nueva contraseña",
    recovery_button_continue: "Continuar",
    recovery_button_back: "Volver al inicio de sesión",
    recovery_failed: "Esta cuenta no se puede recuperar. Por favor, contacta con el soporte.",
    recovery_password_requirements_not_met: "La contraseña debe tener al menos 8 caracteres y contener letras mayúsculas y minúsculas.",

    error_not_found: "La página solicitada no existe. (404)",
    error_title: "Fallo en la autenticación",
    error_details: `Error al autenticar. Por favor, inténtalo de nuevo más tarde.

            Error: {error}
            Descripción: {description}
        `,
    
    en: "Inglés",
    fi: "Finlandés",
    es: "Español",
    vi: "Vietnamita",
    id: "Indonesio",
    sw: "Suajili",
    rw: "Kinyarwanda"
};


export default es;
