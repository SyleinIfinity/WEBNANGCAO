using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WEBPC_KHACHHANG.Models.Requests
{
    public class ChatRequest
    {
        public int MaKhachHang { get; set; }
        public string Message { get; set; }
    }
}