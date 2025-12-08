using System;
using System.Collections.Generic;
using System.Configuration;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web.Mvc;
using Newtonsoft.Json;
using WEBPC_NHANVIEN.Models;

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
                    // Gửi lệnh GET lên API (Bạn check lại Swagger xem đúng đường dẫn /api/KhuyenMai chưa nhé)
                    HttpResponseMessage response = await client.GetAsync("api/KhuyenMai");

                    if (response.IsSuccessStatusCode)
                    {
                        var data = await response.Content.ReadAsStringAsync();
                        listKM = JsonConvert.DeserializeObject<List<KhuyenMai>>(data);
                    }
                }
                catch (Exception)
                {
                    // Nếu lỗi kết nối thì trả về danh sách rỗng để không chết trang
                    listKM = new List<KhuyenMai>();
                }
            }
            return View(listKM);
        }

        // POST: Cập nhật thời gian (Gửi lệnh lên API)
        [HttpPost]
        public async Task<ActionResult> CapNhatThoiGian(int id, DateTime ngayBatDau, DateTime ngayKetThuc)
        {
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(_baseUrl);

                // Tạo cục dữ liệu để gửi đi
                var updateData = new
                {
                    maKhuyenMai = id,
                    ngayBatDau = ngayBatDau,
                    ngayKetThuc = ngayKetThuc
                };

                try
                {
                    // Đường dẫn này cũng phải check trên Swagger (Ví dụ: api/KhuyenMai/UpdateDate)
                    HttpResponseMessage response = await client.PostAsJsonAsync("api/KhuyenMai/UpdateDate", updateData);

                    if (response.IsSuccessStatusCode)
                    {
                        return Json(new { success = true, msg = "Cập nhật thành công!" });
                    }
                    else
                    {
                        return Json(new { success = false, msg = "Lỗi API: " + response.StatusCode });
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