using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WEBPC_KHACHHANG.Models.Responses
{
    public class KhuyenMaiKhachHangResponse
    {
        public int MaKMKH { get; set; }
        public int MaKhuyenMai { get; set; }
        public int MaKhachHang { get; set; }
        // [QUAN TRỌNG] Tên property phải khớp với JSON API trả về
        public string MaCodeKM { get; set; }        // Trong code cũ em gọi là MaCode
        public string TenChuongTrinh { get; set; }  // Trong code cũ em gọi là TenKhuyenMai
        public bool DaSuDung { get; set; }
        public DateTime? NgayThuThap { get; set; }

        // --- CÁC TRƯỜNG BỔ SUNG TỪ BẢNG KHUYENMAI ---
        public decimal GiaTriGiam { get; set; }     // Thay cho PhanTramGiam
        public string LoaiGiam { get; set; }        // "PERCENT" hoặc "DIRECT" (trừ tiền thẳng)
        public decimal DonHangToiThieu { get; set; }// Thay cho DonToiThieu
        public decimal? GiamToiDa { get; set; }
        public DateTime NgayBatDau { get; set; }
        public DateTime NgayKetThuc { get; set; }
    }

}