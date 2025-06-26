using LocatraMain.DAL;
using LocatraMain.Models;
using LocatraMain.Models.Auction;
using LocatraMain.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LocatraMain.Controllers
{
    [Authorize]
    public class AuctionController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IWebHostEnvironment _env;

        public AuctionController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, IWebHostEnvironment env)
        {
            _context = context;
            _userManager = userManager;
            _env = env;
        }

        // INDEX - only active auctions shown
        [AllowAnonymous]
        public async Task<IActionResult> Index()
        {
            var auctions = await _context.Auctions
                .Include(a => a.Images)
                .Where(a => a.Status == AuctionStatus.Active && DateTime.Now < a.EndTime)
                .ToListAsync();


            return View(auctions);
        }

        // CREATE GET
        public IActionResult Create()
        {
            return View();
        }

        // CREATE POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AuctionCreateVm vm)
        {
            if (!ModelState.IsValid)
                return View(vm);

            var user = await _userManager.GetUserAsync(User);

            var auction = new Auction
            {
                Title = vm.Title,
                Description = vm.Description,
                StartingPrice = vm.StartingPrice,
                CurrentPrice = vm.StartingPrice,
                StartTime = vm.StartTime,
                EndTime = vm.EndTime,
                Status = AuctionStatus.Pending,
                CreatedById = user.Id,
                Images = new List<AuctionImage>()
            };

            // Şəkillərin saxlanması
            var wwwRootPath = Path.Combine(_env.WebRootPath, "uploads");
            if (!Directory.Exists(wwwRootPath))
                Directory.CreateDirectory(wwwRootPath);

            foreach (var file in vm.Images)
            {
                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                var filePath = Path.Combine(wwwRootPath, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                auction.Images.Add(new AuctionImage
                {
                    ImageUrl = "/uploads/" + fileName
                });
            }

            _context.Auctions.Add(auction);
            await _context.SaveChangesAsync();

            // TODO: Add notification to admin (can be via SignalR, DB flag, or dashboard view)
            TempData["Message"] = "Auksion yaradıldı. Admin təsdiqlədikdən sonra aktiv olacaq.";

            return RedirectToAction(nameof(Index));
        }





        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Bid(int id, decimal amount)
        {
            var user = await _userManager.GetUserAsync(User);
            var auction = await _context.Auctions
                .Include(a => a.Bids)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (auction == null)
                return BadRequest("Auction tapılmadı.");

            if (auction.Status != AuctionStatus.Active)
                return BadRequest("Status aktiv deyil.");

            if (DateTime.Now > auction.EndTime)
                return BadRequest("Vaxtı bitmişdir.");

            if (auction.CurrentPrice.HasValue && amount <= auction.CurrentPrice)
                return BadRequest("Yeni təklif mövcud qiymətdən yüksək olmalıdır.");

            // Yeni təklifi əlavə et
            var bid = new Bid
            {
                AuctionId = id,
                Amount = amount,
                BidTime = DateTime.Now,
                UserId = user.Id
            };

            // Mövcud qiyməti və qalibi yenilə
            auction.CurrentPrice = amount;
            auction.WinnerId = user.Id;

            _context.Bids.Add(bid);
            await _context.SaveChangesAsync();

            return RedirectToAction("Details", new { id });
        }



        [AllowAnonymous]
        public async Task<IActionResult> Details(int id)
        {
            var auction = await _context.Auctions
                .Include(a => a.Images)
                .Include(a => a.Bids)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (auction == null)
                return NotFound();

            // Digər aktiv auksionlar (hal-hazırkıdan fərqli olanlar)
            var otherAuctions = await _context.Auctions
                .Include(a => a.Images)
                .Where(a => a.Status == AuctionStatus.Active && a.Id != id && DateTime.Now < a.EndTime)
                .ToListAsync();

            ViewBag.OtherAuctions = otherAuctions;

            return View(auction);
        }
    }
}
