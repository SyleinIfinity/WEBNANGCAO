using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using WEBPC_NHANVIEN.Areas.Admin.Models;

namespace WEBPC_NHANVIEN.Areas.Admin.Controllers
{
    public class SanPhamController : Controller
    {
        private readonly string _baseUrl = "https://webapi-1-qldr.onrender.com/api/";

        public async Task<ActionResult> Index()
        {
            var danhSach = new List<SanPhamViewModel>();
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(_baseUrl);
                var response = await client.GetAsync("SanPham");

                if (response.IsSuccessStatusCode)
                {
                    var data = await response.Content.ReadAsStringAsync();
                    danhSach = JsonConvert.DeserializeObject<List<SanPhamViewModel>>(data);
                }
            }
            return View(danhSach);
        }
    }
}