using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using WEBPC_KHACHHANG.Models.Responses;
using WEBPC_KHACHHANG.Models.ViewModels;

namespace WEBPC_KHACHHANG.Controllers
{
    public class PromotionsController : Controller
    {
        private readonly string _apiBaseUrl =
            System.Configuration.ConfigurationManager.AppSettings["ApiBaseUrl"];

        // =====================================================
        // 1. TRANG KHUYẾN MÃI
        // =====================================================
        public async Task<ActionResult> Index()
        {
            var tatCaKM = await GetAllKhuyenMaiFromApi();
            var khoDaLuu = Session["UserID"] == null
            ? new List<KhuyenMaiKhachHangResponse>()
            : await GetKhoKhuyenMaiDaLuuTuApi((int)Session["UserID"]);


            var now = DateTime.Now;

            foreach (var km in tatCaKM)
            {
   
                km.CoTheLuu =
                    km.SoLuongConLai > 0 &&
                    km.NgayBatDau <= now &&
                    km.NgayKetThuc >= now;
            }

            var vm = new PromotionsViewModel
            {
                TatCaKhuyenMai = tatCaKM,
                KhoKhuyenMaiDaLuu = khoDaLuu.Select(x => x.MaKhuyenMai).ToList()
            };

            return View(vm);
        }

        // =====================================================
        // 2. LƯU MÃ – PHẢI LOGIN (LƯU QUA API)
        // =====================================================
        [HttpPost]
        public async Task<ActionResult> LuuMaKhuyenMai(int maKhuyenMai)
        {
            if (Session["UserID"] == null)
            {
                TempData["RequireLogin"] = true;
                return RedirectToAction("Index");
            }

            int maKhachHang = (int)Session["UserID"];

            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(_apiBaseUrl);

                var body = new
                {
                    MaKhachHang = maKhachHang,
                    MaKhuyenMai = maKhuyenMai
                };

                var content = new StringContent(
                    JsonConvert.SerializeObject(body),
                    Encoding.UTF8,
                    "application/json"
                );

                var response = await client.PostAsync("KhuyenMaiKhachHang/collect", content);

                if (!response.IsSuccessStatusCode)
                {
                    var error = await response.Content.ReadAsStringAsync();
                    TempData["ErrorMessage"] =
                        JsonConvert.DeserializeObject<dynamic>(error)?.message;

                    return RedirectToAction("Index");
                }
            }

            return RedirectToAction("Index");
        }

        // =====================================================
        // 3. KHO MÃ ĐÃ LƯU – LOAD TỪ API
        // =====================================================
        public async Task<ActionResult> Saved()
        {
            if (Session["UserID"] == null)
                return RedirectToAction("Index");

            int maKhachHang = (int)Session["UserID"];

            var khoDaLuu = await GetKhoKhuyenMaiDaLuuTuApi(maKhachHang);
            var tatCaKM = await GetAllKhuyenMaiFromApi();

            var result =
                from k in khoDaLuu
                join km in tatCaKM on k.MaKhuyenMai equals km.MaKhuyenMai
                select new KhuyenMaiDaLuuViewModel
                {
                    MaKMKH = k.MaKMKH,          // ⭐ FIX LỖI XÓA
                    MaKhuyenMai = km.MaKhuyenMai,
                    MaCodeKM = km.MaCodeKM,
                    TenChuongTrinh = km.TenChuongTrinh,
                    LoaiGiam = km.LoaiGiam,
                    GiaTriGiam = km.GiaTriGiam,
                    DonHangToiThieu = km.DonHangToiThieu,
                    GiamToiDa = km.GiamToiDa,
                    NgayKetThuc = km.NgayKetThuc
                };

            return View(result.ToList());
        }



        // =====================================================
        // 4. XÓA MÃ (XÓA TRONG DB)
        // =====================================================
        [HttpPost]
        public async Task<ActionResult> XoaMaKhuyenMai(int maKMKH)
        {
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(_apiBaseUrl);
                await client.DeleteAsync($"KhuyenMaiKhachHang/{maKMKH}");
            }

            return RedirectToAction("Saved");
        }

        // =====================================================
        // 5. API – LẤY TẤT CẢ KHUYẾN MÃ
        // =====================================================
        private async Task<List<KhuyenMaiResponse>> GetAllKhuyenMaiFromApi()
        {
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(_apiBaseUrl);
                var response = await client.GetAsync("KhuyenMai");

                if (!response.IsSuccessStatusCode)
                    return new List<KhuyenMaiResponse>();

                var json = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<List<KhuyenMaiResponse>>(json);
            }
        }

        // =====================================================
        // 6. API – LẤY KHO MÃ THEO KHÁCH HÀNG
        // =====================================================
        private async Task<List<KhuyenMaiKhachHangResponse>>
        GetKhoKhuyenMaiDaLuuTuApi(int maKhachHang)
        {
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(_apiBaseUrl);

                var response = await client.GetAsync(
                    $"KhuyenMaiKhachHang/khachhang/{maKhachHang}"
                );

                if (!response.IsSuccessStatusCode)
                    return new List<KhuyenMaiKhachHangResponse>();

                var json = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<List<KhuyenMaiKhachHangResponse>>(json);
            }
        }

    }
}
