using System.ComponentModel.DataAnnotations;

namespace WEBPC_KHACHHANG.Models.Requests
{
    public class RegisterRequest
    {
        [Required]
        public string hoTen { get; set; }

        [Required]
        [RegularExpression(@"^[0-9]{10,11}$")]
        public string soDienThoai { get; set; }

        [Required]
        [EmailAddress]
        public string email { get; set; }

        [Required]
        [MinLength(4)]
        public string tenDangNhap { get; set; }

        [Required]
        [MinLength(6)]
        public string matKhau { get; set; }
    }
}
