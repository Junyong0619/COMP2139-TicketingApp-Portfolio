using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GBC_Ticketing.Models
{
    public class Event
    {
        [Key]
        public int EventId { get; set; }

        [Required]
        public string Title { get; set; } = string.Empty;

        public string? Description { get; set; }
        
        [Required]
        public DateTime StartAt { get; set; }
        
        [Required]
        public decimal Price { get; set; }
        
        public int AvailableTickets { get; set; }
        
        [Required]
        public string Address { get; set; } = string.Empty;
        
        public string? ImagePath { get; set; }

        // FK to Category
        public int CategoryId { get; set; }
        public Category? Category { get; set; }

        // FK to Organizer
        public string? OrganizerId { get; set; }
        public ApplicationUser? Organizer { get; set; }

        // nav
        public ICollection<Purchase>? Purchases { get; set; }
        
        // Computed property: remaining tickets after purchases
        [NotMapped]
        public int RemainingTickets => AvailableTickets - (Purchases?.Sum(p => p.Quantity) ?? 0);
    }
}