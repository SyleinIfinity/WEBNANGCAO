using System;
using System.Collections.Generic;
using System.Configuration;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using Newtonsoft.Json;
using WEBPC_NHANVIEN.Models;

namespace WEBPC_NHANVIEN.Areas.Sale.Controllers
{
    public class DonHangController : Controller
    {
        // Link API gốc: https://webapi-1-qldr.onrender.com/api/
        private readonly string _baseUrl = ConfigurationManager.AppSettings["ApiBaseUrl"];

        public DonHangController()
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
        }

        // --- 1. LẤY DANH SÁCH ---
        public async Task<ActionResult> Index()
        {
            List<DonHang> listDonHang = new List<DonHang>();
            using (var client = CreateClient())
            {
                try
                {
                    HttpResponseMessage response = await client.GetAsync("DonHang");
                    if (response.IsSuccessStatusCode)
                    {
                        var data = await response.Content.ReadAsStringAsync();
                        listDonHang = JsonConvert.DeserializeObject<List<DonHang>>(data);
                    }
                }
                catch (Exception) { listDonHang = new List<DonHang>(); }
            }
            return View(listDonHang);
        }

        // --- 2. LẤY CHI TIẾT ---
        [HttpGet]
        public async Task<ActionResult> GetDetail(int id)
        {
            using (var client = CreateClient())
            {
                try
                {
                    HttpResponseMessage response = await client.GetAsync($"DonHang/{id}");
                    if (response.IsSuccessStatusCode)
                    {
                        var data = await response.Content.ReadAsStringAsync();
                        return Content(data, "application/json");
                    }
                    return Json(new { error = "Không tìm thấy đơn hàng" }, JsonRequestBehavior.AllowGet);
                }
                catch (Exception ex)
                {
                    return Json(new { error = ex.Message }, JsonRequestBehavior.AllowGet);
                }
            }
        }

        // ============================================================
        // --- 3. XỬ LÝ DUYỆT & HỦY (THEO YÊU CẦU CỦA BẠN) ---
        // ============================================================

        [HttpPost]
        public async Task<ActionResult> DuyetDon(int id)
        {
            // API Duyệt: PUT api/DonHang/Approve/{id}
            return await CallApiPut($"DonHang/Approve/{id}");
        }

        [HttpPost]
        public async Task<ActionResult> HuyDon(int id)
        {
            var body = new
            {
                LyDoTuChoi = "Nhân viên hủy đơn"
            };

            return await CallApiPutWithBody($"DonHang/Reject/{id}", body);
        }


        private async Task<ActionResult> CallApiPutWithBody(string endpoint, object body)
        {
            using (var client = CreateClient())
            {
                try
                {
                    if (endpoint.StartsWith("/")) endpoint = endpoint.Substring(1);

                    var json = JsonConvert.SerializeObject(body);
                    var content = new StringContent(json, Encoding.UTF8, "application/json");

                    var response = await client.PutAsync(endpoint, content);

                    if (response.IsSuccessStatusCode)
                    {
                        return Json(new { success = true, msg = "Thành công" });
                    }

                    var errContent = await response.Content.ReadAsStringAsync();
                    return Json(new
                    {
                        success = false,
                        msg = $"Lỗi API ({response.StatusCode}): {errContent}"
                    });
                }
                catch (Exception ex)
                {
                    return Json(new { success = false, msg = "Lỗi Code: " + ex.Message });
                }
            }
        }


        // --- HÀM GỌI API DÙNG CHUNG (METHOD PUT) ---
        private async Task<ActionResult> CallApiPut(string endpoint)
        {
            using (var client = CreateClient())
            {
                try
                {
                    // Body rỗng (API của bạn chỉ cần ID trên URL)
                    var content = new StringContent("", Encoding.UTF8, "application/json");

                    // Bỏ dấu / ở đầu endpoint nếu có để ghép chuỗi cho đúng
                    if (endpoint.StartsWith("/")) endpoint = endpoint.Substring(1);

                    // Debug: In ra URL sẽ gọi
                    // System.Diagnostics.Debug.WriteLine("Calling PUT: " + client.BaseAddress + endpoint);

                    // Gọi PUT
                    var response = await client.PutAsync(endpoint, content);

                    // Thành công
                    if (response.IsSuccessStatusCode)
                    {
                        return Json(new { success = true, msg = "Thành công" });
                    }

                    // Thất bại -> Đọc lỗi chi tiết từ Server
                    var errContent = await response.Content.ReadAsStringAsync();
                    return Json(new
                    {
                        success = false,
                        msg = $"Lỗi API ({response.StatusCode}): {errContent}"
                    });
                }
                catch (Exception ex)
                {
                    return Json(new { success = false, msg = "Lỗi Code: " + ex.Message });
                }
            }
        }

        // Tạo HttpClient chuẩn
        private HttpClient CreateClient()
        {
            var client = new HttpClient();
            var url = _baseUrl;
            if (!url.EndsWith("/")) url += "/"; // Đảm bảo luôn có / ở cuối base

            client.BaseAddress = new Uri(url);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            return client;
        }
    }
}