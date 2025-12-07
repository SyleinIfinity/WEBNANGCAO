using System.Collections.Generic;
using System.Linq;

namespace WEBPC_NHANVIEN.Areas.Admin.Models
{
    public class SanPhamViewModel
    {
        public int MaSanPham { get; set; }
        public string TenSanPham { get; set; }
        public decimal GiaBan { get; set; }
        public decimal? GiaKhuyenMai { get; set; }
        public int SoLuongTon { get; set; }
        public string TenDanhMuc { get; set; }
        public bool TrangThai { get; set; }

        // Hứng danh sách ảnh từ API (Tên class con phải khớp cấu trúc JSON)
        public List<ImageDTO> DanhSachAnh { get; set; }

        public string HinhAnh
        {
            get
            {
                if (DanhSachAnh != null && DanhSachAnh.Any())
                {
                    // Ưu tiên ảnh đại diện, nếu ko có lấy ảnh đầu
                    var img = DanhSachAnh.FirstOrDefault(x => x.LaAnhDaiDien) ?? DanhSachAnh.First();
                    return img.Url; // [SỬA] Dùng .Url thay vì .UrlHinhAnh
                }
                return "";
            }
        }
    }

    // [QUAN TRỌNG] Class này phải khớp y hệt ImageResponse của API
    public class ImageDTO
    {
        public int Id { get; set; }          // [SỬA] API trả về Id
        public string Url { get; set; }      // [SỬA] API trả về Url
        public string PublicId { get; set; }
        public bool LaAnhDaiDien { get; set; }
    }
}