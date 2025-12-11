using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WEBPC_KHACHHANG.Models.Requests
{
    public class AddToCartRequest
    {
        // Phải đúng tên biến như bên API yêu cầu
        public int MaKhachHang { get; set; }
        public int MaSanPham { get; set; }
        public int SoLuong { get; set; }
    }
}