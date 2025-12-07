using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web.Mvc;
using Newtonsoft.Json;
using WEBPC_NHANVIEN.Areas.Admin.Models;

namespace WEBPC_NHANVIEN.Areas.Admin.Controllers
{
    public class SanPhamController : Controller
    {
        // Đọc URL từ Web.config (Đã cấu hình: https://webapi-1-qldr.onrender.com/api/)
        private readonly string _apiBaseUrl = ConfigurationManager.AppSettings["ApiBaseUrl"];

        // --- 1. TRANG DANH SÁCH (INDEX) ---
        public async Task<ActionResult> Index(string search = "", int? categoryId = null)
        {
            var danhSach = new List<SanPhamViewModel>();
            var categories = new List<CategoryViewModel>();

            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(_apiBaseUrl);

                // A. Gọi song song 2 tác vụ: Lấy sản phẩm & Lấy danh mục
                // Nếu có lọc danh mục -> Gọi endpoint lọc
                string endpoint = categoryId.HasValue ? $"SanPham/danhmuc/{categoryId}" : "SanPham";

                var taskProducts = client.GetAsync(endpoint);
                var taskCategories = client.GetAsync("DanhMuc");

                // Chờ cả 2 xong mới chạy tiếp (Tối ưu tốc độ load)
                await Task.WhenAll(taskProducts, taskCategories);

                // B. Xử lý dữ liệu Sản Phẩm
                var resProd = taskProducts.Result;
                if (resProd.IsSuccessStatusCode)
                {
                    var data = await resProd.Content.ReadAsStringAsync();
                    danhSach = JsonConvert.DeserializeObject<List<SanPhamViewModel>>(data);
                }

                // C. Xử lý dữ liệu Danh Mục (để đổ vào Dropdown)
                var resCat = taskCategories.Result;
                if (resCat.IsSuccessStatusCode)
                {
                    var data = await resCat.Content.ReadAsStringAsync();
                    categories = JsonConvert.DeserializeObject<List<CategoryViewModel>>(data);
                }
            }

            // D. Lọc tìm kiếm theo tên (Client-side filtering vì API chưa hỗ trợ search param)
            if (!string.IsNullOrEmpty(search))
            {
                danhSach = danhSach.Where(p => p.TenSanPham.ToLower().Contains(search.ToLower())).ToList();
            }

            // E. Truyền dữ liệu qua View
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

        // --- 3. TẠO MỚI (POST) ---
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(CreateProductViewModel model)
        {
            if (ModelState.IsValid)
            {
                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri(_apiBaseUrl);

                    // Tạo Multipart Form Data để gửi file và dữ liệu
                    using (var content = new MultipartFormDataContent())
                    {
                        content.Add(new StringContent(model.TenSanPham), "TenSanPham");
                        content.Add(new StringContent(model.GiaBan.ToString()), "GiaBan");
                        content.Add(new StringContent(model.SoLuongTon.ToString()), "SoLuongTon");
                        content.Add(new StringContent(model.MaDanhMuc.ToString()), "MaDanhMuc");
                        content.Add(new StringContent(model.TrangThai.ToString()), "TrangThai");

                        if (!string.IsNullOrEmpty(model.MoTa))
                            content.Add(new StringContent(model.MoTa), "MoTa");

                        if (model.GiaKhuyenMai.HasValue)
                            content.Add(new StringContent(model.GiaKhuyenMai.ToString()), "GiaKhuyenMai");

                        // Xử lý file ảnh upload
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

                        var response = await client.PostAsync("SanPham", content);
                        if (response.IsSuccessStatusCode)
                        {
                            return RedirectToAction("Index");
                        }
                        else
                        {
                            ModelState.AddModelError("", "Lỗi API: " + response.ReasonPhrase);
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

                // Gọi song song lấy SP và Danh mục
                var productTask = client.GetAsync($"SanPham/{id}");
                var categoryTask = client.GetAsync("DanhMuc");

                await Task.WhenAll(productTask, categoryTask);

                var response = productTask.Result;
                var catResponse = categoryTask.Result;

                if (response.IsSuccessStatusCode && catResponse.IsSuccessStatusCode)
                {
                    // 1. Xử lý sản phẩm
                    var data = await response.Content.ReadAsStringAsync();
                    var productAPI = JsonConvert.DeserializeObject<SanPhamViewModel>(data);

                    // 2. Xử lý danh mục
                    var catData = await catResponse.Content.ReadAsStringAsync();
                    var listDanhMuc = JsonConvert.DeserializeObject<List<CategoryViewModel>>(catData);
                    ViewBag.Categories = listDanhMuc;

                    // 3. Logic tìm ID danh mục từ tên (Do API ProductResponse thiếu MaDanhMuc)
                    int foundCategoryId = 0;
                    if (!string.IsNullOrEmpty(productAPI.TenDanhMuc))
                    {
                        var matchCat = listDanhMuc.FirstOrDefault(c => c.TenDanhMuc == productAPI.TenDanhMuc);
                        if (matchCat != null) foundCategoryId = matchCat.MaDanhMuc;
                    }

                    // 4. Map sang ViewModel cho trang Edit
                    var editModel = new UpdateProductViewModel
                    {
                        MaSanPham = productAPI.MaSanPham,
                        TenSanPham = productAPI.TenSanPham,
                        GiaBan = productAPI.GiaBan,
                        GiaKhuyenMai = productAPI.GiaKhuyenMai,
                        SoLuongTon = productAPI.SoLuongTon,
                        MaDanhMuc = foundCategoryId, // Gán ID tìm được
                        MoTa = "", // API hiện tại chưa trả về Mô tả, tạm để trống
                        TrangThai = productAPI.TrangThai,
                        AnhHienTai = productAPI.DanhSachAnh ?? new List<ImageDTO>()
                    };

                    return View(editModel);
                }
            }
            return RedirectToAction("Index");
        }

        // --- 5. CẬP NHẬT (POST) ---
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
                        // Add dữ liệu cơ bản
                        content.Add(new StringContent(model.TenSanPham), "TenSanPham");
                        content.Add(new StringContent(model.GiaBan.ToString()), "GiaBan");
                        content.Add(new StringContent(model.SoLuongTon.ToString()), "SoLuongTon");
                        content.Add(new StringContent(model.MaDanhMuc.ToString()), "MaDanhMuc");
                        content.Add(new StringContent(model.TrangThai.ToString()), "TrangThai");

                        if (model.GiaKhuyenMai.HasValue)
                            content.Add(new StringContent(model.GiaKhuyenMai.ToString()), "GiaKhuyenMai");

                        if (!string.IsNullOrEmpty(model.MoTa))
                            content.Add(new StringContent(model.MoTa), "MoTa");

                        // Xử lý ảnh mới
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

                        // Xử lý ảnh cần xóa
                        if (model.PublicIdsToDelete != null)
                        {
                            foreach (var pubId in model.PublicIdsToDelete)
                            {
                                content.Add(new StringContent(pubId), "PublicIdsToDelete");
                            }
                        }

                        // Gửi PATCH request
                        var request = new HttpRequestMessage(new HttpMethod("PATCH"), $"SanPham/{model.MaSanPham}")
                        {
                            Content = content
                        };

                        var response = await client.SendAsync(request);
                        if (response.IsSuccessStatusCode) return RedirectToAction("Index");
                        else ModelState.AddModelError("", "Lỗi cập nhật: " + response.ReasonPhrase);
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

        // --- HELPER: Load Danh Mục ---
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