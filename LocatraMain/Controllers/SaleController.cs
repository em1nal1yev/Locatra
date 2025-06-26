using LocatraMain.DAL;
using LocatraMain.Models;
using LocatraMain.Models.Sale;
using LocatraMain.ViewModels.Sale;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LocatraMain.Controllers
{
    [Authorize]
    public class SaleController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IWebHostEnvironment _env;

        public SaleController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, IWebHostEnvironment env)
        {
            _context = context;
            _userManager = userManager;
            _env = env;
        }


        [AllowAnonymous]
        public async Task<IActionResult> Index()
        {
            var products = await _context.Products
                .Include(p => p.Images)
                .Where(p => p.Status == ProductStatus.Active)
                .ToListAsync();

            return View(products);
        }

        public IActionResult Create()
        {
            if (TempData["Error"] != null)
            {
                ViewBag.Error = TempData["Error"];
            }
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ProductCreateViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Məlumatlar düzgün daxil edilməyib. Zəhmət olmasa boş və ya səhv sahələri yoxlayın.";
                return View(vm);
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                TempData["Error"] = "İstifadəçi tapılmadı.";
                return View(vm);
            }

            var product = new Product
            {
                Name = vm.Name,
                Description = vm.Description,
                Price = vm.Price,
                CreatedById = user.Id,
                Status = ProductStatus.Pending,
                Images = new List<ProductImage>()
            };

            var rootPath = Path.Combine(_env.WebRootPath, "uploads");
            if (!Directory.Exists(rootPath))
            {
                try
                {
                    Directory.CreateDirectory(rootPath);
                }
                catch (Exception ex)
                {
                    TempData["Error"] = $"Yükləmə qovluğu yaradılarkən xəta baş verdi: {ex.Message}";
                    return View(vm);
                }
            }

            try
            {
                foreach (var file in vm.Images)
                {
                    if (file == null || file.Length == 0)
                    {
                        TempData["Error"] = "Boş və ya keçərsiz şəkil yüklənib.";
                        return View(vm);
                    }

                    if (file.Length > 5 * 1024 * 1024)
                    {
                        TempData["Error"] = "Şəkil maksimum 5MB ola bilər.";
                        return View(vm);
                    }

                    var fileName = Guid.NewGuid() + Path.GetExtension(file.FileName);
                    var filePath = Path.Combine(rootPath, fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await file.CopyToAsync(stream);
                    }

                    product.Images.Add(new ProductImage { ImageUrl = "/uploads/" + fileName });
                }

                _context.Products.Add(product);
                await _context.SaveChangesAsync();

                TempData["Message"] = "Məhsul göndərildi. Admin təsdiq etdikdən sonra aktiv olacaq.";
                return RedirectToAction("MyProducts");
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Məhsul yaradılarkən xəta baş verdi: {ex.Message}";
                return View(vm);
            }
        }



        public async Task<IActionResult> MyProducts()
        {
            var user = await _userManager.GetUserAsync(User);
            var products = await _context.Products
                .Include(p => p.Images)
                .Where(p => p.CreatedById == user.Id)
                .ToListAsync();

            return View(products);
        }


        [AllowAnonymous]
        public async Task<IActionResult> Details(int id)
        {
            var product = await _context.Products
                .Include(p => p.Images)
                .Include(p => p.Reviews)
                    .ThenInclude(r => r.User)
                .FirstOrDefaultAsync(p => p.Id == id && p.Status == ProductStatus.Active);

            if (product == null) return NotFound();
            return View(product);
        }

        // ✅ Rəy əlavə et
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddReview(ReviewCreateViewModel vm)
        {
            if (!ModelState.IsValid) return RedirectToAction("Details", new { id = vm.ProductId });

            var user = await _userManager.GetUserAsync(User);

            var review = new Review
            {
                ProductId = vm.ProductId,
                Rating = vm.Rating,
                Comment = vm.Comment,
                UserId = user.Id,
                CreatedAt = DateTime.Now
            };

            _context.Reviews.Add(review);
            await _context.SaveChangesAsync();

            return RedirectToAction("Details", new { id = vm.ProductId });
        }

        // ✅ Wishlist əlavə/sil
        [HttpPost]
        public async Task<IActionResult> ToggleWishlist(int productId)
        {
            var user = await _userManager.GetUserAsync(User);
            var existing = await _context.WishlistItems
                .FirstOrDefaultAsync(w => w.ProductId == productId && w.UserId == user.Id);

            if (existing != null)
            {
                _context.WishlistItems.Remove(existing);
            }
            else
            {
                _context.WishlistItems.Add(new WishlistItem
                {
                    ProductId = productId,
                    UserId = user.Id
                });
            }

            await _context.SaveChangesAsync();
            return Ok();
        }

        // ✅ Səbətə əlavə et
        [HttpPost]
        public async Task<IActionResult> AddToCart(int productId, int quantity)
        {
            var user = await _userManager.GetUserAsync(User);
            var existing = await _context.CartItems
                .FirstOrDefaultAsync(c => c.ProductId == productId && c.UserId == user.Id);

            if (existing != null)
            {
                existing.Quantity += quantity;
            }
            else
            {
                _context.CartItems.Add(new CartItem
                {
                    ProductId = productId,
                    UserId = user.Id,
                    Quantity = quantity
                });
            }

            await _context.SaveChangesAsync();
            return Ok();
        }
    }
}
