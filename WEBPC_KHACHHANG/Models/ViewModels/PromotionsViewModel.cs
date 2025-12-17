using System.Collections.Generic;
using WEBPC_KHACHHANG.Models.Responses;

namespace WEBPC_KHACHHANG.Models.ViewModels
{
    public class PromotionsViewModel
    {
        public List<KhuyenMaiResponse> TatCaKhuyenMai { get; set; }

        // ✅ CHỈ LƯU DANH SÁCH ID MÃ ĐÃ LƯU
        public List<int> KhoKhuyenMaiDaLuu { get; set; }
    }
}
