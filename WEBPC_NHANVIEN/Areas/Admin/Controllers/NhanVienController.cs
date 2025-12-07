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
    public class NhanVienController : Controller
    {
        private readonly string _baseUrl = "https://webapi-1-qldr.onrender.com/api/";

        // GET: Admin/NhanVien
        public async Task<ActionResult> Index()
        {
            var danhSach = new List<NhanVienViewModel>();
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(_baseUrl);
                var response = await client.GetAsync("NhanVien"); // Gọi API GET All Nhân viên

                if (response.IsSuccessStatusCode)
                {
                    var data = await response.Content.ReadAsStringAsync();
                    danhSach = JsonConvert.DeserializeObject<List<NhanVienViewModel>>(data);
                }
            }
            return View(danhSach);
        }

        // Bạn có thể thêm Action Create/Edit/Delete ở đây để gọi API tương ứng
    }
}