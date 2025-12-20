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
using WEBPC_NHANVIEN.Areas.Sale.Filters; // Đảm bảo namespace Filter đúng
using WEBPC_NHANVIEN.Models;

namespace WEBPC_NHANVIEN.Areas.Sale.Controllers
{
    [SaleAuthorize] // [QUAN TRỌNG] Chỉ cho phép nhân viên Sale đăng nhập mới được gọi
    public class DonHangController : Controller
    {
        // Link API gốc lấy từ Web.config
        private readonly string _baseUrl = ConfigurationManager.AppSettings["ApiBaseUrl"];

        public DonHangController()
        {
            // Fix lỗi SSL/TLS trên một số server cũ
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
        }

        // --- 1. VIEW CHÍNH ---
        // Chỉ trả về khung HTML, dữ liệu sẽ được AJAX tải sau (để hiện Loading)
        public ActionResult Index()
        {
            return View();
        }

        // --- 2. API: LẤY DANH SÁCH ĐƠN HÀNG (JSON) ---
        [HttpGet]
        public async Task<ActionResult> GetList()
        {
            using (var client = CreateClient())
            {
                try
                {
                    // Gọi API: GET api/DonHang
                    HttpResponseMessage response = await client.GetAsync("DonHang");

                    if (response.IsSuccessStatusCode)
                    {
                        var data = await response.Content.ReadAsStringAsync();
                        // Trả nguyên JSON string để Client tự parse (tối ưu tốc độ)
                        return Content(data, "application/json");
                    }
                    return Json(new List<object>(), JsonRequestBehavior.AllowGet);
                }
                catch (Exception ex)
                {
                    // Trả về lỗi JSON chuẩn để Client bắt được
                    Response.StatusCode = 500;
                    return Json(new { error = "Lỗi Server: " + ex.Message }, JsonRequestBehavior.AllowGet);
                }
            }
        }

        // --- 3. API: LẤY CHI TIẾT ĐƠN HÀNG (JSON) ---
        [HttpGet]
        public async Task<ActionResult> GetDetail(int id)
        {
            using (var client = CreateClient())
            {
                try
                {
                    // Gọi API: GET api/DonHang/{id}
                    HttpResponseMessage response = await client.GetAsync($"DonHang/{id}");

                    if (response.IsSuccessStatusCode)
                    {
                        var data = await response.Content.ReadAsStringAsync();
                        return Content(data, "application/json");
                    }

                    // Nếu lỗi 404 từ API
                    if (response.StatusCode == HttpStatusCode.NotFound)
                    {
                        Response.StatusCode = 404;
                        return Json(new { error = "Không tìm thấy đơn hàng" }, JsonRequestBehavior.AllowGet);
                    }

                    return Json(new { error = "Lỗi API: " + response.ReasonPhrase }, JsonRequestBehavior.AllowGet);
                }
                catch (Exception ex)
                {
                    Response.StatusCode = 500;
                    return Json(new { error = ex.Message }, JsonRequestBehavior.AllowGet);
                }
            }
        }

        // ============================================================
        // --- 4. XỬ LÝ DUYỆT & HỦY ---
        // ============================================================

        [HttpPost]
        public async Task<ActionResult> DuyetDon(int id)
        {
            // API Duyệt: PUT api/DonHang/approve/{id}
            // Lưu ý: API của em dùng /Approve hay /approve? (nên thống nhất, thường API ko phân biệt hoa thường)
            return await CallApiPut($"DonHang/approve/{id}");
        }

        [HttpPost]
        public async Task<ActionResult> HuyDon(int id)
        {
            // API Hủy: PUT api/DonHang/reject/{id}
            var body = new
            {
                LyDoTuChoi = "Nhân viên hủy đơn qua trang quản lý"
            };

            return await CallApiPutWithBody($"DonHang/reject/{id}", body);
        }

        // ============================================================
        // --- HELPER METHODS (DÙNG CHUNG) ---
        // ============================================================

        private async Task<ActionResult> CallApiPut(string endpoint)
        {
            using (var client = CreateClient())
            {
                try
                {
                    var content = new StringContent("", Encoding.UTF8, "application/json");
                    if (endpoint.StartsWith("/")) endpoint = endpoint.Substring(1);

                    var response = await client.PutAsync(endpoint, content);

                    if (response.IsSuccessStatusCode)
                    {
                        return Json(new { success = true, msg = "Thao tác thành công" });
                    }

                    var errContent = await response.Content.ReadAsStringAsync();
                    // Thử parse lỗi đẹp hơn nếu API trả về JSON
                    try
                    {
                        dynamic errObj = JsonConvert.DeserializeObject(errContent);
                        return Json(new { success = false, msg = errObj.message ?? errContent });
                    }
                    catch
                    {
                        return Json(new { success = false, msg = errContent });
                    }
                }
                catch (Exception ex)
                {
                    return Json(new { success = false, msg = "Lỗi kết nối: " + ex.Message });
                }
            }
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
                        return Json(new { success = true, msg = "Thao tác thành công" });
                    }

                    var errContent = await response.Content.ReadAsStringAsync();
                    try
                    {
                        dynamic errObj = JsonConvert.DeserializeObject(errContent);
                        return Json(new { success = false, msg = errObj.message ?? errContent });
                    }
                    catch
                    {
                        return Json(new { success = false, msg = errContent });
                    }
                }
                catch (Exception ex)
                {
                    return Json(new { success = false, msg = "Lỗi kết nối: " + ex.Message });
                }
            }
        }

        // --- TẠO HTTP CLIENT CHUẨN (CÓ TOKEN & TIMEOUT) ---
        private HttpClient CreateClient()
        {
            var client = new HttpClient();
            var url = _baseUrl;
            if (!url.EndsWith("/")) url += "/";

            client.BaseAddress = new Uri(url);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            // [QUAN TRỌNG] Tăng thời gian chờ lên 5 phút để tránh lỗi timeout khi Render Cold Start
            client.Timeout = TimeSpan.FromMinutes(5);

            // [QUAN TRỌNG] Lấy Token từ Session và gắn vào Header
            var token = Session["Token"] as string;
            if (!string.IsNullOrEmpty(token))
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }

            return client;
        }
    }
}