using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Mvc;
using Newtonsoft.Json;
using WEBPC_KHACHHANG.Models.ViewModels;

namespace WEBPC_KHACHHANG.Controllers
{
    public class CartController : Controller
    {
        // Đọc URL API từ Web.config
        private readonly string _apiBaseUrl = ConfigurationManager.AppSettings["ApiBaseUrl"];

        // Helper: Lấy giỏ hàng từ Session, nếu chưa có thì tạo mới
        private List<CartItemViewModel> GetCart()
        {
            var cart = Session["GioHang"] as List<CartItemViewModel>;
            if (cart == null)
            {
                cart = new List<CartItemViewModel>();
                Session["GioHang"] = cart;
            }
            return cart;
        }

        // 1. HIỂN THỊ GIỎ HÀNG (Cart/Index)
        public ActionResult Index()
        {
            var cart = GetCart();
            return View(cart);
        }

        // 2. THÊM VÀO GIỎ HÀNG (AJAX POST)
        [HttpPost]
        public async Task<ActionResult> AddToCart(int productId, int quantity = 1)
        {
            var cart = GetCart();
            var item = cart.FirstOrDefault(x => x.MaSanPham == productId);

            if (item != null)
            {
                // Nếu đã có -> Cộng dồn số lượng
                item.SoLuong += quantity;
            }
            else
            {
                // Nếu chưa có -> Gọi API lấy thông tin sản phẩm
                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri(_apiBaseUrl);
                    var response = await client.GetAsync($"SanPham/{productId}");

                    if (response.IsSuccessStatusCode)
                    {
                        var json = await response.Content.ReadAsStringAsync();
                        var product = JsonConvert.DeserializeObject<ProductViewModel>(json);

                        if (product != null)
                        {
                            item = new CartItemViewModel
                            {
                                MaSanPham = productId,
                                TenSanPham = product.TenSanPham,
                                DonGia = product.GiaKhuyenMai ?? product.GiaBan,
                                HinhAnh = product.HinhAnhDaiDien,
                                SoLuong = quantity
                            };
                            cart.Add(item);
                        }
                    }
                    else
                    {
                        return Json(new { success = false, message = "Không tìm thấy sản phẩm hoặc lỗi kết nối API." });
                    }
                }
            }

            Session["GioHang"] = cart;
            return Json(new { success = true, message = "Thêm thành công!", totalItems = cart.Sum(x => x.SoLuong) });
        }

        // 3. CẬP NHẬT SỐ LƯỢNG (AJAX POST)
        [HttpPost]
        public ActionResult UpdateQuantity(int productId, int quantity)
        {
            var cart = GetCart();
            var item = cart.FirstOrDefault(x => x.MaSanPham == productId);
            if (item != null)
            {
                item.SoLuong = quantity;
                if (item.SoLuong <= 0) cart.Remove(item);
            }
            Session["GioHang"] = cart;
            return Json(new { success = true });
        }

        // 4. XÓA SẢN PHẨM (AJAX POST)
        [HttpPost]
        public ActionResult Remove(int productId)
        {
            var cart = GetCart();
            var item = cart.FirstOrDefault(x => x.MaSanPham == productId);
            if (item != null)
            {
                cart.Remove(item);
            }
            Session["GioHang"] = cart;
            return Json(new { success = true });
        }

        // 5. TRANG THANH TOÁN (Checkout)
        public ActionResult Checkout(string selectedIds)
        {
            var cart = GetCart();
            var checkoutList = new List<CartItemViewModel>();

            if (!string.IsNullOrEmpty(selectedIds))
            {
                try
                {
                    // Tách chuỗi ID (ví dụ: "1,5,8")
                    var ids = selectedIds.Split(',').Select(int.Parse).ToList();
                    checkoutList = cart.Where(x => ids.Contains(x.MaSanPham)).ToList();
                }
                catch
                {
                    // Nếu lỗi parse ID, chuyển về trang Index
                    return RedirectToAction("Index");
                }
            }

            // Nếu không có sản phẩm nào hợp lệ, quay về giỏ hàng (Index)
            if (checkoutList.Count == 0)
            {
                return RedirectToAction("Index");
            }

            // Trả về View Checkout
            return View(checkoutList);
        }

        // 6. XÁC NHẬN ĐẶT HÀNG (Sau khi nhập form)
        [HttpPost]
        public ActionResult ConfirmOrder(string HoTen, string SoDienThoai, string DiaChi, string selectedIds)
        {
            var cart = GetCart();
            List<CartItemViewModel> orderItems;

            // Nếu có selectedIds, chỉ xử lý các sản phẩm đã chọn
            if (!string.IsNullOrEmpty(selectedIds))
            {
                try
                {
                    var ids = selectedIds.Split(',').Select(int.Parse).ToList();
                    orderItems = cart.Where(x => ids.Contains(x.MaSanPham)).ToList();
                }
                catch
                {
                    return RedirectToAction("Index");
                }
            }
            else
            {
                orderItems = cart;
            }

            if (orderItems == null || !orderItems.Any())
            {
                return RedirectToAction("Index", "Home");
            }

            // === BƯỚC 1: XỬ LÝ DỮ LIỆU VÀ GỌI API LƯU ĐƠN HÀNG ===
            var totalAmount = orderItems.Sum(item => item.ThanhTien);

            // [MÔ PHỎNG] Ghi lại log đơn hàng
            System.Diagnostics.Debug.WriteLine($"[ORDER PLACED] Customer: {HoTen}, Total: {totalAmount:N0} VND");

            // === BƯỚC 2: XÓA CÁC SẢN PHẨM ĐÃ ĐẶT KHỎI GIỎ HÀNG ===
            foreach (var item in orderItems)
            {
                cart.Remove(item);
            }
            Session["GioHang"] = cart;

            // === BƯỚC 3: TRẢ VỀ TRANG THÔNG BÁO THÀNH CÔNG ===
            ViewBag.CustomerName = HoTen;
            ViewBag.TotalAmount = totalAmount;

            return View("OrderSuccess");
        }
    }
}