using System;

namespace WEBPC_NHANVIEN.Areas.Admin.Models
{
    public class KhachHangViewModel
    {
        public int MaKhachHang { get; set; }
        public string HoTen { get; set; }
        public string Email { get; set; }
        public string SoDienThoai { get; set; }

        // API chưa trả về Ngày Tạo, tạm thời để nullable để không lỗi
        public DateTime? NgayTao { get; set; }

        // API trả về 'CoTaiKhoan' (bool), ta map sang string để hiển thị
        public bool CoTaiKhoan { get; set; }

        // Property ảo để hiển thị ra View
        public string TrangThaiHienThi
        {
            get { return CoTaiKhoan ? "Đã có tài khoản" : "Khách vãng lai"; }
        }

        public int? TongDonHang { get; set; }
    }
}