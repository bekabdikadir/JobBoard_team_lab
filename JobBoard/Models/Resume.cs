using System.ComponentModel.DataAnnotations;

namespace JobBoard.Models;

public class Resume
{
    public int Id { get; set; }

    [Required]
    public string SeekerId { get; set; } = string.Empty;
    public ApplicationUser? Seeker { get; set; }

    [Required, MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    public int CategoryId { get; set; }
    public Category? Category { get; set; }

    public decimal? ExpectedSalary { get; set; }

    [MaxLength(100)]
    public string? Telegram { get; set; }

    [MaxLength(150)]
    public string? ContactEmail { get; set; }

    [MaxLength(50)]
    public string? Phone { get; set; }

    [MaxLength(300)]
    public string? Facebook { get; set; }

    [MaxLength(300)]
    public string? LinkedIn { get; set; }

    public bool IsPublished { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public List<WorkExperience> WorkExperiences { get; set; } = new();
    public List<Education> Educations { get; set; } = new();
}
