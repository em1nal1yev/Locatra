using LocatraMain.DAL;
using LocatraMain.Models;
using LocatraMain.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LocatraMain.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class DashboardController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;

        public DashboardController(UserManager<ApplicationUser> userManager, ApplicationDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        public IActionResult Index()
        {
            var users = _userManager.Users
                .Select(u => new UserWithDetailsViewModel
                {
                    Id = u.Id,
                    Name = u.Name,
                    Surname = u.Surname,
                    Email = u.Email,
                    WonAuctions = _context.Auctions
                        .Where(a => a.WinnerId == u.Id)
                        .ToList(),
                    PaidAuctions = _context.Auctions
                        .Where(a => a.WinnerId == u.Id && a.IsPaid)
                        .ToList(),
                    PurchasedProducts = _context.ProductPurchases
                        .Where(p => p.UserId == u.Id)
                        .Include(p => p.Product)
                            .ThenInclude(prod => prod.Images)
                        .Select(p => p.Product)
                        .ToList()
                }).ToList();

            return View(users);
        }   

    }
}
