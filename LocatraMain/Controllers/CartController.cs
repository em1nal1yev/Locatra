using LocatraMain.DAL;
using LocatraMain.Models;
using LocatraMain.Models.Sale;
using LocatraMain.ViewModels.Sale;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Stripe;
using Stripe.Checkout;

namespace LocatraMain.Controllers
{
    [Authorize]
    public class CartController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IConfiguration _configuration;

        public CartController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, IConfiguration configuration)
        {
            _context = context;
            _userManager = userManager;
            _configuration = configuration;
        }

        
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            var cartItems = await _context.CartItems
                .Include(c => c.Product)
                    .ThenInclude(p => p.Images)
                .Where(c => c.UserId == user.Id)
                .ToListAsync();

            return View(cartItems);
        }

        
        [HttpPost]
        public async Task<IActionResult> Remove(int id)
        {
            var item = await _context.CartItems.FindAsync(id);
            if (item != null)
            {
                _context.CartItems.Remove(item);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        
        [HttpGet]
        public async Task<IActionResult> Checkout()
        {
            var user = await _userManager.GetUserAsync(User);
            var cartItems = await _context.CartItems
                .Include(c => c.Product)
                .Where(c => c.UserId == user.Id)
                .ToListAsync();

            var vm = new CheckoutViewModel
            {
                CartItems = cartItems
            };

            ViewBag.PublishableKey = _configuration["Stripe:PublishableKey"];
            return View(vm);
        }

        
        [HttpPost]
        public async Task<IActionResult> CreateStripeSession([FromBody] CheckoutViewModel vm)
        {
            var user = await _userManager.GetUserAsync(User);
            var cartItems = await _context.CartItems
                .Include(c => c.Product)
                .ThenInclude(p => p.CreatedBy)
                .Where(c => c.UserId == user.Id)
                .ToListAsync();

            if (!cartItems.Any())
                return BadRequest("Səbət boşdur.");

            var domain = Request.Scheme + "://" + Request.Host.Value;
            var options = new SessionCreateOptions
            {
                PaymentMethodTypes = new List<string> { "card" },
                LineItems = cartItems.Select(item => new SessionLineItemOptions
                {
                    PriceData = new SessionLineItemPriceDataOptions
                    {
                        UnitAmount = (long)(item.Product.Price * 100),
                        Currency = "azn",
                        ProductData = new SessionLineItemPriceDataProductDataOptions
                        {
                            Name = item.Product.Name
                        }
                    },
                    Quantity = item.Quantity
                }).ToList(),
                Mode = "payment",
                SuccessUrl = domain + Url.Action("PaymentSuccess", "Cart", new { address = vm.Address }),
                CancelUrl = domain + Url.Action("Index", "Cart")
            };

            var service = new SessionService();
            var session = service.Create(options);

            return Json(new { id = session.Id });
        }

        
        public async Task<IActionResult> PaymentSuccess(string address)
        {
            var user = await _userManager.GetUserAsync(User);
            var cartItems = await _context.CartItems
                .Include(c => c.Product)
                .ThenInclude(p => p.CreatedBy)
                .Where(c => c.UserId == user.Id)
                .ToListAsync();

            foreach (var item in cartItems)
            {
                var seller = item.Product.CreatedBy;
                Console.WriteLine($"🔔 Bildiriş: '{item.Product.Name}' adlı məhsul {user.Name} {user.Surname} tərəfindən satın alındı. Ünvan: {address}");
            }

            _context.CartItems.RemoveRange(cartItems);
            await _context.SaveChangesAsync();

            TempData["Message"] = "Ödəniş uğurla tamamlandı. Təşəkkür edirik!";
            return RedirectToAction(nameof(Index));
        }

        public IActionResult Success()
        {
            return View();
        }

    }
}
