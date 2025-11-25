using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using GBC_Ticketing.Data;
using GBC_Ticketing.Models;

namespace GBC_Ticketing.Controllers
{
    public class SearchController : Controller
    {
        private readonly ApplicationDbContext _context;

        public SearchController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Search
        [HttpGet]
        public async Task<IActionResult> Index(
            string searchString, 
            int? categoryId, 
            DateTime? startDate, 
            DateTime? endDate,
            string availabilityFilter,
            string sortBy)
        {
            // Get all categories for dropdown
            ViewData["Categories"] = new SelectList(_context.Categories, "CategoryId", "Name");
            ViewData["CurrentSearch"] = searchString;
            ViewData["CurrentCategory"] = categoryId;
            ViewData["CurrentStartDate"] = startDate?.ToString("yyyy-MM-dd");
            ViewData["CurrentEndDate"] = endDate?.ToString("yyyy-MM-dd");
            ViewData["CurrentAvailability"] = availabilityFilter;
            ViewData["CurrentSort"] = sortBy;

            var events = _context.Events.Include(e => e.Category).Include(e => e.Purchases).AsQueryable();

            // Search by title or description (case-insensitive)
            if (!string.IsNullOrEmpty(searchString))
            {
                events = events.Where(e => e.Title.ToLower().Contains(searchString.ToLower()) || 
                                          (e.Description != null && e.Description.ToLower().Contains(searchString.ToLower())));
            }

            // Filter by category
            if (categoryId.HasValue && categoryId.Value > 0)
            {
                events = events.Where(e => e.CategoryId == categoryId.Value);
            }

            // Filter by date range
            if (startDate.HasValue)
            {
                var utcStartDate = ToUtc(startDate.Value);
                events = events.Where(e => e.StartAt >= utcStartDate);
            }
            if (endDate.HasValue)
            {
                var utcEndDate = ToUtc(endDate.Value);
                var endOfDay = utcEndDate.AddDays(1).AddTicks(-1);
                events = events.Where(e => e.StartAt <= endOfDay);
            }

            // Materialize the query to apply RemainingTickets filter
            var eventsList = await events.ToListAsync();

            // Filter by ticket availability
            if (!string.IsNullOrEmpty(availabilityFilter))
            {
                if (availabilityFilter == "available")
                {
                    eventsList = eventsList.Where(e => e.RemainingTickets > 0).ToList();
                }
                else if (availabilityFilter == "soldout")
                {
                    eventsList = eventsList.Where(e => e.RemainingTickets <= 0).ToList();
                }
            }

            // Sort
            eventsList = sortBy switch
            {
                "title" => eventsList.OrderBy(e => e.Title).ToList(),
                "title_desc" => eventsList.OrderByDescending(e => e.Title).ToList(),
                "date" => eventsList.OrderBy(e => e.StartAt).ToList(),
                "date_desc" => eventsList.OrderByDescending(e => e.StartAt).ToList(),
                "price" => eventsList.OrderBy(e => e.Price).ToList(),
                "price_desc" => eventsList.OrderByDescending(e => e.Price).ToList(),
                _ => eventsList.OrderBy(e => e.StartAt).ToList()
            };

            return View(eventsList);
        }

        [HttpGet]
        public async Task<IActionResult> Live(string searchString)
        {
            var eventsQuery = BuildEventQuery();

            if (!string.IsNullOrWhiteSpace(searchString))
            {
                eventsQuery = eventsQuery.Where(e => e.Title.ToLower().Contains(searchString.ToLower()) ||
                                                     (e.Description != null && e.Description.ToLower().Contains(searchString.ToLower())));
            }

            var results = await eventsQuery
                .OrderBy(e => e.StartAt)
                .Take(12)
                .ToListAsync();

            return PartialView("_SearchResults", results);
        }

        // GET: Search/Details/{id}
        [HttpGet]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var eventItem = await _context.Events
                .Include(e => e.Category)
                .Include(e => e.Purchases)
                .FirstOrDefaultAsync(m => m.EventId == id);
            
            if (eventItem == null)
            {
                throw new InvalidOperationException($"Unable to load event with id {id}.");
            }

            return View(eventItem);
        }


        // Helper method to convert DateTime to UTC
        private static DateTime ToUtc(DateTime input)
        {
            if (input.Kind == DateTimeKind.Utc)
                return input;
            if (input.Kind == DateTimeKind.Unspecified)
                return DateTime.SpecifyKind(input, DateTimeKind.Utc);
            return input.ToUniversalTime();
        }

        private IQueryable<Event> BuildEventQuery()
        {
            return _context.Events
                .Include(e => e.Category)
                .Include(e => e.Purchases);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error(int? statusCode = null)
        {
            var viewModel = new ErrorViewModel 
            { 
                RequestId = System.Diagnostics.Activity.Current?.Id ?? HttpContext.TraceIdentifier,
                StatusCode = statusCode ?? Response.StatusCode
            };
            
            return View(viewModel);
        }
    }
}
