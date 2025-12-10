using System;

namespace WEBPC_KHACHHANG.Models.ViewModels
{
    public class CartItemViewModel
    {
        public int MaSanPham { get; set; }
        public string TenSanPham { get; set; }
        public string HinhAnh { get; set; }
        public decimal DonGia { get; set; }
        public int SoLuong { get; set; }

        // Thành tiền = Đơn giá * Số lượng
        public decimal ThanhTien => DonGia * SoLuong;
    }
}