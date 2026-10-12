using JobBoard.Data;
using JobBoard.Models;
using JobBoard.Services;
using JobBoard.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace JobBoard.Controllers;

[Authorize(Roles = Roles.Seeker)]
public class SeekerController : Controller
{
    private readonly AppDbContext _db;
    private readonly UserManager<ApplicationUser> _users;
    private readonly IFileService _files;

    public SeekerController(AppDbContext db, UserManager<ApplicationUser> users, IFileService files)
    {
        _db = db;
        _users = users;
        _files = files;
    }

    public async Task<IActionResult> Index()
    {
        var user = await _users.GetUserAsync(User);
        if (user == null)
        {
            return Challenge();
        }

        var model = new SeekerCabinetViewModel
        {
            User = user,
            Resumes = await _db.Resumes
                .Include(r => r.Category)
                .Where(r => r.SeekerId == user.Id)
                .OrderByDescending(r => r.UpdatedAt)
                .ToListAsync(),
            Edit = new ProfileEditViewModel
            {
                Name = user.DisplayName,
                Email = user.Email ?? string.Empty,
                Phone = user.PhoneNumber ?? string.Empty
            }
        };
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditProfile(ProfileEditViewModel model)
    {
        var user = await _users.GetUserAsync(User);
        if (user == null)
        {
            return Challenge();
        }

        if (model.Photo is { Length: > 0 } && !_files.IsValidImage(model.Photo))
        {
            ModelState.AddModelError(nameof(model.Photo), "Допустимы изображения JPG, PNG, GIF, WEBP размером до 3 МБ");
        }

        if (!ModelState.IsValid)
        {
            TempData["Error"] = string.Join(" ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
            return RedirectToAction(nameof(Index));
        }

        string? newPhoto = null;
        if (model.Photo is { Length: > 0 })
        {
            newPhoto = await _files.SaveAvatarAsync(model.Photo);
        }

        var oldPhoto = user.PhotoPath;
        user.DisplayName = model.Name.Trim();
        user.PhoneNumber = model.Phone.Trim();
        var email = model.Email.Trim();
        if (!string.Equals(user.Email, email, StringComparison.OrdinalIgnoreCase))
        {
            user.Email = email;
            user.UserName = email;
        }

        if (newPhoto != null)
        {
            user.PhotoPath = newPhoto;
        }
        else if (model.RemovePhoto)
        {
            user.PhotoPath = null;
        }

        var result = await _users.UpdateAsync(user);
        if (!result.Succeeded)
        {
            _files.Delete(newPhoto);
            TempData["Error"] = string.Join(" ", result.Errors.Select(e => e.Description));
            return RedirectToAction(nameof(Index));
        }

        if (user.PhotoPath != oldPhoto)
        {
            _files.Delete(oldPhoto);
        }

        TempData["Success"] = "Профиль обновлён";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Applications()
    {
        var userId = _users.GetUserId(User)!;
        var items = await _db.JobApplications
            .Include(a => a.Vacancy).ThenInclude(v => v!.Employer)
            .Include(a => a.Resume)
            .Include(a => a.Messages)
            .Where(a => a.SeekerId == userId)
            .OrderByDescending(a => a.CreatedAt)
            .ToListAsync();
        return View(items);
    }
}
