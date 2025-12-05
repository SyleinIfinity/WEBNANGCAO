using System;
using System.Configuration;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using Newtonsoft.Json;
using WEBPC_NHANVIEN.Models.Responses;
using WEBPC_NHANVIEN.Models.ViewModels;

namespace WEBPC_NHANVIEN.Controllers
{
    public class AccountController : Controller
    {
        // Lấy link API từ Web.config
        private readonly string _apiBaseUrl = ConfigurationManager.AppSettings["ApiBaseUrl"];

        [HttpGet]
        public ActionResult Login()
        {
            // Nếu đã đăng nhập, đẩy về trang chủ luôn
            if (Session["UserToken"] != null)
            {
                return RedirectToAction("Index", "Home");
            }
            return View();
        }

        [HttpPost]
        public async Task<ActionResult> Login(LoginViewModel model)
        {
            // 1. Kiểm tra validation cơ bản
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // 2. Cấu hình bảo mật TLS để gọi API https
            System.Net.ServicePointManager.SecurityProtocol =
                System.Net.SecurityProtocolType.Tls12 |
                System.Net.SecurityProtocolType.Tls11 |
                System.Net.SecurityProtocolType.Tls;

            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(_apiBaseUrl);
                client.Timeout = TimeSpan.FromSeconds(30);

                // Tạo dữ liệu gửi đi
                var loginData = new
                {
                    TenDangNhap = model.TenDangNhap,
                    MatKhau = model.MatKhau
                };

                var jsonContent = JsonConvert.SerializeObject(loginData);
                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                try
                {
                    // Gọi API Login (Đảm bảo API path đúng với backend của bạn)
                    HttpResponseMessage response = await client.PostAsync("api/TaiKhoan/login", content);
                    string responseBody = await response.Content.ReadAsStringAsync();

                    if (response.IsSuccessStatusCode)
                    {
                        // 3. Giải mã kết quả thành công
                        var userInfo = JsonConvert.DeserializeObject<UserLoginResponse>(responseBody);

                        // 4. Lưu Session quan trọng để hiển thị "Chào (Vai trò) - (Tên)"
                        Session["UserToken"] = userInfo.Token;
                        Session["UserID"] = userInfo.MaNhanVien;

                        // Đây là 2 tham số quan trọng cho yêu cầu của bạn:
                        Session["UserName"] = userInfo.HoTen;        // Tên
                        Session["RoleName"] = userInfo.TenVaiTro;    // Vai trò

                        // Chuyển hướng về trang chủ để hiện thông báo
                        return RedirectToAction("Index", "Home");
                    }
                    else
                    {
                        // Xử lý lỗi từ API
                        if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                        {
                            ModelState.AddModelError("", "Sai tên đăng nhập hoặc mật khẩu.");
                        }
                        else
                        {
                            ModelState.AddModelError("", $"Lỗi đăng nhập: {responseBody}");
                        }
                    }
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Không thể kết nối đến máy chủ: " + ex.Message);
                }
            }

            return View(model);
        }

        // Đăng xuất
        public ActionResult Logout()
        {
            Session.Clear();
            Session.Abandon();
            return RedirectToAction("Login");
        }
    }
}