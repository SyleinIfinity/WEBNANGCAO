using Newtonsoft.Json;

namespace WEBPC_KHACHHANG.Models.Responses
{
    public class UserLoginResponse
    {
        // Map trường "maKhachHang" từ JSON vào biến MaKhachHang của C#
        [JsonProperty("maKhachHang")]
        public int MaKhachHang { get; set; }

        [JsonProperty("hoTen")]
        public string HoTen { get; set; }

        [JsonProperty("token")]
        public string Token { get; set; }

        [JsonProperty("tenVaiTro")]
        public string TenVaiTro { get; set; }

        // Thêm trường này để bắt lỗi nếu API trả về message lỗi
        [JsonProperty("message")]
        public string Message { get; set; }
    }
}