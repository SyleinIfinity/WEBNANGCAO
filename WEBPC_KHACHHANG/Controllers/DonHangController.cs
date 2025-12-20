using System;
using System.Collections.Generic;
using System.Configuration;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web.Mvc;
using Newtonsoft.Json;
using WEBPC_KHACHHANG.Models.Responses;
using WEBPC_KHACHHANG.Models.ViewModels;

namespace WEBPC_KHACHHANG.Controllers
{
    public class DonHangController : Controller
    {
        private readonly string _apiBaseUrl = ConfigurationManager.AppSettings["ApiBaseUrl"];

        // 1. DANH SÁCH ĐƠN HÀNG
        public async Task<ActionResult> Index()
        {
            // Kiểm tra đăng nhập
            var user = Session["user"] as UserLoginResponse;
            if (user == null)
            {
                return RedirectToAction("Index", "Login");
            }

            var orders = new List<LichSuDonHangViewModel>();

            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(_apiBaseUrl);
                // Gán Token vào Header để xác thực với API
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", user.Token);

                // Gọi API lấy đơn hàng theo MaKhachHang
                var response = await client.GetAsync($"DonHang/customer/{user.MaKhachHang}");

                if (response.IsSuccessStatusCode)
                {
                    var data = await response.Content.ReadAsStringAsync();
                    orders = JsonConvert.DeserializeObject<List<LichSuDonHangViewModel>>(data);
                }
            }

            return View(orders);
        }

        // 2. CHI TIẾT ĐƠN HÀNG
        public async Task<ActionResult> Detail(int id)
        {
            var user = Session["user"] as UserLoginResponse;
            if (user == null)
            {
                return RedirectToAction("Index", "Login");
            }

            LichSuDonHangViewModel order = null;

            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(_apiBaseUrl);
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", user.Token);

                // Gọi API lấy chi tiết đơn (API này cần trả về cả GiaoDichThanhToan)
                var response = await client.GetAsync($"DonHang/{id}");

                if (response.IsSuccessStatusCode)
                {
                    var data = await response.Content.ReadAsStringAsync();
                    // Json sẽ tự map list giao dịch vào property GiaoDichs trong ViewModel
                    order = JsonConvert.DeserializeObject<LichSuDonHangViewModel>(data);
                }
            }

            if (order == null)
            {
                return HttpNotFound();
            }

            return View(order);
        }

        // POST: /DonHang/ConfirmReceived/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ConfirmReceived(int id)
        {
            var user = Session["User"] as UserLoginResponse;
            if (user == null) return RedirectToAction("Index", "Login");

            try
            {
                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri(ConfigurationManager.AppSettings["ApiBaseUrl"]);
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", user.Token);

                    // Gọi API POST (không cần body nên để null)
                    var response = await client.PostAsync($"DonHang/confirm-received/{id}", null);

                    if (response.IsSuccessStatusCode)
                    {
                        TempData["Success"] = "Cảm ơn bạn! Đơn hàng đã hoàn thành.";
                    }
                    else
                    {
                        TempData["Error"] = "Có lỗi xảy ra hoặc trạng thái đơn hàng không hợp lệ.";
                    }
                }
            }
            catch
            {
                TempData["Error"] = "Lỗi kết nối server.";
            }

            // Quay lại trang chi tiết đơn hàng
            return RedirectToAction("Detail", new { id = id });
        }

        // 3. HỦY ĐƠN HÀNG (Thêm hàm này vào để Client xử lý)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> CancelOrder(int id)
        {
            // Kiểm tra đăng nhập (xử lý cả trường hợp Session key viết hoa/thường)
            var user = Session["user"] as UserLoginResponse ?? Session["User"] as UserLoginResponse;
            if (user == null) return RedirectToAction("Index", "Login");

            try
            {
                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri(_apiBaseUrl);
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", user.Token);

                    // Tạo dữ liệu gửi đi (Khớp với CancelOrderRequest bên API)
                    var payload = new
                    {
                        MaKhachHang = user.MaKhachHang,
                        LyDoHuy = "Khách hàng hủy trực tiếp trên Website"
                    };

                    var jsonContent = JsonConvert.SerializeObject(payload);
                    var content = new StringContent(jsonContent, System.Text.Encoding.UTF8, "application/json");

                    // [QUAN TRỌNG]: API của em dùng [HttpPut], nên ở đây phải gọi PutAsync
                    var response = await client.PutAsync($"DonHang/cancel/{id}", content);

                    if (response.IsSuccessStatusCode)
                    {
                        TempData["Success"] = "Đã hủy đơn hàng thành công.";
                    }
                    else
                    {
                        // Đọc lỗi từ API trả về để hiện thông báo rõ ràng
                        var errorString = await response.Content.ReadAsStringAsync();
                        try
                        {
                            // Nếu API trả về JSON { "message": "..." }
                            dynamic errObj = JsonConvert.DeserializeObject(errorString);
                            TempData["Error"] = "Lỗi: " + errObj.message;
                        }
                        catch
                        {
                            // Nếu API trả về text thường
                            TempData["Error"] = "Không thể hủy đơn: " + errorString;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Lỗi kết nối: " + ex.Message;
            }

            // Load lại trang chi tiết để thấy trạng thái mới
            return RedirectToAction("Detail", new { id = id });
        }
    }
}