using System;
using System.Configuration;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using Newtonsoft.Json;
using WEBPC_KHACHHANG.Models.ViewModels;

namespace WEBPC_KHACHHANG.Controllers
{
    public class RegisterController : Controller
    {
        // Lấy link API từ Web.config
        private readonly string _apiBaseUrl = ConfigurationManager.AppSettings["ApiBaseUrl"];

        // GET: Register (Hiển thị trang đăng ký)
        [HttpGet]
        public ActionResult Index()
        {
            return View();
        }

        // POST: Register (Xử lý đăng ký)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Index(RegisterViewModel model)
        {
            // 1. Kiểm tra dữ liệu đầu vào (Validation)
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // 2. Gọi API để tạo tài khoản
            using (var client = new HttpClient())
            {
                // Xử lý trường hợp quên cấu hình BaseUrl
                if (string.IsNullOrEmpty(_apiBaseUrl))
                {
                    ModelState.AddModelError("", "Chưa cấu hình ApiBaseUrl trong Web.config");
                    return View(model);
                }

                client.BaseAddress = new Uri(_apiBaseUrl);

                // Tạo object dữ liệu để gửi sang API (khớp với RegisterRequest bên API)
                var registerData = new
                {
                    hoTen = model.HoTen,
                    soDienThoai = model.SoDienThoai,
                    email = model.Email,
                    tenDangNhap = model.TenDangNhap,
                    matKhau = model.MatKhau
                };

                var content = new StringContent(JsonConvert.SerializeObject(registerData), Encoding.UTF8, "application/json");

                try
                {
                    // Gọi API: POST api/KhachHang
                    var response = await client.PostAsync("KhachHang", content);

                    if (response.IsSuccessStatusCode)
                    {
                        // Đăng ký thành công -> Chuyển hướng sang trang Login
                        TempData["LoginSuccess"] = "Đăng ký thành công! Bạn có thể đăng nhập ngay.";
                        return RedirectToAction("Index", "Login");
                    }
                    else
                    {
                        // Đăng ký thất bại (ví dụ: trùng tên đăng nhập, trùng email)
                        var errorContent = await response.Content.ReadAsStringAsync();
                        try
                        {
                            // Cố gắng đọc message lỗi từ API trả về
                            dynamic errObj = JsonConvert.DeserializeObject(errorContent);
                            ModelState.AddModelError("", "Đăng ký thất bại: " + errObj.message);
                        }
                        catch
                        {
                            ModelState.AddModelError("", "Đăng ký thất bại. Vui lòng thử lại.");
                        }

                        return View(model);
                    }
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Lỗi kết nối Server: " + ex.Message);
                    return View(model);
                }
            }
        }
    }
}