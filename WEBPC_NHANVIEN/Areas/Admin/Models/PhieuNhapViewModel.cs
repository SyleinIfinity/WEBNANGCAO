using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using Newtonsoft.Json;

namespace WEBPC_NHANVIEN.Areas.Admin.Models
{
    // --- 1. VIEW MODEL CHO CREATE (Khớp CreatePhieuNhapRequest) ---
    public class PhieuNhapCreateViewModel
    {
        // API yêu cầu bắt buộc trường này
        [Required]
        [JsonProperty("maNhanVienNhap")]
        public int MaNhanVienNhap { get; set; }

        [JsonProperty("ghiChu")]
        public string GhiChu { get; set; }

        // [LƯU Ý] API không nhận NgayNhap, nên trường này chỉ dùng để hiển thị trên View (nếu cần)
        [JsonIgnore]
        public DateTime NgayNhapHienThi { get; set; } = DateTime.Now;

        [Required]
        [JsonProperty("chiTiet")] // Tên field phải là "chiTiet" giống trong API Request
        public List<ChiTietPhieuNhapItemViewModel> ChiTiet { get; set; } = new List<ChiTietPhieuNhapItemViewModel>();
    }

    public class ChiTietPhieuNhapItemViewModel
    {
        [Required]
        [JsonProperty("maSanPham")]
        public int MaSanPham { get; set; }

        [JsonIgnore] // Chỉ dùng để hiển thị tên SP trên giao diện, không gửi API
        public string TenSanPham { get; set; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Số lượng phải > 0")]
        [JsonProperty("soLuongNhap")]
        public int SoLuongNhap { get; set; }

        [Required]
        [Range(0, double.MaxValue, ErrorMessage = "Giá nhập không được âm")]
        [JsonProperty("giaNhap")]
        public decimal GiaNhap { get; set; }

        [JsonIgnore]
        public decimal ThanhTien => SoLuongNhap * GiaNhap;
    }

    // --- 2. VIEW MODEL CHO EDIT (Khớp UpdatePhieuNhapRequest) ---
    public class PhieuNhapEditViewModel
    {
        [Required]
        public int MaPhieuNhap { get; set; }

        public string MaCodePhieu { get; set; } // Để hiển thị header

        // API Update chỉ nhận GhiChu
        [JsonProperty("ghiChu")]
        public string GhiChu { get; set; }

        // Các trường dưới chỉ để hiển thị (Read-only), không gửi đi khi PATCH
        public DateTime NgayNhap { get; set; }
        public string TenNhanVien { get; set; }
        public List<ChiTietPhieuNhapResponseViewModel> ChiTietHienThi { get; set; }
    }

    // --- 3. VIEW MODEL NHẬN DỮ LIỆU TỪ API (Khớp PhieuNhapResponse) ---
    public class PhieuNhapResponseViewModel
    {
        public int MaPhieuNhap { get; set; }
        public string MaCodePhieu { get; set; }
        public DateTime NgayNhap { get; set; }
        public decimal TongTienNhap { get; set; }
        public string TenNhanVien { get; set; }
        public string GhiChu { get; set; }
        public List<ChiTietPhieuNhapResponseViewModel> ChiTiet { get; set; }
    }

    public class ChiTietPhieuNhapResponseViewModel
    {
        public int MaChiTietPhieuNhap { get; set; }
        public int MaSanPham { get; set; }
        public string TenSanPham { get; set; }
        public int SoLuongNhap { get; set; }
        public decimal GiaNhap { get; set; }
        public decimal ThanhTien => SoLuongNhap * GiaNhap;
    }
}