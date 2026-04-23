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

        // Navigation
        public ICollection<MasterProgram> MasterPrograms { get; set; } = new List<MasterProgram>();
        public ICollection<Application> Applications { get; set; } = new List<Application>();
    }
}
