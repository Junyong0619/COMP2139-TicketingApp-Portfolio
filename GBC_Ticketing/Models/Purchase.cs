using System.ComponentModel.DataAnnotations;

namespace GBC_Ticketing.Models
{
    public class Purchase
    {
        public int PurchaseId { get; set; }
        public DateTime PurchasedAt { get; set; } = DateTime.UtcNow;
        
        [Required(ErrorMessage = "Full Name is required.")]
        [Display(Name = "Full Name")]
        public string GuestName { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Please enter a valid email address.")]
        [Display(Name = "Email Address")]
        public string GuestEmail { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Quantity is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1.")]
        public int Quantity { get; set; }
        
        public decimal TotalCost { get; set; }

        // Attendee association
        public string? AttendeeId { get; set; }
        public ApplicationUser? Attendee { get; set; }

        [Range(1, 5)]
        public int? Rating { get; set; }

        // simple link to one Event for A1 (N:M로 갈 거면 Join 테이블은 A2)
        public int EventId { get; set; }
        public Event? Event { get; set; }
    }
}