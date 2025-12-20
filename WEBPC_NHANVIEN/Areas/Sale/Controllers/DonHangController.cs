using System;
using System.Collections.Generic;
using System.Configuration;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web.Mvc;
using Newtonsoft.Json;
using WEBPC_NHANVIEN.Models; // Đảm bảo bạn có namespace này chứa class DonHang

namespace WEBPC_NHANVIEN.Areas.Sale.Controllers
{
    public class DonHangController : Controller
    {
        private readonly string _baseUrl = ConfigurationManager.AppSettings["ApiBaseUrl"];

        public DonHangController()
        {
            // Bỏ qua lỗi SSL (giống KhuyenMaiController)
            System.Net.ServicePointManager.ServerCertificateValidationCallback =
                (sender, certificate, chain, sslPolicyErrors) => true;
        }

        // 1. LẤY DANH SÁCH (SERVER-SIDE RENDERING)
        // Code này sẽ chạy khi bạn vào trang, lấy dữ liệu xong mới hiện web
        public async Task<ActionResult> Index()
        {
            List<DonHang> listDonHang = new List<DonHang>();

            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(_baseUrl);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                try
                {
                    // Gọi API: api/DonHang
                    HttpResponseMessage response = await client.GetAsync("DonHang");

                    if (response.IsSuccessStatusCode)
                    {
                        var data = await response.Content.ReadAsStringAsync();
                        // Tự động chuyển JSON thành List<DonHang>
                        listDonHang = JsonConvert.DeserializeObject<List<DonHang>>(data);
                    }
                }
                catch (Exception)
                {
                    // Gặp lỗi thì trả về danh sách rỗng để không chết trang
                    listDonHang = new List<DonHang>();
                }
            }

            // Truyền danh sách sang View
            return View(listDonHang);
        }

        // 2. LẤY CHI TIẾT (Cho Modal Popup)
        // Giữ nguyên AJAX cho phần này để web load nhanh
        [HttpGet]
        public async Task<ActionResult> GetDetail(int id)
        {
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(_baseUrl);
                try
                {
                    HttpResponseMessage response = await client.GetAsync($"DonHang/{id}");
                    if (response.IsSuccessStatusCode)
                    {
                        var data = await response.Content.ReadAsStringAsync();
                        return Content(data, "application/json");
                    }
                    return Json(new { error = "Không tìm thấy" }, JsonRequestBehavior.AllowGet);
                }
                catch (Exception ex)
                {
                    return Json(new { error = ex.Message }, JsonRequestBehavior.AllowGet);
                }
            }
        }

        // 3. DUYỆT / HỦY
        [HttpPost]
        public async Task<ActionResult> DuyetDon(int id)
        {
            return await CallApiPost($"DonHang/Duyet/{id}");
        }

        [HttpPost]
        public async Task<ActionResult> HuyDon(int id)
        {
            return await CallApiPost($"DonHang/Huy/{id}");
        }

        private async Task<ActionResult> CallApiPost(string endpoint)
        {
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(_baseUrl);
                try
                {
                    var request = new HttpRequestMessage(new HttpMethod("POST"), endpoint);
                    var response = await client.SendAsync(request);
                    if (response.IsSuccessStatusCode) return Json(new { success = true });

                    var err = await response.Content.ReadAsStringAsync();
                    return Json(new { success = false, msg = err });
                }
                catch (Exception ex) { return Json(new { success = false, msg = ex.Message }); }
            }
        }
    }
}