using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json; // Cần thư viện này để map dữ liệu

namespace WEBPC_KHACHHANG.Models.ViewModels
{
    // 1. Class đại diện cho từng món hàng (Map với ChiTietGioHangResponse của API)
    public class CartItemViewModel
    {
        [JsonProperty("maSanPham")] // API gửi 'maSanPham' -> Web nhận vào 'ProductId'
        public int ProductId { get; set; }

        [JsonProperty("tenSanPham")]
        public string ProductName { get; set; }

        [JsonProperty("hinhAnh")]
        public string ProductImage { get; set; }

        [JsonProperty("donGia")]
        public decimal Price { get; set; }

        [JsonProperty("soLuong")]
        public int Quantity { get; set; }

        // Tính thành tiền hiển thị (Giá x Số lượng)
        public decimal Total => Price * Quantity;
    }

    // 2. Class đại diện cho giỏ hàng tổng (Map với GioHangResponse của API)
    public class CartViewModel
    {
        // Quan trọng: Map danh sách 'chiTiet' từ API vào 'Items' của Web
        [JsonProperty("chiTiet")]
        public List<CartItemViewModel> Items { get; set; }

        public CartViewModel()
        {
            Items = new List<CartItemViewModel>();
        }

        // Map tổng tiền tạm tính từ API (nếu cần dùng)
        [JsonProperty("tongTienHang")]
        public decimal TongTienTuAPI { get; set; }

        // Web tự tính lại tổng tiền dựa trên danh sách Items (để hiển thị realtime)
        public decimal TotalAmount => Items?.Sum(x => x.Total) ?? 0;

        public int TotalQuantity => Items?.Sum(x => x.Quantity) ?? 0;
    }

}