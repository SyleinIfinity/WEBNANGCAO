/**
 * File: ~/Areas/Admin/Scripts/khachhang-custom.js
 * Chức năng: Xử lý giao diện và tương tác trang Khách hàng
 */

document.addEventListener("DOMContentLoaded", function () {

    // ===== XỬ LÝ NÚT ẨN/HIỆN MẬT KHẨU =====
    const togglePassBtn = document.getElementById("btnTogglePass");
    const inputPass = document.getElementById("InputMatKhau");

    if (togglePassBtn && inputPass) {
        togglePassBtn.addEventListener("click", function () {
            // Kiểm tra loại hiện tại
            const type = inputPass.getAttribute("type") === "password" ? "text" : "password";
            inputPass.setAttribute("type", type);

            // Đổi icon với hiệu ứng
            const icon = this.querySelector("i");
            if (type === "text") {
                icon.classList.remove("fa-eye");
                icon.classList.add("fa-eye-slash");
                this.style.color = "#f5576c";
            } else {
                icon.classList.remove("fa-eye-slash");
                icon.classList.add("fa-eye");
                this.style.color = "#667eea";
            }
        });

        // Hover effect
        togglePassBtn.addEventListener("mouseenter", function () {
            this.style.transform = "scale(1.1)";
        });

        togglePassBtn.addEventListener("mouseleave", function () {
            this.style.transform = "scale(1)";
        });
    }

    // ===== TỰ ĐỘNG ẨN ALERT SAU 5 GIÂY =====
    const alerts = document.querySelectorAll('.alert-custom, .alert');
    alerts.forEach(alert => {
        setTimeout(() => {
            alert.style.transition = "opacity 0.5s ease, transform 0.5s ease";
            alert.style.opacity = "0";
            alert.style.transform = "translateY(-20px)";
            setTimeout(() => {
                alert.remove();
            }, 500);
        }, 5000);
    });

    // ===== VALIDATION FORM REAL-TIME =====
    const forms = document.querySelectorAll('form');
    forms.forEach(form => {
        const inputs = form.querySelectorAll('input[type="text"], input[type="email"], input[type="password"], input[type="tel"]');

        inputs.forEach(input => {
            input.addEventListener('blur', function () {
                validateField(this);
            });

            input.addEventListener('input', function () {
                if (this.classList.contains('is-invalid')) {
                    validateField(this);
                }
            });
        });
    });

    function validateField(field) {
        const value = field.value.trim();
        const fieldName = field.name;

        // Xóa trạng thái cũ
        field.classList.remove('is-valid', 'is-invalid');

        // Email validation
        if (fieldName.includes('Email') && value) {
            const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
            if (emailRegex.test(value)) {
                field.classList.add('is-valid');
            } else {
                field.classList.add('is-invalid');
            }
        }

        // Phone validation
        if (fieldName.includes('SoDienThoai') && value) {
            const phoneRegex = /^(0|\+84)[0-9]{9,10}$/;
            if (phoneRegex.test(value)) {
                field.classList.add('is-valid');
            } else {
                field.classList.add('is-invalid');
            }
        }

        // Required fields
        if (field.hasAttribute('data-val-required')) {
            if (value) {
                field.classList.add('is-valid');
            } else {
                field.classList.add('is-invalid');
            }
        }
    }

    // ===== XÁC NHẬN XÓA CÓ ANIMATION =====
    const deleteButtons = document.querySelectorAll('a[href*="Delete"]');
    deleteButtons.forEach(btn => {
        btn.addEventListener('click', function (e) {
            e.preventDefault();
            const customerName = this.closest('tr').querySelector('.customer-name')?.textContent || 'khách hàng này';

            showCustomConfirm(
                'Xác nhận xóa',
                `Bạn có chắc chắn muốn xóa ${customerName}? Hành động này không thể hoàn tác!`,
                () => {
                    window.location.href = this.href;
                }
            );
        });
    });

    // Custom confirm dialog
    function showCustomConfirm(title, message, onConfirm) {
        const overlay = document.createElement('div');
        overlay.style.cssText = `
            position: fixed;
            top: 0;
            left: 0;
            right: 0;
            bottom: 0;
            background: rgba(0, 0, 0, 0.5);
            display: flex;
            align-items: center;
            justify-content: center;
            z-index: 9999;
            animation: fadeIn 0.3s ease;
        `;

        const dialog = document.createElement('div');
        dialog.style.cssText = `
            background: white;
            padding: 2rem;
            border-radius: 15px;
            max-width: 400px;
            text-align: center;
            box-shadow: 0 10px 40px rgba(0, 0, 0, 0.3);
            animation: slideUp 0.3s ease;
        `;

        dialog.innerHTML = `
            <div style="font-size: 3rem; color: #dc3545; margin-bottom: 1rem;">
                <i class="fas fa-exclamation-triangle"></i>
            </div>
            <h5 style="color: #2d3748; font-weight: 700; margin-bottom: 0.5rem;">${title}</h5>
            <p style="color: #718096; margin-bottom: 1.5rem;">${message}</p>
            <div style="display: flex; gap: 1rem; justify-content: center;">
                <button id="cancelBtn" style="
                    background: white;
                    border: 2px solid #cbd5e0;
                    color: #4a5568;
                    padding: 0.75rem 1.5rem;
                    border-radius: 10px;
                    font-weight: 600;
                    cursor: pointer;
                    transition: all 0.3s ease;
                ">Hủy bỏ</button>
                <button id="confirmBtn" style="
                    background: linear-gradient(135deg, #dc3545 0%, #c82333 100%);
                    border: none;
                    color: white;
                    padding: 0.75rem 1.5rem;
                    border-radius: 10px;
                    font-weight: 600;
                    cursor: pointer;
                    transition: all 0.3s ease;
                ">Xóa ngay</button>
            </div>
        `;

        overlay.appendChild(dialog);
        document.body.appendChild(overlay);

        // Add CSS animations
        const style = document.createElement('style');
        style.textContent = `
            @keyframes fadeIn {
                from { opacity: 0; }
                to { opacity: 1; }
            }
            @keyframes slideUp {
                from {
                    opacity: 0;
                    transform: translateY(30px);
                }
                to {
                    opacity: 1;
                    transform: translateY(0);
                }
            }
        `;
        document.head.appendChild(style);

        // Event listeners
        document.getElementById('cancelBtn').addEventListener('click', () => {
            overlay.style.opacity = '0';
            setTimeout(() => overlay.remove(), 300);
        });

        document.getElementById('confirmBtn').addEventListener('click', () => {
            overlay.style.opacity = '0';
            setTimeout(() => {
                overlay.remove();
                onConfirm();
            }, 300);
        });

        overlay.addEventListener('click', (e) => {
            if (e.target === overlay) {
                overlay.style.opacity = '0';
                setTimeout(() => overlay.remove(), 300);
            }
        });
    }

    // ===== HOVER EFFECTS CHO ROWS =====
    const tableRows = document.querySelectorAll('tbody tr');
    tableRows.forEach(row => {
        if (!row.querySelector('.empty-state')) {
            row.style.transition = 'all 0.3s ease';

            row.addEventListener('mouseenter', function () {
                this.style.backgroundColor = 'rgba(102, 126, 234, 0.05)';
            });

            row.addEventListener('mouseleave', function () {
                this.style.backgroundColor = '';
            });
        }
    });

    // ===== SEARCH/FILTER FUNCTIONALITY =====
    const searchInput = document.getElementById('searchCustomer');
    if (searchInput) {
        searchInput.addEventListener('input', function () {
            const searchTerm = this.value.toLowerCase();
            const rows = document.querySelectorAll('tbody tr');

            rows.forEach(row => {
                if (!row.querySelector('.empty-state')) {
                    const text = row.textContent.toLowerCase();
                    row.style.display = text.includes(searchTerm) ? '' : 'none';
                }
            });
        });
    }

    // ===== THÊM LOADING ANIMATION KHI SUBMIT FORM =====
    const submitButtons = document.querySelectorAll('button[type="submit"]');
    submitButtons.forEach(btn => {
        btn.addEventListener('click', function (e) {
            const form = this.closest('form');
            if (form && form.checkValidity()) {
                this.disabled = true;
                this.innerHTML = '<i class="fas fa-spinner fa-spin me-2"></i>Đang xử lý...';
            }
        });
    });

    console.log('✅ Khách hàng custom scripts loaded successfully!');
});