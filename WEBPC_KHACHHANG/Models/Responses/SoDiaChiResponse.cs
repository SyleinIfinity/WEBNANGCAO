using Newtonsoft.Json;

namespace WEBPC_KHACHHANG.Models.Responses
{
    public class SoDiaChiResponse
    {
        // [QUAN TRỌNG] Tên trong JsonProperty phải khớp y chang API trả về
        [JsonProperty("maSoDiaChi")]
        public int MaSoDiaChi { get; set; }

        [JsonProperty("tenNguoiNhan")]
        public string TenNguoiNhan { get; set; }

        [JsonProperty("soDienThoai")]
        public string SoDienThoai { get; set; }

        [JsonProperty("diaChiDayDu")] // Kiểm tra kỹ API trả về 'diaChiDayDu' hay 'diaChi'
        public string DiaChiDayDu { get; set; }

        [JsonProperty("macDinh")]
        public bool MacDinh { get; set; }
    }
}