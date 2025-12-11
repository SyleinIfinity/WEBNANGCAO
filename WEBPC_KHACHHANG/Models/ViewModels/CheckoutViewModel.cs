using System.Collections.Generic;
using WEBPC_KHACHHANG.Models.Responses; // Chứa SoDiaChiResponse

namespace WEBPC_KHACHHANG.Models.ViewModels
{
    public class CheckoutViewModel
    {
        // 1. Dữ liệu giỏ hàng (Danh sách sản phẩm, tổng tiền)
        public CartViewModel Cart { get; set; }

        // 2. Danh sách địa chỉ của khách hàng để hiển thị radio button chọn
        public List<SoDiaChiResponse> Addresses { get; set; }

        // 3. Thông tin người dùng (để hiển thị tên, email mặc định nếu cần)
        public UserLoginResponse User { get; set; }
    }
}