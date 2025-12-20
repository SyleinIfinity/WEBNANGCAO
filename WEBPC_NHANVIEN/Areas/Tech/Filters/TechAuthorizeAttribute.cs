using System;
using System.Web;
using System.Web.Mvc;

namespace WEBPC_NHANVIEN.Areas.Tech.Filters
{
    public class TechAuthorizeAttribute : AuthorizeAttribute
    {
        protected override bool AuthorizeCore(HttpContextBase httpContext)
        {
            if (httpContext == null) return false;

            var maVaiTro = httpContext.Session["RoleId"];
            if (maVaiTro == null) return false;

            // Chỉ cho Tech (MaVaiTro == 3) truy cập
            return Convert.ToInt32(maVaiTro) == 3;
        }

        protected override void HandleUnauthorizedRequest(AuthorizationContext filterContext)
        {
            filterContext.Result = new RedirectToRouteResult(
                new System.Web.Routing.RouteValueDictionary(
                    new { controller = "Account", action = "Login", area = "" }
                )
            );
        }
    }
}