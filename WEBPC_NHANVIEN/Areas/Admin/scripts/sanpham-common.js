/**
 * File: ~/Areas/Admin/Scripts/sanpham-common.js
 * Updated: Fix lỗi submit form (v2) - Clean currency format before submit
 */

document.addEventListener("DOMContentLoaded", function () {
    const cfg = window.SANPHAM_CONFIG || {};

    // ================= 1. AUTO FORMAT CURRENCY (UI ONLY) ================= //
    // Tự động thêm dấu chấm khi nhập tiền (VD: 10000 -> 10.000)
    const priceInputs = document.querySelectorAll('input[data-type="currency"]');

    priceInputs.forEach(input => {
        // Hàm format giá trị hiển thị
        const formatValue = (val) => {
            if (!val) return '';
            // Chỉ giữ lại số
            const cleanVal = val.replace(/\D/g, '');
            // Format sang dạng tiền tệ VN (có dấu chấm)
            return cleanVal ? parseInt(cleanVal).toLocaleString('vi-VN') : '';
        };

        input.addEventListener('input', function (e) {
            // Lưu vị trí con trỏ để trải nghiệm nhập tốt hơn
            let cursorPosition = this.selectionStart;
            let oldLength = this.value.length;

            this.value = formatValue(this.value);

            // Điều chỉnh con trỏ nếu độ dài chuỗi thay đổi
            let newLength = this.value.length;
            cursorPosition = cursorPosition + (newLength - oldLength);
            this.setSelectionRange(cursorPosition, cursorPosition);
        });

        // Format ngay khi load trang (nếu có dữ liệu cũ từ model)
        if (input.value) {
            input.value = formatValue(input.value);
        }
    });

    // ================= 2. HANDLE FORM SUBMIT (CRITICAL FIX) ================= //
    // Tìm form theo ID cụ thể (đã thêm ở Bước 1) hoặc fallback tìm thẻ form đầu tiên
    const form = document.getElementById('productForm') || document.querySelector('form');

    if (form) {
        form.addEventListener('submit', function (e) {
            // Tìm tất cả các ô nhập tiền
            const currencyInputs = form.querySelectorAll('input[data-type="currency"]');

            console.log("Đang xử lý form trước khi gửi..."); // Debug log

            currencyInputs.forEach(input => {
                if (input.value) {
                    // CỰC KỲ QUAN TRỌNG:
                    // Xóa TẤT CẢ ký tự không phải là số (dấu chấm, phẩy, chữ...)
                    // VD: "10.000" -> "10000"
                    // VD: "1.000.000 đ" -> "1000000"
                    const rawValue = input.value.replace(/\D/g, '');
                    input.value = rawValue;

                    console.log(`Đã clean input ${input.name}: ${rawValue}`); // Debug log
                }
            });

            // Form sẽ tiếp tục được gửi đi với giá trị số nguyên sạch
        });
    }

    // ================= 3. IMAGE PREVIEW ================= //
    const imageUpload = document.getElementById('imageUpload');
    const previewContainer = document.getElementById('previewContainer');

    if (imageUpload && previewContainer) {
        imageUpload.addEventListener('change', function (e) {
            const files = e.target.files;
            previewContainer.innerHTML = ''; // Clear cũ

            if (files.length > 0) {
                // Hiển thị vùng preview
                const previewSection = previewContainer.closest('.preview-section');
                if (previewSection) previewSection.style.display = 'block';

                Array.from(files).forEach((file, index) => {
                    if (file.type.startsWith('image/')) {
                        const reader = new FileReader();
                        reader.onload = function (evt) {
                            const div = document.createElement('div');
                            div.className = 'col-md-4 col-6 mb-3';
                            div.innerHTML = `
                                <div class="position-relative border rounded-3 overflow-hidden shadow-sm">
                                    <img src="${evt.target.result}" style="width:100%; height:120px; object-fit:cover;">
                                    ${index === 0 ? '<span class="position-absolute top-0 start-0 badge bg-success m-1">Ảnh bìa</span>' : ''}
                                    <div class="position-absolute bottom-0 w-100 bg-dark bg-opacity-75 text-white p-1 text-truncate" style="font-size:0.8rem;">
                                        ${file.name}
                                    </div>
                                </div>
                            `;
                            previewContainer.appendChild(div);
                        }
                        reader.readAsDataURL(file);
                    }
                });
            }
        });
    }
});