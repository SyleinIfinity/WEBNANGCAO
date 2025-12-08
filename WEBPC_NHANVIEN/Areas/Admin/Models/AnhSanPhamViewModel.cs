using System;

namespace WEBPC_NHANVIEN.Areas.Admin.Models
{
    public class AnhSanPhamViewModel
    {
        public int Id { get; set; }

        /// <summary>
        /// Đường dẫn ảnh (URL hiển thị trên web)
        /// </summary>
        public string Url { get; set; }

        /// <summary>
        /// PublicId trên hệ thống lưu trữ (ví dụ Cloudinary)
        /// </summary>
        public string PublicId { get; set; }

        /// <summary>
        /// Có phải ảnh đại diện hay không
        /// </summary>
        public bool LaAnhDaiDien { get; set; }
    }
}
