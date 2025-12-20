using System.Web.Mvc;
using WEBPC_NHANVIEN.Areas.Admin.Filters;

namespace WEBPC_NHANVIEN.Areas.Admin.Controllers
{
    [AdminAuthorize]
    public class DashboardController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }
    }
}
