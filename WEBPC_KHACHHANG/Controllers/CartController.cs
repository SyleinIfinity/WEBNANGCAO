using Newtonsoft.Json;
using System;
using System.Configuration;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using WEBPC_KHACHHANG.Models.Requests;
using WEBPC_KHACHHANG.Models.Responses;
using WEBPC_KHACHHANG.Models.ViewModels;

namespace WEBPC_KHACHHANG.Controllers
{
    public class CartController : Controller
    {
        private readonly string _apiBaseUrl = ConfigurationManager.AppSettings["ApiBaseUrl"];

        // GET: Cart/Index
        public async Task<ActionResult> Index()
        {
            var user = Session["User"] as UserLoginResponse;
            // [SỬA ĐOẠN NÀY]: Thêm returnUrl = /Cart
            if (user == null) return RedirectToAction("Index", "Login", new { returnUrl = "/Cart" });

            CartViewModel cart = new CartViewModel();
            try
            {
                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri(_apiBaseUrl);
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", user.Token);

                    // SỬA LẠI ĐÚNG API: GET api/GioHang/{maKhachHang}
                    var response = await client.GetAsync($"GioHang/{user.MaKhachHang}");

                    if (response.IsSuccessStatusCode)
                    {
                        var content = await response.Content.ReadAsStringAsync();
                        cart = JsonConvert.DeserializeObject<CartViewModel>(content);
                    }
                }
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Lỗi tải giỏ hàng: " + ex.Message;
            }

            return View(cart);
        }

        // POST: Cart/AddToCart
        [HttpPost]
        // Thêm tham số string type = "" vào hàm
        public async Task<ActionResult> AddToCart(int productId, int quantity, string type = "")
        {
            var user = Session["User"] as UserLoginResponse;
            if (user == null) return RedirectToAction("Index", "Login");

            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(_apiBaseUrl);
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", user.Token);

                var requestData = new AddToCartRequest
                {
                    MaKhachHang = user.MaKhachHang,
                    MaSanPham = productId,
                    SoLuong = quantity
                };

                var content = new StringContent(JsonConvert.SerializeObject(requestData), Encoding.UTF8, "application/json");

                var response = await client.PostAsync("GioHang/add", content);

                if (response.IsSuccessStatusCode)
                {
                    if (type == "buy_now")
                    {
                        // Chuyển hướng sang Checkout và truyền luôn ID sản phẩm vừa mua
                        return RedirectToAction("Checkout", "ThanhToan", new { selectedIds = productId });
                    }

                    // Mặc định thì về lại trang giỏ hàng
                    return RedirectToAction("Index");
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    TempData["Message"] = "Lỗi thêm giỏ hàng: " + errorContent;
                    return RedirectToAction("Detail", "Product", new { id = productId });
                }
            }
        }
        // Action: Xóa sản phẩm khỏi giỏ hàng
        public async Task<ActionResult> Remove(int id)
        {
            // 1. Kiểm tra đăng nhập
            var user = Session["User"] as UserLoginResponse;
            if (user == null)
            {
                return RedirectToAction("Index", "Login");
            }

            // 2. Gọi API xóa sản phẩm
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(_apiBaseUrl);
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", user.Token);

                // API Endpoint: DELETE api/GioHang/remove-item?maKhachHang=...&maSanPham=...
                // Lưu ý: Phải truyền đúng tên tham số là 'maKhachHang' và 'maSanPham' như bên API quy định
                string endpoint = $"GioHang/remove-item?maKhachHang={user.MaKhachHang}&maSanPham={id}";

                try
                {
                    var response = await client.DeleteAsync(endpoint);

                    if (response.IsSuccessStatusCode)
                    {
                        // Xóa thành công -> Load lại trang giỏ hàng
                        TempData["Message"] = "Đã xóa sản phẩm khỏi giỏ hàng.";
                    }
                    else
                    {
                        // Xóa thất bại -> Hiện lỗi
                        var errorContent = await response.Content.ReadAsStringAsync();
                        TempData["Message"] = "Xóa thất bại: " + errorContent;
                    }
                }
                catch (Exception ex)
                {
                    TempData["Message"] = "Lỗi kết nối: " + ex.Message;
                }
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<JsonResult> UpdateQuantity(int productId, int quantity)
        {
            // 1. Kiểm tra đăng nhập
            var user = Session["User"] as WEBPC_KHACHHANG.Models.Responses.UserLoginResponse;
            if (user == null)
            {
                // [SỬA ĐOẠN NÀY]: Trả về flag requireLogin = true
                return Json(new { success = false, requireLogin = true, message = "Phiên đăng nhập hết hạn!" });
            }

            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(_apiBaseUrl);
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", user.Token);

                // 2. Tạo payload đúng với API GioHangController (UpdateCartItemRequest)
                // Dựa vào yêu cầu trước đó, tên trường là 'soLuongMoi'
                var requestData = new
                {
                    maKhachHang = user.MaKhachHang,
                    maSanPham = productId,
                    soLuongMoi = quantity // API yêu cầu 'soLuongMoi'
                };

                var jsonContent = JsonConvert.SerializeObject(requestData);
                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                // 3. Gọi API bằng phương thức PATCH
                // API Route: api/GioHang/update-item
                var request = new HttpRequestMessage(new HttpMethod("PATCH"), "GioHang/update-item")
                {
                    Content = content
                };

                try
                {
                    var response = await client.SendAsync(request);

                    if (response.IsSuccessStatusCode)
                    {
                        // Trả về success để JS xử lý tiếp
                        return Json(new { success = true });
                    }
                    else
                    {
                        var errorMsg = await response.Content.ReadAsStringAsync();
                        return Json(new { success = false, message = "Lỗi API: " + errorMsg });
                    }
                }
                catch (Exception ex)
                {
                    return Json(new { success = false, message = "Lỗi hệ thống: " + ex.Message });
                }
            }
        }
    }
}