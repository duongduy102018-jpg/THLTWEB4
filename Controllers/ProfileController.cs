using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Webbanhang.Models;

namespace Webbanhang.Controllers
{
    [Authorize]
    public class ProfileController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IWebHostEnvironment _environment;

        public ProfileController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IWebHostEnvironment environment)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _environment = environment;
        }

        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Challenge();
            }

            var model = new ProfileViewModel
            {
                FullName = user.FullName,
                Address = user.Address,
                Age = user.Age,
                PhoneNumber = user.PhoneNumber,
                Email = user.Email,
                AvatarUrl = user.AvatarUrl
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(ProfileViewModel model)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Challenge();
            }

            model.Email = user.Email;
            model.AvatarUrl = user.AvatarUrl;

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            user.FullName = model.FullName.Trim();
            user.Address = model.Address?.Trim();
            user.Age = model.Age?.Trim();
            user.PhoneNumber = model.PhoneNumber?.Trim();

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
                return View(model);
            }

            await _signInManager.RefreshSignInAsync(user);
            TempData["Success"] = "Đã cập nhật hồ sơ cá nhân.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UploadAvatar(IFormFile? avatar)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Challenge();
            }

            if (avatar == null || avatar.Length == 0)
            {
                TempData["Error"] = "Vui lòng chọn ảnh đại diện.";
                return RedirectToAction(nameof(Index));
            }

            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp", ".gif" };
            var extension = Path.GetExtension(avatar.FileName).ToLowerInvariant();
            if (!allowedExtensions.Contains(extension))
            {
                TempData["Error"] = "Chỉ hỗ trợ ảnh .jpg, .jpeg, .png, .webp hoặc .gif.";
                return RedirectToAction(nameof(Index));
            }

            if (avatar.Length > 2 * 1024 * 1024)
            {
                TempData["Error"] = "Ảnh đại diện không được vượt quá 2MB.";
                return RedirectToAction(nameof(Index));
            }

            var avatarFolder = Path.Combine(_environment.WebRootPath, "avatars");
            Directory.CreateDirectory(avatarFolder);

            var safeFileName = $"avatar-{user.Id}-{Guid.NewGuid():N}{extension}";
            var savePath = Path.Combine(avatarFolder, safeFileName);

            using (var stream = new FileStream(savePath, FileMode.Create))
            {
                await avatar.CopyToAsync(stream);
            }

            user.AvatarUrl = "/avatars/" + safeFileName;
            await _userManager.UpdateAsync(user);
            await _signInManager.RefreshSignInAsync(user);

            TempData["Success"] = "Đã cập nhật ảnh đại diện.";
            return RedirectToAction(nameof(Index));
        }

        public IActionResult ChangePassword()
        {
            return View(new ChangePasswordViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Challenge();
            }

            var result = await _userManager.ChangePasswordAsync(user, model.OldPassword, model.NewPassword);
            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
                return View(model);
            }

            await _signInManager.RefreshSignInAsync(user);
            TempData["Success"] = "Đổi mật khẩu thành công.";
            return RedirectToAction(nameof(Index));
        }
    }
}
