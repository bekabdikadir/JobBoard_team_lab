using System.ComponentModel.DataAnnotations;

namespace JobBoard.Models;

public enum EducationKind
{
    Education = 0,
    Course = 1
}

public class Education
{
    public int Id { get; set; }

    public int ResumeId { get; set; }
    public Resume? Resume { get; set; }

    public EducationKind Kind { get; set; }

    [Required, MaxLength(250)]
    public string Institution { get; set; } = string.Empty;

    [MaxLength(250)]
    public string? Specialty { get; set; }

    public int? YearFrom { get; set; }
    public int? YearTo { get; set; }
}
