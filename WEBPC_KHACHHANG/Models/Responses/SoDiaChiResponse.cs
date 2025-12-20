using Newtonsoft.Json;

namespace WEBPC_KHACHHANG.Models.Responses
{
    public class SoDiaChiResponse
    {
        [JsonProperty("maSoDiaChi")]
        public int MaSoDiaChi { get; set; }

        [JsonProperty("tenNguoiNhan")]
        public string TenNguoiNhan { get; set; }

        [JsonProperty("soDienThoai")]
        public string SoDienThoai { get; set; }

        // --- MAP CÁC TRƯỜNG LẺ TỪ API (Quan Trọng) ---
        [JsonProperty("diaChiCuThe")]
        public string DiaChiCuThe { get; set; }

        [JsonProperty("tenPhuongXa")]
        public string TenPhuongXa { get; set; }

        [JsonProperty("tenQuanHuyen")]
        public string TenQuanHuyen { get; set; }

        [JsonProperty("tenTinhThanh")]
        public string TenTinhThanh { get; set; }

        [JsonProperty("macDinh")]
        public bool MacDinh { get; set; }

        // --- TỰ TẠO ĐỊA CHỈ ĐẦY ĐỦ TẠI CLIENT ---
        // Property này không cần [JsonProperty] vì nó tự tính toán
        public string DiaChiDayDu
        {
            get
            {
                return $"{DiaChiCuThe}, {TenPhuongXa}, {TenQuanHuyen}, {TenTinhThanh}";
            }
        }
    }
}