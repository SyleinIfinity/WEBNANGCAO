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

        // 1. TRANG DANH SÁCH SẢN PHẨM (Có tìm kiếm & lọc)
        public async Task<ActionResult> Index(string search = "", int? categoryId = null)
        {
            var products = new List<ProductViewModel>();
            var categories = new List<CategoryViewModel>();

            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(_apiBaseUrl);

                // A. Gọi API lấy danh mục (để hiển thị sidebar lọc)
                var catTask = client.GetAsync("DanhMuc");

                // B. Gọi API lấy sản phẩm
                // Nếu có lọc danh mục -> Gọi endpoint lọc theo danh mục
                string endpoint = categoryId.HasValue ? $"SanPham/danhmuc/{categoryId}" : "SanPham";
                var prodTask = client.GetAsync(endpoint);

                await Task.WhenAll(catTask, prodTask);

                // Xử lý kết quả Danh mục
                if (catTask.Result.IsSuccessStatusCode)
                {
                    var catData = await catTask.Result.Content.ReadAsStringAsync();
                    categories = JsonConvert.DeserializeObject<List<CategoryViewModel>>(catData);
                }

                // Xử lý kết quả Sản phẩm
                if (prodTask.Result.IsSuccessStatusCode)
                {
                    var prodData = await prodTask.Result.Content.ReadAsStringAsync();
                    products = JsonConvert.DeserializeObject<List<ProductViewModel>>(prodData);
                }
            }

            // C. Lọc tìm kiếm theo tên (Client-side)
            if (!string.IsNullOrEmpty(search))
            {
                products = products.Where(p => p.TenSanPham.ToLower().Contains(search.ToLower())).ToList();
            }

            // D. Chỉ lấy sản phẩm ĐANG KINH DOANH (Tồn > 0 và chưa bị ẩn)
            // Tùy logic của bạn, có thể ẩn sản phẩm hết hàng hoặc vẫn hiện nhưng disable nút mua
            // products = products.Where(p => p.SoLuongTon > 0).ToList();

            // Truyền dữ liệu qua View
            ViewBag.Categories = categories;
            ViewBag.CurrentSearch = search;
            ViewBag.CurrentCategory = categoryId;

            return View(products);
        }

        // 2. TRANG CHI TIẾT SẢN PHẨM
        public async Task<ActionResult> Detail(int id)
        {
            ProductViewModel product = null;

            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(_apiBaseUrl);
                var response = await client.GetAsync($"SanPham/{id}");

                if (response.IsSuccessStatusCode)
                {
                    var data = await response.Content.ReadAsStringAsync();
                    product = JsonConvert.DeserializeObject<ProductViewModel>(data);
                }
            }

            if (product == null)
            {
                return HttpNotFound(); // Hoặc chuyển hướng về trang chủ
            }

            return View(product);
        }
    }
}