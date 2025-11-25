using GBC_Ticketing.Areas.Attendee.ViewModels;
using GBC_Ticketing.Data;
using GBC_Ticketing.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GBC_Ticketing.Areas.Attendee.Controllers;

[Area("Attendee")]
[Authorize(Roles = "Attendee")]
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
        var attendee = await _userManager.GetUserAsync(User);
        if (attendee == null)
        {
            return Challenge();
        }

        var purchases = await _context.Purchases
            .Include(p => p.Event)
            .ThenInclude(e => e!.Category)
            .Where(p => p.AttendeeId == attendee.Id)
            .OrderByDescending(p => p.Event!.StartAt)
            .ToListAsync();

        var upcoming = purchases
            .Where(p => p.Event!.StartAt >= DateTime.UtcNow)
            .Select(ToTicketViewModel)
            .OrderBy(p => p.StartAt)
            .ToList();

        var history = purchases
            .OrderByDescending(p => p.PurchasedAt)
            .Select(ToTicketViewModel)
            .ToList();

        var viewModel = new AttendeeDashboardViewModel
        {
            AttendeeName = attendee.FullName ?? attendee.Email ?? "Attendee",
            ProfilePictureUrl = attendee.ProfilePictureUrl,
            UpcomingTickets = upcoming,
            PurchaseHistory = history
        };

        return View(viewModel);
    }

    private static AttendeeTicketViewModel ToTicketViewModel(Purchase purchase)
    {
        var eventInfo = purchase.Event!;
        return new AttendeeTicketViewModel
        {
            PurchaseId = purchase.PurchaseId,
            EventId = eventInfo.EventId,
            EventTitle = eventInfo.Title,
            CategoryName = eventInfo.Category?.Name ?? "Uncategorized",
            StartAt = eventInfo.StartAt,
            Address = eventInfo.Address,
            Quantity = purchase.Quantity,
            TotalCost = purchase.TotalCost,
            TicketCode = $"GBC-{purchase.PurchaseId:D6}",
            Rating = purchase.Rating
        };
    }
}
