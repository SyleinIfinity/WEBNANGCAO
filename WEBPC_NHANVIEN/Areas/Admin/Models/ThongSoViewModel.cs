using System.ComponentModel.DataAnnotations;

namespace WEBPC_NHANVIEN.Areas.Admin.Models
{
    public class ThongSoViewModel
    {
        public int MaThongSo { get; set; }

        [Required]
        public int MaSanPham { get; set; }

        [Required(ErrorMessage = "Tên thông số không được để trống")]
        public string TenThongSo { get; set; }

        [Required(ErrorMessage = "Giá trị không được để trống")]
        public string GiaTri { get; set; }
    }
}
