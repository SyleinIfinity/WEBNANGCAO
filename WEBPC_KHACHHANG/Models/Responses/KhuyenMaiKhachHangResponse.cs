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
        public string MaCodeKM { get; set; }
        public string TenChuongTrinh { get; set; }
        public bool DaSuDung { get; set; }
        public DateTime? NgayThuThap { get; set; }
    }

}