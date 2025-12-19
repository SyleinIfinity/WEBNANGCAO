using System.Collections.Generic;

namespace WEBPC_KHACHHANG.Models.Responses
{
    public class GioHangResponse
    {
        public int MaGioHang { get; set; }
        public int MaKhachHang { get; set; }
        public decimal TongTien { get; set; }

        // Danh sách chi tiết sản phẩm
        public List<ChiTietGioHangResponse> ChiTietGioHangs { get; set; }
    }

    public class ChiTietGioHangResponse
    {
        public int MaChiTietGioHang { get; set; }
        public int MaSanPham { get; set; }
        public string TenSanPham { get; set; }
        public string HinhAnh { get; set; } // API trả về URL ảnh
        public decimal GiaGoc { get; set; }
        public decimal GiaBan { get; set; } // Giá thực tế (sau khi giảm nếu có)
        public int SoLuong { get; set; }
        public decimal ThanhTien { get; set; }
    }
}