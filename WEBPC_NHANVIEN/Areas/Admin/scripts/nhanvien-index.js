document.addEventListener("DOMContentLoaded", function () {
    var searchInput = document.getElementById("nv-search");
    var table = document.getElementById("nv-table");
    if (!table) return;

    var tbody = table.querySelector("tbody");

    // Tìm kiếm realtime
    if (searchInput) {
        searchInput.addEventListener("input", function () {
            var q = (searchInput.value || "").toLowerCase().trim();

            Array.prototype.forEach.call(tbody.rows, function (row) {
                var text = row.innerText.toLowerCase();
                row.style.display = text.indexOf(q) !== -1 ? "" : "none";
            });
        });
    }

    // Confirm khi xóa
    tbody.addEventListener("click", function (e) {
        var btn = e.target.closest(".nv-btn-delete");
        if (!btn) return;

        var ok = confirm("Bạn có chắc chắn muốn xóa nhân viên này?");
        if (!ok) {
            e.preventDefault();
            e.stopPropagation();
        }
    });
});
