using System.Web.Mvc;
using WEBPC_NHANVIEN.Areas.Tech.Filters; // Sẽ tạo ở bước 3

namespace WEBPC_NHANVIEN.Areas.Tech.Controllers
{
    // Gắn cờ bảo vệ: Chỉ Tech mới vào được
    [TechAuthorize]
    public class DashboardController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }
    }
}