using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization; // Cần thêm thư viện này
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web.Mvc;
using WEBPC_NHANVIEN.Areas.Admin.Filters;
using WEBPC_NHANVIEN.Areas.Admin.Models;

namespace WEBPC_NHANVIEN.Areas.Admin.Controllers
{
    [AdminAuthorize]
    public class SanPhamController : Controller
    {
        private readonly string _apiBaseUrl = ConfigurationManager.AppSettings["ApiBaseUrl"];

        // --- 1. TRANG DANH SÁCH (INDEX) ---
        public async Task<ActionResult> Index(string search = "", int? categoryId = null)
        {
            var danhSach = new List<SanPhamViewModel>();
            var categories = new List<CategoryViewModel>();

            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(_apiBaseUrl);

                string endpoint = categoryId.HasValue ? $"SanPham/danhmuc/{categoryId}" : "SanPham";
                var taskProducts = client.GetAsync(endpoint);
                var taskCategories = client.GetAsync("DanhMuc");

                await Task.WhenAll(taskProducts, taskCategories);

                var resProd = taskProducts.Result;
                if (resProd.IsSuccessStatusCode)
                {
                    var data = await resProd.Content.ReadAsStringAsync();
                    danhSach = JsonConvert.DeserializeObject<List<SanPhamViewModel>>(data);
                }

                var resCat = taskCategories.Result;
                if (resCat.IsSuccessStatusCode)
                {
                    var data = await resCat.Content.ReadAsStringAsync();
                    categories = JsonConvert.DeserializeObject<List<CategoryViewModel>>(data);
                }
            }

            if (!string.IsNullOrEmpty(search))
            {
                danhSach = danhSach.Where(p => p.TenSanPham.ToLower().Contains(search.ToLower())).ToList();
            }

            ViewBag.Categories = categories;
            ViewBag.CurrentSearch = search;
            ViewBag.CurrentCategory = categoryId;

            return View(danhSach);
        }

        // --- 2. TẠO MỚI (GET) ---
        public async Task<ActionResult> Create()
        {
            await LoadCategoriesToViewBag();
            return View();
        }

        // --- 3. TẠO MỚI (POST) - ĐÃ SỬA LỖI ---
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(CreateProductViewModel model)
        {
            if (ModelState.IsValid)
            {
                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri(_apiBaseUrl);

                    using (var content = new MultipartFormDataContent())
                    {
                        // 1. Gửi chuỗi text bình thường
                        content.Add(new StringContent(model.TenSanPham), "TenSanPham");
                        content.Add(new StringContent(model.MaDanhMuc.ToString()), "MaDanhMuc");

                        // [QUAN TRỌNG] Chuyển bool về chữ thường "true"/"false"
                        content.Add(new StringContent(model.TrangThai.ToString().ToLower()), "TrangThai");

                        // [QUAN TRỌNG] Ép kiểu số về định dạng chuẩn Quốc Tế (Invariant) để tránh dấu phẩy
                        content.Add(new StringContent(model.GiaBan.ToString(CultureInfo.InvariantCulture)), "GiaBan");
                        content.Add(new StringContent(model.SoLuongTon.ToString(CultureInfo.InvariantCulture)), "SoLuongTon");

                        if (!string.IsNullOrEmpty(model.MoTa))
                        {
                            content.Add(new StringContent(model.MoTa), "MoTa");
                        }

                        if (model.GiaKhuyenMai.HasValue)
                        {
                            content.Add(new StringContent(model.GiaKhuyenMai.Value.ToString(CultureInfo.InvariantCulture)), "GiaKhuyenMai");
                        }

                        // 2. Xử lý file ảnh
                        if (model.HinhAnhs != null)
                        {
                            foreach (var file in model.HinhAnhs)
                            {
                                if (file != null && file.ContentLength > 0)
                                {
                                    var fileContent = new StreamContent(file.InputStream);
                                    fileContent.Headers.ContentType = new MediaTypeHeaderValue(file.ContentType);
                                    // Tên tham số "HinhAnhs" phải khớp với API (List<IFormFile> HinhAnhs)
                                    content.Add(fileContent, "HinhAnhs", file.FileName);
                                }
                            }
                        }

                        var response = await client.PostAsync("SanPham", content);
                        if (response.IsSuccessStatusCode)
                        {
                            return RedirectToAction("Index");
                        }
                        else
                        {
                            // Đọc nội dung lỗi từ API trả về để debug dễ hơn
                            var errorContent = await response.Content.ReadAsStringAsync();
                            ModelState.AddModelError("", $"Lỗi API ({response.StatusCode}): {errorContent}");
                        }
                    }
                }
            }
            await LoadCategoriesToViewBag();
            return View(model);
        }

        // --- 4. CẬP NHẬT (GET) ---
        public async Task<ActionResult> Edit(int id)
        {
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(_apiBaseUrl);

                var productTask = client.GetAsync($"SanPham/{id}");
                var categoryTask = client.GetAsync("DanhMuc");

                await Task.WhenAll(productTask, categoryTask);

                var response = productTask.Result;
                var catResponse = categoryTask.Result;

                if (response.IsSuccessStatusCode && catResponse.IsSuccessStatusCode)
                {
                    var data = await response.Content.ReadAsStringAsync();
                    var productAPI = JsonConvert.DeserializeObject<SanPhamViewModel>(data);

                    var catData = await catResponse.Content.ReadAsStringAsync();
                    var listDanhMuc = JsonConvert.DeserializeObject<List<CategoryViewModel>>(catData);
                    ViewBag.Categories = listDanhMuc;

                    // Logic tìm ID danh mục
                    int foundCategoryId = 0;
                    if (!string.IsNullOrEmpty(productAPI.TenDanhMuc))
                    {
                        var matchCat = listDanhMuc.FirstOrDefault(c => c.TenDanhMuc == productAPI.TenDanhMuc);
                        if (matchCat != null) foundCategoryId = matchCat.MaDanhMuc;
                    }

                    var editModel = new UpdateProductViewModel
                    {
                        MaSanPham = productAPI.MaSanPham,
                        TenSanPham = productAPI.TenSanPham,
                        GiaBan = productAPI.GiaBan,
                        GiaKhuyenMai = productAPI.GiaKhuyenMai,
                        SoLuongTon = productAPI.SoLuongTon,
                        MaDanhMuc = foundCategoryId,
                        MoTa = productAPI.MoTa,
                        TrangThai = productAPI.TrangThai,
                        AnhHienTai = productAPI.DanhSachAnh ?? new List<ImageDTO>()
                    };

                    return View(editModel);
                }
            }
            return RedirectToAction("Index");
        }

        // --- 5. CẬP NHẬT (POST) - ĐÃ SỬA LỖI ---
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(UpdateProductViewModel model)
        {
            if (ModelState.IsValid)
            {
                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri(_apiBaseUrl);
                    using (var content = new MultipartFormDataContent())
                    {
                        content.Add(new StringContent(model.TenSanPham), "TenSanPham");
                        content.Add(new StringContent(model.MaDanhMuc.ToString()), "MaDanhMuc");

                        // [FIX] Convert chuẩn định dạng
                        content.Add(new StringContent(model.TrangThai.ToString().ToLower()), "TrangThai");
                        content.Add(new StringContent(model.GiaBan.ToString(CultureInfo.InvariantCulture)), "GiaBan");
                        content.Add(new StringContent(model.SoLuongTon.ToString(CultureInfo.InvariantCulture)), "SoLuongTon");

                        if (model.GiaKhuyenMai.HasValue)
                        {
                            content.Add(new StringContent(model.GiaKhuyenMai.Value.ToString(CultureInfo.InvariantCulture)), "GiaKhuyenMai");
                        }

                        if (!string.IsNullOrEmpty(model.MoTa))
                        {
                            content.Add(new StringContent(model.MoTa), "MoTa");
                        }

                        if (model.HinhAnhs != null)
                        {
                            foreach (var file in model.HinhAnhs)
                            {
                                if (file != null && file.ContentLength > 0)
                                {
                                    var fileContent = new StreamContent(file.InputStream);
                                    fileContent.Headers.ContentType = new MediaTypeHeaderValue(file.ContentType);
                                    content.Add(fileContent, "HinhAnhs", file.FileName);
                                }
                            }
                        }

                        if (model.PublicIdsToDelete != null)
                        {
                            foreach (var pubId in model.PublicIdsToDelete)
                            {
                                content.Add(new StringContent(pubId), "PublicIdsToDelete");
                            }
                        }

                        var request = new HttpRequestMessage(new HttpMethod("PATCH"), $"SanPham/{model.MaSanPham}")
                        {
                            Content = content
                        };

                        var response = await client.SendAsync(request);
                        if (response.IsSuccessStatusCode)
                        {
                            return RedirectToAction("Index");
                        }
                        else
                        {
                            var errorContent = await response.Content.ReadAsStringAsync();
                            ModelState.AddModelError("", $"Lỗi cập nhật API ({response.StatusCode}): {errorContent}");
                        }
                    }
                }
            }
            await LoadCategoriesToViewBag();
            return View(model);
        }

        // --- 6. XÓA ---
        [HttpPost]
        public async Task<ActionResult> Delete(int id)
        {
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(_apiBaseUrl);
                await client.DeleteAsync($"SanPham/{id}");
            }
            return RedirectToAction("Index");
        }

        private async Task LoadCategoriesToViewBag()
        {
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(_apiBaseUrl);
                var response = await client.GetAsync("DanhMuc");
                if (response.IsSuccessStatusCode)
                {
                    var data = await response.Content.ReadAsStringAsync();
                    ViewBag.Categories = JsonConvert.DeserializeObject<List<CategoryViewModel>>(data);
                }
            }
        }
    }
}