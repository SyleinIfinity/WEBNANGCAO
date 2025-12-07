using System;
using System.Collections.Generic;
using System.Configuration;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Mvc;
using Newtonsoft.Json;
using WEBPC_NHANVIEN.Areas.Admin.Models; // Namespace chứa ViewModel

namespace WEBPC_NHANVIEN.Areas.Admin.Controllers
{
    public class NhanVienController : Controller
    {
        private readonly string _apiBaseUrl = ConfigurationManager.AppSettings["ApiBaseUrl"];

        // GET: Admin/NhanVien
        public async Task<ActionResult> Index()
        {
            var danhSach = new List<NhanVienViewModel>();

            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(_apiBaseUrl);
                // Gọi API: GET api/NhanVien
                HttpResponseMessage response = await client.GetAsync("NhanVien");

                if (response.IsSuccessStatusCode)
                {
                    var data = await response.Content.ReadAsStringAsync();
                    // Deserialize dữ liệu JSON từ API thành List ViewModel
                    danhSach = JsonConvert.DeserializeObject<List<NhanVienViewModel>>(data);
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