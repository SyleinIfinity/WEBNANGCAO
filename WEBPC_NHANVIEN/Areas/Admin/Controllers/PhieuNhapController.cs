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
                var response = await client.GetAsync("PhieuNhap");

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
        // FILE: Areas/Admin/Controllers/PhieuNhapController.cs

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(PhieuNhapCreateViewModel model)
        {
            // Check if session exists
            if (Session["NhanVien"] == null && Session["UserId"] == null)
            {
                // Fallback for testing ONLY: Hardcode valid ID (e.g., 2)
                model.MaNhanVienNhap = 2;

                // In production, uncomment the line below to force login:
                // return RedirectToAction("Login", "Account", new { area = "" });
            }
            else
            {
                // Real logic: Retrieve ID from Session
                // Adjust "UserId" to whatever key you used in AccountController
                if (Session["UserId"] != null)
                {
                    model.MaNhanVienNhap = (int)Session["UserId"];
                }
                else
                {
                    // Example if you stored an object
                    // var nv = (NhanVien)Session["NhanVien"];
                    // model.MaNhanVienNhap = nv.MaNhanVien;
                    model.MaNhanVienNhap = 2; // Safety fallback
                }
            }

            // Check 2: Validate Detail List
            if (model.ChiTiet == null || !model.ChiTiet.Any())
            {
                ModelState.AddModelError("", "Vui lòng nhập ít nhất 1 sản phẩm.");
            }

            if (ModelState.IsValid)
            {
                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri(_apiBaseUrl);

                    // Serialize with CamelCase to match API expectations
                    var jsonContent = JsonConvert.SerializeObject(model, _jsonSettings);
                    var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                    var response = await client.PostAsync("PhieuNhap", content);

                    if (response.IsSuccessStatusCode)
                    {
                        TempData["Message"] = "Nhập kho thành công!";
                        return RedirectToAction("Index");
                    }
                    else
                    {
                        // Capture detailed API error
                        var errorContent = await response.Content.ReadAsStringAsync();
                        ModelState.AddModelError("", $"Lỗi API ({response.StatusCode}): {errorContent}");
                    }
                }
            }

            // Reload products if failure
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
                    var request = new HttpRequestMessage(new HttpMethod("PATCH"), $"PhieuNhap/{model.MaPhieuNhap}")
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
                await client.DeleteAsync($"PhieuNhap/{id}");
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
                var response = await client.GetAsync($"PhieuNhap/{id}");
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

                // [FIX] Bỏ "api/" vì BaseAddress đã bao gồm nó
                var response = await client.GetAsync("SanPham");

                var listItems = new List<SelectListItem>();

                if (response.IsSuccessStatusCode)
                {
                    var data = await response.Content.ReadAsStringAsync();

                    // Dùng SanPhamViewModel hoặc tạo class DTO nhanh
                    var products = JsonConvert.DeserializeObject<List<SanPhamViewModel>>(data);

                    if (products != null)
                    {
                        listItems = products.Select(p => new SelectListItem
                        {
                            Value = p.MaSanPham.ToString(),
                            // Hiển thị: 10 - Tên SP (Tồn: 50)
                            Text = $"{p.MaSanPham} - {p.TenSanPham} (Tồn: {p.SoLuongTon})"
                        }).ToList();
                    }
                }
                else
                {
                    // [DEBUG] Thêm dòng này để biết nếu API lỗi trên giao diện
                    listItems.Add(new SelectListItem { Value = "", Text = $"Lỗi API: {response.StatusCode}" });
                }

                ViewBag.ProductOptions = listItems;
            }
        }
    }
}