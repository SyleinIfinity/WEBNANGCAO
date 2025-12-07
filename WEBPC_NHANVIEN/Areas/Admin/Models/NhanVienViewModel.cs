using System;

namespace WEBPC_NHANVIEN.Areas.Admin.Models
{
    public class NhanVienViewModel
    {
        public int MaNhanVien { get; set; }
        public string MaCodeNhanVien { get; set; }
        public string HoTen { get; set; }
        public string Email { get; set; }
        public string SoDienThoai { get; set; }
        public string TenVaiTro { get; set; } // Ví dụ: "Admin", "Staff"
        public string TrangThai { get; set; }  // Ví dụ: "Active", "Locked"
    }
}