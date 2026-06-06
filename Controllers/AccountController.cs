using Microsoft.AspNetCore.Mvc;

namespace Webbanhang.Controllers
{
    public class AccountController : Controller
    {
        public IActionResult Login(string? returnUrl = null)
        {
            return Redirect($"/Identity/Account/Login?returnUrl={Uri.EscapeDataString(returnUrl ?? string.Empty)}");
        }

        public IActionResult Register(string? returnUrl = null)
        {
            return Redirect($"/Identity/Account/Register?returnUrl={Uri.EscapeDataString(returnUrl ?? string.Empty)}");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Logout()
        {
            return Redirect("/Identity/Account/Logout");
        }

        public IActionResult AccessDenied()
        {
            return Redirect("/Identity/Account/AccessDenied");
        }
    }
}
