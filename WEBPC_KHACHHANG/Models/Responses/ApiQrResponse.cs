using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WEBPC_KHACHHANG.Models.Responses
{
    public class ApiQrResponse
    {
        public string code { get; set; }
        public string desc { get; set; }
        public ApiQrData data { get; set; }
    }
}