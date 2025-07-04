using LocatraMain.DAL;
using LocatraMain.Models.Auction;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LocatraMain.Areas.Admin.Controllers
{

    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class AdminAuctionController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminAuctionController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var auctions = await _context.Auctions
                .Include(a => a.CreatedBy)
                .Include(a => a.Images)
                .ToListAsync();

            var grouped = auctions.GroupBy(a => a.CreatedBy).ToList();
            return View(grouped);
        }

        public async Task<IActionResult> PendingAuctions()
        {
            var pending = await _context.Auctions
                .Include(a => a.CreatedBy)
                .Include(a => a.Images)
                .Where(a => a.Status == AuctionStatus.Pending)
                .ToListAsync();

            return View(pending);
        }

        
        public async Task<IActionResult> Approve(int id)
        {
            var auction = await _context.Auctions.FindAsync(id);
            if (auction == null) return NotFound();

            auction.Status = AuctionStatus.Active;
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(PendingAuctions));
        }

        
        public async Task<IActionResult> Reject(int id)
        {
            var auction = await _context.Auctions
                .Include(a => a.Images)
                .Include(a => a.Bids)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (auction == null) return NotFound();

            
            _context.AuctionImages.RemoveRange(auction.Images);
            _context.Bids.RemoveRange(auction.Bids);

            _context.Auctions.Remove(auction);
            await _context.SaveChangesAsync();

            TempData["Message"] = "Auksion silindi.";
            return RedirectToAction(nameof(PendingAuctions));
        }
        
        public async Task<IActionResult> EndAuction(int id)
        {
            var auction = await _context.Auctions.FindAsync(id);
            if (auction == null) return NotFound();

            auction.Status = AuctionStatus.Ended;
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(PendingAuctions));
        }
    }
}
