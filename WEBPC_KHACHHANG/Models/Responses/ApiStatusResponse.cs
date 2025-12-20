using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WEBPC_KHACHHANG.Models.Responses
{
    public class ApiStatusResponse
    {
        public string status { get; set; } // "PAID" hoặc "PENDING"
        public string message { get; set; }
        public bool shouldRedirect { get; set; }
    }
}