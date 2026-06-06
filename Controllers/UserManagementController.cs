using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Webbanhang.Models;

namespace Webbanhang.Controllers
{
    [Authorize(Roles = SD.Role_Admin)]
    public class UserManagementController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public UserManagementController(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public async Task<IActionResult> Index()
        {
            var users = _userManager.Users.OrderBy(u => u.Email).ToList();
            var model = new List<UserManagementItemViewModel>();
            var now = DateTimeOffset.Now;

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                model.Add(new UserManagementItemViewModel
                {
                    Id = user.Id,
                    Email = user.Email ?? string.Empty,
                    FullName = user.FullName,
                    PhoneNumber = user.PhoneNumber,
                    AvatarUrl = user.AvatarUrl,
                    EmailConfirmed = user.EmailConfirmed,
                    LockoutEnd = user.LockoutEnd,
                    IsLocked = user.LockoutEnd.HasValue && user.LockoutEnd.Value > now,
                    Roles = roles
                });
            }

            return View(model);
        }

        public async Task<IActionResult> EditRole(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            var roles = await _userManager.GetRolesAsync(user);
            var model = new UserRoleEditViewModel
            {
                UserId = user.Id,
                Email = user.Email ?? string.Empty,
                FullName = user.FullName,
                SelectedRole = roles.FirstOrDefault() ?? SD.Role_User,
                RoleList = _roleManager.Roles
                    .OrderBy(r => r.Name)
                    .Select(r => new SelectListItem { Text = r.Name!, Value = r.Name! })
                    .ToList()
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditRole(UserRoleEditViewModel model)
        {
            var user = await _userManager.FindByIdAsync(model.UserId);
            if (user == null)
            {
                return NotFound();
            }

            if (string.IsNullOrWhiteSpace(model.SelectedRole) || !await _roleManager.RoleExistsAsync(model.SelectedRole))
            {
                ModelState.AddModelError(nameof(model.SelectedRole), "Vai trò không hợp lệ.");
                model.Email = user.Email ?? string.Empty;
                model.FullName = user.FullName;
                model.RoleList = _roleManager.Roles
                    .OrderBy(r => r.Name)
                    .Select(r => new SelectListItem { Text = r.Name!, Value = r.Name! })
                    .ToList();
                return View(model);
            }

            var currentRoles = await _userManager.GetRolesAsync(user);
            if (currentRoles.Any())
            {
                await _userManager.RemoveFromRolesAsync(user, currentRoles);
            }
            await _userManager.AddToRoleAsync(user, model.SelectedRole);

            TempData["Success"] = $"Đã cập nhật vai trò cho {user.Email}.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Lock(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            if (user.Email?.Equals("admin@htpfood.vn", StringComparison.OrdinalIgnoreCase) == true)
            {
                TempData["Error"] = "Không nên khóa tài khoản Admin mẫu.";
                return RedirectToAction(nameof(Index));
            }

            await _userManager.SetLockoutEnabledAsync(user, true);
            await _userManager.SetLockoutEndDateAsync(user, DateTimeOffset.Now.AddYears(100));
            TempData["Success"] = $"Đã khóa tài khoản {user.Email}.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Unlock(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            await _userManager.SetLockoutEndDateAsync(user, null);
            await _userManager.ResetAccessFailedCountAsync(user);
            TempData["Success"] = $"Đã mở khóa tài khoản {user.Email}.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfirmEmail(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            user.EmailConfirmed = true;
            await _userManager.UpdateAsync(user);
            TempData["Success"] = $"Đã xác thực email cho {user.Email}.";
            return RedirectToAction(nameof(Index));
        }
    }
}
