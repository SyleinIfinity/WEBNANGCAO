using System;
using System.Collections.Generic;
using System.Configuration;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using Newtonsoft.Json;
using WEBPC_NHANVIEN.Areas.Admin.Models;

namespace WEBPC_NHANVIEN.Areas.Admin.Controllers
{
    public class ThongSoController : Controller
    {
        // Trong Web.config: <add key="ApiBaseUrl" value="https://webapi-1-qldr.onrender.com/api/" />
        private readonly string _apiBaseUrl = ConfigurationManager.AppSettings["ApiBaseUrl"];

        // 1. LẤY DANH SÁCH THÔNG SỐ THEO SẢN PHẨM
        [HttpGet]
        public async Task<ActionResult> GetByProduct(int productId)
        {
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(_apiBaseUrl);

                // Gọi API: GET api/ThongSoKyThuat/sanpham/{maSanPham}
                var response = await client.GetAsync($"ThongSoKyThuat/sanpham/{productId}");

                if (response.IsSuccessStatusCode)
                {
                    var data = await response.Content.ReadAsStringAsync();
                    // Trả nguyên JSON cho JS xử lý
                    return Content(data, "application/json");
                }

                // Trả về danh sách rỗng nếu lỗi
                return Json(new List<ThongSoViewModel>(), JsonRequestBehavior.AllowGet);
            }
        }

        // 2. THÊM THÔNG SỐ
        [HttpPost]
        public async Task<ActionResult> Create(ThongSoViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return Json(new
                {
                    success = false,
                    error = "Dữ liệu thông số không hợp lệ."
                });
            }

            try
            {
                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri(_apiBaseUrl);

                    // CreateThongSoRequest (API): MaSanPham, TenThongSo, GiaTri
                    var json = JsonConvert.SerializeObject(new
                    {
                        maSanPham = model.MaSanPham,
                        tenThongSo = model.TenThongSo,
                        giaTri = model.GiaTri
                    });

                    var content = new StringContent(json, Encoding.UTF8, "application/json");

                    var response = await client.PostAsync("ThongSoKyThuat", content);

                    if (response.IsSuccessStatusCode)
                    {
                        return Json(new { success = true, message = "Thêm thông số thành công!" });
                    }

                    return Json(new
                    {
                        success = false,
                        error = "Lỗi API: " + response.ReasonPhrase
                    });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = ex.Message });
            }
        }

        // 3. CẬP NHẬT THÔNG SỐ
        [HttpPost]
        public async Task<ActionResult> Update(ThongSoViewModel model)
        {
            if (model.MaThongSo <= 0)
            {
                return Json(new { success = false, error = "Thiếu mã thông số cần cập nhật." });
            }

            if (string.IsNullOrWhiteSpace(model.TenThongSo) ||
                string.IsNullOrWhiteSpace(model.GiaTri))
            {
                return Json(new
                {
                    success = false,
                    error = "Tên thông số và Giá trị là bắt buộc."
                });
            }

            try
            {
                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri(_apiBaseUrl);

                    // UpdateThongSoRequest (API): TenThongSo, GiaTri
                    var jsonBody = JsonConvert.SerializeObject(new
                    {
                        tenThongSo = model.TenThongSo,
                        giaTri = model.GiaTri
                    });

                    var content = new StringContent(jsonBody, Encoding.UTF8, "application/json");

                    var request = new HttpRequestMessage(
                        new HttpMethod("PATCH"),
                        $"ThongSoKyThuat/{model.MaThongSo}")
                    {
                        Content = content
                    };

                    var response = await client.SendAsync(request);

                    if (response.IsSuccessStatusCode)
                    {
                        return Json(new { success = true, message = "Cập nhật thông số thành công!" });
                    }

                    return Json(new
                    {
                        success = false,
                        error = "Lỗi API: " + response.ReasonPhrase
                    });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = ex.Message });
            }
        }

        // 4. XÓA THÔNG SỐ
        [HttpPost]
        public async Task<ActionResult> Delete(int id)
        {
            if (id <= 0)
            {
                return Json(new { success = false, error = "Thiếu mã thông số cần xóa." });
            }

            try
            {
                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri(_apiBaseUrl);

                    var response = await client.DeleteAsync($"ThongSoKyThuat/{id}");

                    if (response.IsSuccessStatusCode)
                    {
                        return Json(new { success = true, message = "Xóa thông số thành công!" });
                    }

                    return Json(new
                    {
                        success = false,
                        error = "Lỗi API: " + response.ReasonPhrase
                    });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = ex.Message });
            }
        }
    }
}
