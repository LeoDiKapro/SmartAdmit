using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace AdmissionsPortal.Models
{
    public class AppUser : IdentityUser
    {
        [Required]
        [StringLength(100)]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string LastName { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation
        public ICollection<Application> Applications { get; set; } = new List<Application>();

        // Computed helper
        public string FullName => $"{FirstName} {LastName}";
    }
}
