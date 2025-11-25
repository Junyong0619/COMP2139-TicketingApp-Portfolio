using GBC_Ticketing.Areas.Admin.ViewModels;
using GBC_Ticketing.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GBC_Ticketing.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public class HomeController : Controller
{
    private readonly ApplicationDbContext _context;

    public HomeController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var now = DateTime.UtcNow;

        var totalEvents = await _context.Events.CountAsync();
        var upcomingEvents = await _context.Events.CountAsync(e => e.StartAt >= now);
        var totalCategories = await _context.Categories.CountAsync();
        var totalPurchases = await _context.Purchases.CountAsync();
        var totalRevenue = await _context.Purchases.SumAsync(p => p.TotalCost);

        var topEvents = await _context.Purchases
            .Include(p => p.Event)
            .GroupBy(p => new { p.EventId, p.Event!.Title })
            .Select(g => new TopEventSummary
            {
                EventId = g.Key.EventId,
                Title = g.Key.Title,
                TicketsSold = g.Sum(p => p.Quantity),
                Revenue = g.Sum(p => p.TotalCost)
            })
            .OrderByDescending(e => e.Revenue)
            .Take(5)
            .ToListAsync();

        var recentPurchases = await _context.Purchases
            .Include(p => p.Event)
            .OrderByDescending(p => p.PurchasedAt)
            .Take(5)
            .Select(p => new RecentPurchaseSummary
            {
                EventTitle = p.Event!.Title,
                GuestName = p.GuestName,
                GuestEmail = p.GuestEmail,
                PurchasedAt = p.PurchasedAt,
                TotalCost = p.TotalCost
            })
            .ToListAsync();

        var viewModel = new AdminDashboardViewModel
        {
            TotalEvents = totalEvents,
            UpcomingEvents = upcomingEvents,
            TotalCategories = totalCategories,
            TotalPurchases = totalPurchases,
            TotalRevenue = totalRevenue,
            TopEvents = topEvents,
            RecentPurchases = recentPurchases
        };

        return View(viewModel);
    }
}
