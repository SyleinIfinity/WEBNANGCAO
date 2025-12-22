using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
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
    [AdminAuthorize]
    public class KhachHangController : Controller
    {
        private readonly string _apiBaseUrl = ConfigurationManager.AppSettings["ApiBaseUrl"];

        // =========================
        // TẠO HTTP CLIENT (CÓ TIMEOUT)
        // =========================
        private HttpClient CreateClient()
        {
            var client = new HttpClient
            {
                BaseAddress = new Uri(_apiBaseUrl),
                Timeout = TimeSpan.FromSeconds(8) // ⭐ CHỐNG TREO
            };

            var token = Session["UserToken"] as string;
            if (!string.IsNullOrEmpty(token))
            {
                client.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);
            }

            return client;
        }

        // =========================
        // SERIALIZE JSON camelCase
        // =========================
        private StringContent CreateJsonContent(object data)
        {
            var settings = new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            };

            var json = JsonConvert.SerializeObject(data, settings);
            return new StringContent(json, Encoding.UTF8, "application/json");
        }

        // =========================
        // 1. DANH SÁCH KHÁCH HÀNG
        // =========================
        public async Task<ActionResult> Index()
        {
            using (var client = CreateClient())
            {
                try
                {
                    var response = await client.GetAsync("KhachHang");
                    if (response.IsSuccessStatusCode)
                    {
                        var data = await response.Content.ReadAsStringAsync();
                        var list = JsonConvert.DeserializeObject<List<KhachHangViewModel>>(data);
                        return View(list);
                    }
                }
                catch
                {
                    TempData["Error"] = "Không thể kết nối API.";
                }
            }

            return View(new List<KhachHangViewModel>());
        }

        // =========================
        // 2. THÊM KHÁCH HÀNG (GET)
        // =========================
        public ActionResult Create()
        {
            return View();
        }

        // =========================
        // 2. THÊM KHÁCH HÀNG (POST)
        // =========================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(KhachHangViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            // DTO đúng chuẩn API
            var requestData = new
            {
                hoTen = model.HoTen,
                email = model.Email,
                soDienThoai = model.SoDienThoai,
                tenDangNhap = model.TenDangNhap,
                matKhau = model.MatKhau,
                xacNhanMatKhau = model.MatKhau // ⭐ BẮT BUỘC
            };

            using (var client = CreateClient())
            {
                try
                {
                    var content = CreateJsonContent(requestData);
                    var response = await client.PostAsync("KhachHang", content);

                    if (response.IsSuccessStatusCode)
                    {
                        TempData["Success"] = "Thêm khách hàng thành công!";
                        return RedirectToAction("Index");
                    }

                    var error = await response.Content.ReadAsStringAsync();
                    ModelState.AddModelError("", $"API lỗi ({response.StatusCode}): {error}");
                }
                catch (TaskCanceledException)
                {
                    ModelState.AddModelError("", "API phản hồi quá lâu, vui lòng thử lại.");
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Lỗi hệ thống: " + ex.Message);
                }
            }

            return View(model);
        }

        // =========================
        // 3. CHỈNH SỬA (GET)
        // =========================
        public async Task<ActionResult> Edit(long id)
        {
            using (var client = CreateClient())
            {
                try
                {
                    var response = await client.GetAsync($"KhachHang/{id}");
                    if (response.IsSuccessStatusCode)
                    {
                        var data = await response.Content.ReadAsStringAsync();
                        var khachHang = JsonConvert.DeserializeObject<KhachHangViewModel>(data);
                        return View(khachHang);
                    }
                }
                catch
                {
                    TempData["Error"] = "Không thể tải thông tin khách hàng.";
                }
            }

            return RedirectToAction("Index");
        }

        // =========================
        // 3. CHỈNH SỬA (POST)
        // =========================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(long id, KhachHangViewModel model)
        {
            ModelState.Remove("MatKhau");
            ModelState.Remove("TenDangNhap");

            if (!ModelState.IsValid)
                return View(model);

            using (var client = CreateClient())
            {
                try
                {
                    // Cập nhật thông tin khách hàng
                    var infoData = new
                    {
                        hoTen = model.HoTen,
                        email = model.Email,
                        soDienThoai = model.SoDienThoai
                    };

                    var infoContent = CreateJsonContent(infoData);
                    var response = await client.PutAsync($"KhachHang/{id}", infoContent);

                    if (!response.IsSuccessStatusCode)
                    {
                        var err = await response.Content.ReadAsStringAsync();
                        ModelState.AddModelError("", err);
                        return View(model);
                    }

                    // Cập nhật trạng thái tài khoản
                    var statusData = new
                    {
                        trangThai = model.TrangThai == 1 ? "Hoạt động" : "Khoá",
                        email = model.Email,
                        tenDangNhap = model.TenDangNhap
                    };

                    var statusContent = CreateJsonContent(statusData);
                    await client.PutAsync($"TaiKhoan/{id}", statusContent);

                    TempData["Success"] = "Cập nhật khách hàng thành công!";
                    return RedirectToAction("Index");
                }
                catch (TaskCanceledException)
                {
                    ModelState.AddModelError("", "API phản hồi quá lâu.");
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Lỗi hệ thống: " + ex.Message);
                }
            }

            return View(model);
        }

        // =========================
        // ❌ ĐÃ BỎ CHỨC NĂNG XÓA
        // =========================
    }
}
