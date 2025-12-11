using System;
using System.Configuration;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using Newtonsoft.Json;
using WEBPC_KHACHHANG.Models.ViewModels;
using WEBPC_KHACHHANG.Models.Responses;
using System.Net.Http.Headers;

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
        public async Task<ActionResult> Index(LoginViewModel model, string returnUrl)
        {
            if (!ModelState.IsValid) return View(model);

            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(_apiBaseUrl);

                // --- 1. GỌI API ĐĂNG NHẬP ---
                // SỬA LẠI ĐOẠN NÀY: Dùng model.TenDangNhap và model.MatKhau
                var loginRequest = new
                {
                    tenDangNhap = model.TenDangNhap, // Thay model.Username -> model.TenDangNhap
                    matKhau = model.MatKhau          // Thay model.Password -> model.MatKhau
                };

                var content = new StringContent(JsonConvert.SerializeObject(loginRequest), Encoding.UTF8, "application/json");

                // POST api/TaiKhoan/login
                var response = await client.PostAsync("TaiKhoan/login", content);

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    // UserLoginResponse hứng Token + MaKhachHang
                    var user = JsonConvert.DeserializeObject<UserLoginResponse>(responseContent);

                    // --- 2. GỌI TIẾP API LẤY THÔNG TIN CHI TIẾT (ĐỂ LẤY SĐT) ---
                    if (user != null && !string.IsNullOrEmpty(user.Token) && user.MaKhachHang > 0)
                    {
                        try
                        {
                            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", user.Token);

                            // GET api/KhachHang/{id}
                            var infoResponse = await client.GetAsync($"KhachHang/{user.MaKhachHang}");

                            if (infoResponse.IsSuccessStatusCode)
                            {
                                var infoContent = await infoResponse.Content.ReadAsStringAsync();
                                var fullInfo = JsonConvert.DeserializeObject<KhachHangResponse>(infoContent);

                                if (fullInfo != null)
                                {
                                    user.SoDienThoai = fullInfo.SoDienThoai; // Lấy SĐT gán vào
                                    user.HoTen = fullInfo.HoTen;
                                }
                            }
                        }
                        catch (Exception)
                        {
                            // Bỏ qua lỗi nếu không lấy được info, vẫn cho login
                        }
                    }

                    // --- 3. LƯU SESSION ---
                    Session["User"] = user;

                    // [THAY THẾ HOẶC BỔ SUNG ĐOẠN CODE DƯỚI ĐÂY]
                    if (user != null)
                    {
                        // 1. Lưu UserID để khắc phục lỗi NullReferenceException tại InfoUserController
                        Session["UserID"] = user.MaKhachHang;

                        // 2. Lưu Token để xác thực (dòng 20 InfoUserController cần cái này)
                        Session["UserToken"] = user.Token;

                        // 3. Lưu Tên để hiển thị trên Header (_Layout.cshtml cần cái này)
                        Session["UserName"] = !string.IsNullOrEmpty(user.HoTen) ? user.HoTen : model.TenDangNhap;
                    }

                    if (!string.IsNullOrEmpty(returnUrl)) return Redirect(returnUrl);
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    ViewBag.Error = "Tên đăng nhập hoặc mật khẩu không đúng!";
                    return View(model);
                }
            }
        }

        public ActionResult Logout()
        {
            Session.Clear();
            Session.Abandon();
            return RedirectToAction("Index", "Home");
        }
    }
}