using AdmissionsPortal.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace AdmissionsPortal.ViewModels
{
    public class ApplicationViewModel
    {
        [Required]
        [Display(Name = "University")]
        public int UniversityId { get; set; }

        [Required]
        [Display(Name = "Master Program")]
        public int MasterProgramId { get; set; }

        [Required]
        [Range(0.0, 4.0, ErrorMessage = "GPA must be between 0.0 and 4.0")]
        [Display(Name = "GPA")]
        public decimal GPA { get; set; }

        [Required]
        [Range(1, 10, ErrorMessage = "Years must be between 1 and 10")]
        [Display(Name = "Years of Study")]
        public int StudyYears { get; set; }

        [Required]
        [Display(Name = "Diploma Status")]
        public DiplomaStatus DiplomaStatus { get; set; }

        [Display(Name = "Transcript (PDF)")]
        public IFormFile? Transcript { get; set; }

        [Display(Name = "Diploma (PDF)")]
        public IFormFile? Diploma { get; set; }

        // Populated by the controller for the dropdowns
        public IEnumerable<SelectListItem> Universities { get; set; } = new List<SelectListItem>();
        public IEnumerable<SelectListItem> MasterPrograms { get; set; } = new List<SelectListItem>();
    }
}
