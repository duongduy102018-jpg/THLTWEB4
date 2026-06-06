using Microsoft.AspNetCore.Mvc.Rendering;

namespace Webbanhang.Models
{
    public class UserManagementItemViewModel
    {
        public string Id { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
        public string? AvatarUrl { get; set; }
        public bool EmailConfirmed { get; set; }
        public bool IsLocked { get; set; }
        public DateTimeOffset? LockoutEnd { get; set; }
        public IList<string> Roles { get; set; } = new List<string>();
    }

    public class UserRoleEditViewModel
    {
        public string UserId { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string SelectedRole { get; set; } = string.Empty;
        public IEnumerable<SelectListItem> RoleList { get; set; } = new List<SelectListItem>();
    }
}
