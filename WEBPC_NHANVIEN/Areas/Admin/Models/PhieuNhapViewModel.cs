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
        // Matches: public int MaNhanVienNhap { get; set; }
        [JsonProperty("maNhanVienNhap")]
        public int MaNhanVienNhap { get; set; }

        // Matches: public string? GhiChu { get; set; }
        [JsonProperty("ghiChu")]
        public string GhiChu { get; set; }

        // Matches: public List<ChiTietPhieuNhapItem> ChiTiet { get; set; }
        [JsonProperty("chiTiet")]
        public List<ChiTietPhieuNhapItemViewModel> ChiTiet { get; set; } = new List<ChiTietPhieuNhapItemViewModel>();

        // Helper property for View (ignored by API)
        [JsonIgnore]
        public DateTime NgayNhap { get; set; } = DateTime.Now;
    }

    public class ChiTietPhieuNhapItemViewModel
    {
        // Matches: public int MaSanPham { get; set; }
        [JsonProperty("maSanPham")]
        public int MaSanPham { get; set; }

        // Matches: public int SoLuongNhap { get; set; }
        [JsonProperty("soLuongNhap")]
        public int SoLuongNhap { get; set; }

        // Matches: public decimal GiaNhap { get; set; }
        [JsonProperty("giaNhap")]
        public decimal GiaNhap { get; set; }

        [JsonIgnore]
        public string TenSanPham { get; set; }

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