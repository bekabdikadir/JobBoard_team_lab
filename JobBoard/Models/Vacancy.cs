using System.ComponentModel.DataAnnotations;

namespace JobBoard.Models;

public class Vacancy
{
    public int Id { get; set; }

    [Required, MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    public decimal? Salary { get; set; }
    [MaxLength(4000)]
    public string? Requirements { get; set; }

    [Required]
    public string Description { get; set; } = string.Empty;

    public int? ExperienceFrom { get; set; }
    public int? ExperienceTo { get; set; }

    public int CategoryId { get; set; }
    public Category? Category { get; set; }

    [Required]
    public string EmployerId { get; set; } = string.Empty;
    public ApplicationUser? Employer { get; set; }

    public bool IsPublished { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
