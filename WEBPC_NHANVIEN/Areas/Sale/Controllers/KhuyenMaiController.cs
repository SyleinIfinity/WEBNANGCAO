using System;
using System.Collections.Generic;
using System.Configuration;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web.Mvc;
using Newtonsoft.Json;
using WEBPC_NHANVIEN.Areas.Sale.Filters; // Nhớ using Filter
using WEBPC_NHANVIEN.Models;
using System.Text;

namespace WEBPC_NHANVIEN.Areas.Sale.Controllers
{
    [SaleAuthorize] // Chỉ nhân viên Sale mới được vào
    public class KhuyenMaiController : Controller
    {
        private readonly string _baseUrl = ConfigurationManager.AppSettings["ApiBaseUrl"];

        public KhuyenMaiController()
        {
            // Bỏ qua lỗi SSL (nếu có)
            System.Net.ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Tls12;
            System.Net.ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
        }

        // GET: Sale/KhuyenMai (Lấy danh sách)
        public async Task<ActionResult> Index()
        {
            List<KhuyenMai> listKM = new List<KhuyenMai>();

            using (var client = CreateClient()) // Dùng hàm CreateClient chuẩn
            {
                try
                {
                    // GET api/KhuyenMai
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

        // POST: Cập nhật thời gian
        // Nhận vào đầy đủ các trường từ Ajax để build lại model
        [HttpPost]
        public async Task<ActionResult> CapNhatThoiGian(
            int maKhuyenMai, // Lưu ý: Ajax gửi 'maKhuyenMai' hay 'id' thì phải đặt tên tham số khớp, hoặc dùng Model Binding
            DateTime ngayBatDau,
            DateTime ngayKetThuc,
            string maCodeKM,
            string tenChuongTrinh,
            string loaiGiam,
            decimal giaTriGiam,
            int soLuongConLai)
        {
            using (var client = CreateClient())
            {
                // Tạo object dữ liệu đầy đủ để gửi lên API (Khớp với KhuyenMaiDto bên API)
                var updateData = new
                {
                    maKhuyenMai = maKhuyenMai,
                    maCodeKM = maCodeKM,
                    tenChuongTrinh = tenChuongTrinh,
                    loaiGiam = loaiGiam,
                    giaTriGiam = giaTriGiam,
                    ngayBatDau = ngayBatDau,
                    ngayKetThuc = ngayKetThuc,
                    soLuongConLai = soLuongConLai,

                    // Các trường mặc định khác (nếu API bắt buộc)
                    trangThai = true,
                    donHangToiThieu = 0
                };

                var jsonContent = JsonConvert.SerializeObject(updateData);
                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                // API Update thường là PUT hoặc PATCH
                // Ở đây em dùng PATCH api/KhuyenMai/{id} (như code cũ em gửi)
                // Hoặc PUT api/KhuyenMai/{id} (tuỳ API em viết)
                // Thầy dùng PATCH theo code cũ nhé.
                var request = new HttpRequestMessage(new HttpMethod("PATCH"), $"KhuyenMai/{maKhuyenMai}")
                {
                    Content = content
                };

                try
                {
                    HttpResponseMessage response = await client.SendAsync(request);

                    if (response.IsSuccessStatusCode)
                    {
                        return Json(new { success = true, msg = "Cập nhật thành công" });
                    }

                    // Đọc lỗi từ API trả về
                    var errStr = await response.Content.ReadAsStringAsync();
                    return Json(new { success = false, msg = $"Lỗi API ({response.StatusCode}): {errStr}" });
                }
                catch (Exception ex)
                {
                    return Json(new { success = false, msg = "Lỗi kết nối: " + ex.Message });
                }
            }
        }

        // --- HELPER: TẠO HTTP CLIENT CHUẨN (TOKEN + TIMEOUT) ---
        private HttpClient CreateClient()
        {
            var client = new HttpClient();
            var url = _baseUrl;
            if (!url.EndsWith("/")) url += "/";

            client.BaseAddress = new Uri(url);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            // Timeout 5 phút cho Render Cold Start
            client.Timeout = TimeSpan.FromMinutes(5);

            // Gắn Token từ Session (Quan trọng cho Authorize)
            var token = Session["Token"] as string;
            if (!string.IsNullOrEmpty(token))
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }

            return client;
        }
    }
}