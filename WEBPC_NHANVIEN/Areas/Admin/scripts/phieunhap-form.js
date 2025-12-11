document.addEventListener("DOMContentLoaded", function () {
    const body = document.getElementById("pn-lines-body");
    const addBtn = document.getElementById("pn-add-line");
    const totalEl = document.getElementById("pn-total");
    const templateSelect = document.getElementById("product-template-select"); // Lấy template ẩn

    // Đánh lại chỉ số index cho name="ChiTiet[i].ThuocTinh" để MVC ModelBinder hiểu
    function reindexRows() {
        const rows = Array.from(body.querySelectorAll("tr"));
        rows.forEach((tr, idx) => {
            const select = tr.querySelector(".pn-product");
            if (select) select.name = `ChiTiet[${idx}].MaSanPham`;

            const qty = tr.querySelector(".pn-qty");
            if (qty) qty.name = `ChiTiet[${idx}].SoLuongNhap`;

            const price = tr.querySelector(".pn-price");
            if (price) price.name = `ChiTiet[${idx}].GiaNhap`;
        });
    }

    function recalcTotals() {
        let total = 0;
        Array.from(body.querySelectorAll("tr")).forEach(tr => {
            const qty = parseFloat((tr.querySelector(".pn-qty") || { value: 0 }).value) || 0;
            const price = parseFloat((tr.querySelector(".pn-price") || { value: 0 }).value) || 0;
            const line = qty * price;

            const lineEl = tr.querySelector(".pn-line-total");
            if (lineEl) lineEl.innerText = line.toLocaleString('vi-VN'); // Format số kiểu VN
            total += line;
        });
        if (totalEl) totalEl.innerText = total.toLocaleString('vi-VN');
    }

    function bindRowEvents(tr) {
        const qty = tr.querySelector(".pn-qty");
        const price = tr.querySelector(".pn-price");
        const remove = tr.querySelector(".pn-remove");

        if (qty) qty.addEventListener("input", recalcTotals);
        if (price) price.addEventListener("input", recalcTotals);

        if (remove) remove.addEventListener("click", function () {
            tr.remove();
            reindexRows();
            recalcTotals();
        });
    }

    function addRow() {
        const tr = document.createElement("tr");

        // 1. Cột Sản phẩm (Clone từ Template)
        const tdProd = document.createElement("td");
        const select = document.createElement("select");
        select.className = "form-select pn-product";

        if (templateSelect) {
            select.innerHTML = templateSelect.innerHTML; // Copy options
        }
        tdProd.appendChild(select);

        // 2. Cột Số lượng
        const tdQty = document.createElement("td");
        const inputQty = document.createElement("input");
        inputQty.type = "number";
        inputQty.min = "1";
        inputQty.className = "form-control pn-qty";
        inputQty.value = 1;
        tdQty.appendChild(inputQty);

        // 3. Cột Giá nhập
        const tdPrice = document.createElement("td");
        const inputPrice = document.createElement("input");
        inputPrice.type = "number";
        inputPrice.step = "1000";
        inputPrice.className = "form-control pn-price";
        inputPrice.value = 0;
        tdPrice.appendChild(inputPrice);

        // 4. Cột Thành tiền
        const tdLine = document.createElement("td");
        tdLine.className = "pn-line-total text-end align-middle";
        tdLine.innerText = "0";

        // 5. Cột Xóa
        const tdAction = document.createElement("td");
        tdAction.className = "text-center";
        const btn = document.createElement("button");
        btn.type = "button";
        btn.className = "btn btn-sm btn-outline-danger pn-remove";
        btn.innerText = "Xóa";
        tdAction.appendChild(btn);

        tr.appendChild(tdProd);
        tr.appendChild(tdQty);
        tr.appendChild(tdPrice);
        tr.appendChild(tdLine);
        tr.appendChild(tdAction);

        body.appendChild(tr);
        reindexRows();
        bindRowEvents(tr);
        recalcTotals();
    }

    if (addBtn) {
        addBtn.addEventListener("click", function () {
            addRow();
        });
    }

    // Khởi tạo event cho các dòng có sẵn (nếu Model trả về dữ liệu khi Validation lỗi)
    Array.from(body.querySelectorAll("tr")).forEach(tr => {
        bindRowEvents(tr);
    });

    // Tính toán lại tổng tiền ban đầu
    recalcTotals();
});