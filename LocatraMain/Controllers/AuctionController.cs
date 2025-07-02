using LocatraMain.DAL;
using LocatraMain.Models;
using LocatraMain.Models.Auction;
using LocatraMain.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Stripe;
using Stripe.Checkout;

namespace LocatraMain.Controllers
{
    [Authorize]
    public class AuctionController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IWebHostEnvironment _env;
        private readonly IEmailSender _emailSender;

        public AuctionController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, IWebHostEnvironment env, IEmailSender emailSender)
        {
            _context = context;
            _userManager = userManager;
            _env = env;
            _emailSender = emailSender;
        }

        
        [AllowAnonymous]
        public async Task<IActionResult> Index()
        {

            var auctions = await _context.Auctions
                .Include(a => a.Images)
                .Where(a => a.Status == AuctionStatus.Active && DateTime.Now < a.EndTime)
                .ToListAsync();


            var activeAuctions = await _context.Auctions
                .Include(a => a.Bids)
                .Where(a => a.EndTime <= DateTime.Now && a.Status == AuctionStatus.Active)
                .ToListAsync();

            foreach (var auction in activeAuctions)
            {
                var lastBid = auction.Bids.OrderByDescending(b => b.BidTime).FirstOrDefault();
                if (lastBid != null)
                {
                    auction.Status = AuctionStatus.Ended;
                    auction.WinnerId = lastBid.UserId;


                    var winner = await _context.Users.FirstOrDefaultAsync(u => u.Id == lastBid.UserId);
                    if (winner != null)
                    {
                        string subject = "🎉 Təbriklər! Auksionu qazandınız";
                        string message = $"<p>Salam <strong>{winner.Name}</strong>,</p><p>\"<strong>{auction.Title}</strong>\" adlı auksionda <strong>{lastBid.Amount} ₼</strong> ilə qalib gəldiniz.</p><p>Ödəniş üçün <a href='https://localhost:7176/Profile'>profil səhifənizə</a> daxil olun.</p><br/><p>Locatra komandası tərəfindən təbrik edirik! 🎊</p>";

                        await _emailSender.SendEmailAsync(winner.Email, subject, message);

                    }

                }
            }
            await _context.SaveChangesAsync();
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


        [HttpGet]
        public async Task<IActionResult> Payment(int id)
        {
            var auction = await _context.Auctions
                .Include(a => a.Images)
                .Include(a => a.Bids)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (auction == null || auction.Status != AuctionStatus.Ended)
                return NotFound();

            var lastBid = auction.Bids.OrderByDescending(b => b.BidTime).FirstOrDefault();
            if (lastBid == null || lastBid.UserId != _userManager.GetUserId(User))
                return Forbid(); // başqasının auctionunu ödəyə bilməz

            var vm = new AuctionPaymentViewModel
            {
                AuctionId = auction.Id,
                Title = auction.Title,
                Amount = lastBid.Amount,
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Payment(AuctionPaymentViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var auction = await _context.Auctions
                .Include(a => a.Bids)
                .FirstOrDefaultAsync(a => a.Id == model.AuctionId);

            var lastBid = auction?.Bids.OrderByDescending(b => b.BidTime).FirstOrDefault();
            if (lastBid == null || lastBid.UserId != _userManager.GetUserId(User))
                return Forbid();

            // Stripe PaymentSession yarad
            var domain = "https://localhost:7176"; // frontend domenin
            var options = new SessionCreateOptions
            {
                LineItems = new List<SessionLineItemOptions>
        {
            new SessionLineItemOptions
            {
                PriceData = new SessionLineItemPriceDataOptions
                {
                    UnitAmountDecimal = model.Amount * 100,
                    Currency = "azn",
                    ProductData = new SessionLineItemPriceDataProductDataOptions
                    {
                        Name = model.Title
                    }
                },
                Quantity = 1
            }
        },
                Mode = "payment",
                SuccessUrl = domain + "/Auction/PaymentSuccess?auctionId=" + model.AuctionId,
                CancelUrl = domain + "/Profile/Index"
            };

            var service = new SessionService();
            Session session = await service.CreateAsync(options);
            return Redirect(session.Url);
        }


        public async Task<IActionResult> PaymentSuccess(int auctionId)
        {
            var userId = _userManager.GetUserId(User);
            var auction = await _context.Auctions.FirstOrDefaultAsync(a => a.Id == auctionId);

            if (auction != null && auction.WinnerId == userId)
            {
                auction.IsPaid = true; 
                await _context.SaveChangesAsync();
                TempData["PaymentSuccess"] = "Ödəniş uğurla tamamlandı.";
            }


            return RedirectToAction("Index", "Profile");
        }


    }
}
