using System.Configuration; // Để đọc Web.config
using System.Web.Mvc;
using WEBPC_KHACHHANG.Models.Responses; // Chứa UserLoginResponse

namespace WEBPC_KHACHHANG.Controllers
{
    public class ChatController : Controller
    {
        // Action này được gọi từ _Layout
        [ChildActionOnly]
        public ActionResult RenderChatBox()
        {
            // 1. Lấy API URL từ Web.config
            // Key trong Web.config là: <add key="ApiBaseUrl" value="..." />
            string apiUrl = ConfigurationManager.AppSettings["ApiBaseUrl"];

            // Xử lý trường hợp quên cấu hình hoặc link có dấu / ở cuối
            if (string.IsNullOrEmpty(apiUrl)) apiUrl = "https://webapi-1-qldr.onrender.com/api"; // Mặc định
            apiUrl = apiUrl.TrimEnd('/');

            ViewBag.ApiUrl = apiUrl;

            // 2. Lấy User ID từ Session
            // (Logic này dựa trên cách em lưu session lúc LoginController)
            var user = Session["User"] as UserLoginResponse;
            int maKhachHang = (user != null) ? user.MaKhachHang : 0;

            ViewBag.MaKhachHang = maKhachHang;

            return PartialView("_ChatBox");
        }
    }
}