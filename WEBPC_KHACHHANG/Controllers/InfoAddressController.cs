using System;
using System.Linq;
using System.Collections.Generic;
using System.Configuration;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using Newtonsoft.Json;
using WEBPC_KHACHHANG.Models.ViewModels;
using WEBPC_KHACHHANG.Models.Requests; // <-- Đã thêm namespace Requests

namespace WEBPC_KHACHHANG.Controllers
{
    public class InfoAddressController : Controller
    {
        private readonly string _apiBaseUrl = ConfigurationManager.AppSettings["ApiBaseUrl"];

        // 1. LẤY DANH SÁCH ĐỊA CHỈ
        public async Task<ActionResult> Index()
        {
            if (Session["UserToken"] == null) return RedirectToAction("Index", "Login");

            int userId = (int)Session["UserID"];
            string token = Session["UserToken"].ToString();
            var list = new List<AddressViewModel>();

            using (var client = CreateClient(token))
            {
                var response = await client.GetAsync($"SoDiaChi/khachhang/{userId}");
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    list = JsonConvert.DeserializeObject<List<AddressViewModel>>(json);
                }
            }
            return View(list);
        }

        // 2. LẤY CHI TIẾT (Cho Modal Edit)
        [HttpGet]
        public async Task<JsonResult> GetDetail(int id)
        {
            if (Session["UserToken"] == null) return Json(null, JsonRequestBehavior.AllowGet);
            string token = Session["UserToken"].ToString();

            using (var client = CreateClient(token))
            {
                var response = await client.GetAsync($"SoDiaChi/{id}");
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var data = JsonConvert.DeserializeObject<AddressCreateRequest>(json);
                    return Json(new { success = true, data = data }, JsonRequestBehavior.AllowGet);
                }
            }
            return Json(new { success = false }, JsonRequestBehavior.AllowGet);
        }

        // 3. XỬ LÝ THÊM MỚI / CẬP NHẬT
        [HttpPost]
        public async Task<JsonResult> SaveAddress(AddressCreateRequest model)
        {
            try
            {
                // 1. Kiểm tra đăng nhập
                if (Session["UserToken"] == null)
                    return Json(new { success = false, message = "Phiên đăng nhập hết hạn. Vui lòng đăng nhập lại." });

                // 2. Gán ID người dùng
                model.MaKhachHang = (int)Session["UserID"];

                // 3. Kiểm tra dữ liệu đầu vào (Validation)
                if (!ModelState.IsValid)
                {
                    var errors = string.Join("; ", ModelState.Values
                                            .SelectMany(v => v.Errors)
                                            .Select(e => e.ErrorMessage));
                    return Json(new { success = false, message = "Dữ liệu không hợp lệ: " + errors });
                }

                string token = Session["UserToken"].ToString();

                using (var client = CreateClient(token))
                {
                    // Serialize với cấu hình mặc định (để nhận diện [JsonProperty])
                    var json = JsonConvert.SerializeObject(model);
                    var content = new StringContent(json, Encoding.UTF8, "application/json");
                    HttpResponseMessage response;

                    if (model.MaSoDiaChi.HasValue && model.MaSoDiaChi > 0)
                    {
                        // Update (PATCH)
                        // IMPORTANT: Make sure 'IsDefault' is being serialized here.
                        // Since 'AddressCreateRequest' is used for both, 'IsDefault' is sent.
                        // The API's 'UpdateSoDiaChiRequest' has 'public bool? MacDinh { get; set; }'.
                        // Sending { "MacDinh": true } or { "MacDinh": false } is valid.

                        var request = new HttpRequestMessage(new HttpMethod("PATCH"), $"SoDiaChi/{model.MaSoDiaChi}")
                        {
                            Content = content
                        };
                        response = await client.SendAsync(request);
                    }
                    else
                    {
                        // POST (Thêm mới)
                        response = await client.PostAsync("SoDiaChi", content);
                    }

                    // 4. Xử lý kết quả từ API
                    if (response.IsSuccessStatusCode)
                    {
                        return Json(new { success = true });
                    }
                    else
                    {
                        // Đọc lỗi chi tiết từ API trả về (nếu có)
                        var errorContent = await response.Content.ReadAsStringAsync();
                        return Json(new { success = false, message = "Lỗi API: " + errorContent });
                    }
                }
            }
            catch (Exception ex)
            {
                // Bắt lỗi hệ thống (ví dụ: mất mạng, code sai...)
                return Json(new { success = false, message = "Lỗi hệ thống: " + ex.Message });
            }
        }

        // 4. XÓA ĐỊA CHỈ
        [HttpPost]
        public async Task<JsonResult> Delete(int id)
        {
            if (Session["UserToken"] == null) return Json(new { success = false });
            string token = Session["UserToken"].ToString();

            using (var client = CreateClient(token))
            {
                var response = await client.DeleteAsync($"SoDiaChi/{id}");
                return Json(new { success = response.IsSuccessStatusCode });
            }
        }

        // --- CÁC HÀM HỖ TRỢ LOCATION (Gọi API Location) ---
        [HttpGet]
        public async Task<JsonResult> GetProvinces()
        {
            return await ProxyGetLocation("SoDiaChi/provinces");
        }

        [HttpGet]
        public async Task<JsonResult> GetDistricts(string provinceId)
        {
            return await ProxyGetLocation($"SoDiaChi/districts/{provinceId}");
        }

        [HttpGet]
        public async Task<JsonResult> GetWards(string districtId)
        {
            return await ProxyGetLocation($"SoDiaChi/wards/{districtId}");
        }

        // Helper: Tạo HttpClient có sẵn Token
        private HttpClient CreateClient(string token)
        {
            var client = new HttpClient();
            client.BaseAddress = new Uri(_apiBaseUrl);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            return client;
        }

        // Helper: Gọi Proxy Location
        private async Task<JsonResult> ProxyGetLocation(string endpoint)
        {
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(_apiBaseUrl);
                var response = await client.GetAsync(endpoint);
                if (response.IsSuccessStatusCode)
                {
                    var data = await response.Content.ReadAsStringAsync();
                    var list = JsonConvert.DeserializeObject<List<LocationItem>>(data);
                    return Json(list, JsonRequestBehavior.AllowGet);
                }
            }
            return Json(new List<LocationItem>(), JsonRequestBehavior.AllowGet);
        }
    }
}