document.addEventListener("DOMContentLoaded", function () {

    var productId = document.getElementById("MaSanPham")
        ? document.getElementById("MaSanPham").value
        : window.PRODUCT_ID;

    var getUrl = window.SPEC_GET_URL;
    var createUrl = window.SPEC_CREATE_URL;
    var updateUrl = window.SPEC_UPDATE_URL;
    var deleteUrl = window.SPEC_DELETE_URL;

    var specLoading = document.getElementById("spec-loading");
    var specEmpty = document.getElementById("spec-empty");
    var specTableBody = document.getElementById("specTableBody");

    var specModal = document.getElementById("specModal");
    var specIdInput = document.getElementById("specId");
    var specNameInput = document.getElementById("specName");
    var specValueInput = document.getElementById("specValue");
    var btnAddSpec = document.getElementById("btnAddSpec");
    var btnSaveSpec = document.getElementById("btnSaveSpec");

    function renderSpecs(list) {
        specTableBody.innerHTML = "";

        if (!list || list.length === 0) {
            specEmpty.style.display = "block";
            return;
        }

        specEmpty.style.display = "none";

        list.forEach(function (item) {
            var id = item.maThongSo || item.MaThongSo;
            var name = item.tenThongSo || item.TenThongSo;
            var value = item.giaTri || item.GiaTri;

            var tr = document.createElement("tr");
            tr.setAttribute("data-id", id);
            tr.setAttribute("data-name", name);
            tr.setAttribute("data-value", value);

            tr.innerHTML = `
                <td>${name}</td>
                <td>${value}</td>
                <td class="text-end">
                    <button type="button" class="btn btn-sm btn-outline-primary btn-edit-spec">Sửa</button>
                    <button type="button" class="btn btn-sm btn-outline-danger btn-delete-spec">Xóa</button>
                </td>
            `;

            specTableBody.appendChild(tr);
        });
    }

    function loadSpecs() {
        specLoading.style.display = "block";
        specEmpty.style.display = "none";
        specTableBody.innerHTML = "";

        fetch(getUrl + "?productId=" + productId)
            .then(res => res.json())
            .then(data => renderSpecs(data))
            .catch(() => {
                specEmpty.textContent = "Không thể tải thông số kỹ thuật.";
                specEmpty.style.display = "block";
            })
            .finally(() => {
                specLoading.style.display = "none";
            });
    }

    function openModal(mode, spec) {
        specModal.style.display = "block";
        specModal.dataset.mode = mode;

        document.getElementById("specModalLabel").innerText =
            mode === "edit" ? "Cập nhật thông số" : "Thêm thông số";

        specIdInput.value = spec ? spec.id : "";
        specNameInput.value = spec ? spec.name : "";
        specValueInput.value = spec ? spec.value : "";
    }

    function closeModal() {
        specModal.style.display = "none";
    }

    btnAddSpec.addEventListener("click", () => openModal("create"));

    btnSaveSpec.addEventListener("click", function () {
        var id = specIdInput.value;
        var name = specNameInput.value.trim();
        var value = specValueInput.value.trim();

        if (!name || !value) {
            alert("Vui lòng nhập đầy đủ thông tin!");
            return;
        }

        var fd = new FormData();
        fd.append("TenThongSo", name);
        fd.append("GiaTri", value);

        var url = createUrl;

        if (id) {
            fd.append("MaThongSo", id);
            url = updateUrl;
        } else {
            fd.append("MaSanPham", productId);
        }

        fetch(url, { method: "POST", body: fd })
            .then(res => res.json())
            .then(res => {
                if (res.success) {
                    closeModal();
                    loadSpecs();
                } else {
                    alert(res.error || "Lỗi lưu thông số");
                }
            });
    });

    specTableBody.addEventListener("click", function (e) {
        var tr = e.target.closest("tr");
        if (!tr) return;

        var id = tr.dataset.id;
        var name = tr.dataset.name;
        var value = tr.dataset.value;

        if (e.target.classList.contains("btn-edit-spec")) {
            openModal("edit", { id, name, value });
        }

        if (e.target.classList.contains("btn-delete-spec")) {
            if (!confirm("Xóa thông số này?")) return;

            var fd = new FormData();
            fd.append("id", id);

            fetch(deleteUrl, { method: "POST", body: fd })
                .then(res => res.json())
                .then(res => {
                    if (res.success) loadSpecs();
                    else alert(res.error || "Xóa thất bại");
                });
        }
    });

    document.querySelectorAll("[data-close-spec-modal]").forEach(btn => {
        btn.addEventListener("click", closeModal);
    });

    loadSpecs();
});
