using GBC_Ticketing.Data;
using GBC_Ticketing.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Serilog;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace GBC_Ticketing.Controllers
{
    public class PurchasesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public PurchasesController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // Index: List  of Purchase 
        [HttpGet]
        public IActionResult Index()
        {
            var purchases = _context.Purchases.ToList();
            return View(purchases);
        }

        // Create Purchase Form
        [HttpGet]
        public async Task<IActionResult> Create(int eventId)
        {
            var ev = await _context.Events
                .Include(e => e.Category)
                .Include(e => e.Purchases)
                .FirstOrDefaultAsync(e => e.EventId == eventId);
            if (ev == null)
                return NotFound();

            var remainingTickets = ev.AvailableTickets - (ev.Purchases?.Sum(p => p.Quantity) ?? 0);

            ViewBag.Event = ev;
            ViewBag.RemainingTickets = remainingTickets;
            return View(new Purchase { EventId = eventId });
        }

        // Process Purchase
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Purchase purchase)
        {
            // Load event with purchases to calculate remaining tickets
            var ev = await _context.Events
                .Include(e => e.Category)
                .Include(e => e.Purchases)
                .FirstOrDefaultAsync(e => e.EventId == purchase.EventId);

                if (ev == null)
                return NotFound();

            // Calculate remaining tickets
            var purchasesForEvent = ev.Purchases ?? Enumerable.Empty<Purchase>();
            int purchasedTickets = purchasesForEvent.Sum(p => p.Quantity);
            int remainingTickets = ev.AvailableTickets - purchasedTickets;

            // Validate quantity against remaining tickets
            if (purchase.Quantity > remainingTickets)
            {
                ModelState.AddModelError("Quantity", 
                    $"Only {remainingTickets} ticket(s) remaining. Please reduce your quantity.");
                Log.Warning("Purchase attempt exceeded remaining tickets for Event {EventId}: requested {Requested}, remaining {Remaining}",
                    purchase.EventId, purchase.Quantity, remainingTickets);
            }

            if (!ModelState.IsValid)
            {
                ViewBag.Event = ev;
                ViewBag.RemainingTickets = remainingTickets;
                return View(purchase);
            }

            // time & total cost 
            purchase.PurchasedAt = DateTime.UtcNow;
            purchase.TotalCost = purchase.Quantity * ev.Price;

            if (User.Identity?.IsAuthenticated == true)
            {
                var attendee = await _userManager.GetUserAsync(User);
                purchase.AttendeeId = attendee?.Id;
                if (attendee != null)
                {
                    if (string.IsNullOrWhiteSpace(purchase.GuestName))
                    {
                        purchase.GuestName = attendee.FullName ?? attendee.Email ?? attendee.UserName ?? purchase.GuestName;
                    }
                    if (string.IsNullOrWhiteSpace(purchase.GuestEmail))
                    {
                        purchase.GuestEmail = attendee.Email ?? purchase.GuestEmail;
                    }
                }
            }

            // save in database
            _context.Purchases.Add(purchase);
            await _context.SaveChangesAsync();

            Log.Information("Purchase {PurchaseId} created for Event {EventId} with quantity {Quantity}",
                purchase.PurchaseId, purchase.EventId, purchase.Quantity);

            // Redirect to confirmation page
            return RedirectToAction(nameof(Confirm), new { id = purchase.PurchaseId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Rate([FromBody] RateRequest request)
        {
            if (request == null || request.Rating is < 1 or > 5)
            {
                return BadRequest("Rating must be between 1 and 5.");
            }

            var purchase = await _context.Purchases.FirstOrDefaultAsync(p => p.PurchaseId == request.PurchaseId);
            if (purchase == null)
            {
                return NotFound();
            }

            var attendee = await _userManager.GetUserAsync(User);
            if (attendee == null || purchase.AttendeeId != attendee.Id)
            {
                Log.Warning("Unauthorized rating attempt on purchase {PurchaseId}", request.PurchaseId);
                return Forbid();
            }

            purchase.Rating = request.Rating;
            await _context.SaveChangesAsync();

            Log.Information("Purchase {PurchaseId} rated {Rating} by attendee {Attendee}", purchase.PurchaseId, purchase.Rating, attendee.Email ?? attendee.Id);

            return Ok(new { purchase.PurchaseId, purchase.Rating });
        }

        public class RateRequest
        {
            public int PurchaseId { get; set; }
            public int Rating { get; set; }
        }

        // Show Purchase Confirmation
        [HttpGet]
        public async Task<IActionResult> Confirm(int id)
        {
            var purchase = await _context.Purchases
                .Include(p => p.Event)
                .ThenInclude(e => e.Category)
                .FirstOrDefaultAsync(p => p.PurchaseId == id);
            
            if (purchase == null)
                return NotFound();

            if (purchase.Event == null)
            {
                Log.Error("Purchase {PurchaseId} has no associated event", id);
                return NotFound("Event not found for this purchase.");
            }

            ViewBag.Event = purchase.Event;
            Log.Information("Purchase confirmation viewed for Purchase {PurchaseId}", purchase.PurchaseId);

            return View(purchase);
        }

        // Edit Purchase
        [HttpGet]
        public IActionResult Edit(int id)
        {
            var purchase = _context.Purchases.FirstOrDefault(p => p.PurchaseId == id);
            if (purchase == null)
                return NotFound();

            return View(purchase);
        }

        // Edit Purchase
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, Purchase purchase)
        {
            if (id != purchase.PurchaseId)
                return NotFound();

            if (ModelState.IsValid)
            {
                purchase.PurchasedAt = DateTime.SpecifyKind(purchase.PurchasedAt, DateTimeKind.Utc);
                _context.Update(purchase);
                _context.SaveChanges();
                return RedirectToAction(nameof(Index));
            }
            return View(purchase);
        }

        // Delete Purchase
        [HttpGet]
        public IActionResult Delete(int id)
        {
            var purchase = _context.Purchases.FirstOrDefault(p => p.PurchaseId == id);
            if (purchase == null)
                return NotFound();

            return View(purchase);
        }

        // Delete Purchase
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            var purchase = _context.Purchases.FirstOrDefault(p => p.PurchaseId == id);
            if (purchase == null)
                return NotFound();

            _context.Purchases.Remove(purchase);
            _context.SaveChanges();
            return RedirectToAction(nameof(Index));
        }
    }
}
