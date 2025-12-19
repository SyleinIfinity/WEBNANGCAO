/**
 * File: ~/Areas/Admin/Scripts/sanpham-common.js
 * Chức năng: Xử lý toàn bộ logic cho module Sản Phẩm (Index, Create, Edit)
 * Theme: PC Hardware Store - Professional Blue & Tech Green
 */

document.addEventListener("DOMContentLoaded", function () {
    const cfg = window.SANPHAM_CONFIG || {};

    // ================= 1. UTILS & UI EFFECTS ================= //

    // Format tiền tệ VNĐ với animation
    const priceInputs = document.querySelectorAll('input[data-type="currency"]');
    priceInputs.forEach(input => {
        input.addEventListener('input', function (e) {
            // Remove non-numeric characters
            let value = this.value.replace(/\D/g, '');
            // Format with thousand separators
            if (value) {
                this.value = parseInt(value).toLocaleString('vi-VN');
            }
        });

        input.addEventListener('blur', function () {
            let val = parseInt(this.value.replace(/\D/g, ''));
            if (!isNaN(val)) {
                // Round to nearest thousand
                val = Math.round(val / 1000) * 1000;
                this.value = val.toLocaleString('vi-VN');
            }
        });

        input.addEventListener('focus', function () {
            // Remove formatting on focus for easier editing
            let val = this.value.replace(/\D/g, '');
            if (val) this.value = val;
        });
    });

    // Enhanced Custom Confirm Dialog
    window.showCustomConfirm = function (message, onConfirm) {
        const overlay = document.createElement('div');
        overlay.style.cssText = `
            position: fixed;
            top: 0;
            left: 0;
            right: 0;
            bottom: 0;
            background: rgba(0, 20, 40, 0.75);
            backdrop-filter: blur(4px);
            z-index: 9999;
            display: flex;
            align-items: center;
            justify-content: center;
            animation: fadeIn 0.2s ease-out;
        `;

        const modal = document.createElement('div');
        modal.style.cssText = `
            background: linear-gradient(135deg, #ffffff 0%, #f8f9fa 100%);
            padding: 30px;
            border-radius: 16px;
            width: 420px;
            max-width: 90vw;
            text-align: center;
            box-shadow: 0 20px 60px rgba(0, 0, 0, 0.3);
            border: 1px solid rgba(0, 123, 255, 0.1);
            animation: slideUp 0.3s ease-out;
        `;

        modal.innerHTML = `
            <div style="
                font-size: 3.5rem;
                background: linear-gradient(135deg, #007bff 0%, #00d4ff 100%);
                -webkit-background-clip: text;
                -webkit-text-fill-color: transparent;
                margin-bottom: 20px;
            ">
                <i class="fas fa-exclamation-triangle"></i>
            </div>
            <h5 style="color: #1a1a1a; font-weight: 700; margin-bottom: 12px; font-size: 1.25rem;">
                Xác nhận thao tác
            </h5>
            <p style="color: #666; margin-bottom: 25px; line-height: 1.6;">
                ${message}
            </p>
            <div style="display: flex; justify-content: center; gap: 12px;">
                <button id="btnCancel" class="btn btn-light border px-4 py-2" style="
                    border-radius: 8px;
                    font-weight: 600;
                    transition: all 0.2s;
                ">
                    <i class="fas fa-times me-1"></i> Hủy
                </button>
                <button id="btnOk" class="btn text-white px-4 py-2" style="
                    background: linear-gradient(135deg, #007bff 0%, #0056b3 100%);
                    border: none;
                    border-radius: 8px;
                    font-weight: 600;
                    box-shadow: 0 4px 12px rgba(0, 123, 255, 0.3);
                    transition: all 0.2s;
                ">
                    <i class="fas fa-check me-1"></i> Đồng ý
                </button>
            </div>
        `;

        overlay.appendChild(modal);
        document.body.appendChild(overlay);

        // Add hover effects
        const btnOk = document.getElementById('btnOk');
        const btnCancel = document.getElementById('btnCancel');

        btnOk.onmouseenter = () => btnOk.style.transform = 'translateY(-2px)';
        btnOk.onmouseleave = () => btnOk.style.transform = 'translateY(0)';
        btnCancel.onmouseenter = () => btnCancel.style.transform = 'translateY(-2px)';
        btnCancel.onmouseleave = () => btnCancel.style.transform = 'translateY(0)';

        btnCancel.onclick = () => {
            overlay.style.animation = 'fadeOut 0.2s ease-out';
            setTimeout(() => overlay.remove(), 200);
        };

        btnOk.onclick = () => {
            overlay.style.animation = 'fadeOut 0.2s ease-out';
            setTimeout(() => {
                overlay.remove();
                onConfirm();
            }, 200);
        };
    };

    // Add CSS animations
    if (!document.getElementById('custom-animations')) {
        const style = document.createElement('style');
        style.id = 'custom-animations';
        style.textContent = `
            @keyframes fadeIn {
                from { opacity: 0; }
                to { opacity: 1; }
            }
            @keyframes fadeOut {
                from { opacity: 1; }
                to { opacity: 0; }
            }
            @keyframes slideUp {
                from { 
                    opacity: 0;
                    transform: translateY(30px) scale(0.95);
                }
                to { 
                    opacity: 1;
                    transform: translateY(0) scale(1);
                }
            }
        `;
        document.head.appendChild(style);
    }

    // ================= 2. XỬ LÝ ẢNH (PREVIEW) ================= //
    const imageUpload = document.getElementById('imageUpload');
    const previewContainer = document.getElementById('previewContainer');

    if (imageUpload && previewContainer) {
        imageUpload.addEventListener('change', function (e) {
            const files = e.target.files;
            previewContainer.innerHTML = '';

            if (files.length > 0) {
                Array.from(files).forEach((file, index) => {
                    if (file.type.startsWith('image/')) {
                        const reader = new FileReader();
                        reader.onload = function (evt) {
                            const div = document.createElement('div');
                            div.className = 'col-md-4 col-6 mb-3';
                            div.style.animation = 'slideUp 0.3s ease-out';
                            div.style.animationDelay = `${index * 0.05}s`;
                            div.style.opacity = '0';
                            div.style.animationFillMode = 'forwards';

                            div.innerHTML = `
                                <div class="position-relative border rounded-3 overflow-hidden shadow-sm hover-lift" 
                                     style="transition: all 0.3s; cursor: pointer;">
                                    <img src="${evt.target.result}" 
                                         style="width:100%; height:140px; object-fit:cover;"
                                         class="img-fluid">
                                    ${index === 0 ? `
                                        <div class="position-absolute top-0 start-0 m-2">
                                            <span class="badge" style="
                                                background: linear-gradient(135deg, #28a745 0%, #20c997 100%);
                                                font-size: 0.7rem;
                                                padding: 4px 8px;
                                                border-radius: 6px;
                                                box-shadow: 0 2px 8px rgba(40, 167, 69, 0.3);
                                            ">
                                                <i class="fas fa-star me-1"></i> Đại diện
                                            </span>
                                        </div>
                                    ` : ''}
                                    <div class="position-absolute bottom-0 start-0 end-0 p-2" 
                                         style="background: linear-gradient(to top, rgba(0,0,0,0.6), transparent);">
                                        <small class="text-white d-block text-truncate">
                                            <i class="fas fa-image me-1"></i> ${file.name}
                                        </small>
                                    </div>
                                </div>
                            `;
                            previewContainer.appendChild(div);
                        }
                        reader.readAsDataURL(file);
                    }
                });

                const previewSection = previewContainer.closest('.preview-section');
                if (previewSection) {
                    previewSection.style.display = 'block';
                    previewSection.style.animation = 'fadeIn 0.3s ease-out';
                }
            }
        });
    }

    // ================= 3. QUẢN LÝ THÔNG SỐ KỸ THUẬT (AJAX) ================= //
    if (cfg.enableSpec) {
        const tableBody = document.getElementById("specTableBody");
        const loading = document.getElementById("spec-loading");
        const empty = document.getElementById("spec-empty");
        const modalSpec = document.getElementById("specModal");

        const specId = document.getElementById("specId");
        const specName = document.getElementById("specName");
        const specValue = document.getElementById("specValue");
        const btnSave = document.getElementById("btnSaveSpec");

        // Load danh sách với animation
        function loadSpecs() {
            loading.style.display = "block";
            empty.style.display = "none";
            tableBody.innerHTML = "";

            fetch(`${cfg.getUrl}?productId=${cfg.productId}`)
                .then(r => r.json())
                .then(data => {
                    if (!data || data.length === 0) {
                        empty.style.display = "block";
                    } else {
                        data.forEach((item, idx) => {
                            const tr = document.createElement("tr");
                            tr.style.animation = 'fadeIn 0.3s ease-out';
                            tr.style.animationDelay = `${idx * 0.05}s`;
                            tr.style.opacity = '0';
                            tr.style.animationFillMode = 'forwards';

                            const id = item.MaThongSo || item.maThongSo;
                            const name = item.TenThongSo || item.tenThongSo;
                            const val = item.GiaTri || item.giaTri;

                            tr.innerHTML = `
                                <td style="width: 35%;">
                                    <div class="d-flex align-items-center">
                                        <div class="spec-icon me-2" style="
                                            width: 32px;
                                            height: 32px;
                                            background: linear-gradient(135deg, #e3f2fd 0%, #bbdefb 100%);
                                            border-radius: 8px;
                                            display: flex;
                                            align-items: center;
                                            justify-content: center;
                                            color: #1976d2;
                                            font-size: 0.85rem;
                                        ">
                                            <i class="fas fa-microchip"></i>
                                        </div>
                                        <span class="fw-bold text-dark">${name}</span>
                                    </div>
                                </td>
                                <td style="width: 50%;">
                                    <span class="text-secondary">${val}</span>
                                </td>
                                <td class="text-end" style="width: 15%;">
                                    <button class="btn btn-sm btn-light border-0 text-primary btn-edit-spec me-1" 
                                            data-id="${id}" data-name="${name}" data-val="${val}"
                                            style="border-radius: 8px; transition: all 0.2s;"
                                            onmouseover="this.style.backgroundColor='#e3f2fd'"
                                            onmouseout="this.style.backgroundColor=''">
                                        <i class="fas fa-edit"></i>
                                    </button>
                                    <button class="btn btn-sm btn-light border-0 text-danger btn-delete-spec" 
                                            data-id="${id}"
                                            style="border-radius: 8px; transition: all 0.2s;"
                                            onmouseover="this.style.backgroundColor='#ffebee'"
                                            onmouseout="this.style.backgroundColor=''">
                                        <i class="fas fa-trash"></i>
                                    </button>
                                </td>
                            `;
                            tableBody.appendChild(tr);
                        });
                    }
                })
                .catch(err => {
                    console.error(err);
                    tableBody.innerHTML = `
                        <tr>
                            <td colspan="3" class="text-center text-danger py-4">
                                <i class="fas fa-exclamation-triangle me-2"></i>
                                Lỗi tải dữ liệu
                            </td>
                        </tr>
                    `;
                })
                .finally(() => loading.style.display = "none");
        }

        // Modal handling
        function toggleModal(show) {
            if (show) {
                modalSpec.style.display = 'block';
                modalSpec.classList.add('show');
                document.body.classList.add('modal-open');

                // Create backdrop
                const backdrop = document.createElement('div');
                backdrop.className = 'modal-backdrop fade show';
                backdrop.id = 'spec-modal-backdrop';
                document.body.appendChild(backdrop);
            } else {
                modalSpec.style.display = 'none';
                modalSpec.classList.remove('show');
                document.body.classList.remove('modal-open');

                const backdrop = document.getElementById('spec-modal-backdrop');
                if (backdrop) backdrop.remove();
            }
        }

        // Event: Add new spec
        document.getElementById("btnAddSpec")?.addEventListener("click", () => {
            specId.value = "";
            specName.value = "";
            specValue.value = "";
            document.getElementById("specModalLabel").innerText = "Thêm Thông Số Kỹ Thuật";
            toggleModal(true);
        });

        // Event: Close modal
        document.querySelectorAll("[data-close-spec-modal]").forEach(btn => {
            btn.addEventListener("click", () => toggleModal(false));
        });

        // Event: Save spec
        btnSave?.addEventListener("click", () => {
            const name = specName.value.trim();
            const val = specValue.value.trim();

            if (!name || !val) {
                // Show inline validation
                if (!name) specName.classList.add('is-invalid');
                if (!val) specValue.classList.add('is-invalid');
                return;
            }

            specName.classList.remove('is-invalid');
            specValue.classList.remove('is-invalid');

            const fd = new FormData();
            fd.append("TenThongSo", name);
            fd.append("GiaTri", val);

            let url = cfg.createUrl;
            if (specId.value) {
                fd.append("MaThongSo", specId.value);
                url = cfg.updateUrl;
            } else {
                fd.append("MaSanPham", cfg.productId);
            }

            const originText = btnSave.innerHTML;
            btnSave.disabled = true;
            btnSave.innerHTML = '<i class="fas fa-spinner fa-spin me-2"></i>Đang xử lý...';

            fetch(url, { method: "POST", body: fd })
                .then(r => r.json())
                .then(res => {
                    if (res.success) {
                        toggleModal(false);
                        loadSpecs();

                        // Show success toast
                        showToast('success', 'Thành công!', 'Đã lưu thông số kỹ thuật');
                    } else {
                        showToast('error', 'Lỗi!', res.error || "Có lỗi xảy ra");
                    }
                })
                .catch(err => {
                    console.error(err);
                    showToast('error', 'Lỗi!', 'Không thể kết nối đến server');
                })
                .finally(() => {
                    btnSave.disabled = false;
                    btnSave.innerHTML = originText;
                });
        });

        // Event: Edit/Delete spec
        tableBody.addEventListener("click", function (e) {
            const btnEdit = e.target.closest(".btn-edit-spec");
            const btnDelete = e.target.closest(".btn-delete-spec");

            if (btnEdit) {
                const d = btnEdit.dataset;
                specId.value = d.id;
                specName.value = d.name;
                specValue.value = d.val;
                document.getElementById("specModalLabel").innerText = "Cập Nhật Thông Số";
                toggleModal(true);
            }

            if (btnDelete) {
                window.showCustomConfirm("Bạn chắc chắn muốn xóa thông số này?", () => {
                    const fd = new FormData();
                    fd.append("id", btnDelete.dataset.id);
                    fetch(cfg.deleteUrl, { method: "POST", body: fd })
                        .then(r => r.json())
                        .then(res => {
                            if (res.success) {
                                loadSpecs();
                                showToast('success', 'Đã xóa!', 'Thông số đã được xóa');
                            } else {
                                showToast('error', 'Lỗi!', 'Không thể xóa thông số');
                            }
                        });
                });
            }
        });

        // Initialize
        loadSpecs();
    }

    // ================= 4. TOAST NOTIFICATION ================= //
    function showToast(type, title, message) {
        const toastContainer = document.getElementById('toast-container') || createToastContainer();

        const toast = document.createElement('div');
        toast.className = 'custom-toast';
        toast.style.cssText = `
            background: white;
            border-radius: 12px;
            padding: 16px;
            margin-bottom: 12px;
            box-shadow: 0 8px 24px rgba(0, 0, 0, 0.15);
            display: flex;
            align-items: center;
            min-width: 320px;
            animation: slideInRight 0.3s ease-out;
            border-left: 4px solid ${type === 'success' ? '#28a745' : '#dc3545'};
        `;

        const icon = type === 'success'
            ? '<i class="fas fa-check-circle" style="color: #28a745; font-size: 1.5rem;"></i>'
            : '<i class="fas fa-exclamation-circle" style="color: #dc3545; font-size: 1.5rem;"></i>';

        toast.innerHTML = `
            ${icon}
            <div style="margin-left: 12px; flex: 1;">
                <div style="font-weight: 600; color: #1a1a1a; margin-bottom: 4px;">${title}</div>
                <div style="font-size: 0.875rem; color: #666;">${message}</div>
            </div>
        `;

        toastContainer.appendChild(toast);

        setTimeout(() => {
            toast.style.animation = 'slideOutRight 0.3s ease-out';
            setTimeout(() => toast.remove(), 300);
        }, 3000);
    }

    function createToastContainer() {
        const container = document.createElement('div');
        container.id = 'toast-container';
        container.style.cssText = `
            position: fixed;
            top: 80px;
            right: 20px;
            z-index: 10000;
        `;
        document.body.appendChild(container);

        // Add toast animations
        if (!document.getElementById('toast-animations')) {
            const style = document.createElement('style');
            style.id = 'toast-animations';
            style.textContent = `
                @keyframes slideInRight {
                    from {
                        opacity: 0;
                        transform: translateX(100%);
                    }
                    to {
                        opacity: 1;
                        transform: translateX(0);
                    }
                }
                @keyframes slideOutRight {
                    from {
                        opacity: 1;
                        transform: translateX(0);
                    }
                    to {
                        opacity: 0;
                        transform: translateX(100%);
                    }
                }
                .hover-lift:hover {
                    transform: translateY(-4px);
                    box-shadow: 0 8px 16px rgba(0, 0, 0, 0.15) !important;
                }
            `;
            document.head.appendChild(style);
        }

        return container;
    }
});