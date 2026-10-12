using System.ComponentModel.DataAnnotations;
using JobBoard.Models;

namespace JobBoard.ViewModels;

public class RegisterViewModel
{
    [Required(ErrorMessage = "Выберите роль")]
    [Display(Name = "Роль")]
    public string Role { get; set; } = Roles.Seeker;

    [Required(ErrorMessage = "Укажите имя")]
    [MaxLength(150)]
    [Display(Name = "Имя пользователя")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Укажите email")]
    [EmailAddress(ErrorMessage = "Некорректный email")]
    [Display(Name = "Email")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Укажите номер телефона")]
    [Phone(ErrorMessage = "Некорректный номер телефона")]
    [Display(Name = "Номер телефона")]
    public string Phone { get; set; } = string.Empty;

    [Required(ErrorMessage = "Укажите пароль")]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "Пароль должен содержать не менее 6 символов")]
    [DataType(DataType.Password)]
    [Display(Name = "Пароль")]
    public string Password { get; set; } = string.Empty;

    [Required(ErrorMessage = "Подтвердите пароль")]
    [Compare(nameof(Password), ErrorMessage = "Пароли не совпадают")]
    [DataType(DataType.Password)]
    [Display(Name = "Подтверждение пароля")]
    public string ConfirmPassword { get; set; } = string.Empty;

    [Display(Name = "Фото профиля")]
    public IFormFile? Photo { get; set; }
}

public class LoginViewModel
{
    [Required(ErrorMessage = "Укажите email")]
    [EmailAddress(ErrorMessage = "Некорректный email")]
    [Display(Name = "Email")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Укажите пароль")]
    [DataType(DataType.Password)]
    [Display(Name = "Пароль")]
    public string Password { get; set; } = string.Empty;

    [Display(Name = "Запомнить меня")]
    public bool RememberMe { get; set; }
}

public class ProfileEditViewModel
{
    [Required(ErrorMessage = "Укажите имя")]
    [MaxLength(150)]
    [Display(Name = "Имя")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Укажите email")]
    [EmailAddress(ErrorMessage = "Некорректный email")]
    [Display(Name = "Email")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Укажите номер телефона")]
    [Phone(ErrorMessage = "Некорректный номер телефона")]
    [Display(Name = "Номер телефона")]
    public string Phone { get; set; } = string.Empty;

    [Display(Name = "Новое фото")]
    public IFormFile? Photo { get; set; }

    public bool RemovePhoto { get; set; }
}
