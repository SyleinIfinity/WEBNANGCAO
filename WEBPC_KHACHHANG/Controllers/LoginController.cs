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
            // Nếu đã đăng nhập -> chuyển về Home
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

            // Fix HTTPS TLS khi gọi API
            System.Net.ServicePointManager.SecurityProtocol =
                System.Net.SecurityProtocolType.Tls12 |
                System.Net.SecurityProtocolType.Tls11 |
                System.Net.SecurityProtocolType.Tls;

            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(_apiBaseUrl);
                client.Timeout = TimeSpan.FromSeconds(30);

                // Gửi đúng JSON theo API
                var payload = new
                {
                    tenDangNhap = model.TenDangNhap,
                    matKhau = model.MatKhau
                };

                var json = JsonConvert.SerializeObject(payload);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                try
                {
                    // Gọi API login
                    var response = await client.PostAsync("TaiKhoan/login", content);
                    var responseBody = await response.Content.ReadAsStringAsync();

                    // LOG để debug
                    System.Diagnostics.Debug.WriteLine("➡️ Login API Response:");
                    System.Diagnostics.Debug.WriteLine(responseBody);

                    // Nếu sai user/pass
                    if (!response.IsSuccessStatusCode)
                    {
                        ModelState.AddModelError("", "Lỗi đăng nhập: " + responseBody);
                        ViewBag.ShowLoginModal = true;
                        return View(model);
                    }

                    // Parse JSON từ API
                    UserLoginResponse user = TryParseUserLoginResponse(responseBody);

                    if (user == null)
                    {
                        ModelState.AddModelError("", "Không thể xử lý dữ liệu máy chủ trả về.");
                        ViewBag.ShowLoginModal = true;
                        return View(model);
                    }

                    if (string.IsNullOrEmpty(user.Token))
                    {
                        ModelState.AddModelError("", "API không trả token.");
                        ViewBag.ShowLoginModal = true;
                        return View(model);
                    }

                    // ============================
                    // ⚡ LƯU SESSION (đã xác thực)
                    // ============================
                    Session["UserToken"] = user.Token;
                    Session["UserID"] = user.MaKhachHang;
                    Session["UserName"] = string.IsNullOrEmpty(user.HoTen) ? user.TenDangNhap : user.HoTen;
                    Session["RoleName"] = user.TenVaiTro ?? "KhachHang";

                    // Log để kiểm tra session thực sự được set
                    System.Diagnostics.Debug.WriteLine("✔ SESSION SAVED:");
                    System.Diagnostics.Debug.WriteLine("Token=" + Session["UserToken"]);
                    System.Diagnostics.Debug.WriteLine("UserID=" + Session["UserID"]);
                    System.Diagnostics.Debug.WriteLine("UserName=" + Session["UserName"]);

                    TempData["LoginSuccess"] = "Đăng nhập thành công!";
                    return RedirectToAction("Index", "Home");
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Không thể kết nối đến API: " + ex.Message);
                    ViewBag.ShowLoginModal = true;
                    return View(model);
                }
            }
        }

        // ===================
        // HÀM XỬ LÝ JSON API
        // ===================
        private UserLoginResponse TryParseUserLoginResponse(string json)
        {
            try
            {
                var direct = JsonConvert.DeserializeObject<UserLoginResponse>(json);
                if (direct != null && (!string.IsNullOrEmpty(direct.Token)))
                    return direct;
            }
            catch { }

            try
            {
                dynamic wrapper = JsonConvert.DeserializeObject<dynamic>(json);

                if (wrapper == null) return null;

                // trường hợp API trả: { data: {...} }
                if (wrapper.data != null)
                {
                    var dataJson = JsonConvert.SerializeObject(wrapper.data);
                    return JsonConvert.DeserializeObject<UserLoginResponse>(dataJson);
                }

                // trường hợp API trả: { token: "...", user: {...} }
                if (wrapper.token != null && wrapper.user != null)
                {
                    var userJson = JsonConvert.SerializeObject(wrapper.user);
                    var parsed = JsonConvert.DeserializeObject<UserLoginResponse>(userJson);
                    parsed.Token = wrapper.token;
                    return parsed;
                }
            }
            catch { }

            return null;
        }

        // Đăng xuất
        public ActionResult Logout()
        {
            Session.Clear();
            Session.Abandon();
            return RedirectToAction("Index", "Home");
        }

        public ActionResult CheckSession()
        {
            return Content(
                "UserToken = " + Session["UserToken"] + "\n" +
                "UserID = " + Session["UserID"] + "\n" +
                "UserName = " + Session["UserName"] + "\n" +
                "RoleName = " + Session["RoleName"]
            );
        }

    }
}
