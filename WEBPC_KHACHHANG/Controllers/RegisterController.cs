using System;
using System.Configuration;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using Newtonsoft.Json;
using WEBPC_KHACHHANG.Models.Requests;

namespace WEBPC_KHACHHANG.Controllers
{
    public class RegisterController : Controller
    {
        private readonly string _apiBaseUrl = ConfigurationManager.AppSettings["ApiBaseUrl"];

        [HttpPost]
        public async Task<JsonResult> Submit(RegisterRequest model)
        {
            if (!ModelState.IsValid)
                return Json(new { success = false, message = "Dữ liệu không hợp lệ" });

            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(_apiBaseUrl);

                // Map dữ liệu Client sang API (KhachHangRequest)
                var payload = new
                {
                    hoTen = model.hoTen,
                    soDienThoai = model.soDienThoai,
                    email = model.email,
                    tenDangNhap = model.tenDangNhap,
                    matKhau = model.matKhau
                };

                var content = new StringContent(JsonConvert.SerializeObject(payload), Encoding.UTF8, "application/json");

                // Gọi endpoint Create trong KhachHangController
                var response = await client.PostAsync("KhachHang", content);
                var responseString = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    return Json(new { success = true, message = "Đăng ký thành công!" });
                }
                else
                {
                    // Lấy message lỗi từ API trả về (VD: "Tên đăng nhập đã tồn tại")
                    dynamic err = JsonConvert.DeserializeObject<dynamic>(responseString);
                    return Json(new { success = false, message = err?.message ?? "Đăng ký thất bại" });
                }
            }
        }
    }
}