namespace GBC_Ticketing.Areas.Admin.ViewModels;

public class AdminDashboardViewModel
{
    public int TotalEvents { get; set; }
    public int UpcomingEvents { get; set; }
    public int TotalCategories { get; set; }
    public int TotalPurchases { get; set; }
    public decimal TotalRevenue { get; set; }
    public IReadOnlyList<TopEventSummary> TopEvents { get; set; } = Array.Empty<TopEventSummary>();
    public IReadOnlyList<RecentPurchaseSummary> RecentPurchases { get; set; } = Array.Empty<RecentPurchaseSummary>();
}

public class TopEventSummary
{
    public int EventId { get; set; }
    public string Title { get; set; } = string.Empty;
    public int TicketsSold { get; set; }
    public decimal Revenue { get; set; }
}

public class RecentPurchaseSummary
{
    public string EventTitle { get; set; } = string.Empty;
    public string GuestName { get; set; } = string.Empty;
    public string GuestEmail { get; set; } = string.Empty;
    public DateTime PurchasedAt { get; set; }
    public decimal TotalCost { get; set; }
}
