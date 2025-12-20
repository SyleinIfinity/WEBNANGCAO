using System;
using System.ComponentModel.DataAnnotations;

namespace WEBPC_KHACHHANG.Models.ViewModels
{
    public class InfoUserViewModel
    {
        public int MaKhachHang { get; set; }

        [Display(Name = "Họ và tên")]
        public string HoTen { get; set; }

        [Display(Name = "Số điện thoại")]
        public string SoDienThoai { get; set; }

        [Display(Name = "Email")]
        public string Email { get; set; }

        [Display(Name = "Tên đăng nhập")]
        public string TenDangNhap { get; set; }

        [Display(Name = "Ngày sinh")]
        public DateTime? NgaySinh { get; set; }

        [Display(Name = "Giới tính")]
        public string GioiTinh { get; set; } // Có thể là "Nam", "Nữ", "Khác"

        // Dùng để hiển thị ảnh đại diện (nếu có)
        public string AvatarUrl { get; set; } = "https://via.placeholder.com/150";
    }
}