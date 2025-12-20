document.addEventListener('DOMContentLoaded', function () {
    // --- 1. KHAI BÁO BIẾN ---
    const selectAllCheckbox = document.getElementById('selectAll');
    const checkoutBtn = document.getElementById('btn-checkout');

    // Hàm định dạng tiền VNĐ
    const formatCurrency = (amount) => {
        return new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND' }).format(amount);
    };

    // --- 2. HÀM TÍNH TOÁN TỔNG TIỀN ---
    window.updateCartSummary = function () {
        let totalAmount = 0;
        let totalCount = 0;
        let allChecked = true;

        const rows = document.querySelectorAll('.cart-item');
        if (rows.length === 0) {
            allChecked = false;
            if (selectAllCheckbox) selectAllCheckbox.disabled = true;
        }

        rows.forEach(row => {
            const checkbox = row.querySelector('.item-checkbox');
            const quantityInput = row.querySelector('.quantity-input');

            const priceRaw = parseFloat(row.getAttribute('data-price')) || 0;
            const quantity = parseInt(quantityInput.value) || 0;

            const lineTotal = priceRaw * quantity;
            row.querySelector('.total-line-price').textContent = formatCurrency(lineTotal);

            if (checkbox.checked) {
                totalAmount += lineTotal;
                totalCount += quantity;
            } else {
                allChecked = false;
            }
        });

        document.getElementById('summary-subtotal').textContent = formatCurrency(totalAmount);
        document.getElementById('summary-total').textContent = formatCurrency(totalAmount);
        document.getElementById('summary-count').textContent = totalCount + ' sản phẩm';

        if (selectAllCheckbox && rows.length > 0) {
            selectAllCheckbox.checked = allChecked;
        }

        if (totalCount > 0) {
            checkoutBtn.removeAttribute('disabled');
        } else {
            checkoutBtn.setAttribute('disabled', 'true');
        }
    };

    // --- 3. GẮN SỰ KIỆN ---
    if (selectAllCheckbox) {
        selectAllCheckbox.addEventListener('change', function () {
            const isChecked = this.checked;
            document.querySelectorAll('.item-checkbox').forEach(cb => {
                cb.checked = isChecked;
            });
            updateCartSummary();
        });
    }

    const cartBody = document.getElementById('cart-body');
    if (cartBody) {
        cartBody.addEventListener('change', function (e) {
            if (e.target.classList.contains('item-checkbox')) {
                updateCartSummary();
            }
        });
    }

    // SỰ KIỆN NÚT THANH TOÁN
    if (checkoutBtn) {
        checkoutBtn.addEventListener('click', function () {
            const selectedIds = [];
            document.querySelectorAll('.item-checkbox:checked').forEach(checkbox => {
                const id = checkbox.value; // Value checkbox là CartItemId
                if (id) selectedIds.push(id);
            });

            if (selectedIds.length === 0) {
                alert("Vui lòng chọn ít nhất một sản phẩm để thanh toán!");
                return;
            }

            const url = '/ThanhToan/Checkout?selectedIds=' + selectedIds.join(',');
            window.location.href = url;
        });
    }

    // Gọi lần đầu
    updateCartSummary();
});

// --- 4. LOGIC UPDATE / REMOVE ---
function checkQuantityChange(input) {
    let currentVal = parseInt(input.value);
    let originalVal = parseInt(input.getAttribute('data-original'));
    let btn = input.nextElementSibling;

    if (!isNaN(currentVal) && currentVal > 0 && currentVal !== originalVal) {
        btn.classList.remove('d-none');
    } else {
        btn.classList.add('d-none');
    }
    if (window.updateCartSummary) window.updateCartSummary();
}

function confirmUpdateQuantity(productId, btnElement) {
    const inputGroup = btnElement.closest('.input-group');
    const inputElement = inputGroup.querySelector('.quantity-input');
    const quantity = parseInt(inputElement.value);

    if (quantity < 1 || isNaN(quantity)) {
        alert("Số lượng phải lớn hơn 0");
        inputElement.value = inputElement.getAttribute('data-original');
        checkQuantityChange(inputElement);
        return;
    }

    const originalIcon = btnElement.innerHTML;
    btnElement.innerHTML = '<span class="spinner-border spinner-border-sm"></span>';
    btnElement.disabled = true;

    fetch('/Cart/UpdateQuantity', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ productId: productId, quantity: quantity })
    })
        .then(response => response.json())
        .then(data => {
            if (data.success) {
                inputElement.setAttribute('data-original', quantity);
                btnElement.classList.add('d-none');
                if (window.updateCartSummary) window.updateCartSummary();
            } else if (data.requireLogin) {
                alert(data.message);
                // Có thể dùng window.CartConfig.loginUrl nếu muốn mềm dẻo hơn
                window.location.href = '/Login?returnUrl=/Cart';
            } else {
                alert("Lỗi: " + data.message);
                inputElement.value = inputElement.getAttribute('data-original');
                checkQuantityChange(inputElement);
            }
        })
        .catch(error => {
            console.error(error);
            alert("Lỗi kết nối server.");
            inputElement.value = inputElement.getAttribute('data-original');
        })
        .finally(() => {
            btnElement.innerHTML = originalIcon;
            btnElement.disabled = false;
        });
}

function removeItem(productId) {
    if (confirm('Bạn có chắc muốn xóa sản phẩm này khỏi giỏ hàng?')) {
        window.location.href = '/Cart/Remove/' + productId;
    }
}