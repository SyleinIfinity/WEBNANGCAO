using System;
using System.Collections.Generic;
using System.Configuration;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Mvc;
using Newtonsoft.Json;
using WEBPC_NHANVIEN.Areas.Admin.Models;

namespace WEBPC_NHANVIEN.Areas.Admin.Controllers
{
    public class KhachHangController : Controller
    {
        private readonly string _apiBaseUrl = ConfigurationManager.AppSettings["ApiBaseUrl"];

        // GET: Admin/KhachHang
        public async Task<ActionResult> Index()
        {
            var danhSach = new List<KhachHangViewModel>();

            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(_apiBaseUrl);
                // Gọi API: GET api/KhachHang
                HttpResponseMessage response = await client.GetAsync("KhachHang");

                if (response.IsSuccessStatusCode)
                {
                    var data = await response.Content.ReadAsStringAsync();
                    danhSach = JsonConvert.DeserializeObject<List<KhachHangViewModel>>(data);
                }
                else
                {
                    ViewBag.Error = "Lỗi tải dữ liệu: " + response.ReasonPhrase;
                }
            }

            return View(danhSach);
        }
    }
}