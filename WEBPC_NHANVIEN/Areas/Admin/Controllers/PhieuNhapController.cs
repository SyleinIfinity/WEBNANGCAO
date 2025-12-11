using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using WEBPC_NHANVIEN.Areas.Admin.Filters;
using WEBPC_NHANVIEN.Areas.Admin.Models;

namespace WEBPC_NHANVIEN.Areas.Admin.Controllers
{
    [AdminAuthorize]
    public class PhieuNhapController : Controller
    {
        private readonly string _apiBaseUrl = ConfigurationManager.AppSettings["ApiBaseUrl"];

        // Cấu hình JSON CamelCase để khớp với API ASP.NET Core
        private readonly JsonSerializerSettings _jsonSettings = new JsonSerializerSettings
        {
            ContractResolver = new CamelCasePropertyNamesContractResolver()
        };

        // --- 1. DANH SÁCH (INDEX) ---
        public async Task<ActionResult> Index()
        {
            var danhSach = new List<PhieuNhapResponseViewModel>();
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(_apiBaseUrl);
                // GET api/PhieuNhap
                var response = await client.GetAsync("api/PhieuNhap");

                if (response.IsSuccessStatusCode)
                {
                    var data = await response.Content.ReadAsStringAsync();
                    danhSach = JsonConvert.DeserializeObject<List<PhieuNhapResponseViewModel>>(data);
                }
            }
            return View(danhSach);
        }

        // --- 2. CHI TIẾT (DETAILS) ---
        public async Task<ActionResult> Details(int id)
        {
            var model = await GetPhieuNhapById(id);
            if (model == null) return RedirectToAction("Index");
            return View(model);
        }

        // --- 3. TẠO MỚI (GET) ---
        public async Task<ActionResult> Create()
        {
            await LoadProductsToViewBag();
            return View(new PhieuNhapCreateViewModel());
        }

        // --- 4. TẠO MỚI (POST) ---
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(PhieuNhapCreateViewModel model)
        {
            // [QUAN TRỌNG] Gán ID Nhân viên nhập. 
            // Thực tế bạn lấy từ Session["NhanVienId"] hoặc User.Identity
            model.MaNhanVienNhap = 1; // Ví dụ gán cứng là 1 (Admin)

            if (model.ChiTiet == null || !model.ChiTiet.Any())
            {
                ModelState.AddModelError("", "Vui lòng nhập ít nhất 1 sản phẩm.");
            }

            if (ModelState.IsValid)
            {
                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri(_apiBaseUrl);

                    // Serialize model sang JSON (Chỉ chứa maNhanVienNhap, ghiChu, chiTiet)
                    var jsonContent = JsonConvert.SerializeObject(model, _jsonSettings);
                    var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                    // POST api/PhieuNhap
                    var response = await client.PostAsync("api/PhieuNhap", content);

                    if (response.IsSuccessStatusCode)
                    {
                        TempData["Message"] = "Nhập kho thành công!";
                        return RedirectToAction("Index");
                    }
                    else
                    {
                        var err = await response.Content.ReadAsStringAsync();
                        ModelState.AddModelError("", $"Lỗi API: {err}");
                    }
                }
            }

            await LoadProductsToViewBag();
            return View(model);
        }

        // --- 5. CẬP NHẬT (GET) ---
        public async Task<ActionResult> Edit(int id)
        {
            var apiModel = await GetPhieuNhapById(id);
            if (apiModel == null) return RedirectToAction("Index");

            // Map dữ liệu từ API sang Edit View Model
            var editModel = new PhieuNhapEditViewModel
            {
                MaPhieuNhap = apiModel.MaPhieuNhap,
                MaCodePhieu = apiModel.MaCodePhieu,
                GhiChu = apiModel.GhiChu,
                NgayNhap = apiModel.NgayNhap,
                TenNhanVien = apiModel.TenNhanVien,
                ChiTietHienThi = apiModel.ChiTiet // List này chỉ để hiển thị
            };

            return View(editModel);
        }

        // --- 6. CẬP NHẬT (POST - PATCH) ---
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(PhieuNhapEditViewModel model)
        {
            if (ModelState.IsValid)
            {
                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri(_apiBaseUrl);

                    // [QUAN TRỌNG] Tạo object anonymous chỉ chứa trường cần update theo UpdatePhieuNhapRequest
                    var patchPayload = new
                    {
                        ghiChu = model.GhiChu
                    };

                    var jsonContent = JsonConvert.SerializeObject(patchPayload, _jsonSettings);
                    var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                    // Dùng HttpMethod.Patch
                    var request = new HttpRequestMessage(new HttpMethod("PATCH"), $"api/PhieuNhap/{model.MaPhieuNhap}")
                    {
                        Content = content
                    };

                    var response = await client.SendAsync(request);

                    if (response.IsSuccessStatusCode)
                    {
                        TempData["Message"] = "Cập nhật ghi chú thành công!";
                        return RedirectToAction("Index");
                    }

                    var err = await response.Content.ReadAsStringAsync();
                    ModelState.AddModelError("", "Lỗi cập nhật: " + err);
                }
            }

            // Nếu lỗi, phải load lại thông tin hiển thị (vì form submit không gửi lại list chi tiết)
            var currentData = await GetPhieuNhapById(model.MaPhieuNhap);
            if (currentData != null)
            {
                model.ChiTietHienThi = currentData.ChiTiet;
                model.NgayNhap = currentData.NgayNhap;
                model.TenNhanVien = currentData.TenNhanVien;
                model.MaCodePhieu = currentData.MaCodePhieu;
            }

            return View(model);
        }

        // --- 7. XÓA ---
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Delete(int id)
        {
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(_apiBaseUrl);
                await client.DeleteAsync($"api/PhieuNhap/{id}");
            }
            TempData["Message"] = "Đã xóa phiếu nhập.";
            return RedirectToAction("Index");
        }

        // --- HELPERS ---
        private async Task<PhieuNhapResponseViewModel> GetPhieuNhapById(int id)
        {
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(_apiBaseUrl);
                var response = await client.GetAsync($"api/PhieuNhap/{id}");
                if (response.IsSuccessStatusCode)
                {
                    var data = await response.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<PhieuNhapResponseViewModel>(data);
                }
            }
            return null;
        }

        private async Task LoadProductsToViewBag()
        {
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(_apiBaseUrl);
                // Giả sử API lấy list SP là GET api/SanPham
                var response = await client.GetAsync("api/SanPham");
                var listItems = new List<SelectListItem>();

                if (response.IsSuccessStatusCode)
                {
                    var data = await response.Content.ReadAsStringAsync();
                    // Deserialize thành list SP (dùng tạm class dynamic hoặc ViewModel SP nếu có)
                    var products = JsonConvert.DeserializeObject<List<SanPhamViewModel>>(data);
                    if (products != null)
                    {
                        listItems = products.Select(p => new SelectListItem
                        {
                            Value = p.MaSanPham.ToString(),
                            Text = $"{p.MaSanPham} - {p.TenSanPham} (Tồn: {p.SoLuongTon})"
                        }).ToList();
                    }
                }
                ViewBag.ProductOptions = listItems;
            }
        }
    }
}