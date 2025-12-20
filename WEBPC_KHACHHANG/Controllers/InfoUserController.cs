using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web.Mvc;
using Newtonsoft.Json;
using WEBPC_KHACHHANG.Models.ViewModels;

namespace WEBPC_KHACHHANG.Controllers
{
    public class InfoUserController : Controller
    {
        private readonly string _apiBaseUrl = ConfigurationManager.AppSettings["ApiBaseUrl"];

        public async Task<ActionResult> Index()
        {
            if (Session["UserToken"] == null) return RedirectToAction("Index", "Login");

            int userId = (int)Session["UserID"];
            string token = Session["UserToken"].ToString();
            InfoUserViewModel user = new InfoUserViewModel();

            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(_apiBaseUrl);
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                // LƯU Ý: API KhachHangController hiện tại thiếu GetById
                // Nên tạm thời gọi GetAll và lọc (Không tối ưu, nên bổ sung GetById vào API sau)
                var response = await client.GetAsync("KhachHang");

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var listUsers = JsonConvert.DeserializeObject<List<InfoUserViewModel>>(json);

                    // Lọc tìm người dùng hiện tại
                    user = listUsers.FirstOrDefault(u => u.MaKhachHang == userId) ?? new InfoUserViewModel();
                }
            }

            return View(user);
        }
    }
}