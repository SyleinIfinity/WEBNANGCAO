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
        [HttpPost]
        public async Task<ActionResult> CapNhatThoiGian(int id, DateTime ngayBatDau, DateTime ngayKetThuc,
    string maCodeKM, string tenChuongTrinh, string loaiGiam, decimal giaTriGiam, int soLuongConLai)
        {
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(_baseUrl);

                // Đóng gói dữ liệu gửi lên API thực tế
                var updateData = new
                {
                    maKhuyenMai = id,
                    maCodeKM = maCodeKM,
                    tenChuongTrinh = tenChuongTrinh,
                    loaiGiam = loaiGiam,
                    giaTriGiam = giaTriGiam,
                    ngayBatDau = ngayBatDau,
                    ngayKetThuc = ngayKetThuc,
                    soLuongConLai = soLuongConLai,
                    trangThai = true // Gửi mặc định true nếu API bắt buộc
                };

                var jsonContent = JsonConvert.SerializeObject(updateData);
                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                // Gửi PATCH đến endpoint: KhuyenMai/{id}
                var request = new HttpRequestMessage(new HttpMethod("PATCH"), $"KhuyenMai/{id}")
                {
                    Content = content
                };

                try
                {
                    HttpResponseMessage response = await client.SendAsync(request);
                    if (response.IsSuccessStatusCode)
                    {
                        return Json(new { success = true });
                    }
                    return Json(new { success = false, msg = "Lỗi API: " + response.StatusCode });
                }
                catch (Exception ex)
                {
                    return Json(new { success = false, msg = ex.Message });
                }
            }
        }
    }
}