using System;
using Newtonsoft.Json; // Cần dùng thư viện này nếu tên không khớp

namespace WEBPC_NHANVIEN.Areas.Admin.Models
{
    public class NhanVienViewModel
    {
        public int MaNhanVien { get; set; }
        public string MaCodeNhanVien { get; set; }
        public string HoTen { get; set; }
        public string Email { get; set; }
        public string SoDienThoai { get; set; }
        public string TenVaiTro { get; set; }

        // [SỬA] Đổi tên cho khớp với API (TrangThaiTaiKhoan)
        // Hoặc dùng [JsonProperty("TrangThaiTaiKhoan")] nếu muốn giữ tên ngắn gọn
        [JsonProperty("TrangThaiTaiKhoan")]
        public string TrangThai { get; set; }
    }
}