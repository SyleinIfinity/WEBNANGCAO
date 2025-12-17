using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace WEBPC_KHACHHANG.Models.ViewModels
{
    // 1. Class item
    public class CartItemViewModel
    {
        // JSON không có MaChiTietGioHang, ta dùng tạm MaSanPham để định danh
        [JsonProperty("maSanPham")]
        public int ProductId { get; set; }

        [JsonProperty("tenSanPham")]
        public string ProductName { get; set; }

        [JsonProperty("hinhAnh")]
        public string ProductImage { get; set; }

        [JsonProperty("donGia")]
        public decimal OriginalPrice { get; set; }

        [JsonProperty("giaKhuyenMai")]
        public decimal PromotionPrice { get; set; }

        [JsonProperty("soLuong")]
        public int Quantity { get; set; }

        [JsonProperty("thanhTien")]
        public decimal Total { get; set; }

        // Logic hiển thị: Nếu có giá khuyến mãi thì lấy, không thì lấy đơn giá
        public decimal Price => PromotionPrice > 0 ? PromotionPrice : OriginalPrice;
    }

    // 2. Class tổng
    public class CartViewModel
    {
        // [SỬA LẠI]: Map đúng với key "chiTiet" trong JSON
        [JsonProperty("chiTiet")]
        public List<CartItemViewModel> Items { get; set; }

        public CartViewModel()
        {
            Items = new List<CartItemViewModel>();
        }

        [JsonProperty("tongTienHang")]
        public decimal TongTienTuAPI { get; set; }

        // Tính toán lại để hiển thị realtime
        public decimal TotalAmount => Items?.Sum(x => x.Total) ?? 0;
        public int TotalQuantity => Items?.Sum(x => x.Quantity) ?? 0;
    }
}