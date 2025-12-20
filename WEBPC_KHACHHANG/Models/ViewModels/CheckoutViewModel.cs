using System.Collections.Generic;
using WEBPC_KHACHHANG.Models.Responses;

namespace WEBPC_KHACHHANG.Models.ViewModels
{
    public class CheckoutViewModel
    {
        // 1. Thông tin người dùng
        public UserLoginResponse User { get; set; } // Đổi tên NguoiDung -> User cho ngắn gọn hoặc giữ nguyên tùy ý

        // 2. Danh sách địa chỉ (Thống nhất dùng tên này)
        public List<SoDiaChiResponse> Addresses { get; set; }

        // 3. Giỏ hàng (Chứa danh sách sản phẩm)
        // Dùng CartViewModel vì nó đã cấu hình map đúng với JSON API
        public CartViewModel Cart { get; set; }

        // 4. Các trường phục vụ Submit Form
        public string SelectedIdsString { get; set; } // "101,102"
        public string PhuongThucThanhToan { get; set; } // "COD", "VietQR"
        public string DiaChiGiaoHang { get; set; } // Địa chỉ text
        public string NguoiNhan { get; set; }
        public string SoDienThoai { get; set; }

        public List<KhuyenMaiKhachHangResponse> DanhSachKhuyenMai { get; set; } = new List<KhuyenMaiKhachHangResponse>();

        // Các biến tính tiền
        public decimal TamTinh { get; set; }
        public decimal PhiVanChuyen { get; set; }
        public decimal TongThanhToan { get; set; }
    }
}