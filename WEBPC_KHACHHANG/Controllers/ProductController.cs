using System;
using System.Collections.Generic;
using System.Configuration; // Đọc Web.config
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Mvc;
using Newtonsoft.Json; // Cần cài gói Newtonsoft.Json
using WEBPC_KHACHHANG.Models.ViewModels;

namespace WEBPC_KHACHHANG.Controllers
{
    public class ProductController : Controller
    {
        // Đọc URL API từ Web.config
        private readonly string _apiBaseUrl = ConfigurationManager.AppSettings["ApiBaseUrl"];

        // 1. TRANG DANH SÁCH SẢN PHẨM
        public async Task<ActionResult> Index(string search = "", int? categoryId = null)
        {
            var products = new List<ProductViewModel>();
            var categories = new List<CategoryViewModel>();

            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(_apiBaseUrl);

                var catTask = client.GetAsync("DanhMuc");
                string endpoint = categoryId.HasValue ? $"SanPham/danhmuc/{categoryId}" : "SanPham";
                var prodTask = client.GetAsync(endpoint);

                await Task.WhenAll(catTask, prodTask);

                if (catTask.Result.IsSuccessStatusCode)
                {
                    var catData = await catTask.Result.Content.ReadAsStringAsync();
                    categories = JsonConvert.DeserializeObject<List<CategoryViewModel>>(catData);
                }

                if (prodTask.Result.IsSuccessStatusCode)
                {
                    var prodData = await prodTask.Result.Content.ReadAsStringAsync();
                    products = JsonConvert.DeserializeObject<List<ProductViewModel>>(prodData);
                }
            }

            if (!string.IsNullOrEmpty(search))
            {
                products = products.Where(p => p.TenSanPham.ToLower().Contains(search.ToLower())).ToList();
            }

            ViewBag.Categories = categories;
            ViewBag.CurrentSearch = search;
            ViewBag.CurrentCategory = categoryId;

            return View(products);
        }

        // 2. TRANG CHI TIẾT SẢN PHẨM
        public async Task<ActionResult> Detail(int id)
        {
            ProductViewModel product = null;
            List<ThongSoKyThuatViewModel> thongSoKyThuat = new List<ThongSoKyThuatViewModel>();

            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(_apiBaseUrl);

                // Gọi song song 2 API
                var productTask = client.GetAsync($"SanPham/{id}");
                var thongSoTask = client.GetAsync($"ThongSoKyThuat/sanpham/{id}");

                await Task.WhenAll(productTask, thongSoTask);

                // Xử lý kết quả Sản phẩm
                if (productTask.Result.IsSuccessStatusCode)
                {
                    var data = await productTask.Result.Content.ReadAsStringAsync();
                    product = JsonConvert.DeserializeObject<ProductViewModel>(data);
                }

                // Xử lý kết quả Thông số kỹ thuật
                if (thongSoTask.Result.IsSuccessStatusCode)
                {
                    var data = await thongSoTask.Result.Content.ReadAsStringAsync();
                    thongSoKyThuat = JsonConvert.DeserializeObject<List<ThongSoKyThuatViewModel>>(data);
                }
            }

            if (product == null)
            {
                return HttpNotFound();
            }

            // Gán thông số kỹ thuật vào product
            product.ThongSoKyThuat = thongSoKyThuat;

            return View(product);
        }
    }
}