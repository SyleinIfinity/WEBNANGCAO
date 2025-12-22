namespace WEBPC_KHACHHANG.Models.Responses
{
    // Dùng để hứng kết quả tạo đơn
    public class CreateOrderResponse
    {
        public string message { get; set; }
        public int maDonHang { get; set; }
        public string maCode { get; set; }
        public decimal tongTien { get; set; }
    }

    // Dùng để hứng kết quả lấy mã QR
    public class VietQrResponse
    {
        public string qrDataURL { get; set; } // Link ảnh QR base64
        public string message { get; set; }
    }
}