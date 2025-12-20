using Newtonsoft.Json;
using System;
using System.Configuration;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Web.Security;
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
            // Nếu đã đăng nhập, kiểm tra session để điều hướng lại cho đúng trang
            if (Session["UserToken"] != null && Session["RoleId"] != null)
            {
                int roleId = Convert.ToInt32(Session["RoleId"]);
                switch (roleId)
                {
                    case 1: return RedirectToAction("Index", "Dashboard", new { area = "Admin" });
                    case 2: return RedirectToAction("Index", "Dashboard", new { area = "Sale" });
                    case 3: return RedirectToAction("Index", "Dashboard", new { area = "Tech" });
                    default: return RedirectToAction("Index", "Home");
                }
            }
            return View();
        }

        // Hàm xử lý Đăng Xuất
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LogOff()
        {
            // 1. Xóa Session (Dữ liệu phiên làm việc hiện tại)
            Session.Clear();
            Session.Abandon();

            // 2. Xóa Cookie xác thực (Nếu dùng FormsAuthentication)
            FormsAuthentication.SignOut();

            // 3. Chuyển hướng về trang Đăng nhập
            // Lưu ý: area = "" để nó tìm về thư mục gốc, không tìm trong Area Sale
            return RedirectToAction("Login", "Account", new { area = "" });
        }


        [HttpPost]
        public async Task<ActionResult> Login(LoginViewModel model)
        {
            // 1. Kiểm tra validation cơ bản
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // 2. Cấu hình bảo mật TLS để gọi API https (nếu cần thiết với server cũ)
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
                    // Gọi API Login
                    HttpResponseMessage response = await client.PostAsync("TaiKhoan/login", content);
                    string responseBody = await response.Content.ReadAsStringAsync();

                    if (response.IsSuccessStatusCode)
                    {
                        // 3. Giải mã kết quả thành công
                        var userInfo = JsonConvert.DeserializeObject<UserLoginResponse>(responseBody);

                        // 4. Lưu Session quan trọng
                        Session["UserToken"] = userInfo.Token;
                        Session["UserID"] = userInfo.MaNhanVien;
                        Session["UserName"] = userInfo.HoTen;
                        Session["RoleName"] = userInfo.TenVaiTro;
                        Session["RoleId"] = userInfo.MaVaiTro;

                        // 5. XỬ LÝ ĐIỀU HƯỚNG THEO VAI TRÒ
                        switch (userInfo.MaVaiTro)
                        {
                            case 1: // Admin -> Vào Area Admin
                                return RedirectToAction("Index", "Dashboard", new { area = "Admin" });

                            case 2: // Sale -> Vào Area Sale
                                // Đảm bảo Areas/Sale/Controllers/DashboardController.cs tồn tại
                                return RedirectToAction("Index", "Dashboard", new { area = "Sale" });

                            case 3: // Tech -> Vào Area Tech
                                // Đảm bảo Areas/Tech/Controllers/DashboardController.cs tồn tại
                                return RedirectToAction("Index", "Dashboard", new { area = "Tech" });

                            default:
                                // Không phù hợp cả 3 vai trò -> Báo lỗi và hủy session vừa tạo
                                Session.Clear();
                                Session.Abandon();
                                ModelState.AddModelError("", "Tài khoản của bạn không có quyền truy cập hệ thống quản trị.");
                                return View(model);
                        }
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
        [HttpGet]
        public ActionResult Logout()
        {
            // Xóa session đăng nhập
            Session.Clear();
            Session.Abandon();

            // Nếu dùng FormsAuthentication
            FormsAuthentication.SignOut();

            // Về lại trang đăng nhập
            return RedirectToAction("Login", "Account");
        }
    }
}