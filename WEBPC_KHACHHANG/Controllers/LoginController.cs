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
        private readonly string _apiBaseUrl = ConfigurationManager.AppSettings["ApiBaseUrl"];

        // GET: Login
        [HttpGet]
        public ActionResult Index()
        {
            if (Session["UserToken"] != null)
                return RedirectToAction("Index", "Home");

            ViewBag.ShowLoginModal = true;
            return View();
        }

        // POST: Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Index(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.ShowLoginModal = true;
                return View(model);
            }

            System.Net.ServicePointManager.SecurityProtocol =
                System.Net.SecurityProtocolType.Tls12 |
                System.Net.SecurityProtocolType.Tls11 |
                System.Net.SecurityProtocolType.Tls;

            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(_apiBaseUrl);
                client.Timeout = TimeSpan.FromSeconds(30);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

                // <-- SỬA: gửi đúng key theo API (camelCase)
                var payload = new
                {
                    tenDangNhap = model.TenDangNhap,
                    matKhau = model.MatKhau
                };

                var json = JsonConvert.SerializeObject(payload);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                try
                {
                    // Gọi đúng endpoint (BaseAddress đã có /api/)
                    var response = await client.PostAsync("TaiKhoan/login", content);
                    var responseBody = await response.Content.ReadAsStringAsync();

                    if (!response.IsSuccessStatusCode)
                    {
                        // show response body để debug (400/401/500)
                        ModelState.AddModelError("", "Lỗi đăng nhập: " + responseBody);
                        ViewBag.ShowLoginModal = true;
                        return View(model);
                    }

                    // Thử deserialize linh hoạt: có thể trả trực tiếp object hoặc wrapper { data: ... }
                    UserLoginResponse user = TryParseUserLoginResponse(responseBody);

                    if (user == null)
                    {
                        // Nếu không thể map vào UserLoginResponse, lưu raw response để debug
                        ModelState.AddModelError("", "Không thể xử lý dữ liệu trả về từ server: " + responseBody);
                        ViewBag.ShowLoginModal = true;
                        return View(model);
                    }

                    // Lưu thông tin vào Session
                    Session["UserToken"] = user.Token;
                    Session["UserName"] = user.HoTen;
                    Session["UserID"] = user.MaKhachHang;

                    TempData["LoginSuccess"] = "Đăng nhập thành công!";
                    return RedirectToAction("Index", "Home");
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Không thể kết nối đến máy chủ: " + ex.Message);
                    ViewBag.ShowLoginModal = true;
                    return View(model);
                }
            }
        }

        // Helper tĩnh bên trong controller (hoặc Extract ra util)
        private UserLoginResponse TryParseUserLoginResponse(string json)
        {
            try
            {
                // 1) Thử trực tiếp vào model
                var direct = JsonConvert.DeserializeObject<UserLoginResponse>(json);
                if (direct != null && (direct.Token != null || direct.MaKhachHang != 0 || direct.HoTen != null))
                    return direct;
            }
            catch { }

            try
            {
                // 2) Thử kiểu wrapper { data: { ... } }
                var wrapper = JsonConvert.DeserializeObject<dynamic>(json);
                if (wrapper != null)
                {
                    // nếu có data
                    if (wrapper.data != null)
                    {
                        var dataJson = JsonConvert.SerializeObject(wrapper.data);
                        var user = JsonConvert.DeserializeObject<UserLoginResponse>(dataJson);
                        if (user != null) return user;
                    }

                    // nếu có token + user inside e.g. { token: "...", user: { ... } }
                    if (wrapper.token != null && wrapper.user != null)
                    {
                        var userJson = JsonConvert.SerializeObject(wrapper.user);
                        var user = JsonConvert.DeserializeObject<UserLoginResponse>(userJson);
                        if (user != null)
                        {
                            user.Token = (string)wrapper.token;
                            return user;
                        }
                    }
                }
            }
            catch { }

            return null;
        }


        public ActionResult Logout()
        {
            Session.Clear();
            Session.Abandon();
            return RedirectToAction("Index", "Home");
        }
    }
}
