using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Mvc;
using WEBPC_KHACHHANG.Models.ViewModels;

namespace WEBPC_KHACHHANG.Controllers
{
    public class BuildPCController : Controller
    {
        private readonly HttpClient client;
        private readonly string _apiBaseUrl = ConfigurationManager.AppSettings["ApiBaseUrl"];

        public BuildPCController()
        {
            client = new HttpClient();
            client.BaseAddress = new Uri(_apiBaseUrl ?? "https://localhost:44391/api/");
        }

        // [ACTION QUAN TRỌNG]: Hiển thị trang cấu hình
        public ActionResult Index()
        {
            // 1. Tạo Model và điền dữ liệu danh mục
            var model = new BuildPCViewModel
            {
                Categories = new List<string>
                {
                    "Vi xử lý (CPU)",
                    "Bo mạch chủ (Mainboard)",
                    "RAM",
                    "Ổ cứng SSD",
                    "Ổ cứng HDD",
                    "Card màn hình (VGA)",
                    "Nguồn (PSU)",
                    "Vỏ Case",
                    "Tản nhiệt"
                }
            };

            // 2. Truyền model này sang View (Nếu quên biến 'model' ở đây sẽ bị lỗi trắng trang)
            return View(model);
        }

        // API lấy sản phẩm (Giữ nguyên logic cũ của bạn)
        [HttpGet]
        public async Task<ActionResult> GetProductsByCategory(string category)
        {
            var products = new List<ProductViewModel>();
            try
            {
                var response = await client.GetAsync("SanPham");
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    products = JsonConvert.DeserializeObject<List<ProductViewModel>>(content);
                }

                string keyword = GetKeywordFromCategory(category);
                if (!string.IsNullOrEmpty(keyword) && products != null)
                {
                    products = products.Where(p => IsMatch(p, keyword)).ToList();
                }

                var result = products.OrderByDescending(p => p.MaSanPham).Take(50).Select(p => new {
                    Id = p.MaSanPham,
                    Name = p.TenSanPham,
                    Price = (p.GiaKhuyenMai.HasValue && p.GiaKhuyenMai > 0) ? p.GiaKhuyenMai.Value : p.GiaBan,
                    OldPrice = (p.GiaKhuyenMai.HasValue && p.GiaKhuyenMai > 0) ? p.GiaBan : 0,
                    Image = p.HinhAnhDaiDien
                });

                return Json(result, JsonRequestBehavior.AllowGet);
            }
            catch { return Json(new List<object>(), JsonRequestBehavior.AllowGet); }
        }

        private bool IsMatch(ProductViewModel p, string keyword)
        {
            if (string.IsNullOrEmpty(keyword)) return true;
            string s = keyword.ToLower();
            return (p.TenSanPham != null && p.TenSanPham.ToLower().Contains(s)) ||
                   (p.TenDanhMuc != null && p.TenDanhMuc.ToLower().Contains(s));
        }

        private string GetKeywordFromCategory(string category)
        {
            if (string.IsNullOrEmpty(category)) return "";
            if (category.Contains("CPU")) return "CPU";
            if (category.Contains("Mainboard")) return "Mainboard";
            if (category.Contains("RAM")) return "RAM";
            if (category.Contains("SSD")) return "SSD";
            if (category.Contains("HDD")) return "HDD";
            if (category.Contains("VGA")) return "VGA";
            if (category.Contains("Nguồn")) return "Nguồn";
            if (category.Contains("Case")) return "Case";
            if (category.Contains("Tản nhiệt")) return "Tản nhiệt";
            return "";
        }
    }
}