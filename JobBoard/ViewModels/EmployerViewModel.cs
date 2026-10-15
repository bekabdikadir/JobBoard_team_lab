using System.ComponentModel.DataAnnotations;
using JobBoard.Models;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace JobBoard.ViewModels;

public class EmployerCabinetViewModel
{
    public ApplicationUser User { get; set; } = null!;
    public List<Vacancy> Vacancies { get; set; } = new();
    public ProfileEditViewModel Edit { get; set; } = new();
}

public class VacancyFormViewModel 
{
    public int? Id { get; set; }

    [Required(ErrorMessage = "Укажите название вакансии")]
    [MaxLength(200)]
    [Display(Name = "Название вакансии")]
    public string Title { get; set; } = string.Empty;

    [Range(0, 1000000000, ErrorMessage = "Зарплата должна быть от 0 до 1 000 000 000")]
    [Display(Name = "Желаемая оплата")]
    public decimal? Salary { get; set; }

    [Required(ErrorMessage = "Добавьте описание вакансии")]
    [MaxLength(8000)]
    [Display(Name = "Описание")]
    public string Description { get; set; } = string.Empty;

    [MaxLength(4000)]
    [Display(Name = "Требования")]
    public string? Requirements { get; set; }

    [Range(0, 50, ErrorMessage = "Опыт должен быть от 0 до 50 лет")]
    [Display(Name = "Опыт работы от (лет)")]
    public int? ExperienceFrom { get; set; }

    [Range(0, 50, ErrorMessage = "Опыт должен быть от 0 до 50 лет")]
    [Display(Name = "Опыт работы до (лет)")]
    public int? ExperienceTo { get; set; }

    [Required(ErrorMessage = "Выберите категорию")]
    [Display(Name = "Категория")]
    public int? CategoryId { get; set; }

    public List<SelectListItem>? Categories { get; set; }

   
}