// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.


import { Locale } from ".";

const vi: Locale = {
    settings_language: "Ngôn ngữ giao diện người dùng",

    auth_please_wait: "Vui lòng chờ...",
    
    login_title: "Đăng nhập",
    login_username: "Tên người dùng hoặc email",
    login_password: "Mật khẩu",
    login_submit: "Đăng nhập",
    login_cancel: "Hủy",
    login_recover_password: "Quên mật khẩu?",
    login_failed: "Đăng nhập thất bại! Vui lòng kiểm tra tên người dùng và mật khẩu.",

    logout_please_wait: "Đang đăng xuất...",
    logout_logged_out: "Bạn đã đăng xuất. Bây giờ bạn có thể đóng trang này.",

    consent_title: "Xác nhận Đăng nhập",
    consent_logged_in_as: "Bạn đã đăng nhập với tên {username}.",
    consent_click_to_logout: "Không phải bạn? Nhấp vào đây để đăng xuất.",
    consent_disclaimer: "Bạn đang đăng nhập vào {service}. Điều này cho phép dịch vụ truy cập vào thông tin của bạn. Bạn có muốn tiếp tục không?",
    consent_button_consent: "Xác nhận",
    consent_button_cancel: "Hủy",

    verification_title: "Xác minh Tài khoản",
    verification_description: "Vui lòng nhập mã xác minh mà chúng tôi đã gửi đến địa chỉ email của bạn.",
    verification_resend: "Gửi mã mới",
    verification_code_sent: "Mã mới đang trên đường đến!",
    verification_logout: "Đăng xuất",
    verification_failed: "Xác minh tài khoản thất bại. Mã có thể không hợp lệ hoặc đã hết hạn.",
    verification_resend_failed: "Không thể gửi mã mới. Vui lòng thử lại sau.",
    verification_button_confirm: "Xác minh",

    recovery_title: "Khôi phục Tài khoản",
    recovery_step1: "Vui lòng nhập địa chỉ email của bạn. Nếu tài khoản tồn tại, bạn sẽ nhận được email từ chúng tôi với mã khôi phục cần nhập ở bước tiếp theo.",
    recovery_step2: "Vui lòng nhập mã khôi phục bạn đã nhận được và mật khẩu mới bạn muốn sử dụng.",
    recovery_step3: "Mật khẩu của bạn đã được thay đổi thành công!",
    recovery_label_code: "Mã khôi phục",
    recovery_label_password: "Mật khẩu mới",
    recovery_button_continue: "Tiếp tục",
    recovery_button_back: "Quay lại đăng nhập",
    recovery_failed: "Không thể khôi phục tài khoản này. Vui lòng liên hệ với bộ phận hỗ trợ.",
    recovery_password_requirements_not_met: "Mật khẩu phải chứa ít nhất 8 ký tự và bao gồm cả chữ hoa và chữ thường.",

    error_not_found: "Trang yêu cầu không tồn tại. (404)",
    error_title: "Xác thực thất bại",
    error_details: `Không thể xác thực. Vui lòng thử lại sau.

            Lỗi: {error}
            Mô tả: {description}
        `,
    
    en: "Tiếng Anh",
    fi: "Tiếng Phần Lan",
    es: "Tiếng Tây Ban Nha",
    vi: "Tiếng Việt",
    id: "Tiếng Indonesia",
    sw: "Tiếng Swahili"
};

export default vi;
