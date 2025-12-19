using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace WEBPC_KHACHHANG.Models.ViewModels
{
    public class LichSuDonHangViewModel
    {
        [JsonProperty("maDonHang")]
        public int MaDonHang { get; set; }

        [JsonProperty("maCodeDonHang")]
        public string MaCodeDonHang { get; set; }

        [JsonProperty("maKhachHang")]
        public int MaKhachHang { get; set; }

        [JsonProperty("ngayDat")]
        public DateTime NgayDat { get; set; }

        [JsonProperty("trangThai")]
        public string TrangThai { get; set; }

        [JsonProperty("phuongThucThanhToan")]
        public string PhuongThucThanhToan { get; set; }

        [JsonProperty("nguoiNhan")]
        public string NguoiNhan { get; set; }

        [JsonProperty("soDienThoaiGiao")]
        public string SoDienThoaiGiao { get; set; }

        [JsonProperty("diaChiGiaoHang")]
        public string DiaChiGiaoHang { get; set; }

        [JsonProperty("phiVanChuyen")]
        public decimal PhiVanChuyen { get; set; }

        [JsonProperty("tongTien")]
        public decimal TongTien { get; set; }

        [JsonProperty("chiTiet")]
        public List<ChiTietDonHangViewModel> ChiTiet { get; set; }

        // [QUAN TRỌNG] Ánh xạ danh sách giao dịch
        [JsonProperty("giaoDichs")]
        public List<GiaoDichViewModel> GiaoDichs { get; set; }
    }

    public class ChiTietDonHangViewModel
    {
        [JsonProperty("maSanPham")]
        public int MaSanPham { get; set; }

        [JsonProperty("tenSanPham")]
        public string TenSanPham { get; set; }

        [JsonProperty("hinhAnh")]
        public string HinhAnh { get; set; }

        [JsonProperty("soLuong")]
        public int SoLuong { get; set; }

        [JsonProperty("donGiaLucMua")]
        public decimal DonGiaLucMua { get; set; }

        [JsonProperty("thanhTien")]
        public decimal ThanhTien { get; set; }
    }

    // [CẦN SỬA LẠI CLASS NÀY] Thêm JsonProperty cho từng thuộc tính
    public class GiaoDichViewModel
    {
        [JsonProperty("maGiaoDich")]
        public int MaGiaoDich { get; set; }

        [JsonProperty("ngayTao")]
        public DateTime NgayGiaoDich { get; set; }

        [JsonProperty("soTien")]
        public decimal SoTien { get; set; }

        [JsonProperty("trangThai")]
        public string TrangThai { get; set; } // "Pending", "Success", "Failed"

        [JsonProperty("phuongThuc")]
        public string PhuongThuc { get; set; }
    }
}