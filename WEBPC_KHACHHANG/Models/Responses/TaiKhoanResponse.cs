namespace WEBPC_NHANVIEN.Models.Responses
{
    // Class này hứng dữ liệu JSON trả về từ API Login
    public class UserLoginResponse
    {
        public int MaNhanVien { get; set; }
        public string HoTen { get; set; }
        public string Token { get; set; }
        public int MaVaiTro { get; set; } // 1: Admin, 2: Sale, 3: Tech
        public string TenVaiTro { get; set; }
    }
}