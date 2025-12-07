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
    /// <summary>
    /// Controller for managing Product Specifications (Thông số kỹ thuật)
    /// CREATE NEW FILE: Areas/Admin/Controllers/ThongSoController.cs
    /// API Endpoint: ThongSoKyThuat
    /// </summary>
    public class ThongSoController : Controller
    {
        private readonly string _apiBaseUrl = ConfigurationManager.AppSettings["ApiBaseUrl"];

        /// <summary>
        /// GET: Load all specifications for a product
        /// MVC Route: /Admin/ThongSo/GetByProduct?productId=123
        /// API Call: GET /ThongSoKyThuat/sanpham/{productId}
        /// </summary>
        [HttpGet]
        public async Task<JsonResult> GetByProduct(int productId)
        {
            try
            {
                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri(_apiBaseUrl);

                    // GET request to API
                    var response = await client.GetAsync($"ThongSoKyThuat/sanpham/{productId}");

                    if (response.IsSuccessStatusCode)
                    {
                        var data = await response.Content.ReadAsStringAsync();
                        var specifications = JsonConvert.DeserializeObject<List<ThongSoViewModel>>(data);
                        return Json(specifications, JsonRequestBehavior.AllowGet);
                    }

                    return Json(new { error = "Failed to load specifications", status = response.StatusCode }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// POST: Create a new specification
        /// MVC Route: /Admin/ThongSo/Create (POST)
        /// API Call: POST /ThongSoKyThuat
        /// </summary>
        [HttpPost]
        public async Task<JsonResult> Create(CreateThongSoViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return Json(new { success = false, error = "Dữ liệu không hợp lệ" });
            }

            try
            {
                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri(_apiBaseUrl);

                    var json = JsonConvert.SerializeObject(model);
                    var content = new StringContent(json, Encoding.UTF8, "application/json");

                    // POST request to API
                    var response = await client.PostAsync("ThongSoKyThuat", content);

                    if (response.IsSuccessStatusCode)
                    {
                        var data = await response.Content.ReadAsStringAsync();
                        var result = JsonConvert.DeserializeObject<ThongSoViewModel>(data);
                        return Json(new { success = true, data = result, message = "Thêm thông số thành công" });
                    }

                    return Json(new { success = false, error = "Không thể thêm thông số", status = response.StatusCode });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = ex.Message });
            }
        }

        /// <summary>
        /// POST: Update an existing specification
        /// MVC Route: /Admin/ThongSo/Update (POST)
        /// API Call: PUT /ThongSoKyThuat/{id}
        /// </summary>
        [HttpPost]
        public async Task<JsonResult> Update(UpdateThongSoViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return Json(new { success = false, error = "Dữ liệu không hợp lệ" });
            }

            try
            {
                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri(_apiBaseUrl);

                    var json = JsonConvert.SerializeObject(model);
                    var content = new StringContent(json, Encoding.UTF8, "application/json");

                    // PUT request to API (standard REST update)
                    var response = await client.PutAsync($"ThongSoKyThuat/{model.MaThongSo}", content);

                    if (response.IsSuccessStatusCode)
                    {
                        return Json(new { success = true, message = "Cập nhật thông số thành công" });
                    }

                    return Json(new { success = false, error = "Không thể cập nhật thông số", status = response.StatusCode });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = ex.Message });
            }
        }

        /// <summary>
        /// POST: Delete a specification
        /// MVC Route: /Admin/ThongSo/Delete (POST)
        /// API Call: DELETE /ThongSoKyThuat/{id}
        /// </summary>
        [HttpPost]
        public async Task<JsonResult> Delete(int id)
        {
            try
            {
                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri(_apiBaseUrl);

                    // DELETE request to API (standard REST delete)
                    var response = await client.DeleteAsync($"ThongSoKyThuat/{id}");

                    if (response.IsSuccessStatusCode)
                    {
                        return Json(new { success = true, message = "Xóa thông số thành công" });
                    }

                    return Json(new { success = false, error = "Không thể xóa thông số", status = response.StatusCode });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = ex.Message });
            }
        }
    }
}