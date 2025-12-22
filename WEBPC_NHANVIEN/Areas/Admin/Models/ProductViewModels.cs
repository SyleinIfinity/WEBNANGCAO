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
        public string CoverImagePublicId { get; set; }
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
}