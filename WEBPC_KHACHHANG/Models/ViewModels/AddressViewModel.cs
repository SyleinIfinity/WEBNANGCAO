using System.ComponentModel.DataAnnotations;

namespace WEBPC_KHACHHANG.Models.ViewModels
{
    // Dùng để hiển thị danh sách
    public class AddressViewModel
    {
        public int MaSoDiaChi { get; set; }
        public string TenNguoiNhan { get; set; }
        public string SoDienThoai { get; set; }
        public string TinhThanh { get; set; }
        public string QuanHuyen { get; set; }
        public string PhuongXa { get; set; }
        public string DiaChiCuThe { get; set; }
        public bool IsDefault { get; set; }

        // Helper để hiển thị địa chỉ đầy đủ
        public string FullAddress => $"{DiaChiCuThe}, {PhuongXa}, {QuanHuyen}, {TinhThanh}";
    }

    // Class phụ dùng cho Dropdown
    public class LocationItem
    {
        public string Id { get; set; }
        public string Name { get; set; }
    }
}