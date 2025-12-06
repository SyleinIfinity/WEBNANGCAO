using System;

namespace WEBPC_NHANVIEN.Areas.Admin.Models
{
    public class KhachHangViewModel
    {
        public int MaKhachHang { get; set; }
        public string HoTen { get; set; }
        public string Email { get; set; }
        public string SoDienThoai { get; set; }
        public DateTime? NgayTao { get; set; }
        public string TrangThai { get; set; } // Active/Banned
        public int? TongDonHang { get; set; } // (Optional) Số đơn đã mua
    }
}