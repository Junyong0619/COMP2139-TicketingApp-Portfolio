using GBC_Ticketing.Areas.Organizer.ViewModels;
using GBC_Ticketing.Data;
using GBC_Ticketing.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GBC_Ticketing.Areas.Organizer.Controllers;

[Area("Organizer")]
[Authorize(Roles = "Organizer")]
public class HomeController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public HomeController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    public async Task<IActionResult> Index()
    {
        var organizer = await _userManager.GetUserAsync(User);
        if (organizer == null)
        {
            return Challenge();
        }

        var events = await _context.Events
            .Include(e => e.Category)
            .Include(e => e.Purchases)
            .Where(e => e.OrganizerId == organizer.Id)
            .OrderBy(e => e.StartAt)
            .ToListAsync();

        var eventSummaries = events.Select(e => new OrganizerEventSummary
        {
            EventId = e.EventId,
            Title = e.Title,
            StartAt = e.StartAt,
            TicketsAvailable = e.AvailableTickets,
            TicketsSold = e.Purchases?.Sum(p => p.Quantity) ?? 0,
            Revenue = e.Purchases?.Sum(p => p.TotalCost) ?? 0m,
            CategoryName = e.Category?.Name ?? "Uncategorized"
        }).ToList();

        var allPurchases = eventSummaries.Any()
            ? events.SelectMany(e => e.Purchases ?? Enumerable.Empty<Purchase>()).ToList()
            : new List<Purchase>();

        var viewModel = new OrganizerDashboardViewModel
        {
            OrganizerName = organizer.FullName ?? organizer.Email ?? "Organizer",
            TotalEvents = eventSummaries.Count,
            UpcomingEvents = eventSummaries.Count(e => e.StartAt >= DateTime.UtcNow),
            TotalTicketsSold = eventSummaries.Sum(e => e.TicketsSold),
            TotalRevenue = eventSummaries.Sum(e => e.Revenue),
            Events = eventSummaries,
            CategoryRevenue = BuildCategorySeries(eventSummaries),
            MonthlyRevenue = BuildMonthlySeries(allPurchases)
        };

        return View(viewModel);
    }

    private static ChartSeries BuildCategorySeries(IEnumerable<OrganizerEventSummary> summaries)
    {
        var groups = summaries
            .GroupBy(e => e.CategoryName)
            .Select(g => new
            {
                Label = g.Key,
                Value = g.Sum(e => e.Revenue)
            })
            .OrderByDescending(g => g.Value)
            .ToList();

        return new ChartSeries
        {
            Labels = groups.Select(g => g.Label).ToList(),
            Values = groups.Select(g => g.Value).ToList()
        };
    }

    private static ChartSeries BuildMonthlySeries(IEnumerable<Purchase> purchases)
    {
        var lastSixMonths = Enumerable.Range(0, 6)
            .Select(offset => DateTime.UtcNow.AddMonths(-offset))
            .Select(dt => new DateTime(dt.Year, dt.Month, 1))
            .Distinct()
            .OrderBy(dt => dt)
            .ToList();

        var data = purchases
            .GroupBy(p => new { p.PurchasedAt.Year, p.PurchasedAt.Month })
            .ToDictionary(
                g => new DateTime(g.Key.Year, g.Key.Month, 1),
                g => g.Sum(p => p.TotalCost));

        var labels = new List<string>();
        var values = new List<decimal>();
        foreach (var month in lastSixMonths)
        {
            labels.Add(month.ToString("MMM yyyy"));
            values.Add(data.TryGetValue(month, out var value) ? value : 0m);
        }

        return new ChartSeries
        {
            Labels = labels,
            Values = values
        };
    }
}
