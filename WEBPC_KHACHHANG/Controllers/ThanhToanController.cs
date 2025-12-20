using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using WEBPC_KHACHHANG.Models.Requests;
using WEBPC_KHACHHANG.Models.Responses;
using WEBPC_KHACHHANG.Models.ViewModels;

namespace WEBPC_KHACHHANG.Controllers
{
    public class ThanhToanController : Controller
    {
        private readonly string _apiBaseUrl = ConfigurationManager.AppSettings["ApiBaseUrl"];

        // 1. GET: Hiển thị trang thanh toán
        [HttpGet]
        public async Task<ActionResult> Checkout(string selectedIds)
        {
            // Kiểm tra đăng nhập
            var user = Session["User"] as UserLoginResponse;
            if (user == null) return RedirectToAction("Index", "Login", new { returnUrl = "/ThanhToan/Checkout" });

            if (string.IsNullOrEmpty(selectedIds))
            {
                TempData["Error"] = "Vui lòng chọn sản phẩm.";
                return RedirectToAction("Index", "Cart");
            }

            try
            {
                var model = new CheckoutViewModel
                {
                    // Gán thông tin User vào Model (nếu ViewModel có trường này)
                    // User = user, 
                    SelectedIdsString = selectedIds,
                    Cart = new CartViewModel(),
                    Addresses = new List<SoDiaChiResponse>(),
                    DanhSachKhuyenMai = new List<KhuyenMaiKhachHangResponse>() // Khởi tạo list rỗng
                };

                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri(_apiBaseUrl);
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", user.Token);

                    // A. LẤY GIỎ HÀNG & LỌC SẢN PHẨM ĐÃ CHỌN
                    decimal tongTienHang = 0;
                    var responseFullCart = await client.GetAsync($"GioHang/{user.MaKhachHang}");

                    if (responseFullCart.IsSuccessStatusCode)
                    {
                        var content = await responseFullCart.Content.ReadAsStringAsync();
                        var fullCart = JsonConvert.DeserializeObject<CartViewModel>(content);
                        var selectedCartItemIds = selectedIds.Split(',').Select(int.Parse).ToList();

                        if (fullCart != null && fullCart.Items != null)
                        {
                            model.Cart.Items = fullCart.Items
                                .Where(x => selectedCartItemIds.Contains(x.CartItemId))
                                .ToList();

                            // Tính tổng tiền hàng (Tạm tính)
                            tongTienHang = model.Cart.Items.Sum(x => x.Total);
                        }
                    }

                    // B. LẤY SỔ ĐỊA CHỈ
                    var responseAddr = await client.GetAsync($"SoDiaChi/khachhang/{user.MaKhachHang}");
                    if (responseAddr.IsSuccessStatusCode)
                    {
                        var dataAddr = await responseAddr.Content.ReadAsStringAsync();
                        model.Addresses = JsonConvert.DeserializeObject<List<SoDiaChiResponse>>(dataAddr);
                    }

                    // C. [MỚI] LẤY DANH SÁCH KHUYẾN MÃI CỦA KHÁCH HÀNG
                    var responseKM = await client.GetAsync($"KhuyenMaiKhachHang/khachhang/{user.MaKhachHang}");
                    if (responseKM.IsSuccessStatusCode)
                    {
                        var jsonKM = await responseKM.Content.ReadAsStringAsync();
                        var allVouchers = JsonConvert.DeserializeObject<List<KhuyenMaiKhachHangResponse>>(jsonKM);

                        // [LOGIC LỌC]: Sửa lại tên thuộc tính cho khớp với Model mới
                        model.DanhSachKhuyenMai = allVouchers.Where(km =>
                            km.DaSuDung == false &&                 // Chưa dùng
                            km.NgayKetThuc > DateTime.Now &&        // Chưa hết hạn
                            km.NgayBatDau <= DateTime.Now &&        // Đã bắt đầu
                            km.DonHangToiThieu <= tongTienHang      // [Sửa DonToiThieu -> DonHangToiThieu]
                        ).ToList();
                    }

                    // D. TÍNH TOÁN CÁC CON SỐ HIỂN THỊ BAN ĐẦU
                    model.TamTinh = tongTienHang;
                    model.PhiVanChuyen = 30000; // Mặc định hoặc tính theo API
                    model.TongThanhToan = tongTienHang + model.PhiVanChuyen;
                }

                return View(model);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Lỗi kết nối: " + ex.Message;
                return RedirectToAction("Index", "Cart");
            }
        }

        // 2. POST: Xử lý đặt hàng
        [HttpPost]
        [ValidateAntiForgeryToken]
        // Thêm tham số MaCodeVoucher để nhận từ Input Hidden
        public async Task<ActionResult> PlaceOrder(CheckoutViewModel model, string MaCodeVoucher)
        {
            var user = Session["User"] as UserLoginResponse;
            if (user == null) return RedirectToAction("Index", "Login");

            try
            {
                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri(_apiBaseUrl);
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", user.Token);

                    // Tạo Request gửi đi
                    // Tạo Request gửi đi
                    var request = new TaoDonHangRequest
                    {
                        MaKhachHang = user.MaKhachHang,
                        NguoiNhan = model.NguoiNhan,
                        SoDienThoai = model.SoDienThoai,
                        DiaChiGiaoHang = model.DiaChiGiaoHang,
                        PhuongThucThanhToan = model.PhuongThucThanhToan,
                        SelectedCartItemIds = new List<int>(),

                        MaCodeVoucher = MaCodeVoucher,

                        // [MỚI] Gửi phí vận chuyển (Hardcode 30k hoặc lấy từ Model nếu có logic tính)
                        PhiVanChuyen = 30000
                    };

                    // Lọc CartItemId lại để đảm bảo an toàn dữ liệu
                    if (!string.IsNullOrEmpty(model.SelectedIdsString))
                    {
                        var listIds = model.SelectedIdsString.Split(',').Select(int.Parse).ToList();
                        var cartResponse = await client.GetAsync($"GioHang/{user.MaKhachHang}");

                        if (cartResponse.IsSuccessStatusCode)
                        {
                            var cartContent = await cartResponse.Content.ReadAsStringAsync();
                            var fullCart = JsonConvert.DeserializeObject<CartViewModel>(cartContent);

                            if (fullCart != null && fullCart.Items != null)
                            {
                                var selectedItems = fullCart.Items
                                    .Where(x => listIds.Contains(x.CartItemId))
                                    .ToList();

                                foreach (var item in selectedItems)
                                {
                                    request.SelectedCartItemIds.Add(item.CartItemId);
                                }
                            }
                        }
                    }

                    // Kiểm tra danh sách trước khi gửi
                    if (request.SelectedCartItemIds.Count == 0)
                    {
                        TempData["Error"] = "Lỗi dữ liệu: Không tìm thấy sản phẩm trong giỏ hàng.";
                        return RedirectToAction("Index", "Cart");
                    }

                    // Gọi API Tạo Đơn Hàng
                    var content = new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json");
                    var response = await client.PostAsync("DonHang/create", content);

                    if (response.IsSuccessStatusCode)
                    {
                        var responseString = await response.Content.ReadAsStringAsync();
                        dynamic result = JsonConvert.DeserializeObject(responseString);
                        int maDonHang = result.maDonHang;

                        if (model.PhuongThucThanhToan == "VietQR")
                            return RedirectToAction("Payment", new { id = maDonHang });
                        else
                            return RedirectToAction("Success", new { id = maDonHang });
                    }
                    else
                    {
                        var errorContent = await response.Content.ReadAsStringAsync();
                        // Parse lỗi cho đẹp nếu Server trả về JSON
                        try
                        {
                            dynamic errObj = JsonConvert.DeserializeObject(errorContent);
                            TempData["Error"] = "Đặt hàng thất bại: " + errObj.message;
                        }
                        catch
                        {
                            TempData["Error"] = "Đặt hàng thất bại: " + errorContent;
                        }

                        // Quan trọng: Truyền lại selectedIds để không bị đá về Cart
                        return RedirectToAction("Checkout", new { selectedIds = model.SelectedIdsString });
                    }
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Lỗi hệ thống: " + ex.Message;
                return RedirectToAction("Index", "Cart");
            }
        }

        // Action Payment (GIỮ NGUYÊN)
        [HttpGet]
        public async Task<ActionResult> Payment(int id)
        {
            var user = Session["User"] as UserLoginResponse;
            if (user == null) return RedirectToAction("Index", "Login");
            try
            {
                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri(_apiBaseUrl);
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", user.Token);
                    var response = await client.GetAsync($"Payment/get-qr/{id}");
                    if (response.IsSuccessStatusCode)
                    {
                        var jsonString = await response.Content.ReadAsStringAsync();
                        var apiResult = JsonConvert.DeserializeObject<ApiQrResponse>(jsonString);
                        if (apiResult != null && apiResult.data != null)
                        {
                            ViewBag.QrImage = apiResult.data.qrDataURL;
                            ViewBag.OrderId = id;
                            return View();
                        }
                    }
                }
                return RedirectToAction("Success", new { id = id });
            }
            catch { return RedirectToAction("Success", new { id = id }); }
        }

        // Action Success (GIỮ NGUYÊN)
        public ActionResult Success(int id)
        {
            ViewBag.OrderId = id;
            return View();
        }

        // Action CheckStatus (GIỮ NGUYÊN)
        [HttpGet]
        public async Task<JsonResult> CheckStatus(int orderId)
        {
            try
            {
                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri(_apiBaseUrl);
                    var response = await client.GetAsync($"Payment/check-status/{orderId}");
                    if (response.IsSuccessStatusCode)
                    {
                        var content = await response.Content.ReadAsStringAsync();
                        return Json(new { status = content.Trim('"') }, JsonRequestBehavior.AllowGet);
                    }
                }
                return Json(new { status = "ERROR" }, JsonRequestBehavior.AllowGet);
            }
            catch { return Json(new { status = "ERROR" }, JsonRequestBehavior.AllowGet); }
        }
    }
}