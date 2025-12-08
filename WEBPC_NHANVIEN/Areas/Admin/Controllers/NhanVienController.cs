using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using Newtonsoft.Json;
using WEBPC_NHANVIEN.Areas.Admin.Models;

namespace WEBPC_NHANVIEN.Areas.Admin.Controllers
{
    public class NhanVienController : Controller
    {
        // Đọc URL từ Web.config (đã cấu hình: https://webapi-1-qldr.onrender.com/api/)
        private readonly string _apiBaseUrl = ConfigurationManager.AppSettings["ApiBaseUrl"];

        // ---------------------------
        // 1. DANH SÁCH (INDEX)
        // ---------------------------
        public async Task<ActionResult> Index()
        {
            var danhSach = new List<NhanVienListItemViewModel>();

            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(_apiBaseUrl);

                var response = await client.GetAsync("NhanVien");
                if (response.IsSuccessStatusCode)
                {
                    var data = await response.Content.ReadAsStringAsync();

                    // API trả về NhanVienResponse => map thẳng sang NhanVienListItemViewModel
                    danhSach = JsonConvert.DeserializeObject<List<NhanVienListItemViewModel>>(data);
                }
            }

            // View hiện tại đang dùng @model IEnumerable<dynamic> nên cần Cast sang dynamic
            return View(danhSach);

        }

        // ---------------------------
        // 2. TẠO MỚI (GET)
        // ---------------------------
        public async Task<ActionResult> Create()
        {
            await LoadRolesToViewBag();
            return View();
        }

        // ---------------------------
        // 3. TẠO MỚI (POST)
        // ---------------------------
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(NhanVienCreateViewModel model)
        {
            if (!ModelState.IsValid)
            {
                await LoadRolesToViewBag();
                return View(model);
            }

            // Map sang DTO gửi lên API
            var requestDto = new NhanVienApiRequest
            {
                HoTen = model.HoTen,
                SoDienThoai = model.SoDienThoai,
                MaVaiTro = model.MaVaiTro,
                TenDangNhap = model.TenDangNhap,
                MatKhau = model.MatKhau,
                Email = model.Email
                // TrangThaiTaiKhoan: API hiện chưa hỗ trợ set trạng thái qua NhanVienRequest
            };

            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(_apiBaseUrl);

                var json = JsonConvert.SerializeObject(requestDto);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await client.PostAsync("NhanVien", content);

                var body = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    // API trả: { message = "..." }
                    dynamic result = JsonConvert.DeserializeObject(body);
                    TempData["Message"] = result?.message ?? "Thêm nhân viên thành công.";
                    return RedirectToAction("Index");
                }
                else
                {
                    try
                    {
                        dynamic result = JsonConvert.DeserializeObject(body);
                        string message = result?.message ?? response.ReasonPhrase;
                        ModelState.AddModelError("", "Lỗi API: " + message);
                    }
                    catch
                    {
                        ModelState.AddModelError("", "Lỗi API: " + response.ReasonPhrase);
                    }
                }
            }

            await LoadRolesToViewBag();
            return View(model);
        }

        // ---------------------------
        // 4. CẬP NHẬT (GET)
        // ---------------------------
        public async Task<ActionResult> Edit(int id)
        {
            NhanVienEditViewModel editModel = null;

            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(_apiBaseUrl);

                var response = await client.GetAsync($"NhanVien/{id}");
                if (!response.IsSuccessStatusCode)
                {
                    TempData["Message"] = "Không tìm thấy nhân viên.";
                    return RedirectToAction("Index");
                }

                var data = await response.Content.ReadAsStringAsync();

                // Map từ NhanVienResponse sang NhanVienEditViewModel
                var nvResponse = JsonConvert.DeserializeObject<NhanVienListItemViewModel>(data);

                editModel = new NhanVienEditViewModel
                {
                    MaNhanVien = nvResponse.MaNhanVien,
                    MaCodeNhanVien = nvResponse.MaCodeNhanVien,
                    HoTen = nvResponse.HoTen,
                    SoDienThoai = nvResponse.SoDienThoai,
                    MaVaiTro = nvResponse.MaVaiTro,
                    Email = nvResponse.Email,
                    TenDangNhap = nvResponse.TenDangNhap,
                    TrangThaiTaiKhoan = nvResponse.TrangThaiTaiKhoan
                    // MatKhau: để trống, chỉ nhập nếu đổi
                };
            }

            await LoadRolesToViewBag();

            // View hiện dùng @model dynamic
           return View(editModel);
        }

        // ---------------------------
        // 5. CẬP NHẬT (POST)
        // ---------------------------
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(NhanVienEditViewModel model)
        {
            if (!ModelState.IsValid)
            {
                await LoadRolesToViewBag();
                return View((dynamic)model);
            }

            if (model.MaNhanVien <= 0)
            {
                ModelState.AddModelError("", "Thiếu mã nhân viên cần cập nhật.");
                await LoadRolesToViewBag();
                return View((dynamic)model);
            }

            // Map sang DTO request
            var requestDto = new NhanVienApiRequest
            {
                HoTen = model.HoTen,
                SoDienThoai = model.SoDienThoai,
                MaVaiTro = model.MaVaiTro,
                TenDangNhap = model.TenDangNhap,
                Email = model.Email
            };

            // Nếu user nhập mật khẩu mới thì gửi lên, không thì để null/empty
            if (!string.IsNullOrWhiteSpace(model.MatKhau))
            {
                requestDto.MatKhau = model.MatKhau;
            }
            else
            {
                // Để chống nhầm lẫn, vẫn set chuỗi rỗng => service bên API nên xử lý
                requestDto.MatKhau = null;
            }

            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(_apiBaseUrl);

                var json = JsonConvert.SerializeObject(requestDto);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await client.PutAsync($"NhanVien/{model.MaNhanVien}", content);

                var body = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    dynamic result = JsonConvert.DeserializeObject(body);
                    TempData["Message"] = result?.message ?? "Cập nhật nhân viên thành công.";
                    return RedirectToAction("Index");
                }
                else
                {
                    try
                    {
                        dynamic result = JsonConvert.DeserializeObject(body);
                        string message = result?.message ?? response.ReasonPhrase;
                        ModelState.AddModelError("", "Lỗi API: " + message);
                    }
                    catch
                    {
                        ModelState.AddModelError("", "Lỗi API: " + response.ReasonPhrase);
                    }
                }
            }

            await LoadRolesToViewBag();
            return View((dynamic)model);
        }

        // ---------------------------
        // 6. XÓA
        // ---------------------------
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Delete(int id)
        {
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(_apiBaseUrl);

                var response = await client.DeleteAsync($"NhanVien/{id}");
                var body = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    try
                    {
                        dynamic result = JsonConvert.DeserializeObject(body);
                        TempData["Message"] = result?.message ?? "Đã xóa nhân viên.";
                    }
                    catch
                    {
                        TempData["Message"] = "Đã xóa nhân viên.";
                    }
                }
                else
                {
                    try
                    {
                        dynamic result = JsonConvert.DeserializeObject(body);
                        TempData["Message"] = "Lỗi xóa nhân viên: " + (result?.message ?? response.ReasonPhrase);
                    }
                    catch
                    {
                        TempData["Message"] = "Lỗi xóa nhân viên: " + response.ReasonPhrase;
                    }
                }
            }

            return RedirectToAction("Index");
        }

        // ---------------------------
        // HELPER: Load VaiTro cho dropdown
        // ---------------------------
        private async Task LoadRolesToViewBag()
        {
            var roles = new List<VaiTroViewModel>();

            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(_apiBaseUrl);

                var response = await client.GetAsync("VaiTro");
                if (response.IsSuccessStatusCode)
                {
                    var data = await response.Content.ReadAsStringAsync();
                    roles = JsonConvert.DeserializeObject<List<VaiTroViewModel>>(data);
                }
            }

            var selectList = roles
                .Select(r => new SelectListItem
                {
                    Value = r.MaVaiTro.ToString(),
                    Text = string.IsNullOrEmpty(r.TenVaiTro)
                        ? $"Vai trò #{r.MaVaiTro}"
                        : r.TenVaiTro
                })
                .ToList();

            ViewBag.Roles = selectList;
        }
    }
}
