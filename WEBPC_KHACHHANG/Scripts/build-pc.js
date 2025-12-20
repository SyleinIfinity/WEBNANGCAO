// --- KHAI BÁO BIẾN TOÀN CỤC ---
var currentConfig = {}; // Lưu trữ sản phẩm đã chọn theo slot index
var currentSlotIndex = 0; // Lưu slot đang thao tác

// Khởi chạy khi trang tải xong
document.addEventListener('DOMContentLoaded', function () {
    updateSummary();
});

// --- 1. MỞ MODAL & GỌI API ---
function openProductModal(slotIndex, categoryName) {
    currentSlotIndex = slotIndex;

    // Đổi tên Modal
    var titleEl = document.getElementById('modalTitle');
    if (titleEl) titleEl.innerText = 'Chọn ' + categoryName;

    // Mở Modal Bootstrap
    var modalEl = document.getElementById('productModal');
    if (modalEl) {
        var myModal = new bootstrap.Modal(modalEl);
        myModal.show();
    }

    // Hiển thị loading spinner
    var container = document.getElementById('modal-product-list');
    if (container) {
        container.innerHTML = '<div class="text-center py-5 w-100"><div class="spinner-border text-primary"></div><div class="mt-2">Đang tải dữ liệu...</div></div>';
    }

    // Gọi API lấy dữ liệu từ Controller
    fetch('/BuildPC/GetProductsByCategory?category=' + encodeURIComponent(categoryName))
        .then(response => {
            if (!response.ok) throw new Error('Lỗi kết nối Server');
            return response.json();
        })
        .then(data => {
            renderProductList(data);
        })
        .catch(error => {
            console.error('Lỗi API:', error);
            if (container) container.innerHTML = '<div class="text-center py-5 w-100 text-danger">Không thể tải dữ liệu. Vui lòng thử lại.</div>';
        });
}

// --- 2. RENDER DANH SÁCH SẢN PHẨM ---
function renderProductList(products) {
    var container = document.getElementById('modal-product-list');
    if (!container) return;

    container.innerHTML = '';

    if (!products || products.length === 0) {
        container.innerHTML = '<div class="text-center py-5 w-100 text-muted"><i class="bi bi-box-seam fs-1"></i><p>Không tìm thấy linh kiện phù hợp.</p></div>';
        return;
    }

    products.forEach(p => {
        // Xử lý hiển thị giá
        let priceHtml = `<div class="text-danger fw-bold mb-2">${formatCurrency(p.Price)}</div>`;
        if (p.OldPrice > 0) {
            priceHtml = `
                <div class="text-muted text-decoration-line-through small">${formatCurrency(p.OldPrice)}</div>
                <div class="text-danger fw-bold mb-2">${formatCurrency(p.Price)}</div>
            `;
        }

        let imgUrl = p.Image ? p.Image : 'https://via.placeholder.com/150?text=No+Image';

        var html = `
            <div class="col-lg-3 col-md-4 col-6 mb-4">
                <div class="card h-100 border-0 shadow-sm product-card-hover">
                    <div class="card-body text-center d-flex flex-column p-3">
                        <img src="${imgUrl}" class="img-fluid mb-3 mx-auto" style="height: 120px; object-fit: contain;" 
                             onerror="this.src='https://via.placeholder.com/150?text=No+Image'">
                        
                        <h6 class="fw-bold text-truncate mb-1" style="font-size: 0.9rem;" title="${p.Name}">${p.Name}</h6>
                        
                        <div class="mt-auto">
                            ${priceHtml}
                            <button class="btn btn-sm btn-outline-primary w-100 rounded-pill" onclick='selectProduct(${JSON.stringify(p)})'>
                                <i class="bi bi-plus-lg"></i> Thêm
                            </button>
                        </div>
                    </div>
                </div>
            </div>
        `;
        container.innerHTML += html;
    });
}

// --- 3. CHỌN SẢN PHẨM ---
function selectProduct(product) {
    currentConfig[currentSlotIndex] = product;
    updateSlotUI(currentSlotIndex, product);
    updateSummary();

    var modalEl = document.getElementById('productModal');
    var modal = bootstrap.Modal.getInstance(modalEl);
    if (modal) modal.hide();
}

// --- 4. CẬP NHẬT UI SLOT ---
function updateSlotUI(index, product) {
    var row = document.getElementById('slot-row-' + index);
    if (!row) return;

    var emptyState = row.querySelector('.state-empty');
    if (emptyState) emptyState.style.display = 'none';

    var selectedBox = row.querySelector('.state-selected');
    if (selectedBox) {
        selectedBox.style.display = 'block';
        var imgEl = selectedBox.querySelector('.selected-img');
        if (imgEl) imgEl.src = product.Image || 'https://via.placeholder.com/50';
        var nameEl = selectedBox.querySelector('.selected-name');
        if (nameEl) nameEl.innerText = product.Name;
        var priceEl = selectedBox.querySelector('.selected-price');
        if (priceEl) priceEl.innerText = formatCurrency(product.Price);
    }

    var btnSelect = row.querySelector('.btn-select');
    if (btnSelect) btnSelect.style.display = 'none';

    var btnActionGroup = row.querySelector('.btn-action-group');
    if (btnActionGroup) btnActionGroup.style.display = 'block';

    row.classList.add('bg-light-success');
}

// --- 5. XÓA SẢN PHẨM ---
function removeProduct(index) {
    delete currentConfig[index];
    var row = document.getElementById('slot-row-' + index);
    if (!row) return;

    var emptyState = row.querySelector('.state-empty');
    if (emptyState) emptyState.style.display = 'block';

    var selectedBox = row.querySelector('.state-selected');
    if (selectedBox) selectedBox.style.display = 'none';

    var btnSelect = row.querySelector('.btn-select');
    if (btnSelect) btnSelect.style.display = 'inline-block';

    var btnActionGroup = row.querySelector('.btn-action-group');
    if (btnActionGroup) btnActionGroup.style.display = 'none';

    row.classList.remove('bg-light-success');
    updateSummary();
}

// --- 6. TÍNH TỔNG TIỀN ---
function updateSummary() {
    var list = document.getElementById('summary-items');
    var defaultText = document.getElementById('summary-text-default');
    var checkoutBtn = document.getElementById('btn-checkout-config');
    var totalPriceEl = document.getElementById('total-price');

    if (list) list.innerHTML = '';
    var total = 0;
    var count = 0;

    for (const [key, p] of Object.entries(currentConfig)) {
        total += p.Price;
        count++;

        var row = document.getElementById('slot-row-' + key);
        var catName = row ? row.getAttribute('data-category') : 'Linh kiện';

        if (list) {
            var li = document.createElement('li');
            li.className = "d-flex justify-content-between mb-2 small border-bottom pb-1";
            li.innerHTML = `
                <div class="text-truncate me-2" style="max-width: 180px;">
                    <span class="fw-bold text-dark">${catName}:</span> ${p.Name}
                </div> 
                <span class="fw-bold text-primary flex-shrink-0">${formatCurrency(p.Price)}</span>
            `;
            list.appendChild(li);
        }
    }

    if (totalPriceEl) totalPriceEl.innerText = formatCurrency(total);

    if (count > 0) {
        if (defaultText) defaultText.style.display = 'none';
        if (checkoutBtn) checkoutBtn.removeAttribute('disabled');
    } else {
        if (defaultText) defaultText.style.display = 'block';
        if (checkoutBtn) checkoutBtn.setAttribute('disabled', 'true');
    }
}

function resetConfig() {
    if (confirm('Bạn có chắc muốn xóa toàn bộ cấu hình đang chọn?')) {
        currentConfig = {};
        location.reload();
    }
}

// --- 7. THÊM VÀO GIỎ & THANH TOÁN (Logic mới) ---
function addToCartAll() {
    // 1. Kiểm tra cấu hình
    if (!window.BuildConfig) {
        alert("Lỗi tải trang: Không tìm thấy thông tin cấu hình.");
        return;
    }

    // 2. Kiểm tra đăng nhập (Tự động bật Modal nếu chưa login)
    if (window.BuildConfig.isUserLoggedIn !== 'true') {
        var loginModalElement = document.getElementById('loginModal');
        if (loginModalElement) {
            // Dùng getOrCreateInstance để lấy hoặc tạo Modal
            var myModal = bootstrap.Modal.getOrCreateInstance(loginModalElement);
            myModal.show();
        } else {
            // Fallback
            window.location.href = window.BuildConfig.loginUrl;
        }
        return; // Dừng lại tại đây
    }

    // 3. Lấy danh sách ID sản phẩm
    var selectedIds = [];
    for (const [key, p] of Object.entries(currentConfig)) {
        selectedIds.push(p.Id);
    }

    if (selectedIds.length === 0) {
        alert('Vui lòng chọn ít nhất 1 linh kiện!');
        return;
    }

    // Hiển thị trạng thái đang xử lý
    var btn = document.getElementById('btn-checkout-config');
    var oldText = "";
    if (btn) {
        oldText = btn.innerText;
        btn.innerText = "Đang xử lý...";
        btn.disabled = true;
    }

    // 4. Gọi API AddBuildPCToCart (Thay vì add từng cái)
    fetch('/Cart/AddBuildPCToCart', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json'
        },
        body: JSON.stringify({ productIds: selectedIds })
    })
        .then(response => response.json())
        .then(data => {
            if (data.success) {
                // Chuyển hướng ngay lập tức đến trang Checkout (URL server trả về)
                window.location.href = data.url;
            } else {
                alert(data.message || 'Có lỗi xảy ra khi xử lý cấu hình.');
                if (btn) {
                    btn.innerText = oldText;
                    btn.disabled = false;
                }
            }
        })
        .catch(err => {
            console.error(err);
            alert('Lỗi kết nối đến máy chủ.');
            if (btn) {
                btn.innerText = oldText;
                btn.disabled = false;
            }
        });
}

function formatCurrency(amount) {
    return new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND' }).format(amount);
}