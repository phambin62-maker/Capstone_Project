using Microsoft.AspNetCore.Http;

namespace FE_Capstone_Project.Helpers
{
    public class AuthHelper
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AuthHelper(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        /// <summary>
        /// Kiểm tra user đã đăng nhập chưa
        /// </summary>
        public bool IsAuthenticated()
        {
            var token = _httpContextAccessor.HttpContext?.Session?.GetString("JwtToken");
            return !string.IsNullOrEmpty(token);
        }

        /// <summary>
        /// Lấy RoleId từ session
        /// </summary>
        public int? GetRoleId()
        {
            return _httpContextAccessor.HttpContext?.Session?.GetInt32("UserRoleId");
        }

        /// <summary>
        /// Lấy UserId từ session
        /// </summary>
        public int? GetUserId()
        {
            var userIdString = _httpContextAccessor.HttpContext?.Session?.GetString("UserId");
            if (int.TryParse(userIdString, out int userId))
                return userId;
            return null;
        }

        /// <summary>
        /// Lấy Username từ session
        /// </summary>
        public string? GetUsername()
        {
            return _httpContextAccessor.HttpContext?.Session?.GetString("UserName");
        }

        /// <summary>
        /// Lấy Token từ session
        /// </summary>
        public string? GetToken()
        {
            return _httpContextAccessor.HttpContext?.Session?.GetString("JwtToken");
        }

        /// <summary>
        /// Kiểm tra user có phải Admin không
        /// </summary>
        public bool IsAdmin()
        {
            return GetRoleId() == 1;
        }

        /// <summary>
        /// Kiểm tra user có phải Staff không
        /// </summary>
        public bool IsStaff()
        {
            return GetRoleId() == 2;
        }

        /// <summary>
        /// Kiểm tra user có phải Customer không
        /// </summary>
        public bool IsCustomer()
        {
            return GetRoleId() == 3;
        }

        /// <summary>
        /// Kiểm tra user có phải Admin hoặc Staff không
        /// </summary>
        public bool IsAdminOrStaff()
        {
            var roleId = GetRoleId();
            return roleId == 1 || roleId == 2;
        }

        /// <summary>
        /// Kiểm tra user có role trong danh sách roles được phép không
        /// </summary>
        public bool HasRole(params int[] allowedRoles)
        {
            var roleId = GetRoleId();
            if (roleId == null) return false;
            return allowedRoles.Contains(roleId.Value);
        }
    }
}

