using System.ComponentModel.DataAnnotations;

namespace AdmissionsPortal.ViewModels
{
    public class ProfileViewModel
    {
        [Required]
        [StringLength(100)]
        [Display(Name = "First Name")]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        [Display(Name = "Last Name")]
        public string LastName { get; set; } = string.Empty;

        [Required]
        [StringLength(200)]
        [Display(Name = "Address")]
        public string Address { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        [Display(Name = "City")]
        public string City { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        [Display(Name = "Country")]
        public string Country { get; set; } = string.Empty;

        [Required]
        [StringLength(20)]
        [Display(Name = "Phone Number")]
        public string PhoneNumber2 { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "Date of Birth")]
        public DateTime DateOfBirth { get; set; }

        [Required]
        [StringLength(50)]
        [Display(Name = "National ID / Passport Number")]
        public string NationalId { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        [Display(Name = "Nationality")]
        public string Nationality { get; set; } = string.Empty;
    }
}