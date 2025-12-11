using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WEBPC_KHACHHANG.Models.ViewModels
{
    // Class đại diện cho từng sản phẩm trong giỏ
    public class CartItemViewModel
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public string ProductImage { get; set; }
        public decimal Price { get; set; }
        public int Quantity { get; set; }

        // Tính thành tiền của dòng này (Giá x Số lượng)
        public decimal Total => Price * Quantity;
    }

    // Class đại diện cho toàn bộ giỏ hàng
    public class CartViewModel
    {
        public List<CartItemViewModel> Items { get; set; }

        public CartViewModel()
        {
            Items = new List<CartItemViewModel>();
        }

        // Tính tổng tiền toàn bộ giỏ hàng
        public decimal TotalAmount => Items.Sum(x => x.Total);

        // Tính tổng số lượng sản phẩm (để hiển thị trên icon giỏ hàng header nếu cần)
        public int TotalQuantity => Items.Sum(x => x.Quantity);
    }
}