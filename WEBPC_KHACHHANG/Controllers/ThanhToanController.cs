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
    public class ThanhToanController : Controller
    {
        private readonly string _apiBaseUrl = ConfigurationManager.AppSettings["ApiBaseUrl"];

        // 1. GET: Hiển thị trang thanh toán
        [HttpGet]
        public async Task<ActionResult> Checkout(string selectedIds)
        {
            var user = Session["User"] as UserLoginResponse;
            if (user == null) return RedirectToAction("Index", "Login", new { returnUrl = "/ThanhToan/Checkout" });

            if (string.IsNullOrEmpty(selectedIds))
            {
                TempData["Error"] = "Vui lòng chọn sản phẩm.";
                return RedirectToAction("Index", "Cart");
            }

            try
            {
                var model = new CheckoutViewModel
                {
                    User = user,
                    SelectedIdsString = selectedIds,
                    Cart = new CartViewModel(),
                    Addresses = new List<SoDiaChiResponse>()
                };

                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri(_apiBaseUrl);
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", user.Token);

                    // Lấy giỏ hàng
                    var responseFullCart = await client.GetAsync($"GioHang/{user.MaKhachHang}");
                    if (responseFullCart.IsSuccessStatusCode)
                    {
                        var content = await responseFullCart.Content.ReadAsStringAsync();
                        var fullCart = JsonConvert.DeserializeObject<CartViewModel>(content);

                        var selectedCartItemIds = selectedIds.Split(',').Select(int.Parse).ToList();

                        if (fullCart != null && fullCart.Items != null)
                        {
                            model.Cart.Items = fullCart.Items
                                .Where(x => selectedCartItemIds.Contains(x.CartItemId))
                                .ToList();
                        }
                    }

                    // Lấy địa chỉ
                    var responseAddr = await client.GetAsync($"SoDiaChi/khachhang/{user.MaKhachHang}");
                    if (responseAddr.IsSuccessStatusCode)
                    {
                        var dataAddr = await responseAddr.Content.ReadAsStringAsync();
                        model.Addresses = JsonConvert.DeserializeObject<List<SoDiaChiResponse>>(dataAddr);
                    }
                }
                return View(model);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Lỗi kết nối: " + ex.Message;
                return RedirectToAction("Index", "Cart");
            }
        }

        // 2. POST: Xử lý đặt hàng
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> PlaceOrder(CheckoutViewModel model)
        {
            var user = Session["User"] as UserLoginResponse;
            if (user == null) return RedirectToAction("Index", "Login");

            try
            {
                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri(_apiBaseUrl);
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", user.Token);

                    var request = new TaoDonHangRequest
                    {
                        MaKhachHang = user.MaKhachHang,
                        NguoiNhan = model.NguoiNhan,
                        SoDienThoai = model.SoDienThoai,
                        DiaChiGiaoHang = model.DiaChiGiaoHang,
                        PhuongThucThanhToan = model.PhuongThucThanhToan,
                        SelectedCartItemIds = new List<int>()
                    };

                    // Lọc CartItemId
                    if (!string.IsNullOrEmpty(model.SelectedIdsString))
                    {
                        var listIds = model.SelectedIdsString.Split(',').Select(int.Parse).ToList();
                        var cartResponse = await client.GetAsync($"GioHang/{user.MaKhachHang}");

                        if (cartResponse.IsSuccessStatusCode)
                        {
                            var cartContent = await cartResponse.Content.ReadAsStringAsync();
                            var fullCart = JsonConvert.DeserializeObject<CartViewModel>(cartContent);

                            if (fullCart != null && fullCart.Items != null)
                            {
                                var selectedItems = fullCart.Items
                                    .Where(x => listIds.Contains(x.CartItemId))
                                    .ToList();

                                foreach (var item in selectedItems)
                                {
                                    request.SelectedCartItemIds.Add(item.CartItemId);
                                }
                            }
                        }
                    }

                    // Kiểm tra danh sách trước khi gửi
                    if (request.SelectedCartItemIds.Count == 0)
                    {
                        TempData["Error"] = "Lỗi dữ liệu: Không tìm thấy sản phẩm trong giỏ hàng.";
                        return RedirectToAction("Index", "Cart");
                    }

                    // Gọi API
                    var content = new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json");
                    var response = await client.PostAsync("DonHang/create", content);

                    if (response.IsSuccessStatusCode)
                    {
                        var responseString = await response.Content.ReadAsStringAsync();
                        dynamic result = JsonConvert.DeserializeObject(responseString);
                        int maDonHang = result.maDonHang;

                        if (model.PhuongThucThanhToan == "VietQR")
                            return RedirectToAction("Payment", new { id = maDonHang });
                        else
                            return RedirectToAction("Success", new { id = maDonHang });
                    }
                    else
                    {
                        var errorContent = await response.Content.ReadAsStringAsync();
                        TempData["Error"] = "Đặt hàng thất bại: " + errorContent;
                        // Quan trọng: Truyền lại selectedIds để không bị đá về Cart
                        return RedirectToAction("Checkout", new { selectedIds = model.SelectedIdsString });
                    }
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Lỗi hệ thống: " + ex.Message;
                return RedirectToAction("Index", "Cart");
            }
        }

        // Action Payment
        [HttpGet]
        public async Task<ActionResult> Payment(int id)
        {
            var user = Session["User"] as UserLoginResponse;
            if (user == null) return RedirectToAction("Index", "Login");
            try
            {
                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri(_apiBaseUrl);
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", user.Token);
                    var response = await client.GetAsync($"Payment/get-qr/{id}");
                    if (response.IsSuccessStatusCode)
                    {
                        var jsonString = await response.Content.ReadAsStringAsync();
                        var apiResult = JsonConvert.DeserializeObject<ApiQrResponse>(jsonString);
                        if (apiResult != null && apiResult.data != null)
                        {
                            ViewBag.QrImage = apiResult.data.qrDataURL;
                            ViewBag.OrderId = id;
                            return View();
                        }
                    }
                }
                return RedirectToAction("Success", new { id = id });
            }
            catch { return RedirectToAction("Success", new { id = id }); }
        }

        // Action Success
        public ActionResult Success(int id)
        {
            ViewBag.OrderId = id;
            return View();
        }

        // Action CheckStatus
        [HttpGet]
        public async Task<JsonResult> CheckStatus(int orderId)
        {
            try
            {
                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri(_apiBaseUrl);
                    var response = await client.GetAsync($"Payment/check-status/{orderId}");
                    if (response.IsSuccessStatusCode)
                    {
                        var content = await response.Content.ReadAsStringAsync();
                        return Json(new { status = content.Trim('"') }, JsonRequestBehavior.AllowGet);
                    }
                }
                return Json(new { status = "ERROR" }, JsonRequestBehavior.AllowGet);
            }
            catch { return Json(new { status = "ERROR" }, JsonRequestBehavior.AllowGet); }
        }
    }
}