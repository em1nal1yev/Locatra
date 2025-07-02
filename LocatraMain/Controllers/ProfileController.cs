using LocatraMain.DAL;
using LocatraMain.Models;
using LocatraMain.Models.Auction;
using LocatraMain.Models.Sale;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LocatraMain.Controllers
{
    [Authorize]
    public class ProfileController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public ProfileController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();

            var myProducts = await _context.Products
                .Include(p => p.Images)
                .Where(p => p.CreatedById == user.Id)
                .ToListAsync();

            var myReviews = await _context.Reviews
                .Include(r => r.Product)
                .Where(r => r.UserId == user.Id)
                .ToListAsync();

            var myAuctions = await _context.Auctions
                .Include(a => a.Images)
                .Where(a => a.CreatedById == user.Id)
                .ToListAsync();

            var myBids = await _context.Bids
                .Include(b => b.Auction)
                .Where(b => b.UserId == user.Id)
                .ToListAsync();

            var purchasedProducts = await _context.CartItems
                .Include(c => c.Product)
                .ThenInclude(p => p.Images)
                .Where(c => c.UserId == user.Id)
                .ToListAsync();

            var endedAuctions = await _context.Auctions
                .Include(a => a.Images)
                .Include(a => a.Bids)
                .Where(a => a.EndTime <= DateTime.Now )
                .ToListAsync();

            var wonAuctions = endedAuctions
                .Where(a => a.Bids.Any() &&
                    a.Bids.OrderByDescending(b => b.BidTime).FirstOrDefault()?.UserId == user.Id 
                    )
                .ToList();

            ViewBag.WonAuctions = wonAuctions;
            ViewBag.MyProducts = myProducts;
            ViewBag.MyReviews = myReviews;
            ViewBag.MyAuctions = myAuctions;
            ViewBag.MyBids = myBids;
            ViewBag.Purchased = purchasedProducts;
            ViewBag.CurrentUser = new
            {
                Name = user.Name,
                Email = user.Email
            };


            return View();
        }
    }
}
