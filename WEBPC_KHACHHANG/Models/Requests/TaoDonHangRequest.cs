using System.Collections.Generic;

namespace WEBPC_KHACHHANG.Models.Requests
{
    public class TaoDonHangRequest
    {
        public int MaKhachHang { get; set; }
        public string NguoiNhan { get; set; }
        public string SoDienThoai { get; set; }
        public string DiaChiGiaoHang { get; set; }
        public string GhiChu { get; set; }

        // [MỚI] Bổ sung 2 trường này để khớp với API Server
        public string PhuongThucThanhToan { get; set; } // "COD" hoặc "VietQR"
        public List<int> SelectedCartItemIds { get; set; } = new List<int>();
    }
}