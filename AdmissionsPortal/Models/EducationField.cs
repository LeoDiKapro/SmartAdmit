using System.ComponentModel.DataAnnotations;

namespace AdmissionsPortal.Models
{
    public enum EducationField
    {
        [Display(Name = "Computer Science / IT")]
        ComputerScience,

        [Display(Name = "Engineering")]
        Engineering,

        [Display(Name = "Business / Economics")]
        Business,

        [Display(Name = "Law")]
        Law,

        [Display(Name = "Medicine / Health")]
        Medicine,

        [Display(Name = "Arts / Humanities")]
        Arts,

        [Display(Name = "Social Sciences")]
        SocialSciences,

        [Display(Name = "Other")]
        Other
    }
}