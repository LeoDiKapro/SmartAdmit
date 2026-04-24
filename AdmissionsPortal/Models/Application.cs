using System.ComponentModel.DataAnnotations;
using System.Reflection.Metadata;

namespace AdmissionsPortal.Models
{
    public enum DiplomaStatus
    {
        Completed,
        Ongoing
    }

    public enum ApplicationStatus
    {
        Draft,
        Pending,
        AutoRejected,
        UnderReview,
        Withdrawn,
        Accepted
    }

    public class Application
    {
        public int Id { get; set; }

        // Who applied
        [Required]
        public string StudentId { get; set; } = string.Empty;
        public AppUser Student { get; set; } = null!;

        // Where they applied
        public int UniversityId { get; set; }
        public University University { get; set; } = null!;

        public int MasterProgramId { get; set; }
        public MasterProgram MasterProgram { get; set; } = null!;

        // Application fields
        [Required]
        [Range(0.0, 4.0)]
        public decimal GPA { get; set; }

        [Required]
        [Range(1, 10)]
        public int StudyYears { get; set; }

        [Required]
        public DiplomaStatus DiplomaStatus { get; set; }

        [Required]
        [StringLength(100)]
        [Display(Name = "Mother Tongue")]
        public string MotherTongue { get; set; } = string.Empty;

        // Navigation
        public ICollection<ApplicationLanguage> Languages { get; set; } = new List<ApplicationLanguage>();

        // Auto-screening result
        public ApplicationStatus Status { get; set; } = ApplicationStatus.Pending;

        public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;
        public DateTime? ReviewedAt { get; set; }

        [StringLength(500)]
        public string? RejectionReason { get; set; }

        [StringLength(1000)]
        public string? RepNotes { get; set; }

        // Navigation
        public ICollection<Document> Documents { get; set; } = new List<Document>();
    }
}
