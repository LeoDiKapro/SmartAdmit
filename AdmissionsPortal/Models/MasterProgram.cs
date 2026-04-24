using System.ComponentModel.DataAnnotations;
using static System.Net.Mime.MediaTypeNames;

namespace AdmissionsPortal.Models
{
    public class MasterProgram
    {
        public int Id { get; set; }

        [Required]
        [StringLength(200)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [Range(0.0, 4.0)]
        [Display(Name = "Minimum GPA")]
        public decimal MinGPA { get; set; }

        [Required]
        [Range(1, 10)]
        [Display(Name = "Minimum Years of Study")]
        public int MinYears { get; set; }
        public int? AvailableSpots { get; set; }
        public decimal? MinScore { get; set; }

        public int UniversityId { get; set; }
        public University University { get; set; } = null!;

        public ICollection<Application> Applications { get; set; } = new List<Application>();
    }
}
