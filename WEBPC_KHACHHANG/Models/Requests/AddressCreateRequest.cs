using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;

namespace WEBPC_KHACHHANG.Models.Requests
{
    public class AddressCreateRequest
    {
        // 1. Map MaSoDiaChi -> Id (để phục vụ sửa/xóa nếu cần, API Create không dùng nhưng API Update/Delete dùng id trên URL)
        public int? MaSoDiaChi { get; set; }

        // 2. SỬA LỖI QUAN TRỌNG: Map đúng tên trường API yêu cầu là "MaKhachHang"
        // (Trước đây map là "KhachHangId" nên bị lỗi)
        [JsonProperty("MaKhachHang")]
        public int MaKhachHang { get; set; }

        // --- CẢNH BÁO: API hiện tại KHÔNG CÓ 2 trường này. Gửi lên sẽ không được lưu. ---
        // Bạn nên giữ lại để validate form, nhưng cần báo Backend bổ sung thêm vào API.
        [Required(ErrorMessage = "Vui lòng nhập tên người nhận")]
        public string TenNguoiNhan { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập số điện thoại")]
        [RegularExpression(@"^[0-9]{10,11}$", ErrorMessage = "SĐT không hợp lệ")]
        public string SoDienThoai { get; set; }
        // --------------------------------------------------------------------------------

        // 3. Map đúng các trường ID địa điểm theo yêu cầu API
        [Required(ErrorMessage = "Vui lòng chọn Tỉnh/Thành")]
        [JsonProperty("TinhThanhId")]
        public string MaTinh { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn Quận/Huyện")]
        [JsonProperty("QuanHuyenId")]
        public string MaHuyen { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn Phường/Xã")]
        [JsonProperty("PhuongXaId")]
        public string MaXa { get; set; }

        // 4. Map đúng tên trường địa chỉ cụ thể
        [Required(ErrorMessage = "Vui lòng nhập địa chỉ cụ thể")]
        [JsonProperty("DiaChiCuThe")]
        public string DiaChiCuThe { get; set; }

        // Các trường tên hiển thị (Client dùng để hiện UI, không gửi cho API)
        [JsonIgnore] // Thêm cái này để không gửi dữ liệu thừa lên API
        public string TinhThanh { get; set; }
        [JsonIgnore]
        public string QuanHuyen { get; set; }
        [JsonIgnore]
        public string PhuongXa { get; set; }

        // 5. Map IsDefault -> MacDinh
        [JsonProperty("MacDinh")]
        public bool IsDefault { get; set; }
    }
}