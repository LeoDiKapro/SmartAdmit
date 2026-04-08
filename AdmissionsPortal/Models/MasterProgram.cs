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

        // Foreign key
        public int UniversityId { get; set; }
        public University University { get; set; } = null!;

        // Navigation
        public ICollection<Application> Applications { get; set; } = new List<Application>();
    }
}
