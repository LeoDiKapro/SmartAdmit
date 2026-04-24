using System.ComponentModel.DataAnnotations;

namespace AdmissionsPortal.Models
{
    public class ScoringWeights
    {
        public int Id { get; set; }

        public int MasterProgramId { get; set; }
        public MasterProgram MasterProgram { get; set; } = null!;

        // Weights must add up to 100
        [Range(0, 100)]
        [Display(Name = "GPA Weight (%)")]
        public int GPAWeight { get; set; } = 60;

        [Range(0, 100)]
        [Display(Name = "Years of Study Weight (%)")]
        public int YearsWeight { get; set; } = 20;

        [Range(0, 100)]
        [Display(Name = "Language Weight (%)")]
        public int LanguageWeight { get; set; } = 20;

        // Bonus points (added on top of the weighted score)
        [Display(Name = "Bonus per Extra Language")]
        public decimal LanguageBonus { get; set; } = 0.5m;

        [Display(Name = "Bonus for Completed Diploma")]
        public decimal DiplomaBonus { get; set; } = 1.0m;

        [Display(Name = "Bonus per Recommendation Letter")]
        public decimal RecommendationBonus { get; set; } = 0.5m;

        [Display(Name = "Bonus for Complete Documents")]
        public decimal DocumentBonus { get; set; } = 0.5m;
    }
}