using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using WEBPC_NHANVIEN.Areas.Admin.Filters;
using WEBPC_NHANVIEN.Areas.Admin.Models;

namespace WEBPC_NHANVIEN.Areas.Admin.Controllers
{
    [AdminAuthorize] // Chỉ Admin mới được truy cập
    public class KhachHangController : Controller
    {
        private readonly string _apiBaseUrl = ConfigurationManager.AppSettings["ApiBaseUrl"];

        // Helper: Tạo HttpClient có gắn Token
        private HttpClient CreateClient()
        {
            var client = new HttpClient();
            client.BaseAddress = new Uri(_apiBaseUrl);
            var token = Session["UserToken"] as string;
            if (!string.IsNullOrEmpty(token))
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }
            return client;
        }

        // 1. Danh sách khách hàng
        public async Task<ActionResult> Index()
        {
            using (var client = CreateClient())
            {
                HttpResponseMessage response = await client.GetAsync("KhachHang");
                if (response.IsSuccessStatusCode)
                {
                    string data = await response.Content.ReadAsStringAsync();
                    var listKhachHang = JsonConvert.DeserializeObject<List<KhachHangViewModel>>(data);
                    return View(listKhachHang);
                }
            }
            return View(new List<KhachHangViewModel>());
        }

        // 2. Thêm mới (GET)
        public ActionResult Create()
        {
            return View();
        }

        // 2. Thêm mới (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(KhachHangViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            // Mapping ViewModel sang Request Body của API
            var requestData = new
            {
                TenDangNhap = model.TenDangNhap,
                MatKhau = model.MatKhau,
                HoTen = model.HoTen,
                Email = model.Email,
                SoDienThoai = model.SoDienThoai,
                GioiTinh = model.GioiTinh ?? "Khác"
            };

            using (var client = CreateClient())
            {
                var content = new StringContent(JsonConvert.SerializeObject(requestData), Encoding.UTF8, "application/json");
                // API tạo khách hàng thường là endpoint đăng ký hoặc create riêng cho admin
                // Giả sử dùng chung endpoint POST api/KhachHang
                HttpResponseMessage response = await client.PostAsync("KhachHang", content);

                if (response.IsSuccessStatusCode)
                {
                    TempData["Success"] = "Thêm khách hàng thành công!";
                    return RedirectToAction("Index");
                }
                else
                {
                    ModelState.AddModelError("", "Lỗi API: " + await response.Content.ReadAsStringAsync());
                }
            }
            return View(model);
        }

        // 3. Chỉnh sửa (GET)
        // 3. Chỉnh sửa (GET) - Đã sửa để hiện lỗi
        public async Task<ActionResult> Edit(long id)
        {
            using (var client = CreateClient())
            {
                HttpResponseMessage response = await client.GetAsync($"KhachHang/{id}");
                if (response.IsSuccessStatusCode)
                {
                    string data = await response.Content.ReadAsStringAsync();
                    var khachHang = JsonConvert.DeserializeObject<KhachHangViewModel>(data);
                    return View(khachHang);
                }
                else
                {
                    // Thêm đoạn này để bắt lỗi
                    string errorContent = await response.Content.ReadAsStringAsync();
                    TempData["Error"] = $"Lỗi khi lấy thông tin khách hàng (Mã {response.StatusCode}): {errorContent}";
                }
            }
            return RedirectToAction("Index");
        }

        // 3. Chỉnh sửa (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(long id, KhachHangViewModel model)
        {
            // Bỏ qua validate password khi edit vì API update thường không bắt buộc đổi pass tại đây
            ModelState.Remove("MatKhau");
            ModelState.Remove("TenDangNhap");

            if (!ModelState.IsValid) return View(model);

            var updateData = new
            {
                HoTen = model.HoTen,
                Email = model.Email,
                SoDienThoai = model.SoDienThoai,
                GioiTinh = model.GioiTinh,
                TrangThai = model.TrangThai // Nếu API cho phép sửa trạng thái
            };

            using (var client = CreateClient())
            {
                var content = new StringContent(JsonConvert.SerializeObject(updateData), Encoding.UTF8, "application/json");
                HttpResponseMessage response = await client.PutAsync($"KhachHang/{id}", content);

                if (response.IsSuccessStatusCode)
                {
                    TempData["Success"] = "Cập nhật thành công!";
                    return RedirectToAction("Index");
                }
                else
                {
                    ModelState.AddModelError("", "Lỗi cập nhật: " + await response.Content.ReadAsStringAsync());
                }
            }
            return View(model);
        }

        // 4. Xóa (Khóa tài khoản)
        public async Task<ActionResult> Delete(long id)
        {
            using (var client = CreateClient())
            {
                HttpResponseMessage response = await client.DeleteAsync($"KhachHang/{id}");
                if (response.IsSuccessStatusCode)
                {
                    TempData["Success"] = "Đã xóa khách hàng!";
                }
                else
                {
                    TempData["Error"] = "Không thể xóa khách hàng này.";
                }
            }
            return RedirectToAction("Index");
        }
    }
}