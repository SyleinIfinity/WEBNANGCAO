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
        private readonly string _apiBaseUrl = ConfigurationManager.AppSettings["ApiBaseUrl"];

        // --- 1. TRANG DANH SÁCH (Hỗ trợ Tìm kiếm & Lọc) ---
        public async Task<ActionResult> Index(string search = "", int? categoryId = null)
        {
            var danhSach = new List<SanPhamViewModel>();

            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(_apiBaseUrl);

                // A. Lấy danh sách sản phẩm
                // Nếu có lọc danh mục -> Gọi API lọc danh mục
                string endpoint = categoryId.HasValue ? $"SanPham/danhmuc/{categoryId}" : "SanPham";

                var response = await client.GetAsync(endpoint);
                if (response.IsSuccessStatusCode)
                {
                    var data = await response.Content.ReadAsStringAsync();
                    danhSach = JsonConvert.DeserializeObject<List<SanPhamViewModel>>(data);
                }

                // B. Lọc theo tên (Client-side filtering vì API chưa có search param)
                if (!string.IsNullOrEmpty(search))
                {
                    search = search.ToLower();
                    danhSach = danhSach.Where(p => p.TenSanPham.ToLower().Contains(search)).ToList();
                }

                // C. Lấy danh sách Danh Mục để đổ vào Dropdown lọc
                var catResponse = await client.GetAsync("DanhMuc");
                if (catResponse.IsSuccessStatusCode)
                {
                    var catData = await catResponse.Content.ReadAsStringAsync();
                    var categories = JsonConvert.DeserializeObject<List<CategoryViewModel>>(catData);
                    ViewBag.Categories = categories; // Truyền qua View để vẽ Dropdown
                }
            }

            // Giữ lại giá trị filter để hiển thị trên giao diện
            ViewBag.CurrentSearch = search;
            ViewBag.CurrentCategory = categoryId;

            return View(danhSach);
        }

        // --- 2. TẠO MỚI (GET: Hiển thị form) ---
        public async Task<ActionResult> Create()
        {
            await LoadCategoriesToViewBag();
            return View();
        }

        // --- 3. TẠO MỚI (POST: Gửi dữ liệu lên API) ---
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(CreateProductViewModel model)
        {
            if (ModelState.IsValid)
            {
                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri(_apiBaseUrl);

                    // Tạo Multipart Form Data để gửi file
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

                        // Xử lý file ảnh
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
        // GET: Admin/SanPham/Edit/5
        public async Task<ActionResult> Edit(int id)
        {
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(_apiBaseUrl);

                // 1. Lấy thông tin sản phẩm
                var productTask = client.GetAsync($"SanPham/{id}");

                // 2. Lấy luôn danh sách danh mục để chuẩn bị cho Dropdown
                var categoryTask = client.GetAsync("DanhMuc");

                await Task.WhenAll(productTask, categoryTask);

                var response = productTask.Result;
                var catResponse = categoryTask.Result;

                if (response.IsSuccessStatusCode && catResponse.IsSuccessStatusCode)
                {
                    // A. Xử lý sản phẩm
                    var data = await response.Content.ReadAsStringAsync();
                    var productAPI = JsonConvert.DeserializeObject<SanPhamViewModel>(data);

                    // B. Xử lý danh mục (Để đổ vào Dropdown + Tìm ID)
                    var catData = await catResponse.Content.ReadAsStringAsync();
                    var listDanhMuc = JsonConvert.DeserializeObject<List<CategoryViewModel>>(catData);
                    ViewBag.Categories = listDanhMuc; // Gán vào ViewBag để View dùng

                    // [LOGIC MỚI] Tìm ID danh mục dựa trên Tên danh mục API trả về
                    int foundCategoryId = 0;
                    if (!string.IsNullOrEmpty(productAPI.TenDanhMuc))
                    {
                        var matchCat = listDanhMuc.FirstOrDefault(c => c.TenDanhMuc == productAPI.TenDanhMuc);
                        if (matchCat != null) foundCategoryId = matchCat.MaDanhMuc;
                    }

                    // C. Map sang UpdateProductViewModel
                    var editModel = new UpdateProductViewModel
                    {
                        MaSanPham = productAPI.MaSanPham,
                        TenSanPham = productAPI.TenSanPham,
                        GiaBan = productAPI.GiaBan,
                        GiaKhuyenMai = productAPI.GiaKhuyenMai,
                        SoLuongTon = productAPI.SoLuongTon,

                        // Gán ID vừa tìm được vào đây -> Dropdown sẽ tự chọn đúng
                        MaDanhMuc = foundCategoryId,

                        TrangThai = productAPI.TrangThai,
                        AnhHienTai = productAPI.DanhSachAnh ?? new List<ImageDTO>()
                    };

                    return View(editModel);
                }
            }
            return RedirectToAction("Index");
        }

        // --- 5. CẬP NHẬT (POST - PATCH) ---
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(UpdateProductViewModel model)
        {
            // Logic tương tự Create nhưng dùng method PATCH
            // .NET 4.7.2 HttpClient không có PatchAsync trực tiếp, dùng SendAsync
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(_apiBaseUrl);
                using (var content = new MultipartFormDataContent())
                {
                    // Add các field cần update (cho phép null nếu không đổi)
                    content.Add(new StringContent(model.TenSanPham), "TenSanPham");
                    content.Add(new StringContent(model.GiaBan.ToString()), "GiaBan");
                    content.Add(new StringContent(model.SoLuongTon.ToString()), "SoLuongTon");
                    content.Add(new StringContent(model.MaDanhMuc.ToString()), "MaDanhMuc");
                    content.Add(new StringContent(model.TrangThai.ToString()), "TrangThai");
                    // ... Thêm các field khác

                    // Xử lý ảnh mới
                    if (model.HinhAnhs != null) { /* Logic add file stream như Create */ }

                    // Xử lý ảnh xóa (PublicIdsToDelete)
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
                    if (response.IsSuccessStatusCode) return RedirectToAction("Index");
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

        // Helper lấy danh mục
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