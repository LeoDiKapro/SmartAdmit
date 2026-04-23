using System.ComponentModel.DataAnnotations;

public class MasterProgramViewModel
{
    public int Id { get; set; }

    public int UniversityId { get; set; }

    [Required]
    [StringLength(200)]
    [Display(Name = "Program Name")]
    public string Name { get; set; } = string.Empty;

    [Required]
    [Range(0.0, 4.0)]
    [Display(Name = "Minimum GPA")]
    public decimal MinGPA { get; set; }

    [Required]
    [Range(1, 10)]
    [Display(Name = "Minimum Years of Study")]
    public int MinYears { get; set; }
}