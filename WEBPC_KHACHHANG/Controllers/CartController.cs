using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
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
            if (user == null) return RedirectToAction("Index", "Login", new { returnUrl = "/Cart" });

            CartViewModel cart = new CartViewModel();
            try
            {
                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri(_apiBaseUrl);
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", user.Token);

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
                    // Xử lý Mua ngay (Mua lẻ 1 sản phẩm)
                    if (type == "buy_now")
                    {
                        var cartResponse = await client.GetAsync($"GioHang/{user.MaKhachHang}");
                        if (cartResponse.IsSuccessStatusCode)
                        {
                            var cartContent = await cartResponse.Content.ReadAsStringAsync();
                            var fullCart = JsonConvert.DeserializeObject<CartViewModel>(cartContent);

                            if (fullCart != null && fullCart.Items != null)
                            {
                                var itemVuaThem = fullCart.Items.FirstOrDefault(x => x.ProductId == productId);
                                if (itemVuaThem != null)
                                {
                                    return RedirectToAction("Checkout", "ThanhToan", new { selectedIds = itemVuaThem.CartItemId });
                                }
                            }
                        }
                        return RedirectToAction("Index");
                    }

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
            var user = Session["User"] as UserLoginResponse;
            if (user == null) return RedirectToAction("Index", "Login");

            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(_apiBaseUrl);
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", user.Token);

                string endpoint = $"GioHang/remove-item?maKhachHang={user.MaKhachHang}&maSanPham={id}";

                try
                {
                    var response = await client.DeleteAsync(endpoint);
                    if (response.IsSuccessStatusCode) TempData["Message"] = "Đã xóa sản phẩm khỏi giỏ hàng.";
                    else TempData["Message"] = "Xóa thất bại.";
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
            var user = Session["User"] as UserLoginResponse;
            if (user == null) return Json(new { success = false, requireLogin = true, message = "Phiên đăng nhập hết hạn!" });

            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(_apiBaseUrl);
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", user.Token);

                var requestData = new { maKhachHang = user.MaKhachHang, maSanPham = productId, soLuongMoi = quantity };
                var content = new StringContent(JsonConvert.SerializeObject(requestData), Encoding.UTF8, "application/json");
                var request = new HttpRequestMessage(new HttpMethod("PATCH"), "GioHang/update-item") { Content = content };

                try
                {
                    var response = await client.SendAsync(request);
                    if (response.IsSuccessStatusCode) return Json(new { success = true });
                    else return Json(new { success = false, message = "Lỗi API" });
                }
                catch (Exception ex) { return Json(new { success = false, message = ex.Message }); }
            }
        }

        // [MỚI] Action xử lý Build PC -> Thêm tất cả và trả về Link Checkout
        [HttpPost]
        public async Task<ActionResult> AddBuildPCToCart(List<int> productIds)
        {
            var user = Session["User"] as UserLoginResponse;
            // Nếu chưa đăng nhập, trả về URL login (JS sẽ xử lý modal, đây là fallback)
            if (user == null) return Json(new { success = false, url = Url.Action("Index", "Login") });

            if (productIds == null || !productIds.Any())
            {
                return Json(new { success = false, message = "Danh sách sản phẩm trống" });
            }

            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(_apiBaseUrl);
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", user.Token);

                // 1. Lặp qua danh sách và gọi API thêm từng món
                foreach (var id in productIds)
                {
                    var requestData = new AddToCartRequest
                    {
                        MaKhachHang = user.MaKhachHang,
                        MaSanPham = id,
                        SoLuong = 1
                    };
                    var content = new StringContent(JsonConvert.SerializeObject(requestData), Encoding.UTF8, "application/json");

                    // Gọi API thêm (await từng cái để đảm bảo thứ tự)
                    await client.PostAsync("GioHang/add", content);
                }

                // 2. Lấy lại giỏ hàng để tìm CartItemId của các món vừa thêm
                var cartResponse = await client.GetAsync($"GioHang/{user.MaKhachHang}");
                if (cartResponse.IsSuccessStatusCode)
                {
                    var cartContent = await cartResponse.Content.ReadAsStringAsync();
                    var fullCart = JsonConvert.DeserializeObject<CartViewModel>(cartContent);

                    if (fullCart != null && fullCart.Items != null)
                    {
                        // Lọc các món có ProductId nằm trong danh sách build
                        var selectedItems = fullCart.Items.Where(x => productIds.Contains(x.ProductId)).ToList();

                        if (selectedItems.Any())
                        {
                            // Tạo chuỗi ID: 10,11,12
                            var selectedIdsStr = string.Join(",", selectedItems.Select(x => x.CartItemId));

                            // Trả về URL Checkout
                            return Json(new
                            {
                                success = true,
                                url = Url.Action("Checkout", "ThanhToan", new { selectedIds = selectedIdsStr })
                            });
                        }
                    }
                }

                // Fallback: Về giỏ hàng thường nếu lỗi
                return Json(new { success = true, url = Url.Action("Index", "Cart") });
            }
        }
    }
}