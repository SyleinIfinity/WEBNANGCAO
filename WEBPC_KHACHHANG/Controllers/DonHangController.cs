using System;
using System.Collections.Generic;
using System.Configuration;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web.Mvc;
using Newtonsoft.Json;
using WEBPC_KHACHHANG.Models.Responses;
using WEBPC_KHACHHANG.Models.ViewModels;

namespace WEBPC_KHACHHANG.Controllers
{
    public class DonHangController : Controller
    {
        private readonly string _apiBaseUrl = ConfigurationManager.AppSettings["ApiBaseUrl"];

        // 1. DANH SÁCH ĐƠN HÀNG
        public async Task<ActionResult> Index()
        {
            // Kiểm tra đăng nhập
            var user = Session["user"] as UserLoginResponse;
            if (user == null)
            {
                return RedirectToAction("Index", "Login");
            }

            var orders = new List<LichSuDonHangViewModel>();

            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(_apiBaseUrl);
                // Gán Token vào Header để xác thực với API
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", user.Token);

                // Gọi API lấy đơn hàng theo MaKhachHang
                var response = await client.GetAsync($"DonHang/customer/{user.MaKhachHang}");

                if (response.IsSuccessStatusCode)
                {
                    var data = await response.Content.ReadAsStringAsync();
                    orders = JsonConvert.DeserializeObject<List<LichSuDonHangViewModel>>(data);
                }
            }

            return View(orders);
        }

        // 2. CHI TIẾT ĐƠN HÀNG
        public async Task<ActionResult> Detail(int id)
        {
            var user = Session["user"] as UserLoginResponse;
            if (user == null)
            {
                return RedirectToAction("Index", "Login");
            }

            LichSuDonHangViewModel order = null;

            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(_apiBaseUrl);
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", user.Token);

                // Gọi API lấy chi tiết đơn (API này cần trả về cả GiaoDichThanhToan)
                var response = await client.GetAsync($"DonHang/{id}");

                if (response.IsSuccessStatusCode)
                {
                    var data = await response.Content.ReadAsStringAsync();
                    // Json sẽ tự map list giao dịch vào property GiaoDichs trong ViewModel
                    order = JsonConvert.DeserializeObject<LichSuDonHangViewModel>(data);
                }
            }

            if (order == null)
            {
                return HttpNotFound();
            }

            return View(order);
        }
    }
}