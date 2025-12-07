using System;

namespace WEBPC_NHANVIEN.Areas.Admin.Models
{
    public class SanPhamViewModel
    {
        public int MaSanPham { get; set; }
        public string TenSanPham { get; set; }
        public decimal GiaBan { get; set; }
        public decimal? GiaKhuyenMai { get; set; } // Có thể null
        public int SoLuongTon { get; set; }
        public string TenDanhMuc { get; set; }
        public string HinhAnh { get; set; } // URL hoặc tên file ảnh
        public bool TrangThai { get; set; }
    }
}