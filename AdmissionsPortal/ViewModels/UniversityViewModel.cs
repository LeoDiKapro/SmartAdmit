using System.ComponentModel.DataAnnotations;

namespace AdmissionsPortal.ViewModels
{
    public class UniversityViewModel
    {
        public int Id { get; set; }

        [Required]
        [StringLength(200)]
        [Display(Name = "University Name")]
        public string Name { get; set; } = string.Empty;

    }
}