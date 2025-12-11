using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WEBPC_KHACHHANG.Models.ViewModels;
// using WEBPC_KHACHHANG.Models; // Bỏ comment dòng này nếu bạn cần dùng DB Context

namespace WEBPC_KHACHHANG.Controllers
{
    public class CartController : Controller
    {
        private const string CartSessionKey = "CartSession";

        // GET: Cart/Index
        //public ActionResult Index()
        //{
        //    var cart = GetCartService();
        //    return View(cart);
        //}
        // GET: Cart/Index
        public ActionResult Index()
        {
            var cart = GetCartService();

            // --- ĐOẠN CODE TEST: Tự động thêm 1 sản phẩm nếu giỏ hàng trống ---
            if (cart.Items.Count == 0)
            {
                cart.Items.Add(new WEBPC_KHACHHANG.Models.ViewModels.CartItemViewModel
                {
                    ProductId = 999, // ID giả
                    ProductName = "Sản phẩm Test (Intel Core i9)",
                    ProductImage = "https://images.unsplash.com/photo-1591799264318-7e6ef8ddb7ea?w=100",
                    Price = 2500000,
                    Quantity = 1
                });

                // Lưu tạm vào Session để các nút Tăng/Giảm/Xóa hoạt động được luôn
                Session["CartSession"] = cart;
            }
            // ------------------------------------------------------------------

            return View(cart);
        }

        // Action: Thêm vào giỏ hàng
        public ActionResult AddToCart(int productId, int quantity = 1)
        {
            var cart = GetCartService();
            var item = cart.Items.FirstOrDefault(x => x.ProductId == productId);

            if (item != null)
            {
                // Nếu sản phẩm đã có -> Tăng số lượng
                item.Quantity += quantity;
            }
            else
            {
                // Nếu chưa có -> Lấy thông tin từ DB và thêm mới
                var product = GetProductFromDatabase(productId);
                if (product != null)
                {
                    cart.Items.Add(new CartItemViewModel
                    {
                        ProductId = product.ProductId,
                        ProductName = product.ProductName,
                        ProductImage = product.ProductImage, // Đường dẫn ảnh
                        Price = product.Price,
                        Quantity = quantity
                    });
                }
            }

            // Lưu lại Session
            SaveCartSession(cart);

            // Redirect lại trang hiện tại hoặc về trang giỏ hàng
            return RedirectToAction("Index");
        }

        // Action: Xóa sản phẩm
        public ActionResult Remove(int id)
        {
            var cart = GetCartService();
            var item = cart.Items.FirstOrDefault(x => x.ProductId == id);

            if (item != null)
            {
                cart.Items.Remove(item);
                SaveCartSession(cart);
            }

            return RedirectToAction("Index");
        }

        // Action: Cập nhật số lượng (Dùng cho Javascript Ajax hoặc Redirect)
        // Đây là hàm sẽ xử lý khi bạn bỏ comment dòng window.location.href trong file View
        public ActionResult Update(int productId, int quantity)
        {
            var cart = GetCartService();
            var item = cart.Items.FirstOrDefault(x => x.ProductId == productId);

            if (item != null)
            {
                if (quantity > 0)
                {
                    item.Quantity = quantity;
                }
                else
                {
                    // Nếu số lượng <= 0 thì xóa luôn
                    cart.Items.Remove(item);
                }
                SaveCartSession(cart);
            }

            return RedirectToAction("Index");
        }

        // Action: Xóa hết giỏ hàng
        public ActionResult Clear()
        {
            Session[CartSessionKey] = null;
            return RedirectToAction("Index");
        }

        // --- CÁC HÀM BỔ TRỢ (HELPER) ---

        // Lấy giỏ hàng từ Session
        private CartViewModel GetCartService()
        {
            var cart = Session[CartSessionKey] as CartViewModel;
            if (cart == null)
            {
                cart = new CartViewModel();
                Session[CartSessionKey] = cart;
            }
            return cart;
        }

        // Lưu giỏ hàng vào Session
        private void SaveCartSession(CartViewModel cart)
        {
            Session[CartSessionKey] = cart;
        }

        // Giả lập lấy sản phẩm từ DB (BẠN CẦN SỬA LẠI HÀM NÀY)
        private CartItemViewModel GetProductFromDatabase(int id)
        {
            // TODO: Kết nối Entity Framework hoặc DAO của bạn ở đây
            // Ví dụ: var product = db.SanPhams.Find(id);
            // return new CartItemViewModel { ... };

            // Code demo giả lập (Xóa đi khi ghép DB thật):
            if (id == 1) return new CartItemViewModel { ProductId = 1, ProductName = "Intel Core i9-14900K", Price = 2400000, ProductImage = "https://images.unsplash.com/photo-1591799264318-7e6ef8ddb7ea?w=100" };
            if (id == 2) return new CartItemViewModel { ProductId = 2, ProductName = "GeoForce RTX 5080", Price = 28990000, ProductImage = "https://images.unsplash.com/photo-1587202372634-32705e3bf49c?w=100" };

            return null; // Không tìm thấy
        }
    }
}