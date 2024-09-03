// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.


import { createI18n } from "vue-i18n";

const STRINGS = {
    en: {
        auth: {
            please_wait: "Please wait..."
        },
        login: {
            title: "Login",
            username: "Username or email",
            password: "Password",
            submit: "Login",
            cancel: "Cancel",
            recover_password: "Forgot password?",
            failed: "Login failed! Please check the username and password."
        },
        logout: {
            please_wait: "Logging out...",
            logged_out: "You have been logged out. You can now close this page."
        },
        consent: {
            title: "Confirm Login",
            logged_in_as: "You are logged in as {username}.",
            click_to_logout: "Not you? Click here to logout.",
            disclaimer: "You are logging into {service}. This gives the service access to your information. Do you wish proceed?",
            button_consent: "Confirm",
            button_cancel: "Cancel"
        },
        verification: {
            title: "Account Verification",
            description: "Please enter the verification code we have sent to your email address.",
            resend: "Send a new code",
            code_sent: "A new code is on its way!",
            logout: "Log out",
            failed: "Failed to verify the account. The code may be invalid or expired.",
            resend_failed: "Could not send a new code. Please try again later.",
            button_confirm: "Verify"
        },
        recovery: {
            title: "Account Recovery",
            step1: "Please enter your email address. If the account exists, you'll receive an email from us with the recovery code you need to enter in the next step.",
            step2: "Please enter the recovery code you received and a new password you wish to use.",
            step3: "Your password has been changed successfully!",
            label_code: "Recovery code",
            label_password: "New password",
            button_continue: "Continue",
            button_back: "Back to login",
            failed: "This account cannot be recovered. Please contact support.",
            password_requirements_not_met: "The password must contain at least 8 characters as well as upper and lower case letters"
        },
        not_found: "The requested page does not exist. (404)",
        error: {
            title: "Authentication failed",
            details: `Failed to authenticate. Please try again later.

                Error: {error}
                Description: {description}
            `
        }
    },
    fi: {
        auth: {
            please_wait: "Hetkinen..."
        },
        login: {
            title: "Kirjautuminen",
            username: "Käyttäjätunnus",
            password: "Salasana",
            submit: "Kirjaudu",
            cancel: "Peruuta",
            failed: "Kirjautuminen epäonnistui! Tarkista käyttäjätunnus ja salasana.",
            recover_password: "Salasana unohtunut?"
        },
        logout: {
            please_wait: "Kirjaudutaan ulos...",
            logged_out: "Sinut on kirjattu ulos. Voit sulkea tämän sivun."
        },
        consent: {
            title: "Vahvista kirjautuminen",
            logged_in_as: "Olet kirjautunut käyttäjänä {username}.",
            click_to_logout: "Etkö ole hän? Kirjaudu ulos klikkaamalla tästä.",
            disclaimer: "Olet kirjautumassa palveluun {service}. Tämä antaa palvelulle pääsyn tietoihisi. Haluatko jatkaa?",
            button_consent: "Vahvista",
            button_cancel: "Peruuta"
        },
        verification: {
            title: "Tilin vahvistaminen",
            description: "Syötä tähän vahvistuskoodi, jonka lähetimme antamaasi sähköpostiosoitteeseen.",
            resend: "Lähetä uusi koodi",
            code_sent: "Uusi koodi on matkalla!",
            logout: "Kirjaudu ulos",
            failed: "Tilin vahvistaminen epäonnistui. Koodi voi olla väärä tai vanhentunut.",
            resend_failed: "Koodin lähettäminen epäonnistui. Yritä myöhemmin uudelleen.",
            button_confirm: "Vahvista"
        },
        recovery: {
            title: "Tilin palauttaminen",
            step1: "Syötä käyttäjätilisi sähköpostiosoite. Mikäli tili on olemassa, lähetämme palautuskoodin, jota tarvitset seuraavassa vaiheessa.",
            step2: "Syötä saamasi palautuskoodi sekä uusi salasana.",
            step3: "Salasanasi on vaihdettu onnistuneesti!",
            label_code: "Palautuskoodi",
            label_password: "Uusi salasana",
            button_continue: "Jatka",
            button_back: "Takaisin kirjautumiseen",
            failed: "Tätä tiliä ei voida palauttaa. Ole hyvä ja ota yhteys tukeen.",
            password_requirements_not_met: "Salasanan tulee olla vähintään 8 merkkiä pitkä sekä sisältää isoja ja pieniä kirjaimia."
        },
        not_found: "Pyydettyä sivua ei löydy. (404)",
        error: {
            title: "Tunnistautuminen epäonnistui",
            details: `Tunnistautuminen ei onnistunut. Yritä myöhemmin uudelleen.

                Virhe: {error}
                Lisätiedot: {description}
            `
        }
    },
    id: {
        auth: {
            please_wait: "Silakan tunggu..."
        },
        login: {
            title: "Masuk",
            username: "Nama pengguna atau email",
            password: "Kata sandi",
            submit: "Masuk",
            cancel: "Batal",
            recover_password: "Lupa kata sandi?",
            failed: "Login gagal! Harap periksa nama pengguna dan kata sandi."
        },
        logout: {
            please_wait: "Keluar...",
            logged_out: "Anda telah keluar. Sekarang Anda dapat menutup halaman ini."
        },
        consent: {
            title: "Konfirmasi Login",
            logging_in_as: "Anda login sebagai {username}.",
            click_to_logout: "Bukan Anda? Klik di sini untuk logout.",
            disclaimer: "Anda login ke {service}. Ini memberi layanan akses ke informasi Anda. Apakah Anda ingin melanjutkan?",
            button_consent: "Konfirmasi",
            button_cancel: "Batal"
        },
        verification: {
            title: "Verifikasi Akun",
            description: "Silakan masukkan kode verifikasi yang telah kami kirim ke alamat email Anda.",
            resend: "Kirim kode baru",
            code_sent: "Kode baru sedang dalam perjalanan!",
            logout: "Keluar",
            failed: "Gagal memverifikasi akun. Kode mungkin tidak valid atau kedaluwarsa.",
            resend_failed: "Tidak dapat mengirim kode baru. Silakan coba lagi nanti.",
            button_confirm: "Verifikasi"
        },
        recovery: {
            title: "Pemulihan Akun",
            step1: "Silakan masukkan alamat email Anda. Jika akun tersebut ada, Anda akan menerima email dari kami dengan kode pemulihan yang perlu Anda masukkan pada langkah berikutnya.",
            step2: "Silakan masukkan kode pemulihan yang Anda terima dan kata sandi baru yang ingin Anda gunakan.",
            step3: "Kata sandi Anda telah berhasil diubah!",
            label_code: "Kode pemulihan",
            label_password: "Kata sandi baru",
            button_continue: "Lanjutkan",
            button_back: "Kembali ke login",
            failed: "Akun ini tidak dapat dipulihkan. Silakan hubungi dukungan.",
            password_requirements_not_met: "Kata sandi harus berisi setidaknya 8 karakter serta huruf besar dan kecil"
        },
        not_found: "Halaman yang diminta tidak ada. (404)",
        error: {
            title: "Autentikasi gagal",
            details: `Gagal mengautentikasi. Silakan coba lagi nanti.
        
                Kesalahan: {error}
                Deskripsi: {description}
            `
        }
    },
    es: {
        auth: {
            please_wait: "Por favor, espere..."
        },
        login: {
            title: "Iniciar sesión",
            username: "Nombre de usuario o correo electrónico",
            password: "Contraseña",
            submit: "Iniciar sesión",
            cancel: "Cancelar",
            recover_password: "¿Olvidó su contraseña?",
            failed: "¡Error al iniciar sesión! Verifique el nombre de usuario y la contraseña."
        },
        logout: {
            please_wait: "Cerrando sesión...",
            logged_out: "Se ha cerrado la sesión. Ahora puede cerrar esta página.",
        },
        consent: {
            title: "Confirmar inicio de sesión",
            logged_in_as: "Ha iniciado sesión como {username}.",
            click_to_logout: "¿No es usted? Haga clic aquí para cerrar sesión.",
            disclaimer: "Está iniciando sesión en {service}. Esto le da al servicio acceso a su información. ¿Desea continuar?",
            button_consent: "Confirmar",
            button_cancel: "Cancelar"
        },
        verification: {
            title: "Verificación de cuenta",
            description: "Ingrese el código de verificación que le enviamos a su dirección de correo electrónico.",
            resend: "Enviar un nuevo código",
            code_sent: "¡Un nuevo código está en camino!",
            logout: "Cerrar sesión",
            failed: "No se pudo verificar la cuenta. El código puede no ser válido o estar vencido.",
            resend_failed: "No se pudo enviar un nuevo código. Inténtelo nuevamente más tarde.",
            button_confirm: "Verificar"
        },
        recovery: {
            title: "Recuperación de cuenta",
            step1: "Por favor, introduzca su dirección de correo electrónico. Si la cuenta existe, recibirá un correo electrónico nuestro con el código de recuperación que deberá introducir en el siguiente paso.",
            step2: "Por favor, introduzca el código de recuperación que ha recibido y la nueva contraseña que desea utilizar.",
            step3: "¡Su contraseña se ha cambiado correctamente!",
            label_code: "Código de recuperación",
            label_password: "Nueva contraseña",
            button_continue: "Continuar",
            button_back: "Volver al inicio de sesión",
            failed: "Esta cuenta no se puede recuperar. Póngase en contacto con el servicio de asistencia.",
            password_requirements_not_met: "La contraseña debe contener al menos 8 caracteres, así como letras mayúsculas y minúsculas"
        },
        not_found: "La página solicitada no existe. (404)",
        error: {
            title: "Error de autenticación",
            details: `Error de autenticación. Inténtelo de nuevo más tarde.
        
                Error: {error}
                Descripción: {description}
            `
        }
    },
    vi: {
        auth: {
            please_wait: "Vui lòng đợi..."
        },
        login: {
            title: "Đăng nhập",
            username: "Tên người dùng hoặc email",
            password: "Mật khẩu",
            submit: "Đăng nhập",
            cancel: "Hủy",
            recover_password: "Quên mật khẩu?",
            failed: "Đăng nhập không thành công! Vui lòng kiểm tra tên người dùng và mật khẩu."
        },
        logout: {
            please_wait: "Đang đăng xuất...",
            logged_out: "Bạn đã đăng xuất. Bây giờ bạn có thể đóng trang này."
        },
        consent: {
            title: "Xác nhận đăng nhập",
            logged_in_as: "Bạn đã đăng nhập với tư cách là {username}.",
            click_to_logout: "Không phải bạn? Nhấp vào đây để đăng xuất.",
            disclaimer: "Bạn đang đăng nhập vào {service}. Điều này cho phép dịch vụ truy cập thông tin của bạn. Bạn có muốn tiếp tục không?",
            button_consent: "Xác nhận",
            button_cancel: "Hủy"
        },
        verification: {
            title: "Xác minh tài khoản",
            description: "Vui lòng nhập mã xác minh mà chúng tôi đã gửi đến địa chỉ email của bạn.",
            resend: "Gửi mã mới",
            code_sent: "Một mã mới đang trên đường đến!",
            logout: "Đăng xuất",
            failed: "Không xác minh được tài khoản. Mã có thể không hợp lệ hoặc đã hết hạn.",
            resend_failed: "Không thể gửi mã mới. Vui lòng thử lại sau.",
            button_confirm: "Xác minh"
        },
        recovery: {
            title: "Khôi phục tài khoản",
            step1: "Vui lòng nhập địa chỉ email của bạn. Nếu tài khoản tồn tại, bạn sẽ nhận được email từ chúng tôi có mã khôi phục mà bạn cần nhập ở bước tiếp theo.",
            step2: "Vui lòng nhập mã khôi phục mà bạn đã nhận được và mật khẩu mới mà bạn muốn sử dụng.",
            step3: "Mật khẩu của bạn đã được thay đổi thành công!",
            label_code: "Mã khôi phục",
            label_password: "Mật khẩu mới",
            button_continue: "Tiếp tục",
            button_back: "Quay lại đăng nhập",
            failed: "Không thể khôi phục tài khoản này. Vui lòng liên hệ với bộ phận hỗ trợ.",
            password_requirements_not_met: "Mật khẩu phải chứa ít nhất 8 ký tự cũng như chữ hoa và chữ thường"
        },
        not_found: "Trang được yêu cầu không tồn tại. (404)",
        error: {
            title: "Xác thực không thành công",
            details: `Không xác thực được. Vui lòng thử lại sau.
        
                Lỗi: {error}
                Mô tả: {description}
            `
        }
    },
    sw: {
        auth: {
            please_wait: "Tafadhali subiri..."
        },
        login: {
            title: "Ingia",
            username: "Jina la mtumiaji au barua pepe",
            password: "Nenosiri",
            submit: "Ingia",
            cancel: "Ghairi",
            recover_password: "Umesahau nenosiri?",
            failed: "Imeshindwa kuingia! Tafadhali angalia jina la mtumiaji na nenosiri."
        },
        logout: {
            please_wait: "Kutoka nje...",
            logged_out: "Umetoka nje. Sasa unaweza kufunga ukurasa huu."
        },
        consent: {
            title: "Thibitisha Kuingia",
            logged_in_as: "Umeingia kama {username}.",
            click_to_logout: "Si wewe? Bofya hapa ili kuondoka.",
            disclaimer: "Unaingia kwenye {service}. Hii huipa huduma ufikiaji wa taarifa zako. Je, ungependa kuendelea?",
            button_consent: "Thibitisha",
            button_cancel: "Ghairi"
        },
        verification: {
            title: "Uthibitishaji wa Akaunti",
            description: "Tafadhali weka msimbo wa uthibitishaji ambao tumetuma kwa anwani yako ya barua pepe.",
            resend: "Tuma nambari mpya",
            code_sent: "Msimbo mpya uko njiani!",
            logout: "Toka",
            failed: "Imeshindwa kuthibitisha akaunti. Nambari inaweza kuwa batili au imekwisha muda wake.",
            resend_failed: "Haikuweza kutuma msimbo mpya. Tafadhali jaribu tena baadaye.",
            button_confirm: "Thibitisha"
        },
        recovery: {
            title: "Ufufuaji wa Akaunti",
            step1: "Tafadhali weka anwani yako ya barua pepe. Ikiwa akaunti ipo, utapokea barua pepe kutoka kwetu yenye msimbo wa urejeshaji unaohitaji kuingiza katika hatua inayofuata.",
            step2: "Tafadhali weka msimbo wa uokoaji uliopokea na nenosiri jipya ambalo ungependa kutumia.",
            step3: "Nenosiri lako limebadilishwa kwa mafanikio!",
            label_code: "Msimbo wa uokoaji",
            label_password: "Nenosiri jipya",
            button_continue: "Endelea",
            button_back: "Rudi kuingia",
            failed: "Akaunti hii haiwezi kurejeshwa. Tafadhali wasiliana na usaidizi.",
            password_requirements_not_met: "Nenosiri lazima liwe na angalau vibambo 8 pamoja na herufi kubwa na ndogo"
        },
        not_found: "Ukurasa ulioombwa haupo. (404)",
        error: {
            title: "Uthibitishaji umeshindwa",
            details: `Imeshindwa kuthibitisha. Tafadhali jaribu tena baadaye.
           
                Hitilafu: {error}
                Maelezo: {description}
            `
        }
    }
};

const DEFAULT_LOCALE = "en";
const AVAILABLE_LOCALES = [
    "en", "fi", "es", "id", "vi", "sw"
]

export function requested_locale() {
    const query = new URLSearchParams(window.location.search);
    return query.get("locale") || undefined
}

function get_ui_locale() {
    const requested = requested_locale() || DEFAULT_LOCALE;
    return AVAILABLE_LOCALES.includes(requested) ? requested : DEFAULT_LOCALE;
}

const i18n = createI18n({
    locale: get_ui_locale(),
    messages: STRINGS,
    fallbackLocale: DEFAULT_LOCALE,
    availableLocales: AVAILABLE_LOCALES
});

export default i18n;
