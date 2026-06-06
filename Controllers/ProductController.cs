using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Text.Json;
using Webbanhang.Helpers;
using Webbanhang.Models;
using Webbanhang.Repositories;

namespace Webbanhang.Controllers
{
    public class ProductController : Controller
    {
        private readonly IProductRepository _productRepository;
        private readonly ICategoryRepository _categoryRepository;

        public ProductController(
            IProductRepository productRepository,
            ICategoryRepository categoryRepository)
        {
            _productRepository = productRepository;
            _categoryRepository = categoryRepository;
        }

        public async Task<IActionResult> Index(
            string? searchTerm,
            int? categoryId,
            string? sortOrder,
            decimal? minPrice,
            decimal? maxPrice)
        {
            var products = await _productRepository.GetAllAsync();
            var categories = await _categoryRepository.GetAllAsync();

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                products = products.Where(p =>
                    p.Name.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)
                    || (p.Description != null &&
                        p.Description.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)));
            }

            if (categoryId.HasValue && categoryId.Value > 0)
            {
                products = products.Where(p => p.CategoryId == categoryId.Value);
            }

            if (minPrice.HasValue && minPrice.Value > 0)
            {
                products = products.Where(p => p.Price >= minPrice.Value);
            }

            if (maxPrice.HasValue && maxPrice.Value > 0)
            {
                products = products.Where(p => p.Price <= maxPrice.Value);
            }

            products = sortOrder switch
            {
                "price_asc" => products.OrderBy(p => p.Price),
                "price_desc" => products.OrderByDescending(p => p.Price),
                "name_desc" => products.OrderByDescending(p => p.Name),
                _ => products.OrderBy(p => p.Name)
            };

            return View(new ProductListViewModel
            {
                Products = products,
                Categories = categories,
                SearchTerm = searchTerm,
                CategoryId = categoryId,
                SortOrder = sortOrder,
                MinPrice = minPrice,
                MaxPrice = maxPrice
            });
        }

        public async Task<IActionResult> Display(int id)
        {
            var product = await _productRepository.GetByIdAsync(id);

            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        [Authorize(Roles = SD.Role_Admin)]
        public async Task<IActionResult> Add()
        {
            var categories = await _categoryRepository.GetAllAsync();
            ViewBag.Categories = new SelectList(categories, "Id", "Name");

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = SD.Role_Admin)]
        public async Task<IActionResult> Add(Product product, IFormFile? imageUrl)
        {
            ModelState.Remove("ImageUrl");
            ModelState.Remove("Category");
            ModelState.Remove("Images");

            if (ModelState.IsValid)
            {
                if (imageUrl != null && imageUrl.Length > 0)
                {
                    product.ImageUrl = await SaveImage(imageUrl);
                }

                if (string.IsNullOrWhiteSpace(product.Description))
                {
                    product.Description = "Trái cây tươi ngon, được tuyển chọn kỹ lưỡng mỗi ngày tại HTP Food.";
                }

                if (string.IsNullOrWhiteSpace(product.ImageUrl))
                {
                    product.ImageUrl = "/images/khay-thap-cam.jpg";
                }

                await _productRepository.AddAsync(product);

                TempData["Success"] = "Đã thêm sản phẩm mới.";
                return RedirectToAction(nameof(Index));
            }

            var categories = await _categoryRepository.GetAllAsync();
            ViewBag.Categories = new SelectList(categories, "Id", "Name", product.CategoryId);

            return View(product);
        }

        [Authorize(Roles = SD.Role_Admin)]
        public async Task<IActionResult> Update(int id)
        {
            var product = await _productRepository.GetByIdAsync(id);

            if (product == null)
            {
                return NotFound();
            }

            var categories = await _categoryRepository.GetAllAsync();
            ViewBag.Categories = new SelectList(categories, "Id", "Name", product.CategoryId);

            return View(product);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = SD.Role_Admin)]
        public async Task<IActionResult> Update(int id, Product product, IFormFile? imageUrl)
        {
            ModelState.Remove("ImageUrl");
            ModelState.Remove("Category");
            ModelState.Remove("Images");

            if (id != product.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                var existingProduct = await _productRepository.GetByIdAsync(id);

                if (existingProduct == null)
                {
                    return NotFound();
                }

                existingProduct.Name = product.Name;
                existingProduct.Price = product.Price;
                existingProduct.Description = string.IsNullOrWhiteSpace(product.Description)
                    ? "Trái cây tươi ngon, được tuyển chọn kỹ lưỡng mỗi ngày tại HTP Food."
                    : product.Description;
                existingProduct.CategoryId = product.CategoryId;

                if (imageUrl != null && imageUrl.Length > 0)
                {
                    existingProduct.ImageUrl = await SaveImage(imageUrl);
                }

                await _productRepository.UpdateAsync(existingProduct);

                TempData["Success"] = "Đã cập nhật sản phẩm.";
                return RedirectToAction(nameof(Index));
            }

            var categories = await _categoryRepository.GetAllAsync();
            ViewBag.Categories = new SelectList(categories, "Id", "Name", product.CategoryId);

            return View(product);
        }

        [Authorize(Roles = SD.Role_Admin)]
        public async Task<IActionResult> Delete(int id)
        {
            var product = await _productRepository.GetByIdAsync(id);

            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        [HttpPost, ActionName("DeleteConfirmed")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = SD.Role_Admin)]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _productRepository.DeleteAsync(id);

            TempData["Success"] = "Đã xóa sản phẩm.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ToggleFavorite(int id)
        {
            var favorites = GetFavorites();

            if (favorites.Contains(id))
            {
                favorites.Remove(id);
                TempData["Success"] = "Đã bỏ sản phẩm khỏi danh sách yêu thích.";
            }
            else
            {
                favorites.Add(id);
                TempData["Success"] = "Đã thêm sản phẩm vào danh sách yêu thích.";
            }

            SaveFavorites(favorites);

            var referer = Request.Headers.Referer.ToString();

            return string.IsNullOrWhiteSpace(referer)
                ? RedirectToAction(nameof(Index))
                : Redirect(referer);
        }

        public async Task<IActionResult> Favorites()
        {
            var favoriteIds = GetFavorites();
            var products = await _productRepository.GetAllAsync();

            return View(products.Where(p => favoriteIds.Contains(p.Id)).ToList());
        }

        private List<int> GetFavorites()
        {
            var json = HttpContext.Session.GetString("HTPFoodFavorites");

            return string.IsNullOrWhiteSpace(json)
                ? new List<int>()
                : JsonSerializer.Deserialize<List<int>>(json) ?? new List<int>();
        }

        private void SaveFavorites(List<int> favorites)
        {
            HttpContext.Session.SetString(
                "HTPFoodFavorites",
                JsonSerializer.Serialize(favorites.Distinct().ToList()));
        }

        private async Task<string> SaveImage(IFormFile image)
        {
            var uploadsFolder = Path.Combine(
                Directory.GetCurrentDirectory(),
                "wwwroot",
                "images");

            Directory.CreateDirectory(uploadsFolder);

            var extension = Path.GetExtension(image.FileName);
            var safeFileName = $"product-{Guid.NewGuid():N}{extension}";
            var savePath = Path.Combine(uploadsFolder, safeFileName);

            using var fileStream = new FileStream(savePath, FileMode.Create);
            await image.CopyToAsync(fileStream);

            return "/images/" + safeFileName;
        }
    }
}