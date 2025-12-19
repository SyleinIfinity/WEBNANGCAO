using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace WEBPC_NHANVIEN.Areas.Sale.Controllers
{
    public class DashboardController : Controller
    {
        // GET: Sale/Dashboard
        public ActionResult Index()
        {
            return View();
        }
    }
}