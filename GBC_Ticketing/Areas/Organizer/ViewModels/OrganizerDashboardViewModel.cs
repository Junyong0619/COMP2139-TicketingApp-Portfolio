namespace GBC_Ticketing.Areas.Organizer.ViewModels;

public class OrganizerDashboardViewModel
{
    public string OrganizerName { get; set; } = string.Empty;
    public int TotalEvents { get; set; }
    public int UpcomingEvents { get; set; }
    public int TotalTicketsSold { get; set; }
    public decimal TotalRevenue { get; set; }
    public IReadOnlyList<OrganizerEventSummary> Events { get; set; } = Array.Empty<OrganizerEventSummary>();
    public ChartSeries CategoryRevenue { get; set; } = new();
    public ChartSeries MonthlyRevenue { get; set; } = new();
}

public class OrganizerEventSummary
{
    public int EventId { get; set; }
    public string Title { get; set; } = string.Empty;
    public DateTime StartAt { get; set; }
    public int TicketsAvailable { get; set; }
    public int TicketsSold { get; set; }
    public decimal Revenue { get; set; }
    public string CategoryName { get; set; } = string.Empty;
}

public class ChartSeries
{
    public IReadOnlyList<string> Labels { get; set; } = Array.Empty<string>();
    public IReadOnlyList<decimal> Values { get; set; } = Array.Empty<decimal>();
}
