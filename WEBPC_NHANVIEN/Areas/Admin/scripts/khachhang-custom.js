/* * File: ~/Areas/Admin/Scripts/khachhang-custom.js
 * Chức năng: Xử lý giao diện trang Khách hàng
 */

document.addEventListener("DOMContentLoaded", function () {

    // Xử lý nút Ẩn/Hiện mật khẩu
    const togglePassBtn = document.getElementById("btnTogglePass");
    const inputPass = document.getElementById("InputMatKhau");

    if (togglePassBtn && inputPass) {
        togglePassBtn.addEventListener("click", function () {
            // Kiểm tra loại hiện tại
            const type = inputPass.getAttribute("type") === "password" ? "text" : "password";
            inputPass.setAttribute("type", type);

            // Đổi icon
            const icon = this.querySelector("i");
            if (type === "text") {
                icon.classList.remove("fa-eye");
                icon.classList.add("fa-eye-slash");
            } else {
                icon.classList.remove("fa-eye-slash");
                icon.classList.add("fa-eye");
            }
        });
    }
});