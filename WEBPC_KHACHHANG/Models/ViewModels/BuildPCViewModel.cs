using System.Collections.Generic;

namespace WEBPC_KHACHHANG.Models.ViewModels
{
    public class BuildPCViewModel
    {
        // Khởi tạo danh sách mặc định để tránh lỗi null
        public List<string> Categories { get; set; } = new List<string>();
    }
}