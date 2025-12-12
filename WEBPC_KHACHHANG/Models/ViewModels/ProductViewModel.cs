using System.Collections.Generic;
using System.Linq;

namespace WEBPC_KHACHHANG.Models.ViewModels
{
    public class ProductViewModel
    {
        public int MaSanPham { get; set; }
        public string TenSanPham { get; set; }
        public decimal GiaBan { get; set; }
        public decimal? GiaKhuyenMai { get; set; }
        public int SoLuongTon { get; set; }
        public string TenDanhMuc { get; set; }
        public string MoTa { get; set; }
        public List<ThongSoKyThuatViewModel> ThongSoKyThuat { get; set; }

        public List<ImageDTO> DanhSachAnh { get; set; }

        // Logic lấy ảnh đại diện để hiển thị ngoài danh sách
        public string HinhAnhDaiDien
        {
            get
            {
                if (DanhSachAnh != null && DanhSachAnh.Any())
                {
                    // Lấy ảnh được đánh dấu là đại diện hoặc ảnh đầu tiên
                    var img = DanhSachAnh.FirstOrDefault(x => x.LaAnhDaiDien) ?? DanhSachAnh.First();
                    return img.Url; 
                }
                return "https://via.placeholder.com/300x300?text=No+Image";
            }
        }
        
    }
    public class ThongSoKyThuatViewModel
    {
        public int MaThongSo { get; set; }
        public int MaSanPham { get; set; }
        public string TenThongSo { get; set; }
        public string GiaTri { get; set; }
    }


    // Class con để hứng dữ liệu ảnh
    public class ImageDTO
    {
        public int Id { get; set; }
        public string Url { get; set; }
        public bool LaAnhDaiDien { get; set; }
    }

    // Class dùng cho Dropdown lọc danh mục (Optional)
    // Tìm class CategoryViewModel ở cuối file và sửa thành:
    public class CategoryViewModel
    {
        public int MaDanhMuc { get; set; }
        public string TenDanhMuc { get; set; }

        // --- BỔ SUNG ---
        public int? MaDanhMucCha { get; set; }
        public int SoLuongDanhMucCon { get; set; }
    }
}