function addToCart(id) {
    // 1. Kiểm tra trạng thái đăng nhập (Lấy từ biến toàn cục gán ở View)
    if (window.ProductConfig.isUserLoggedIn === "false") {
        var loginModalElement = document.getElementById('loginModal');
        if (loginModalElement && typeof bootstrap !== 'undefined') {
            var myModal = new bootstrap.Modal(loginModalElement);
            myModal.show();
        } else {
            if (confirm('Bạn cần đăng nhập để thêm sản phẩm. Chuyển đến trang đăng nhập?')) {
                window.location.href = window.ProductConfig.loginUrl;
            }
        }
        return;
    }

    // 3. Nếu đã đăng nhập: Thực hiện Submit form
    var qty = 1;
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