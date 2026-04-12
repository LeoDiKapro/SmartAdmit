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

        [StringLength(200)]
        [Display(Name = "Address")]
        public string? Address { get; set; }

        [StringLength(100)]
        [Display(Name = "City")]
        public string? City { get; set; }

        [StringLength(100)]
        [Display(Name = "Country")]
        public string? Country { get; set; }

        [StringLength(20)]
        [Display(Name = "Phone Number")]
        public string? PhoneNumber2 { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "Date of Birth")]
        public DateTime? DateOfBirth { get; set; }

        [StringLength(50)]
        [Display(Name = "National ID / Passport Number")]
        public string? NationalId { get; set; }

        [StringLength(100)]
        [Display(Name = "Nationality")]
        public string? Nationality { get; set; }

        public bool ProfileCompleted { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation
        public ICollection<Application> Applications { get; set; } = new List<Application>();

        public string FullName => $"{FirstName} {LastName}";
    }
}