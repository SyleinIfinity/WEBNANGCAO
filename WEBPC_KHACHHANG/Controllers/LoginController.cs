using System;
using System.Configuration;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using Newtonsoft.Json;
using WEBPC_KHACHHANG.Models.ViewModels;
using WEBPC_KHACHHANG.Models.Responses;

namespace WEBPC_KHACHHANG.Controllers
{
    public class LoginController : Controller
    {
        // Đảm bảo Web.config có key này: <add key="ApiBaseUrl" value="https://localhost:xxxx/api/" />
        private readonly string _apiBaseUrl = ConfigurationManager.AppSettings["ApiBaseUrl"];

        [HttpGet]
        public ActionResult Index()
        {
            if (Session["UserToken"] != null) return RedirectToAction("Index", "Home");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Index(LoginViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(_apiBaseUrl);

                // API TaiKhoanController nhận LoginRequest { tenDangNhap, matKhau }
                var payload = new
                {
                    tenDangNhap = model.TenDangNhap,
                    matKhau = model.MatKhau
                };

                var content = new StringContent(JsonConvert.SerializeObject(payload), Encoding.UTF8, "application/json");

                try
                {
                    // Gọi đúng endpoint trong TaiKhoanController
                    var response = await client.PostAsync("TaiKhoan/login", content);
                    var responseBody = await response.Content.ReadAsStringAsync();

                    if (response.IsSuccessStatusCode)
                    {
                        var userInfo = JsonConvert.DeserializeObject<UserLoginResponse>(responseBody);

                        if (userInfo != null)
                        {
                            // --- THÊM DÒNG NÀY (BẮT BUỘC) ---
                            // Để CartController có thể lấy toàn bộ thông tin User (Token, ID, Tên...)
                            Session["User"] = userInfo;
                            // --------------------------------

                            Session["UserToken"] = userInfo.Token; // Code cũ của bạn
                            Session["UserID"] = userInfo.MaKhachHang; // Code cũ của bạn
                            Session["UserName"] = userInfo.HoTen; // Code cũ của bạn

                            TempData["LoginSuccess"] = "Đăng nhập thành công!";
                            return RedirectToAction("Index", "Home");
                        }
                    }

                    // Xử lý lỗi từ API (trả về { message = "..." })
                    dynamic error = JsonConvert.DeserializeObject<dynamic>(responseBody);
                    ModelState.AddModelError("", error?.message ?? "Đăng nhập thất bại.");
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Lỗi kết nối: " + ex.Message);
                }
            }

            return View(model);
        }

        public ActionResult Logout()
        {
            Session.Clear();
            Session.Abandon();
            return RedirectToAction("Index", "Home");
        }
    }
}