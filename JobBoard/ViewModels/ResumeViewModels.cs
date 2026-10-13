using System.ComponentModel.DataAnnotations;
using JobBoard.Models;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace JobBoard.ViewModels;

public class WorkExperienceItem
{
    [Required(ErrorMessage = "Укажите компанию")]
    [MaxLength(200)]
    public string CompanyName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Укажите должность")]
    [MaxLength(200)]
    public string Position { get; set; } = string.Empty;

    [Range(1950, 2100, ErrorMessage = "Некорректный год начала")]
    public int YearFrom { get; set; } = DateTime.UtcNow.Year;

    [Range(1950, 2100, ErrorMessage = "Некорректный год окончания")]
    public int? YearTo { get; set; }

    [MaxLength(4000)]
    public string? Responsibilities { get; set; }
}

public class EducationItem
{
    public EducationKind Kind { get; set; }

    [Required(ErrorMessage = "Укажите учебное заведение или организатора курсов")]
    [MaxLength(250)]
    public string Institution { get; set; } = string.Empty;

    [MaxLength(250)]
    public string? Specialty { get; set; }

    [Range(1950, 2100, ErrorMessage = "Некорректный год начала")]
    public int? YearFrom { get; set; }

    [Range(1950, 2100, ErrorMessage = "Некорректный год окончания")]
    public int? YearTo { get; set; }
}

public class ResumeFormViewModel
{
    public int? Id { get; set; }

    [Required(ErrorMessage = "Укажите название резюме")]
    [MaxLength(200)]
    [Display(Name = "Название резюме (должность)")]
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "Выберите категорию")]
    [Display(Name = "Категория")]
    public int? CategoryId { get; set; }

    [Range(0, 1000000000, ErrorMessage = "Некорректная сумма")]
    [Display(Name = "Ожидаемая заработная плата")]
    public decimal? ExpectedSalary { get; set; }

    [MaxLength(100)]
    [Display(Name = "Telegram")]
    public string? Telegram { get; set; }

    [EmailAddress(ErrorMessage = "Некорректный email")]
    [MaxLength(150)]
    [Display(Name = "Email")]
    public string? ContactEmail { get; set; }

    [MaxLength(50)]
    [Display(Name = "Номер телефона")]
    public string? Phone { get; set; }

    [MaxLength(300)]
    [Display(Name = "Ссылка на Facebook")]
    public string? Facebook { get; set; }

    [MaxLength(300)]
    [Display(Name = "Ссылка на LinkedIn")]
    public string? LinkedIn { get; set; }

    public List<WorkExperienceItem> WorkExperiences { get; set; } = new();
    public List<EducationItem> Educations { get; set; } = new();

    public List<SelectListItem>? Categories { get; set; }
}

public class PagerModel
{
    public int Page { get; set; }
    public int TotalPages { get; set; }
    public string Action { get; set; } = "Index";
    public string Controller { get; set; } = string.Empty;
    public Dictionary<string, string?> Route { get; set; } = new();
}

public class ResumeCatalogViewModel
{
    public List<Resume> Items { get; set; } = new();
    public List<Category> Categories { get; set; } = new();
    public int? CategoryId { get; set; }
    public int Page { get; set; }
    public int TotalPages { get; set; }
}

public class SeekerCabinetViewModel
{
    public ApplicationUser User { get; set; } = null!;
    public List<Resume> Resumes { get; set; } = new();
    public ProfileEditViewModel Edit { get; set; } = new();
}

public class SeekerProfileViewModel
{
    public ApplicationUser User { get; set; } = null!;
    public List<Resume> Resumes { get; set; } = new();
}

public class VacancyListViewModel
{
    public List<Vacancy> Items { get; set; } = new();
    public List<Category> Categories { get; set; } = new();
    public string? Query { get; set; }
    public int? CategoryId { get; set; }
    public string Sort { get; set; } = "new";
    public int Page { get; set; }
    public int TotalPages { get; set; }
}

public class VacancyDetailsViewModel
{
    public Vacancy Vacancy { get; set; } = null!;
    public List<Resume> MyResumes { get; set; } = new();
    public int? ExistingApplicationId { get; set; }
}

public class ChatViewModel
{
    public JobApplication Application { get; set; } = null!;
    public List<ChatMessage> Messages { get; set; } = new();
    public string CurrentUserId { get; set; } = string.Empty;
    public bool IsSeeker { get; set; }
}

public class EmployerApplicationsViewModel
{
    public List<JobApplication> Applications { get; set; } = new();
    public List<Vacancy> Vacancies { get; set; } = new();
    public int? VacancyId { get; set; }
}
