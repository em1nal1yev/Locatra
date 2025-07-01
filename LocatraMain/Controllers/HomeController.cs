using LocatraMain.DAL;
using LocatraMain.Models.Auction;
using LocatraMain.Models.Sale;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LocatraMain.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var auctions = await _context.Auctions
                .Include(a => a.Images)
                .Where(a => a.Status == AuctionStatus.Active && DateTime.Now < a.EndTime)
                .Take(4)
                .ToListAsync();

            var products = await _context.Products
                .Include(p => p.Images)
                .Where(p => p.Status == ProductStatus.Active)
                .Take(4)
                .ToListAsync();

            ViewBag.LatestAuctions = auctions;
            ViewBag.LatestProducts = products;

            return View();
        }
    }
}
