// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.


import { Locale } from ".";

const id: Locale = {
    settings_language: "Bahasa antarmuka pengguna",

    auth_please_wait: "Mohon tunggu...",
    
    login_title: "Masuk",
    login_username: "Nama pengguna atau email",
    login_password: "Kata sandi",
    login_submit: "Masuk",
    login_cancel: "Batal",
    login_recover_password: "Lupa kata sandi?",
    login_failed: "Gagal masuk! Silakan periksa nama pengguna dan kata sandi.",

    logout_please_wait: "Keluar...",
    logout_logged_out: "Anda telah keluar. Anda sekarang dapat menutup halaman ini.",

    consent_title: "Konfirmasi Masuk",
    consent_logged_in_as: "Anda masuk sebagai {username}.",
    consent_click_to_logout: "Bukan Anda? Klik di sini untuk keluar.",
    consent_disclaimer: "Anda masuk ke {service} di {platform}. Ini memberi layanan akses ke informasi Anda. Apakah Anda ingin melanjutkan?",
    consent_button_consent: "Konfirmasi",
    consent_button_cancel: "Batal",

    verification_title: "Verifikasi Akun",
    verification_description: "Silakan masukkan kode verifikasi yang telah kami kirimkan ke alamat email Anda.",
    verification_resend: "Kirim kode baru",
    verification_code_sent: "Kode baru sedang dalam perjalanan!",
    verification_logout: "Keluar",
    verification_failed: "Gagal memverifikasi akun. Kode mungkin tidak valid atau telah kedaluwarsa.",
    verification_resend_failed: "Tidak dapat mengirim kode baru. Silakan coba lagi nanti.",
    verification_button_confirm: "Verifikasi",

    recovery_title: "Pemulihan Akun",
    recovery_step1: "Silakan masukkan alamat email Anda. Jika akun tersebut ada, Anda akan menerima email dari kami dengan kode pemulihan yang perlu dimasukkan pada langkah berikutnya.",
    recovery_step2: "Silakan masukkan kode pemulihan yang Anda terima dan kata sandi baru yang ingin Anda gunakan.",
    recovery_step3: "Kata sandi Anda telah berhasil diubah!",
    recovery_label_code: "Kode pemulihan",
    recovery_label_password: "Kata sandi baru",
    recovery_button_continue: "Lanjutkan",
    recovery_button_back: "Kembali ke masuk",
    recovery_failed: "Akun ini tidak dapat dipulihkan. Silakan hubungi dukungan.",
    recovery_password_requirements_not_met: "Kata sandi harus terdiri dari setidaknya 8 karakter serta huruf besar dan kecil.",

    error_not_found: "Halaman yang diminta tidak ada. (404)",
    error_title: "Otentikasi gagal",
    error_details: `Gagal mengautentikasi. Silakan coba lagi nanti.

            Kesalahan: {error}
            Deskripsi: {description}
        `,
    
    en: "Inggris",
    fi: "Suomi",
    es: "Spanyol",
    vi: "Vietnam",
    id: "Indonesia",
    sw: "Swahili",
    rw: "Kinyarwanda"
};


export default id;