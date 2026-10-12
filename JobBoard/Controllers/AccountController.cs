using JobBoard.Models;
using JobBoard.Services;
using JobBoard.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace JobBoard.Controllers;

public class AccountController : Controller
{
    private readonly UserManager<ApplicationUser> _users;
    private readonly SignInManager<ApplicationUser> _signIn;
    private readonly IFileService _files;

    public AccountController(UserManager<ApplicationUser> users, SignInManager<ApplicationUser> signIn, IFileService files)
    {
        _users = users;
        _signIn = signIn;
        _files = files;
    }

    [HttpGet]
    public IActionResult Register()
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirectToAction("Index", "Home");
        }
        return View(new RegisterViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (model.Role != Roles.Seeker && model.Role != Roles.Employer)
        {
            ModelState.AddModelError(nameof(model.Role), "Недопустимая роль");
        }

        if (model.Photo is { Length: > 0 } && !_files.IsValidImage(model.Photo))
        {
            ModelState.AddModelError(nameof(model.Photo), "Допустимы изображения JPG, PNG, GIF, WEBP размером до 3 МБ");
        }

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        string? photoPath = null;
        if (model.Photo is { Length: > 0 })
        {
            photoPath = await _files.SaveAvatarAsync(model.Photo);
        }

        var user = new ApplicationUser
        {
            UserName = model.Email.Trim(),
            Email = model.Email.Trim(),
            PhoneNumber = model.Phone.Trim(),
            DisplayName = model.Name.Trim(),
            PhotoPath = photoPath
        };

        var result = await _users.CreateAsync(user, model.Password);
        if (!result.Succeeded)
        {
            _files.Delete(photoPath);
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
            return View(model);
        }

        await _users.AddToRoleAsync(user, model.Role);
        await _signIn.SignInAsync(user, isPersistent: false);
        return RedirectToRoleHome(model.Role);
    }

    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirectToAction("Index", "Home");
        }
        ViewData["ReturnUrl"] = returnUrl;
        return View(new LoginViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var result = await _signIn.PasswordSignInAsync(model.Email.Trim(), model.Password, model.RememberMe, lockoutOnFailure: false);
        if (!result.Succeeded)
        {
            ModelState.AddModelError(string.Empty, "Неверный email или пароль");
            return View(model);
        }

        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
        {
            return Redirect(returnUrl);
        }

        var user = await _users.FindByEmailAsync(model.Email.Trim());
        var role = user != null && await _users.IsInRoleAsync(user, Roles.Employer) ? Roles.Employer : Roles.Seeker;
        return RedirectToRoleHome(role);
    }

    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await _signIn.SignOutAsync();
        return RedirectToAction("Index", "Home");
    }

    [HttpGet]
    public IActionResult AccessDenied()
    {
        return View();
    }

    private IActionResult RedirectToRoleHome(string role)
    {
        return role == Roles.Employer
            ? RedirectToAction("Catalog", "Resumes")
            : RedirectToAction("Index", "Seeker");
    }
}
