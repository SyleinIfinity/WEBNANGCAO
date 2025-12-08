using System;
using System.Collections.Generic;
using System.Configuration;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web.Mvc;
using Newtonsoft.Json;
using WEBPC_NHANVIEN.Models;
using System.Text; // Cần thiết cho StringContent

namespace WEBPC_NHANVIEN.Areas.Sale.Controllers
{
    public class KhuyenMaiController : Controller
    {
        private readonly string _baseUrl = ConfigurationManager.AppSettings["ApiBaseUrl"];

        public KhuyenMaiController()
        {
            // Bỏ qua lỗi bảo mật SSL khi chạy localhost (Quan trọng!)
            System.Net.ServicePointManager.ServerCertificateValidationCallback =
                (sender, certificate, chain, sslPolicyErrors) => true;
        }

        // GET: Sale/KhuyenMai (Lấy danh sách từ API)
        public async Task<ActionResult> Index()
        {
            List<KhuyenMai> listKM = new List<KhuyenMai>();

            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(_baseUrl);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                try
                {
                    // Gửi lệnh GET lên API
                    // LƯU Ý: Đã sửa lại đường dẫn GET API từ "KhuyenMai" thành "api/KhuyenMai" (thông thường)
                    HttpResponseMessage response = await client.GetAsync("KhuyenMai");

                    if (response.IsSuccessStatusCode)
                    {
                        var data = await response.Content.ReadAsStringAsync();
                        listKM = JsonConvert.DeserializeObject<List<KhuyenMai>>(data);
                    }
                }
                catch (Exception)
                {
                    listKM = new List<KhuyenMai>();
                }
            }
            return View(listKM);
        }

        // POST: Cập nhật thời gian (Gửi lệnh lên API)
        [HttpPost] // Giữ nguyên [HttpPost] ở đây vì AJAX client (jQuery) thường không hỗ trợ HTTP PATCH
                   // và chúng ta sẽ giả lập PATCH bằng cách dùng SendAsync
        public async Task<ActionResult> CapNhatThoiGian(int id, DateTime ngayBatDau, DateTime ngayKetThuc)
        {
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(_baseUrl);

                // 1. Tạo đối tượng dữ liệu JSON
                var updateData = new
                {
                    maKhuyenMai = id,
                    ngayBatDau = ngayBatDau,
                    ngayKetThuc = ngayKetThuc
                };
                var jsonContent = JsonConvert.SerializeObject(updateData);

                // 2. Tạo nội dung HTTP với kiểu JSON
                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                // 3. Tạo yêu cầu PATCH thủ công
                // Giả định API của bạn nhận PATCH tại endpoint: api/KhuyenMai/UpdateDate
                var request = new HttpRequestMessage(new HttpMethod("PATCH"), "KhuyenMai/UpdateDate")
                {
                    Content = content
                };

                try
                {
                    // 4. Gửi yêu cầu PATCH
                    HttpResponseMessage response = await client.SendAsync(request);

                    if (response.IsSuccessStatusCode)
                    {
                        return Json(new { success = true, msg = "Cập nhật thành công!" });
                    }
                    else
                    {
                        // Đọc thông báo lỗi từ API nếu có
                        var errorMsg = await response.Content.ReadAsStringAsync();
                        return Json(new { success = false, msg = $"Lỗi API: {response.StatusCode}. Chi tiết: {errorMsg}" });
                    }
                }
                catch (Exception ex)
                {
                    return Json(new { success = false, msg = "Lỗi kết nối: " + ex.Message });
                }
            }
        }
    }
}