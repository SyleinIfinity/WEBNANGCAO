using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web.Mvc;
using WEBPC_KHACHHANG.Models.Responses;
using WEBPC_KHACHHANG.Models.ViewModels;

namespace WEBPC_KHACHHANG.Controllers
{
    public class ThanhToanController : Controller
    {
        private readonly string _apiBaseUrl = ConfigurationManager.AppSettings["ApiBaseUrl"];

        // GET: ThanhToan/Checkout
        // GET: ThanhToan/Checkout
        // [QUAN TRỌNG] Thêm tham số string selectedIds vào hàm
        public async Task<ActionResult> Checkout(string selectedIds)
        {
            // 1. Kiểm tra đăng nhập
            var user = Session["User"] as UserLoginResponse;
            if (user == null)
            {
                return RedirectToAction("Index", "Login", new { returnUrl = "/ThanhToan/Checkout" });
            }

            var model = new CheckoutViewModel
            {
                User = user,
                Cart = new CartViewModel(),
                Addresses = new List<SoDiaChiResponse>()
            };

            try
            {
                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri(_apiBaseUrl);
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", user.Token);

                    // --- CALL API 1: Lấy Giỏ Hàng (Toàn bộ) ---
                    var cartTask = client.GetAsync($"GioHang/{user.MaKhachHang}");

                    // --- CALL API 2: Lấy Địa chỉ ---
                    // Đảm bảo API này đúng đường dẫn. Thông thường API địa chỉ là: api/SoDiaChi?maKhachHang=...
                    var addressTask = client.GetAsync($"SoDiaChi/khachhang/{user.MaKhachHang}");

                    await Task.WhenAll(cartTask, addressTask);

                    var cartResponse = await cartTask;
                    var addressResponse = await addressTask;

                    // XỬ LÝ GIỎ HÀNG
                    if (cartResponse.IsSuccessStatusCode)
                    {
                        var cartContent = await cartResponse.Content.ReadAsStringAsync();
                        model.Cart = JsonConvert.DeserializeObject<CartViewModel>(cartContent);

                        // --- [LOGIC MỚI] LỌC SẢN PHẨM THEO SELECTED IDS ---
                        if (!string.IsNullOrEmpty(selectedIds) && model.Cart != null && model.Cart.Items != null)
                        {
                            // Tách chuỗi "1,2,3" thành List<int>
                            var idList = new List<int>();
                            foreach (var s in selectedIds.Split(','))
                            {
                                if (int.TryParse(s, out int id)) idList.Add(id);
                            }

                            // Chỉ giữ lại sản phẩm có trong danh sách chọn
                            model.Cart.Items = model.Cart.Items.Where(x => idList.Contains(x.ProductId)).ToList();

                        }
                    }

                    // XỬ LÝ ĐỊA CHỈ
                    if (addressResponse.IsSuccessStatusCode)
                    {
                        var addressContent = await addressResponse.Content.ReadAsStringAsync();

                        try
                        {
                            model.Addresses = JsonConvert.DeserializeObject<List<SoDiaChiResponse>>(addressContent);
                        }
                        catch
                        {
                            // Phòng hờ lỗi parse thì gán list rỗng để không chết trang web
                            model.Addresses = new List<SoDiaChiResponse>();
                        }
                    }
                    else
                    {
                        model.Addresses = new List<SoDiaChiResponse>();
                    }
                }
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Lỗi tải dữ liệu: " + ex.Message;
            }

            // Nếu sau khi lọc mà giỏ hàng trống thì đẩy về trang Giỏ hàng
            if (model.Cart == null || model.Cart.Items == null || model.Cart.Items.Count == 0)
            {
                TempData["Message"] = "Vui lòng chọn sản phẩm để thanh toán.";
                return RedirectToAction("Index", "Cart");
            }

            return View(model);
        }
    }
}