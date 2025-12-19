using System;
using System.ComponentModel.DataAnnotations;

namespace WEBPC_NHANVIEN.Areas.Admin.Models
{
    public class KhachHangViewModel
    {
        [Display(Name = "Mã khách hàng")]
        public long MaNguoiDung { get; set; } // Khớp với API Response

        [Required(ErrorMessage = "Họ tên là bắt buộc")]
        [Display(Name = "Họ và tên")]
        public string HoTen { get; set; }

        [Required(ErrorMessage = "Email là bắt buộc")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Số điện thoại là bắt buộc")]
        [Display(Name = "Số điện thoại")]
        public string SoDienThoai { get; set; }

        [Display(Name = "Giới tính")]
        public string GioiTinh { get; set; }

        [Display(Name = "Ngày sinh")]
        [DataType(DataType.Date)]
        public DateTime? NgaySinh { get; set; }

        [Display(Name = "Trạng thái")]
        public int TrangThai { get; set; } // 1: Hoạt động, 0: Khóa

        // Các trường dùng cho Create (API yêu cầu)
        [Display(Name = "Tên đăng nhập")]
        public string TenDangNhap { get; set; }

        [Display(Name = "Mật khẩu")]
        [DataType(DataType.Password)]
        public string MatKhau { get; set; }
    }
}