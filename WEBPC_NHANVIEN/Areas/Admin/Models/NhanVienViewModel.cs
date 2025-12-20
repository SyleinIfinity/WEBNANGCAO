using System.ComponentModel.DataAnnotations;

namespace WEBPC_NHANVIEN.Areas.Admin.Models
{
    // Dùng cho trang danh sách
    public class NhanVienListItemViewModel
    {
        public int MaNhanVien { get; set; }
        public string MaCodeNhanVien { get; set; }

        public string HoTen { get; set; }
        public string SoDienThoai { get; set; }

        public int MaVaiTro { get; set; }
        public string TenVaiTro { get; set; }

        public int MaTaiKhoan { get; set; }
        public string TenDangNhap { get; set; }
        public string Email { get; set; }

        public string TrangThaiTaiKhoan { get; set; }
    }

    // Dùng cho Create (POST)
    public class NhanVienCreateViewModel
    {
        // Thông tin cá nhân
        [Required(ErrorMessage = "Họ tên không được để trống")]
        public string HoTen { get; set; }

        public string SoDienThoai { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn vai trò")]
        [Display(Name = "Vai trò")]
        public int MaVaiTro { get; set; }

        public string Email { get; set; }

        // Thông tin tài khoản
        [Required(ErrorMessage = "Tên đăng nhập không được để trống")]
        public string TenDangNhap { get; set; }

        [Required(ErrorMessage = "Mật khẩu không được để trống")]
        [DataType(DataType.Password)]
        public string MatKhau { get; set; }

        // API hiện tại không nhận trạng thái, nhưng View đang có field này
        public string TrangThaiTaiKhoan { get; set; }
    }

    // Dùng cho Edit (GET + POST)
    public class NhanVienEditViewModel
    {
        [Required]
        public int MaNhanVien { get; set; }

        public string MaCodeNhanVien { get; set; }

        // Thông tin cá nhân
        [Required(ErrorMessage = "Họ tên không được để trống")]
        public string HoTen { get; set; }

        public string SoDienThoai { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn vai trò")]
        [Display(Name = "Vai trò")]
        public int MaVaiTro { get; set; }

        public string Email { get; set; }

        // Thông tin tài khoản
        [Required(ErrorMessage = "Tên đăng nhập không được để trống")]
        public string TenDangNhap { get; set; }

        // Khi sửa: để trống nếu không đổi mật khẩu
        [DataType(DataType.Password)]
        public string MatKhau { get; set; }

        public string TrangThaiTaiKhoan { get; set; }
    }

    // Vai trò (dùng cho dropdown + có thể tái sử dụng sau này)
    public class VaiTroViewModel
    {
        public int MaVaiTro { get; set; }
        public string TenVaiTro { get; set; }
        public string MoTa { get; set; }
    }

    // DTO gửi lên API (map 1-1 với NhanVienRequest trong webpc-api)
    public class NhanVienApiRequest
    {
        // Thông tin cá nhân
        public string HoTen { get; set; }
        public string SoDienThoai { get; set; }
        public int MaVaiTro { get; set; }

        // Thông tin tài khoản
        public string TenDangNhap { get; set; }
        public string MatKhau { get; set; }
        public string Email { get; set; }
    }
}
