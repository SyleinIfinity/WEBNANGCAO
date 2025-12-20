using System;
using System.Web;
using System.Web.Mvc;

namespace WEBPC_NHANVIEN.Areas.Admin.Filters
{
    public class AdminAuthorizeAttribute : AuthorizeAttribute
    {
        protected override bool AuthorizeCore(HttpContextBase httpContext)
        {
            if (httpContext == null) return false;

            // Kiểm tra đã đăng nhập chưa
            var maVaiTro = httpContext.Session["RoleId"];
            if (maVaiTro == null) return false;

            // Chỉ cho Admin (MaVaiTro == 1)
            return Convert.ToInt32(maVaiTro) == 1;
        }

        protected override void HandleUnauthorizedRequest(AuthorizationContext filterContext)
        {
            filterContext.Result = new RedirectToRouteResult(
                new System.Web.Routing.RouteValueDictionary(
                    new
                    {
                        controller = "Account",
                        action = "Login",
                        area = ""
                    }
                )
            );
        }
    }
}
