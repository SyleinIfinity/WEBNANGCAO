// Hàm thay đổi ảnh sản phẩm
function changeImage(src) {
    document.getElementById('mainImage').src = src;
}

// HÀM XỬ LÝ SỰ KIỆN NÚT "THÊM VÀO GIỎ"
function addToCartDetail(id) {
    // 1. Kiểm tra trạng thái đăng nhập
    if (window.DetailConfig.isUserLoggedIn === "false") {
        var loginModalElement = document.getElementById('loginModal');
        if (loginModalElement && typeof bootstrap !== 'undefined') {
            var myModal = new bootstrap.Modal(loginModalElement);
            myModal.show();
        } else {
            if (confirm('Bạn cần đăng nhập để thêm sản phẩm. Chuyển đến trang đăng nhập?')) {
                window.location.href = window.DetailConfig.loginUrl;
            }
        }
        return;
    }

    // 3. Nếu đã đăng nhập: Submit form
    var qty = document.getElementById('inputQty').value;
    var form = document.createElement("form");
    form.method = "POST";
    form.action = "/Cart/AddToCart";

    var inputId = document.createElement("input");
    inputId.type = "hidden";
    inputId.name = "productId";
    inputId.value = id;
    form.appendChild(inputId);

    var inputQty = document.createElement("input");
    inputQty.type = "hidden";
    inputQty.name = "quantity";
    inputQty.value = qty;
    form.appendChild(inputQty);

    document.body.appendChild(form);
    form.submit();
}

function buyNow(id) {
    // 1. Kiểm tra đăng nhập
    if (window.DetailConfig.isUserLoggedIn === "false") {
        var loginModalElement = document.getElementById('loginModal');
        if (loginModalElement && typeof bootstrap !== 'undefined') {
            var myModal = new bootstrap.Modal(loginModalElement);
            myModal.show();
        } else {
            if (confirm('Bạn cần đăng nhập để mua hàng. Chuyển đến trang đăng nhập?')) {
                window.location.href = window.DetailConfig.loginUrl;
            }
        }
        return;
    }

    // 2. Xử lý "Mua ngay"
    var qty = document.getElementById('inputQty').value;

    var form = document.createElement("form");
    form.method = "POST";
    form.action = "/Cart/AddToCart";

    var inputId = document.createElement("input");
    inputId.type = "hidden";
    inputId.name = "productId";
    inputId.value = id;
    form.appendChild(inputId);

    var inputQty = document.createElement("input");
    inputQty.type = "hidden";
    inputQty.name = "quantity";
    inputQty.value = qty;
    form.appendChild(inputQty);

    // Cờ hiệu Mua ngay
    var inputType = document.createElement("input");
    inputType.type = "hidden";
    inputType.name = "type";
    inputType.value = "buy_now";
    form.appendChild(inputType);

    document.body.appendChild(form);
    form.submit();
}