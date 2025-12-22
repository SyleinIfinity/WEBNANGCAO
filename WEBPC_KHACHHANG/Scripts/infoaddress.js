var myModal;

$(document).ready(function () {
    myModal = new bootstrap.Modal(document.getElementById('addressModal'));
    loadProvinces();

    // 1. Chọn Tỉnh
    $('#ddlProvince').change(function () {
        var pid = $(this).val();
        var pname = $("#ddlProvince option:selected").text();
        $('#textProvince').val(pname);
        $('#maTinhHidden').val(pid);

        $('#ddlDistrict').empty().append('<option value="">Chọn Quận/Huyện</option>').prop('disabled', true);
        $('#ddlWard').empty().append('<option value="">Chọn Phường/Xã</option>').prop('disabled', true);

        if (pid) loadDistricts(pid);
    });

    // 2. Chọn Huyện
    $('#ddlDistrict').change(function () {
        var did = $(this).val();
        var dname = $("#ddlDistrict option:selected").text();
        $('#textDistrict').val(dname);
        $('#maHuyenHidden').val(did);

        $('#ddlWard').empty().append('<option value="">Chọn Phường/Xã</option>').prop('disabled', true);

        if (did) loadWards(did);
    });

    // 3. Chọn Xã
    $('#ddlWard').change(function () {
        var wname = $("#ddlWard option:selected").text();
        $('#textWard').val(wname);
        $('#maXaHidden').val($(this).val());
    });
});

// --- AJAX HELPERS ---
function loadProvinces() {
    return $.get('/InfoAddress/GetProvinces', function (data) {
        var html = '<option value="">Chọn Tỉnh/Thành</option>';
        $.each(data, function (key, item) {
            html += `<option value="${item.Id}">${item.Name}</option>`;
        });
        $('#ddlProvince').html(html);
    });
}

function loadDistricts(pid, selectedId = null) {
    return $.get('/InfoAddress/GetDistricts?provinceId=' + pid, function (data) {
        var html = '<option value="">Chọn Quận/Huyện</option>';
        $.each(data, function (key, item) {
            html += `<option value="${item.Id}">${item.Name}</option>`;
        });
        $('#ddlDistrict').html(html).prop('disabled', false);
        if (selectedId) $('#ddlDistrict').val(selectedId);
    });
}

function loadWards(did, selectedId = null) {
    return $.get('/InfoAddress/GetWards?districtId=' + did, function (data) {
        var html = '<option value="">Chọn Phường/Xã</option>';
        $.each(data, function (key, item) {
            html += `<option value="${item.Id}">${item.Name}</option>`;
        });
        $('#ddlWard').html(html).prop('disabled', false);
        if (selectedId) $('#ddlWard').val(selectedId);
    });
}

// --- UI HANDLERS ---
function openModalAdd() {
    $('#modalTitle').text('Thêm Địa Chỉ Mới');
    $('#addressForm')[0].reset();
    $('#maSoDiaChi').val(0);
    $('#ddlProvince').val('').trigger('change');
    $('#ddlDistrict').prop('disabled', true);
    $('#ddlWard').prop('disabled', true);
    myModal.show();
}

function openModalEdit(id) {
    $('#modalTitle').text('Cập Nhật Địa Chỉ');
    $.get('/InfoAddress/GetDetail?id=' + id, function (res) {
        if (res.success) {
            var d = res.data;
            $('#maSoDiaChi').val(d.MaSoDiaChi);
            $('#tenNguoiNhan').val(d.TenNguoiNhan);
            $('#soDienThoai').val(d.SoDienThoai);
            $('#diaChiCuThe').val(d.DiaChiCuThe);
            $('#isDefault').prop('checked', d.IsDefault);

            $('#maTinhHidden').val(d.MaTinh);
            $('#maHuyenHidden').val(d.MaHuyen);
            $('#maXaHidden').val(d.MaXa);
            $('#textProvince').val(d.TinhThanh);
            $('#textDistrict').val(d.QuanHuyen);
            $('#textWard').val(d.PhuongXa);

            $('#ddlProvince').val(d.MaTinh);

            loadDistricts(d.MaTinh, d.MaHuyen).done(function () {
                loadWards(d.MaHuyen, d.MaXa).done(function () {
                    myModal.show();
                });
            });
        } else {
            alert('Không thể tải thông tin địa chỉ.');
        }
    });
}

function submitAddress() {
    // 1. Chặn nút bấm
    var btn = $('button[onclick="submitAddress()"]');
    btn.prop('disabled', true).text('Đang lưu...');

    // 2. Tự tạo Object dữ liệu (SỬA LẠI TÊN KEY CHO KHỚP C#)
    var data = {
        MaSoDiaChi: $('#maSoDiaChi').val(),
        MaKhachHang: 0,

        TenNguoiNhan: $('#tenNguoiNhan').val(),
        SoDienThoai: $('#soDienThoai').val(),

        // --- SỬA TẠI ĐÂY: Đổi tên key thành MaTinh, MaHuyen, MaXa ---
        MaTinh: $('#maTinhHidden').val(),   // Khớp với public string MaTinh { get; set; }
        MaHuyen: $('#maHuyenHidden').val(), // Khớp với public string MaHuyen { get; set; }
        MaXa: $('#maXaHidden').val(),       // Khớp với public string MaXa { get; set; }

        DiaChiCuThe: $('#diaChiCuThe').val(),

        // Đảm bảo lấy true/false
        IsDefault: $('#isDefault').is(':checked')
    };

    // 3. Gửi Ajax
    $.ajax({
        url: '/InfoAddress/SaveAddress',
        type: 'POST',
        data: data,
        success: function (res) {
            if (res.success) {
                alert('Lưu địa chỉ thành công!');
                location.reload();
            } else {
                alert(res.message);
                btn.prop('disabled', false).text('Lưu Địa Chỉ');
            }
        },
        error: function (xhr, status, error) {
            console.error(xhr.responseText);
            alert('Lỗi kết nối server: ' + error);
            btn.prop('disabled', false).text('Lưu Địa Chỉ');
        }
    });
}

function deleteAddress(id) {
    if (confirm('Bạn có chắc muốn xóa địa chỉ này?')) {
        $.post('/InfoAddress/Delete?id=' + id, function (res) {
            if (res.success) {
                location.reload();
            } else {
                alert('Xóa thất bại.');
            }
        });
    }
}