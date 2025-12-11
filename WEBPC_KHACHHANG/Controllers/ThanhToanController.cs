using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace WEBPC_KHACHHANG.Controllers
{
    public class ThanhToanController : Controller
    {
        // GET: ThanhToan
        public ActionResult Index()
        {
            return View();
        }

        // --- BỔ SUNG ĐOẠN NÀY ---
        // GET: ThanhToan/Checkout
        public ActionResult Checkout()
        {
            // Hàm này sẽ tìm file View Checkout.cshtml để hiển thị
            return View();
        }
    }
}