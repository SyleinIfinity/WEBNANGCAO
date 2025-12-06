using System;
using System.Collections.Generic;
using System.Linq;

namespace WEBPC_NHANVIEN.Areas.Admin.Models
{
    public class SanPhamViewModel
    {
        public int MaSanPham { get; set; }
        public string TenSanPham { get; set; }
        public decimal GiaBan { get; set; }
        public decimal? GiaKhuyenMai { get; set; }
        public int SoLuongTon { get; set; }
        public string TenDanhMuc { get; set; }
        public bool TrangThai { get; set; }

        // [SỬA] Thêm property để hứng danh sách ảnh từ API
        public List<ImageDTO> DanhSachAnh { get; set; }

        // [SỬA] Logic lấy ảnh đại diện (ảnh đầu tiên) để hiển thị trên bảng
        public string HinhAnh
        {
            get
            {
                if (DanhSachAnh != null && DanhSachAnh.Count > 0)
                    return DanhSachAnh[0].UrlHinhAnh;
                return "https://via.placeholder.com/150"; // Ảnh mặc định nếu ko có
            }
        }
    }

    // Class phụ để hứng object ảnh bên trong SanPham
    public class ImageDTO
    {
        public int MaHinhAnh { get; set; }
        public string UrlHinhAnh { get; set; }
        public bool LaAnhDaiDien { get; set; }
    }
}