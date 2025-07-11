﻿using LocatraMain.DAL;
using LocatraMain.Models.Sale;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LocatraMain.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class AdminSaleController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminSaleController(ApplicationDbContext context)
        {
            _context = context;
        }


        public async Task<IActionResult> Index()
        {
            var products = await _context.Products
                .Include(p => p.CreatedBy)
                .Include(p => p.Images)
                .Where(p => p.Status != ProductStatus.Rejected)
                .ToListAsync();

            var grouped = products
                .GroupBy(p => p.CreatedBy)
                .ToList();

            return View(grouped);
        }
        public async Task<IActionResult> PendingProducts()
        {
            var pendingProducts = await _context.Products
                .Include(p => p.CreatedBy)
                .Where(p => p.Status == ProductStatus.Pending)
                .ToListAsync();

            return View(pendingProducts);
        }

        
        public async Task<IActionResult> Approve(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null) return NotFound();

            product.Status = ProductStatus.Active;
            await _context.SaveChangesAsync();

            return RedirectToAction("pendingProducts");
        }


        public async Task<IActionResult> Reject(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null) return NotFound();

            product.Status = ProductStatus.Rejected;
            await _context.SaveChangesAsync();

            return RedirectToAction("pendingProducts");
        }
    }
}
