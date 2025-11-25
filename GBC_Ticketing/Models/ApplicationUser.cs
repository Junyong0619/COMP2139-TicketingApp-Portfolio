using Microsoft.AspNetCore.Identity;

namespace GBC_Ticketing.Models;

public class ApplicationUser : IdentityUser
{
    public string? FullName { get; set; }

    public override string? PhoneNumber { get; set; }

    public DateTime? DateOfBirth { get; set; }

    public string? ProfilePictureUrl { get; set; }
}
