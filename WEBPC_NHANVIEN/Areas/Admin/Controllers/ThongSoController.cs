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
        // Lấy URL API gốc từ Web.config
        private readonly string _apiBaseUrl = ConfigurationManager.AppSettings["ApiBaseUrl"];
        // Giả sử config là: https://webapi-1-qldr.onrender.com/api/

        // 1. LẤY DANH SÁCH THÔNG SỐ THEO ID SẢN PHẨM
        [HttpGet]
        public async Task<ActionResult> GetByProduct(int productId)
        {
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(_apiBaseUrl);
                // Giả định API endpoint là: api/ThongSoKyThuat/sanpham/{id}
                var response = await client.GetAsync($"ThongSoKyThuat/sanpham/{productId}");

                if (response.IsSuccessStatusCode)
                {
                    var data = await response.Content.ReadAsStringAsync();
                    // Trả về JSON nguyên bản cho View xử lý
                    return Content(data, "application/json");
                }

                // Nếu API chưa có dữ liệu hoặc lỗi, trả về mảng rỗng
                return Json(new List<object>(), JsonRequestBehavior.AllowGet);
            }
        }

        // 2. THÊM MỚI THÔNG SỐ
        [HttpPost]
        public async Task<ActionResult> Create(ThongSoViewModel model)
        {
            try
            {
                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri(_apiBaseUrl);
                    var json = JsonConvert.SerializeObject(model);
                    var content = new StringContent(json, Encoding.UTF8, "application/json");

                    // Gọi API POST: api/ThongSoKyThuat
                    var response = await client.PostAsync("ThongSoKyThuat", content);

                    if (response.IsSuccessStatusCode)
                    {
                        return Json(new { success = true, message = "Thêm thông số thành công!" });
                    }
                    else
                    {
                        return Json(new { success = false, error = "Lỗi API: " + response.ReasonPhrase });
                    }
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
            try
            {
                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri(_apiBaseUrl);
                    var json = JsonConvert.SerializeObject(model);
                    var content = new StringContent(json, Encoding.UTF8, "application/json");

                    // Gọi API PUT: api/ThongSoKyThuat/{id}
                    var response = await client.PutAsync($"ThongSoKyThuat/{model.MaThongSo}", content);

                    if (response.IsSuccessStatusCode)
                    {
                        return Json(new { success = true, message = "Cập nhật thành công!" });
                    }
                    else
                    {
                        return Json(new { success = false, error = "Lỗi API: " + response.ReasonPhrase });
                    }
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
            try
            {
                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri(_apiBaseUrl);

                    // Gọi API DELETE: api/ThongSoKyThuat/{id}
                    var response = await client.DeleteAsync($"ThongSoKyThuat/{id}");

                    if (response.IsSuccessStatusCode)
                    {
                        return Json(new { success = true, message = "Xóa thành công!" });
                    }
                    else
                    {
                        return Json(new { success = false, error = "Lỗi API: " + response.ReasonPhrase });
                    }
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = ex.Message });
            }
        }
    }
}