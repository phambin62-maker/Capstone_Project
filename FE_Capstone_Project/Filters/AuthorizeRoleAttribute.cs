using FE_Capstone_Project.Helpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace FE_Capstone_Project.Filters
{
    /// <summary>
    /// Attribute để bảo vệ controller/action theo role
    /// Sử dụng: [AuthorizeRole(1)] cho Admin, [AuthorizeRole(1, 2)] cho Admin và Staff
    /// </summary>
    public class AuthorizeRoleAttribute : Attribute, IAuthorizationFilter
    {
        private readonly int[] _allowedRoles;

        public AuthorizeRoleAttribute(params int[] allowedRoles)
        {
            _allowedRoles = allowedRoles;
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var authHelper = context.HttpContext.RequestServices
                .GetRequiredService<AuthHelper>();

            // Kiểm tra đã đăng nhập chưa
            if (!authHelper.IsAuthenticated())
            {
                context.Result = new RedirectToActionResult("Login", "AuthWeb", null);
                return;
            }

            // Kiểm tra role
            var roleId = authHelper.GetRoleId();
            if (roleId == null || !_allowedRoles.Contains(roleId.Value))
            {
                // Không có quyền truy cập
                context.Result = new RedirectToActionResult("Forbidden", "Home", null);
                return;
            }
        }
    }
}

