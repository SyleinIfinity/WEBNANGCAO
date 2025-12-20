using Newtonsoft.Json;
using System;

namespace WEBPC_KHACHHANG.Models.Responses
{
    public class KhachHangResponse
    {
        [JsonProperty("maKhachHang")]
        public int MaKhachHang { get; set; }

        [JsonProperty("hoTen")]
        public string HoTen { get; set; }

        [JsonProperty("soDienThoai")]
        public string SoDienThoai { get; set; } // Đây là cái ta cần

        [JsonProperty("email")]
        public string Email { get; set; }
    }
}