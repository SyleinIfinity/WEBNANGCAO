using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WEBPC_KHACHHANG.Models.Responses
{
    public class ApiOrderResponse
    {
        public string message { get; set; }
        public int maDonHang { get; set; }
        public string maCode { get; set; }
        public decimal tongTien { get; set; }
    }
}