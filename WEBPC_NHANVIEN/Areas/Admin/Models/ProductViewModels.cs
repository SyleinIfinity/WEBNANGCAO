using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web; // Dùng cho HttpPostedFileBase

namespace WEBPC_NHANVIEN.Areas.Admin.Models
{
    // Dùng cho Dropdown danh mục
    public class CategoryViewModel
    {
        public int MaDanhMuc { get; set; }
        public string TenDanhMuc { get; set; }
    }

    // Dùng cho Form Tạo mới
    public class CreateProductViewModel
    {
        [Required(ErrorMessage = "Tên sản phẩm là bắt buộc")]
        public string TenSanPham { get; set; }

        [Required]
        public decimal GiaBan { get; set; }
        public decimal? GiaKhuyenMai { get; set; }

        [Required]
        public int SoLuongTon { get; set; }
        public string MoTa { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn danh mục")]
        public int MaDanhMuc { get; set; }

        public bool TrangThai { get; set; } = true;

        // MVC 5 dùng HttpPostedFileBase để hứng file upload
        public List<HttpPostedFileBase> HinhAnhs { get; set; }
    }

    // Dùng cho Form Chỉnh sửa
    public class UpdateProductViewModel
    {
        // Các field cơ bản (Lặp lại hoặc kế thừa từ Create)
        public int MaSanPham { get; set; }

        [System.ComponentModel.DataAnnotations.Required]
        public string TenSanPham { get; set; }

        public decimal GiaBan { get; set; }
        public decimal? GiaKhuyenMai { get; set; }
        public int SoLuongTon { get; set; }
        public string MoTa { get; set; }
        public int MaDanhMuc { get; set; }
        public bool TrangThai { get; set; }

        // Xử lý ảnh
        public List<System.Web.HttpPostedFileBase> HinhAnhs { get; set; } // Ảnh mới upload

        // Quan trọng: Danh sách ảnh cũ để hiển thị
        public List<ImageDTO> AnhHienTai { get; set; } = new List<ImageDTO>();

        // Danh sách ID ảnh muốn xóa
        public List<string> PublicIdsToDelete { get; set; }
    }
    public class ThongSoViewModel
    {
        public int MaThongSo { get; set; }
        public int MaSanPham { get; set; }

        [Required(ErrorMessage = "Tên thông số là bắt buộc")]
        [StringLength(100, ErrorMessage = "Tên thông số không được quá 100 ký tự")]
        public string TenThongSo { get; set; }

        [Required(ErrorMessage = "Giá trị là bắt buộc")]
        [StringLength(500, ErrorMessage = "Giá trị không được quá 500 ký tự")]
        public string GiaTri { get; set; }
    }
    public class CreateThongSoViewModel
    {
        [Required(ErrorMessage = "Mã sản phẩm là bắt buộc")]
        public int MaSanPham { get; set; }

        [Required(ErrorMessage = "Tên thông số là bắt buộc")]
        [StringLength(100, ErrorMessage = "Tên thông số không được quá 100 ký tự")]
        public string TenThongSo { get; set; }

        [Required(ErrorMessage = "Giá trị là bắt buộc")]
        [StringLength(500, ErrorMessage = "Giá trị không được quá 500 ký tự")]
        public string GiaTri { get; set; }
    }
    public class UpdateThongSoViewModel
    {
        [Required]
        public int MaThongSo { get; set; }

        [Required]
        public int MaSanPham { get; set; }

        [Required(ErrorMessage = "Tên thông số là bắt buộc")]
        [StringLength(100, ErrorMessage = "Tên thông số không được quá 100 ký tự")]
        public string TenThongSo { get; set; }

        [Required(ErrorMessage = "Giá trị là bắt buộc")]
        [StringLength(500, ErrorMessage = "Giá trị không được quá 500 ký tự")]
        public string GiaTri { get; set; }
    }
}