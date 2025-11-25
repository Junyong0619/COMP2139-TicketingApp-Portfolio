namespace GBC_Ticketing.Areas.Attendee.ViewModels;

public class AttendeeDashboardViewModel
{
    public string AttendeeName { get; set; } = string.Empty;
    public string? ProfilePictureUrl { get; set; }
    public IReadOnlyList<AttendeeTicketViewModel> UpcomingTickets { get; set; } = Array.Empty<AttendeeTicketViewModel>();
    public IReadOnlyList<AttendeeTicketViewModel> PurchaseHistory { get; set; } = Array.Empty<AttendeeTicketViewModel>();
}

public class AttendeeTicketViewModel
{
    public int PurchaseId { get; set; }
    public int EventId { get; set; }
    public string EventTitle { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;
    public DateTime StartAt { get; set; }
    public string Address { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal TotalCost { get; set; }
    public string TicketCode { get; set; } = string.Empty;
    public int? Rating { get; set; }
    public bool CanRate => StartAt < DateTime.UtcNow;
}
