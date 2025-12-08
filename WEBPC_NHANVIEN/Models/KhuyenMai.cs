using System;

namespace WEBPC_NHANVIEN.Models
{
    public class KhuyenMai
    {
        // Tên biến phải khớp y hệt JSON từ API trả về (hoặc cột trong SQL)
        public int maKhuyenMai { get; set; }
        public string maCodeKM { get; set; }
        public string tenChuongTrinh { get; set; }
        public decimal giaTriGiam { get; set; }
        public string loaiGiam { get; set; } // "PERCENT" hoặc "DIRECT"
        public decimal? donHangToiThieu { get; set; }
        public decimal? giamToiDa { get; set; }
        public DateTime ngayBatDau { get; set; }
        public DateTime ngayKetThuc { get; set; }
        public int? soLuongConLai { get; set; }
    }
}