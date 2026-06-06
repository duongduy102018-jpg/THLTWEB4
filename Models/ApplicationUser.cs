using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace Webbanhang.Models
{
    public class ApplicationUser : IdentityUser
    {
        [Required]
        public string FullName { get; set; } = string.Empty;

        public string? Address { get; set; }

        public string? Age { get; set; }

        public string? AvatarUrl { get; set; }
    }
}
