using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WEBPC_KHACHHANG.Models.ViewModels
{
    public class KhuyenMaiDaLuuViewModel
    {
        public int MaKMKH { get; set; }        // ⭐ QUAN TRỌNG
        public int MaKhuyenMai { get; set; }
        public string MaCodeKM { get; set; }
        public string TenChuongTrinh { get; set; }
        public string LoaiGiam { get; set; }
        public decimal GiaTriGiam { get; set; }
        public decimal DonHangToiThieu { get; set; }
        public decimal? GiamToiDa { get; set; }
        public DateTime NgayKetThuc { get; set; }
    }

}