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
        // Lấy URL từ Web.config
        private readonly string _apiBaseUrl = ConfigurationManager.AppSettings["ApiBaseUrl"];

        // 1. GET: Hiển thị trang thanh toán (GD1)
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

                // [SỬA LỖI]: Khởi tạo HttpClient mới trong khối using để đảm bảo BaseAddress luôn đúng
                using (var client = new HttpClient())
                {
                    // Thiết lập Base Address từ Config
                    if (string.IsNullOrEmpty(_apiBaseUrl))
                        throw new Exception("Chưa cấu hình ApiBaseUrl trong Web.config");

                    client.BaseAddress = new Uri(_apiBaseUrl);
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", user.Token);

                    // --- LẤY FULL GIỎ HÀNG VÀ LỌC ---
                    // Vì BaseAddress đã có "https://.../api/", ta chỉ cần truyền phần đuôi
                    var responseFullCart = await client.GetAsync($"GioHang/{user.MaKhachHang}");

                    if (responseFullCart.IsSuccessStatusCode)
                    {
                        var content = await responseFullCart.Content.ReadAsStringAsync();
                        var fullCart = JsonConvert.DeserializeObject<CartViewModel>(content);

                        var selectedProductIds = selectedIds.Split(',').Select(int.Parse).ToList();

                        // Lọc sản phẩm theo ID đã chọn
                        if (fullCart != null && fullCart.Items != null)
                        {
                            model.Cart.Items = fullCart.Items
                                .Where(x => selectedProductIds.Contains(x.ProductId))
                                .ToList();
                        }
                    }

                    // --- LẤY ĐỊA CHỈ ---
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

        // 2. POST: Xử lý đặt hàng (GD2 & GD3)
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

                    // 1. Chuẩn bị dữ liệu gửi API
                    var request = new TaoDonHangRequest
                    {
                        MaKhachHang = user.MaKhachHang,
                        NguoiNhan = model.NguoiNhan,
                        SoDienThoai = model.SoDienThoai,
                        DiaChiGiaoHang = model.DiaChiGiaoHang,
                        PhuongThucThanhToan = model.PhuongThucThanhToan,
                        SelectedCartItemIds = new List<int>()
                    };

                    // [LOGIC MAPPING]: Lấy CartItemId nếu cần thiết
                    // Hiện tại Client đang giữ ProductId trong SelectedIdsString
                    if (!string.IsNullOrEmpty(model.SelectedIdsString))
                    {
                        // Logic cũ: API DonHang nhận ProductID thì giữ nguyên dòng này
                        request.SelectedCartItemIds = model.SelectedIdsString.Split(',').Select(int.Parse).ToList();

                        // Nếu API DonHang bắt buộc nhận CartItemID (chứ ko phải ProductID), 
                        // bạn cần gọi lại API GetGioHang ở đây để map ID sang.
                    }

                    // 2. Gọi API Tạo Đơn Hàng
                    var content = new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json");
                    var response = await client.PostAsync("DonHang/create", content);

                    if (response.IsSuccessStatusCode)
                    {
                        var responseString = await response.Content.ReadAsStringAsync();
                        // Dùng dynamic để lấy nhanh maDonHang trả về
                        dynamic result = JsonConvert.DeserializeObject(responseString);
                        int maDonHang = result.maDonHang;

                        // 3. Phân luồng
                        if (model.PhuongThucThanhToan == "VietQR")
                        {
                            return RedirectToAction("Payment", new { id = maDonHang });
                        }
                        else
                        {
                            return RedirectToAction("Success", new { id = maDonHang });
                        }
                    }
                    else
                    {
                        var errorContent = await response.Content.ReadAsStringAsync();
                        // Thử parse lỗi từ API nếu có
                        try
                        {
                            dynamic err = JsonConvert.DeserializeObject(errorContent);
                            TempData["Error"] = "Đặt hàng thất bại: " + err.message;
                        }
                        catch
                        {
                            TempData["Error"] = "Đặt hàng thất bại: " + errorContent;
                        }

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

        // 3. GET: Trang Thanh Toán QR (GD3)
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

                    // Gọi API lấy thông tin QR
                    var response = await client.GetAsync($"Payment/get-qr/{id}");

                    if (response.IsSuccessStatusCode)
                    {
                        var jsonString = await response.Content.ReadAsStringAsync();
                        var apiResult = JsonConvert.DeserializeObject<ApiQrResponse>(jsonString);

                        if (apiResult != null && apiResult.data != null)
                        {
                            ViewBag.QrImage = apiResult.data.qrDataURL;
                            ViewBag.OrderId = id;
                            // Truyền thêm số tiền và mã đơn để hiển thị nếu cần
                            // ViewBag.Amount = ...
                            return View();
                        }
                    }
                }

                TempData["Error"] = "Không thể tạo mã QR thanh toán.";
                return RedirectToAction("Index", "Home");
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Lỗi: " + ex.Message;
                return RedirectToAction("Index", "Home");
            }
        }

        // 4. GET: Trang Thành công (GD2)
        public ActionResult Success(int id)
        {
            ViewBag.OrderId = id;
            return View();
        }

        // 5. Check Status (Polling AJAX cho trang Payment)
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
            catch
            {
                return Json(new { status = "ERROR" }, JsonRequestBehavior.AllowGet);
            }
        }
    }
}