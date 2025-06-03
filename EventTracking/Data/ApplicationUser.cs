using Microsoft.AspNetCore.Identity;

namespace EventTracking.Data
{
    // Add profile data for application users by adding properties to the ApplicationUser class
    public class ApplicationUser : IdentityUser
    {
        public string? Sex { get; set; }
        public string? Address { get; set; }
    }

}
