using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace WEBPC_KHACHHANG.Models.ViewModels
{
    public class CartItemViewModel
    {
        // [QUAN TRỌNG]: Map đúng tên maChiTietGioHang từ API
        [JsonProperty("maChiTietGioHang")]
        public int CartItemId { get; set; }

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

        // Logic tính giá hiển thị
        public decimal Price => PromotionPrice > 0 ? PromotionPrice : OriginalPrice;

        // Tự tính tổng tiền tại Client để tránh lỗi số 0 từ API
        public decimal Total => Price * Quantity;
    }

    public class CartViewModel
    {
        // [CỰC KỲ QUAN TRỌNG]: Phải là "chiTietGioHangs" mới khớp với log JSON bạn gửi
        [JsonProperty("chiTietGioHangs")]
        public List<CartItemViewModel> Items { get; set; }

        public CartViewModel()
        {
            Items = new List<CartItemViewModel>();
        }

        [JsonProperty("tongTienHang")]
        public decimal TongTienTuAPI { get; set; }

        public decimal TotalAmount => Items?.Sum(x => x.Total) ?? 0;
        public int TotalQuantity => Items?.Sum(x => x.Quantity) ?? 0;
    }
}