using GBC_Ticketing.Data;
using GBC_Ticketing.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace GBC_Ticketing.Controllers
{
    public class EventsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public EventsController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Events List
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var events = _context.Events
                .Include(e => e.Category)
                .Include(e => e.Purchases)
                .AsQueryable();
            
            events = events.OrderBy(e => e.StartAt);

            return View(await events.ToListAsync());
        }

        // GET: Events/Create
        [HttpGet]
        [Authorize(Roles = "Organizer,Admin")]
        public IActionResult Create()
        {
            ViewData["CategoryId"] = new SelectList(_context.Categories, "CategoryId", "Name");
            return View();
        }

        // POST: Events/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Organizer,Admin")]
        public async Task<IActionResult> Create([Bind("EventId,Title,Description,StartAt,Price,AvailableTickets,CategoryId,Address,ImagePath")] Event eventItem)
        {
            if (ModelState.IsValid)
            {
                eventItem.StartAt = ToUtc(eventItem.StartAt);
                var currentUser = await _userManager.GetUserAsync(User);
                if (currentUser != null && await _userManager.IsInRoleAsync(currentUser, "Organizer"))
                {
                    eventItem.OrganizerId = currentUser.Id;
                }
                else if (eventItem.OrganizerId == null)
                {
                    eventItem.OrganizerId = currentUser?.Id;
                }

                _context.Add(eventItem);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["CategoryId"] = new SelectList(_context.Categories, "CategoryId", "Name", eventItem.CategoryId);
            if (!await CanEditEventAsync(eventItem))
            {
                return Forbid();
            }

            return View(eventItem);
        }

        private DateTime ToUtc(DateTime input)
    {
        if (input.Kind == DateTimeKind.Utc)
            return input;
        if (input.Kind == DateTimeKind.Unspecified)
            return DateTime.SpecifyKind(input, DateTimeKind.Utc); // assume local is already UTC
        return input.ToUniversalTime();
    }

        // GET: Events/Edit/{id}
        [HttpGet]
        [Authorize(Roles = "Organizer,Admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var eventItem = await _context.Events.FindAsync(id);
            if (eventItem == null)
            {
                return NotFound();
            }

            if (!await CanEditEventAsync(eventItem))
            {
                return Forbid();
            }
            ViewData["CategoryId"] = new SelectList(_context.Categories, "CategoryId", "Name", eventItem.CategoryId);
            return View(eventItem);
        }

        // POST: Events/Edit/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Organizer,Admin")]
        public async Task<IActionResult> Edit(int id, [Bind("EventId,Title,Description,StartAt,Price,AvailableTickets,CategoryId,Address,ImagePath")] Event eventItem)
        {
            if (id != eventItem.EventId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                var existingEvent = await _context.Events.AsNoTracking().FirstOrDefaultAsync(e => e.EventId == id);
                if (existingEvent == null)
                {
                    return NotFound();
                }

                if (!await CanEditEventAsync(existingEvent))
                {
                    return Forbid();
                }

                eventItem.OrganizerId = existingEvent.OrganizerId;
                eventItem.StartAt = ToUtc(eventItem.StartAt);
                _context.Update(eventItem);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["CategoryId"] = new SelectList(_context.Categories, "CategoryId", "Name", eventItem.CategoryId);
            return View(eventItem);
        }

        // GET: Events/Delete/{id}
        [HttpGet]
        [Authorize(Roles = "Organizer,Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var eventItem = await _context.Events
                .Include(e => e.Category)
                .FirstOrDefaultAsync(m => m.EventId == id);

            if (eventItem == null)
            {
                return NotFound();
            }

            if (!await CanEditEventAsync(eventItem))
            {
                return Forbid();
            }

            return View(eventItem);
        }

        // POST: Events/Delete/{id}
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Organizer,Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var eventItem = await _context.Events.FindAsync(id);
            if (eventItem != null)
            {
                if (!await CanEditEventAsync(eventItem))
                {
                    return Forbid();
                }
                _context.Events.Remove(eventItem);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private async Task<bool> CanEditEventAsync(Event eventItem)
        {
            if (User.IsInRole("Admin"))
            {
                return true;
            }

            if (User.IsInRole("Organizer"))
            {
                var currentUser = await _userManager.GetUserAsync(User);
                return currentUser != null && eventItem.OrganizerId == currentUser.Id;
            }

            return false;
        }

        private bool EventExists(int id)
        {
            return _context.Events.Any(e => e.EventId == id);
        }
    }
}