using System.ComponentModel.DataAnnotations;
using static System.Net.Mime.MediaTypeNames;

namespace AdmissionsPortal.Models
{
    public class University
    {
        public int Id { get; set; }

        [Required]
        [StringLength(200)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [Range(0.0, 4.0)]
        public decimal MinGPA { get; set; }

        [Required]
        [Range(1, 10)]
        public int MinYears { get; set; }

        // Navigation properties
        public ICollection<MasterProgram> MasterPrograms { get; set; } = new List<MasterProgram>();
        public ICollection<Application> Applications { get; set; } = new List<Application>();
    }
}
