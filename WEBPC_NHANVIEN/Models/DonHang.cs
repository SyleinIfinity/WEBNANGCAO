using System;
using System.Collections.Generic;

namespace WEBPC_NHANVIEN.Models
{
    public class DonHang
    {
        public int maDonHang { get; set; }
        public string maCodeDonHang { get; set; }
        public int maKhachHang { get; set; }
        public int? maNhanVienDuyet { get; set; } // Có thể NULL
        public DateTime ngayDat { get; set; }
        public decimal tongTien { get; set; }
        public string trangThai { get; set; }
        public string diaChiGiaoHang { get; set; }
        public string soDienThoaiGiao { get; set; }
        public string nguoiNhan { get; set; }
        public decimal phiVanChuyen { get; set; }
        public string TrangThai { get; set; }

        // Trường bổ sung để hiển thị (từ JOIN API)
        public string tenKhachHang { get; set; }
    }
}