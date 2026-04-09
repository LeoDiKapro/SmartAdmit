using System.ComponentModel.DataAnnotations;

namespace AdmissionsPortal.Models
{
    public class ApplicationLanguage
    {
        public int Id { get; set; }

        public int ApplicationId { get; set; }
        public Application Application { get; set; } = null!;

        [Required]
        [StringLength(100)]
        public string Language { get; set; } = string.Empty;

        [Required]
        [StringLength(10)]
        public string Level { get; set; } = string.Empty;

        // Path to the uploaded certificate for this language (optional)
        [StringLength(500)]
        public string? CertificatePath { get; set; }

        [StringLength(300)]
        public string? CertificateFileName { get; set; }
    }
}