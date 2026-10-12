using System.ComponentModel.DataAnnotations;

namespace JobBoard.Models;

public class WorkExperience
{
    public int Id { get; set; }

    public int ResumeId { get; set; }
    public Resume? Resume { get; set; }

    [Required, MaxLength(200)]
    public string CompanyName { get; set; } = string.Empty;

    [Required, MaxLength(200)]
    public string Position { get; set; } = string.Empty;

    public int YearFrom { get; set; }
    public int? YearTo { get; set; }

    [MaxLength(4000)]
    public string? Responsibilities { get; set; }
}
