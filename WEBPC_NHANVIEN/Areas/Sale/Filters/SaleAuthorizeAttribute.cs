using System;
using System.Web;
using System.Web.Mvc;

namespace WEBPC_NHANVIEN.Areas.Sale.Filters
{
    public class SaleAuthorizeAttribute : AuthorizeAttribute
    {
        protected override bool AuthorizeCore(HttpContextBase httpContext)
        {
            if (httpContext == null) return false;

            var maVaiTro = httpContext.Session["RoleId"];
            if (maVaiTro == null) return false;

            // Chỉ cho Sale (MaVaiTro == 2) truy cập
            return Convert.ToInt32(maVaiTro) == 2;
        }

        protected override void HandleUnauthorizedRequest(AuthorizationContext filterContext)
        {
            // Nếu không phải Sale, đẩy về trang Login
            filterContext.Result = new RedirectToRouteResult(
                new System.Web.Routing.RouteValueDictionary(
                    new { controller = "Account", action = "Login", area = "" }
                )
            );
        }
    }
}