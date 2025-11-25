using System.ComponentModel.DataAnnotations;

namespace GBC_Ticketing.Models
{
    public class Category
    {
        [Key]
        public int CategoryId { get; set; }

        [Required]
        public string Name { get; set; } = string.Empty;
        
        public string? Description { get; set; }

        // navigation property
        public ICollection<Event>? Events { get; set; }
    }
}