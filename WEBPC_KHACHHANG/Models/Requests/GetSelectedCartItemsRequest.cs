using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WEBPC_KHACHHANG.Models.Requests
{
    public class GetSelectedCartItemsRequest
    {
        public int MaKhachHang { get; set; }
        public List<int> SelectedCartItemIds { get; set; }
    }
}