using Newtonsoft.Json;

namespace WEBPC_KHACHHANG.Models.Responses
{
    public class UserLoginResponse
    {
        [JsonProperty("maKhachHang")]
        public int MaKhachHang { get; set; }

        [JsonProperty("hoTen")]
        public string HoTen { get; set; }

        [JsonProperty("token")]
        public string Token { get; set; }

        [JsonProperty("tenVaiTro")]
        public string TenVaiTro { get; set; }

        // [QUAN TRỌNG] Thêm dòng này. 
        // API Login không trả về, nhưng ta sẽ gán thủ công ở Controller.
        public string SoDienThoai { get; set; }

        [JsonProperty("message")]
        public string Message { get; set; }
    }
}