using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WEBPC_NHANVIEN.Areas.Sale.Filters;

namespace WEBPC_NHANVIEN.Areas.Sale.Controllers
{
    [SaleAuthorize]
    public class DashboardController : Controller
    {
        // GET: Sale/Dashboard
        public ActionResult Index()
        {
            return View();
        }
    }
}