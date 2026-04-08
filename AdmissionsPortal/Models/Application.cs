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

        // Auto-screening result
        public ApplicationStatus Status { get; set; } = ApplicationStatus.Pending;

        public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;
        public DateTime? ReviewedAt { get; set; }

        // Navigation
        public ICollection<Document> Documents { get; set; } = new List<Document>();
    }
}
