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
    public class SanPhamController : Controller
    {
        private readonly string _apiBaseUrl = ConfigurationManager.AppSettings["ApiBaseUrl"];

        // GET: Admin/SanPham
        public async Task<ActionResult> Index()
        {
            var danhSach = new List<SanPhamViewModel>();

            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(_apiBaseUrl);
                // Gọi API: GET api/SanPham
                HttpResponseMessage response = await client.GetAsync("SanPham");

                if (response.IsSuccessStatusCode)
                {
                    var data = await response.Content.ReadAsStringAsync();

                    // Lưu ý: API của bạn trả về ProductResponse, cần đảm bảo
                    // SanPhamViewModel có các thuộc tính khớp tên (TenSanPham, GiaBan...)
                    danhSach = JsonConvert.DeserializeObject<List<SanPhamViewModel>>(data);
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