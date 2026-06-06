namespace Webbanhang.Helpers
{
    public static class AuthSession
    {
        public const string UserNameKey = "HTPFoodUserName";
        public const string FullNameKey = "HTPFoodFullName";
        public const string RoleKey = "HTPFoodRole";
        public const string MyOrderIdsKey = "HTPFoodMyOrderIds";

        public static bool IsLoggedIn(HttpContext httpContext)
            => httpContext.User?.Identity?.IsAuthenticated == true
               || !string.IsNullOrWhiteSpace(httpContext.Session.GetString(UserNameKey));

        public static bool IsAdmin(HttpContext httpContext)
            => httpContext.User?.IsInRole("Admin") == true
               || string.Equals(httpContext.Session.GetString(RoleKey), "Admin", StringComparison.OrdinalIgnoreCase);

        public static bool IsUser(HttpContext httpContext)
            => httpContext.User?.IsInRole("User") == true
               || string.Equals(httpContext.Session.GetString(RoleKey), "User", StringComparison.OrdinalIgnoreCase);
    }
}
