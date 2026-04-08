using System.ComponentModel.DataAnnotations;

namespace AdmissionsPortal.Models
{
    public enum DocumentType
    {
        Transcript,
        Diploma,
        RecommendationLetter,
        CV,
        LanguageCertificate,
        Other
    }

    public class Document
    {
        public int Id { get; set; }

        public int ApplicationId { get; set; }
        public Application Application { get; set; } = null!;

        [Required]
        public DocumentType Type { get; set; }

        [Required]
        [StringLength(300)]
        public string FileName { get; set; } = string.Empty;   // original name shown to user

        [Required]
        [StringLength(500)]
        public string FilePath { get; set; } = string.Empty;   // path on server disk

        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
    }
}
