using System;
using System.Collections.Generic;
using System.Configuration;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web.Mvc;
using Newtonsoft.Json;
using WEBPC_NHANVIEN.Models;
using System.Text; // Required for StringContent/UTF8

namespace WEBPC_NHANVIEN.Areas.Sale.Controllers
{
    public class DonHangController : Controller
    {
        // Lấy địa chỉ API từ Web.config
        private readonly string _baseUrl = ConfigurationManager.AppSettings["ApiBaseUrl"];

        // Đường dẫn API cho cập nhật trạng thái (Cần kiểm tra lại Swagger của bạn)
        private const string API_UPDATE_STATUS_ENDPOINT = "DonHang/CapNhatTrangThai";

        public DonHangController()
        {
            // Bỏ qua lỗi bảo mật SSL khi chạy localhost
            System.Net.ServicePointManager.ServerCertificateValidationCallback =
                (sender, certificate, chain, sslPolicyErrors) => true;
        }

        // GET: Sale/DonHang (Lấy danh sách Đơn hàng từ API)
        public async Task<ActionResult> Index()
        {
            List<DonHang> listDH = new List<DonHang>();

            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(_baseUrl);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                try
                {
                    // GỌI API: Giả định endpoint là "api/DonHang"
                    HttpResponseMessage response = await client.GetAsync("DonHang");

                    if (response.IsSuccessStatusCode)
                    {
                        var data = await response.Content.ReadAsStringAsync();
                        listDH = JsonConvert.DeserializeObject<List<DonHang>>(data);
                    }
                }
                catch (Exception)
                {
                    listDH = new List<DonHang>();
                    ViewBag.Error = "Không thể kết nối đến máy chủ API hoặc dữ liệu trống.";
                }
            }
            return View(listDH);
        }

        // POST: Cập nhật trạng thái đơn hàng (Duyệt/Từ chối)
        [HttpPost]
        public async Task<ActionResult> CapNhatTrangThai(int maDonHang, string trangThaiMoi)
        {
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(_baseUrl);

                // Tạo đối tượng dữ liệu (chỉ gửi những trường cần cập nhật)
                var updateData = new
                {
                    maDonHang = maDonHang,
                    trangThai = trangThaiMoi // Ví dụ: "Đã duyệt" hoặc "Đã từ chối"
                };

                // Chuyển đối tượng thành JSON
                var jsonContent = JsonConvert.SerializeObject(updateData);
                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                // Tạo yêu cầu HTTP (Thường là PATCH/PUT nhưng dùng POST cho đơn giản với MVC cũ)
                // LƯU Ý: Nếu API của bạn hỗ trợ PATCH, bạn nên dùng PATCH.
                // Nếu dùng POST, endpoint API cần biết nó là update trạng thái.

                try
                {
                    // Gửi yêu cầu lên API (Giả định API chấp nhận POST tại endpoint này)
                    HttpResponseMessage response = await client.PostAsync(API_UPDATE_STATUS_ENDPOINT, content);

                    if (response.IsSuccessStatusCode)
                    {
                        return Json(new { success = true, msg = $"{trangThaiMoi} đơn hàng thành công!" });
                    }
                    else
                    {
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